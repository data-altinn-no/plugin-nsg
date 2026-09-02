using Altinn.Dan.Plugin.Nsg.Config;
using Altinn.Dan.Plugin.Nsg.Exceptions;
using Altinn.Dan.Plugin.Nsg.Extensions;
using Altinn.Dan.Plugin.Nsg.Models;
using Altinn.Dan.Plugin.Nsg.Models.RegisteredInformation;
using Dan.Common;
using Dan.Common.Enums;
using Dan.Common.Exceptions;
using Dan.Common.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Identifier = Altinn.Dan.Plugin.Nsg.Models.RegisteredInformation.Identifier;

namespace Altinn.Dan.Plugin.Nsg
{
    public class NSGv1
    {
        private readonly HttpClient _client;
        private readonly IEntityRegistryService _entityRegistryService;
        private readonly ApplicationSettings _settings;
        private readonly ILogger _logger;
        private readonly ITokenCacheProvider _tokenCacheProvider;

        public NSGv1(IHttpClientFactory httpClientFactory, IEntityRegistryService entityRegistryService, IOptions<ApplicationSettings> settings, ILoggerFactory loggerFactory, ITokenCacheProvider tokenCacheProvider)
        {
            _entityRegistryService = entityRegistryService;
            _client = httpClientFactory.CreateClient(Constants.SafeHttpClient);
            _settings = settings.Value;
            _logger = loggerFactory.CreateLogger<NSGv1>();
            _tokenCacheProvider = tokenCacheProvider;
        }

        [Function("Is-Alive")]
        public async Task<HttpResponseData> CheckIsAlive(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req)
        {
            if (await _entityRegistryService.IsMainUnit("985619433"))
                return req.CreateResponse(HttpStatusCode.OK);
            else
                return req.CreateResponse(HttpStatusCode.InternalServerError);

        }

        [Function("registered-organisations")]
        public async Task<HttpResponseData> RegisteredInformation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
        {
            var requestHeader = req.Headers.TryGetValues("x-request-id", out var values) ? values.First() : "NOT_SET";
            requestHeader = string.IsNullOrEmpty(requestHeader) ? "NOT SET" : requestHeader;
            _logger.LogInformation($"registered-organisations called with custom header {requestHeader}");

            var input = await req.ReadFromJsonAsync<RegisteredInformationRequest>();
            try
            {
                if (input == null)
                {
                    _logger.LogWarning("registered-organisations called with empty or invalid JSON body. requestHeader={RequestHeader}", requestHeader);
                    throw new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "invalid", "Body",
                        "Request body is missing, empty, or not valid JSON", 400, "Invalid request body");
                }

                _logger.DanLog(LogAction.DatasetRequested, owner: "NSG", requestor: "OpenData", serviceContext: "NSG", evidenceCodeName: "Registered Organisations");
                var info = await GetRegisteredInformation(input, requestHeader);
                var response = req.CreateResponse();
                await response.WriteAsJsonAsync(info);
                _logger.DanLog(LogAction.DatasetRetrieved, owner: "NSG", requestor: "OpenData", serviceContext: "NSG", evidenceCodeName: "Registered Organisations");
                return response;
            }
            catch (NsgException ex)
            {
                var errorResponse = new NSGErrorModel()
                {
                    code = ex.ErrorCode,
                    detail = ex.ErrorDetail,
                    instance = ex.ErrorInstance,
                    requestId = requestHeader,
                    source = ex.ErrorSource,
                    status = ex.ErrorStatus,
                    timestamp = DateTime.Now.ToUniversalTime(),
                    title = ex.ErrorTitle,
                    type = ex.ErrorType
                };

                var response = req.CreateResponse((HttpStatusCode)ex.ErrorStatus);
                await response.WriteAsJsonAsync(errorResponse);
                return response;
            }
        }

        private async Task<RegisteredInformationResponse> GetRegisteredInformation(RegisteredInformationRequest input, string headerValue)
        {
            _logger.LogInformation("GetRegisteredInformation called with Country={Country}, Notation={Notation}",
                input.Country, input.Notation);

            // Tolerér ulike casinger fra konsumenter ("no", "Se", "fi", osv.)
            var country = input.Country?.ToUpperInvariant();

            switch (country)
            {
                case "":
                case "NO": return await GetFromNorway(input.Notation, headerValue);
                case "SE": return await GetFromSweden(input.Notation, headerValue);
                case "FI": return await GetFromFinland(input.Notation, headerValue);
                case "IS": return await GetFromIceland(input.Notation);
                case "DE": return await GetFromDenmark(input.Notation);
                default:
                    _logger.LogWarning("Invalid Country code received: '{Country}'", input.Country);
                    throw new EvidenceSourcePermanentClientException(1, "Invalid Country code");
            }
        }

        private Task<RegisteredInformationResponse> GetFromDenmark(string organisationNumber)
        {
            throw new NotImplementedException();
        }

        private async Task<RegisteredInformationResponse> GetFromIceland(string organisationNumber)
        {
            var request = new HttpRequestMessage()
            {
                // Content = new StringContent(JsonConvert.SerializeObject(requestbody), Encoding.UTF8, "application/json"),
                Method = HttpMethod.Get,
                RequestUri = new Uri(string.Format(_settings.GetRegisteredInformationUrl("IS"), organisationNumber))
            };

            request.Headers.TryAddWithoutValidation("ocp-apim-subscription-key", _settings.ClientSecretIs);
            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            request.Headers.TryAddWithoutValidation("Accept", "application/json;charset=utf-8");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Successfully retrieved from Iceland for Notation {organisationNumber}");
                return TryDeserializeOrThrow<RegisteredInformationResponse>(content, "Iceland", organisationNumber);
            }
            else
            {
                var errorResponse = JsonConvert.DeserializeObject<NSGErrorModel>(await response.Content.ReadAsStringAsync());

                if (errorResponse == null)
                {
                    throw new NsgException("TBD", "urn:bronnoysundregistrene:error:unknown", "server.error", "",
                        "Could not process response from external api, " + response.ReasonPhrase, (int)response.StatusCode, "Remote server error");
                }
                else
                {
                    throw new NsgException(errorResponse);
                }
            }
        }

        private async Task<RegisteredInformationResponse> GetFromFinland(string organisationNumber, string headerValue)
        {
            var requestbody = new RegisteredInformationRequest()
            {
                Notation = organisationNumber,
                Country = "FI"
            };

            var request = new HttpRequestMessage()
            {
                Content = new StringContent(JsonConvert.SerializeObject(requestbody), Encoding.UTF8, "application/json"),
                Method = HttpMethod.Post,
                RequestUri = new Uri(string.Format(_settings.ProxyUrl, _settings.GetRegisteredInformationUrl("FI").Replace("https://", "")))
            };

            request.Content.Headers.ContentType.CharSet = string.Empty;

            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };

            var client = new HttpClient(handler);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Successfully retrieved from Finland for Notation {organisationNumber}");
                return TryDeserializeOrThrow<RegisteredInformationResponse>(content, "Finland", organisationNumber);
            }
            else
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonConvert.DeserializeObject<NSGErrorModel>(content);

                    if (errorResponse == null)
                    {
                        throw new NsgException("TBD", "urn:bronnoysundregistrene:error:unknown", "server.error", "",
                            "Could not process response from external api, " + response.ReasonPhrase, (int)response.StatusCode, "Remote server error");
                    }
                    else
                    {
                        throw new NsgException(errorResponse);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing response from Finland");
                    throw new NsgException("TBD", "urn:bronnoysundregistrene:error:unknown", "server.error", "",
                                               "Could not process response from external api, " + response.ReasonPhrase, (int)response.StatusCode, "Remote server error");
                }

            }
        }


        private async Task<RegisteredInformationResponse> GetFromSweden(string organisationNumber, string header)
        {
            if (string.IsNullOrWhiteSpace(organisationNumber))
            {
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "invalid", "Notation",
                    "Notation cannot be null or empty", 400, "Invalid Notation");
            }

            var digits = new string(organisationNumber.Where(char.IsDigit).ToArray());

            if (digits.Length != 10 && digits.Length != 12)
            {
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "invalid", "Notation",
                    "Invalid identifier format", 400, "Invalid Notation");
            }

            if (!IsValidSwedishCheckDigit(digits))
            {
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "invalid", "Notation",
                    "Company registration number has an invalid check digit", 400, "Invalid check digit");
            }

            try
            {
                // VDM er primær datakilde. Hvis den feiler, gir vi opp hele forespørselen.
                var verdifullDatamengdeResponse = await GetFromVardefullaDatamangdeResponse(digits, header);

                // NSGB er sekundær — beriker svaret med registeredAddress.
                // Returnerer null ved feil; vi fortsetter da med VDM-data alene.
                var nsgbResponse = await GetFromSwedenNSGB(digits, header);

                return await MapOrgData(verdifullDatamengdeResponse, nsgbResponse);
            }
            catch (NsgException)
            {
                throw;
            }
            catch (VardefullaDatamangderException ex)
            {
                if (ex.Status == 404)
                {
                    _logger.LogWarning("404 source: Bolagsverket VDM API returned 404 for Notation {Notation}", digits);
                    throw new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "not.found", "Notation",
                        "Organisation does not exist or has been deleted", 404, "Not found");
                }
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:unknown", "server.error", "",
                    ex.Detail ?? "Error from Bolagsverket", ex.Status, ex.Title ?? "Remote server error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching from Sweden for Notation {Notation}", digits);
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:unknown", "server.error", "",
                    "Unexpected error fetching organisation data", 500, "Internal error");
            }
        }

        // Trygg deserialisering: logger body-en og kaster en pen NsgException
        // hvis JSON-en er ugyldig (f.eks. inneholder JavaScript-literalet `undefined`,
        // HTML feilside, tom respons, eller noe annet rart).
        private T TryDeserializeOrThrow<T>(string content, string source, string notation) where T : class
        {
            try
            {
                var result = JsonConvert.DeserializeObject<T>(content);
                if (result == null)
                {
                    _logger.LogError("Empty or null deserialization result from {Source} for Notation {Notation}. Body: {Body}",
                        source, notation, content);
                    throw new NsgException("TBD", "urn:bronnoysundregistrene:error:unknown", "server.error", "",
                        $"Empty or malformed response from {source} API", 500, "Remote server error");
                }
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from {Source} for Notation {Notation}. Body: {Body}",
                    source, notation, content);
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:unknown", "server.error", "",
                    $"Invalid JSON in response from {source} API: {ex.Message}", 500, "Remote server error");
            }
        }

        // Bolagsverkets "org finnes ikke"-signal er innbakt i et 200-svar der subfeltene
        // har fel.typ = "ORGANISATION_FINNS_EJ". Vi sjekker flere subfelt for å være robust
        // mot varierende svar (samme fel-typ dukker opp på flere sub-objekter i praksis).
        private static bool IsOrganisationNotFoundShell(Organisasjon org)
        {
            const string NotFoundErrorType = "ORGANISATION_FINNS_EJ";
            return org.AvregistreradOrganisation?.Fel?.Typ == NotFoundErrorType
                || org.Avregistreringsorsak?.Fel?.Typ == NotFoundErrorType
                || org.Organisationsnamn?.Fel?.Typ == NotFoundErrorType
                || org.Organisationsform?.Fel?.Typ == NotFoundErrorType;
        }

        // Bruk semikolon som skilletegn
        private static string BuildAddress(IEnumerable<string> streetLines, string postnummer, string poststed, string country)
        {
            var streetPart = streetLines == null
                ? null
                : string.Join(";", streetLines.Where(s => !string.IsNullOrWhiteSpace(s)));
            var postnummerAndSted = string.Join(" ",
                new[] { postnummer, poststed }.Where(s => !string.IsNullOrWhiteSpace(s)));

            var parts = new[] { streetPart, postnummerAndSted, country }
                .Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join(";", parts);
        }

        // Bygger en NACE Activity fra en rå Kode-streng. Returnerer null om
        // koden er null/blank, slik at kalleren kan hoppe over den.
        private static Activity TryCreateNaceActivity(string rawKode, int sequence)
        {
            if (string.IsNullOrWhiteSpace(rawKode)) return null;
            var digits = rawKode.Replace(".", "");
            if (string.IsNullOrEmpty(digits)) return null;
            var naceCode = digits.Length >= 4 ? digits.Substring(0, 4) : digits;
            return new Activity
            {
                code = naceCode,
                Sequence = sequence,
                InClassification = "http://data.europa.eu/ux2/nace2/nace2",
                Reference = $"http://data.europa.eu/ux2/nace2/{naceCode}",
            };
        }

        // Luhn-validering for svenske organisasjonsnummer og personnummer.
        // Bruker siste 10 siffer (dropper århundre for 12-sifret personnummer).
        private static bool IsValidSwedishCheckDigit(string digitsOnly)
        {
            if (digitsOnly == null || (digitsOnly.Length != 10 && digitsOnly.Length != 12))
                return false;

            var n = digitsOnly.Substring(digitsOnly.Length - 10);
            int sum = 0;
            for (int i = 0; i < 10; i++)
            {
                int d = n[i] - '0';
                int p = d * ((i % 2 == 0) ? 2 : 1);
                sum += (p > 9) ? p - 9 : p;
            }
            return sum % 10 == 0;
        }

        private async Task<VerdifullDatamengdeResponse> GetFromVardefullaDatamangdeResponse(string organisationNumber, string header)
        {
            organisationNumber = new string(organisationNumber.Where(char.IsDigit).ToArray());

            //Get auth token
            var token = await GenerateTokenSE(useCache: true);

            var requestbody = new OrganisationerRequest()
            {
                Identitetsbeteckning = organisationNumber
            };

            var request = new HttpRequestMessage()
            {
                Content = new StringContent(JsonConvert.SerializeObject(requestbody), Encoding.UTF8, "application/json"),
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_settings.HvdBaseUrl}organisationer")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Successfully retrieved from Sweden for Notation {organisationNumber}");

                return JsonConvert.DeserializeObject<VerdifullDatamengdeResponse>(content);
            }
            else
            {
                var errorResponse = JsonConvert.DeserializeObject<VardefullaDatamangderErrorModel>(await response.Content.ReadAsStringAsync());

                if (errorResponse == null)
                {
                    throw new VardefullaDatamangderException(
                        instance: "server.error",
                        status: (int)response.StatusCode,
                        timestamp: DateTime.UtcNow,
                        requestId: header,
                        title: "Remote server error",
                        detail: $"Could not process response from Bolagsverket API ({response.ReasonPhrase})");
                }
                else
                {
                    throw new VardefullaDatamangderException(errorResponse);
                }
            }
        }
        internal async Task<TokenResponse> GetTokenSE(bool useCache = false)
        {
            if (useCache && _settings.TokenCaching)
            {
                (bool hasCachedValue, TokenResponse cachedToken) = await _tokenCacheProvider.TryGetToken("TokenSE");
                if (hasCachedValue)
                {
                    _logger.LogInformation("Found cached TokenSE");
                    return cachedToken;
                }
            }
            string baseAddress = _settings.TokenUrlSE;

            string grant_type = "client_credentials";
            string client_id = _settings.ClientIdSE;
            string client_secret = _settings.ClientSecretSE;

            var clientCreds = Encoding.UTF8.GetBytes($"{client_id}:{client_secret}");
            var basicAuth = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(clientCreds));

            var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("grant_type", grant_type),
                new("scope", _settings.ScopeSE)
            });

            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, baseAddress)
            {
                Content = formContent
            };

            tokenRequest.Headers.Authorization = basicAuth;

            HttpResponseMessage tokenResponse = await _client.SendAsync(tokenRequest);
            var responseBody = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to obtain Bolagsverket token: {Status} {Reason} - {Body}",
                    (int)tokenResponse.StatusCode, tokenResponse.ReasonPhrase, responseBody);
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:unknown", "server.error", "",
                    $"Failed to obtain authentication token ({(int)tokenResponse.StatusCode} {tokenResponse.ReasonPhrase})",
                    500, "Authentication error");
            }

            var token = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

            if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                _logger.LogError("Token response from Bolagsverket was empty or malformed");
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:unknown", "server.error", "",
                    "Empty or malformed token response from authentication server",
                    500, "Authentication error");
            }

            if (useCache && _settings.TokenCaching)
            {
                // Cache token for 59 minutes
                await _tokenCacheProvider.Set("TokenSE", token,
                    new TimeSpan(0, 0, Math.Max(0, token.ExpiresIn - 60)));
            }

            return token;
        }

        /// <summary>
        /// Best-effort lookup mot NSGB-API-et. NSGB er en sekundærkilde som
        /// kun brukes for å berike svaret med registeredAddress (som VDM ikke har).
        /// Returnerer null ved enhver feil — caller fortsetter med VDM-data alene.
        /// </summary>
        private async Task<RegisteredInformationResponse> GetFromSwedenNSGB(string organisationNumber, string header)
        {
            organisationNumber = new string(organisationNumber.Where(char.IsDigit).ToArray());

            try
            {
                var token = await GetTokenSE(true);

                var requestbody = new RegisteredInformationRequest()
                {
                    Notation = organisationNumber
                };

                var request = new HttpRequestMessage()
                {
                    Content = new StringContent(JsonConvert.SerializeObject(requestbody), Encoding.UTF8, "application/json"),
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_settings.GetRegisteredInformationUrl("SE"))
                };

                request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token.AccessToken);
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json;charset=utf-8");

                var response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Successfully retrieved from NSGB for Notation {Notation}", organisationNumber);
                    return JsonConvert.DeserializeObject<RegisteredInformationResponse>(content);
                }

                // Ikke-success — logg og returner null. NSGB er ikke kritisk, så vi unngår å
                // kaste exception bare for at caller skal swallowe den.
                var rawBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation(
                    "NSGB Sweden returned non-success {Status} for Notation {Notation} — continuing with VDM only. Body: {Body}",
                    (int)response.StatusCode, organisationNumber, rawBody);
                return null;
            }
            catch (Exception ex)
            {
                // Uventet feil (token-fetch, nettverk, deserialisering osv.). NSGB er sekundær
                // — logg og fortsett uten å feile hele forespørselen.
                _logger.LogWarning(ex, "NSGB Sweden lookup failed for {Notation} — continuing with VDM only", organisationNumber);
                return null;
            }
        }

        private async Task<RegisteredInformationResponse> GetFromNorway(string organizationNumber, string header)
        {
            organizationNumber = new string(organizationNumber.Where(char.IsDigit).ToArray());
            if (!Regex.IsMatch(organizationNumber, @"^\d{9}$"))
            {
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "invalid", "Notation",
                    "Invalid identifier format", 500, "Invalid Notation");
            }

            var unit = await _entityRegistryService.GetFull(organizationNumber, attemptSubUnitLookupIfNotFound: true);

            if (unit is null || unit.Slettedato is not null)
            {
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "not.found", "Notation",
                    "Organisation does not exist or has been deleted", 404, "Not found");

            }

            var response = new RegisteredInformationResponse();

            response.RegistrationDate = unit.RegistreringsdatoEnhetsregisteret?.UtcDateTime.ToString("yyyy-MM-dd");
            response.Name = unit.Navn;
            //identifier = "",

            if (unit.Forretningsadresse != null)
            {
                response.RegisteredAddress = new Registeredaddress()
                {
                    FullAddress = BuildAddress(
                        unit.Forretningsadresse.Adresse,
                        unit.Forretningsadresse.Postnummer,
                        unit.Forretningsadresse.Poststed,
                        CountryCodesHelper.GetByCode(unit.Forretningsadresse.Landkode))
                };
            }

            if (unit.Postadresse != null)
            {
                response.PostalAddress = new Postaladdress()
                {
                    FullAddress = BuildAddress(
                        unit.Postadresse.Adresse,
                        unit.Postadresse.Postnummer,
                        unit.Postadresse.Poststed,
                        CountryCodesHelper.GetByCode(unit.Postadresse.Landkode))
                };
            }

            response.LegalForm = new Legalform()
            {
                Name = unit.Organisasjonsform?.Beskrivelse,
                Code = unit.Organisasjonsform?.Kode != null ? "NO_" + unit.Organisasjonsform.Kode : null
            };
            response.Activity = new List<Activity>();

            // Trygg mapping av norske naeringskoder. Beskytter mot null Kode og
            // koder kortere enn 4 tegn (Substring kaster ArgumentOutOfRangeException).
            var activity1 = TryCreateNaceActivity(unit.Naeringskode1?.Kode, 1);
            if (activity1 != null)
                response.Activity.Add(activity1);

            var activity2 = TryCreateNaceActivity(unit.Naeringskode2?.Kode, 2);
            if (activity2 != null)
                response.Activity.Add(activity2);

            var activity3 = TryCreateNaceActivity(unit.Naeringskode3?.Kode, 3);
            if (activity3 != null)
                response.Activity.Add(activity3);

            response.Identifier = new Identifier()
            {
                IssuingAuthorityName = "The Brønnøysund Register Centre",
                Notation = unit.Organisasjonsnummer
            };
            response.LegalStatus = new Legalstatus();
            var hasExtraordinaryCircumstances =
                (unit.UnderTvangsavviklingEllerTvangsopplosning ?? false) ||
                (unit.UnderAvvikling ?? false) ||
                (unit.Konkurs ?? false);
            response.LegalStatus.Code = hasExtraordinaryCircumstances ? "SOME" : "NONE";
            response.LegalStatus.Name = response.LegalStatus.Code == "NONE"
                    ? "No extraordinary circumstances registered"
                    : "Some extraordinary circumstances registered";

            return response;
        }

        internal async Task<TokenResponse> GenerateTokenSE(bool useCache = false)
        {
            // VDM bruker andre credentials/scope enn NSGB, så vi cacher under egen nøkkel.
            const string CacheKey = "TokenVdmSE";

            if (useCache && _settings.TokenCaching)
            {
                (bool hasCachedValue, TokenResponse cachedToken) = await _tokenCacheProvider.TryGetToken(CacheKey);
                if (hasCachedValue)
                {
                    _logger.LogInformation("Found cached {CacheKey}", CacheKey);
                    return cachedToken;
                }
            }

            var clientId = _settings.HvdClientId;
            var clientSecret = _settings.HvdClientSecret;
            var scope = _settings.HvdScope;

            var url = $"{_settings.HvdTokenUrl}oauth2/token";

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", scope)
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await _client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to retrieve token from Sweden API. Status: {response.StatusCode}, Response: {json}");
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:authentication", "authentication.failed", "",
                    "Failed to retrieve access token from Sweden API", (int)response.StatusCode, "Authentication failed");
            }

            var token = JsonConvert.DeserializeObject<TokenResponse>(json);

            if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                _logger.LogError("VDM token response from Bolagsverket was empty or malformed");
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:authentication", "authentication.failed", "",
                    "Empty or malformed VDM token response", 500, "Authentication failed");
            }

            if (useCache && _settings.TokenCaching)
            {
                // Cache token for 59 minutes
                await _tokenCacheProvider.Set(CacheKey, token,
                    new TimeSpan(0, 0, Math.Max(0, token.ExpiresIn - 60)));
            }

            return token;
        }

        private async Task<RegisteredInformationResponse> MapOrgData(VerdifullDatamengdeResponse orgData, RegisteredInformationResponse nsgbResponse)
        {
            // Svenske enkeltpersonforetak (enskild näringsverksamhet) kan ha flere
            // aktive registreringer (trade names) knyttet til samme legal entity.
            // Mappings-reglene (avklart med svensk side):
            //   1. Filtrer ut avregistrerte registreringer. Det inkluderer not found.
            //   2. identitetsbeteckning skal mappes til legalIdentifier. Samme legal identifier brukes for alle registreringer assosiert med samme legal identity.
            //   3. Name = navnene fra alle aktive registeringer, semikolon-separert i responsens rekkefølge.
            //   4. Activity = union av alle aktivitetskoder, deduplisert på kode vises bare en gang; sequence 1..N i responsens rekkefølge.
            //   5. RegistrationDate = eldste aktive registreringsdato.
            //   6. postalAddress = fra første aktive registrering (registeredAddress fra NSGB som før).
            //   7. LegalStatus = SOME_REGISTERED hvis noen aktiv registrering har extraordinary circumstances.
            var orgs = orgData?.Organisationer;
            if (orgs == null || orgs.Count == 0)
            {
                _logger.LogWarning("404 source: VDM returned 200 but empty Organisationer list");
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "not.found", "Notation",
                    "Organisation does not exist or has been deleted", 404, "Not found");
            }

            // Rule 1: filtrer bort avregistrerte registreringer
            var activeOrgs = orgs
                .Where(o => !IsOrganisationNotFoundShell(o))
                .Where(o => o.AvregistreradOrganisation?.Avregistreringsdatum.HasValue != true)
                .ToList();

            if (activeOrgs.Count == 0)
            {
                _logger.LogWarning("404 source: no active registrations after filtering shell/deregistered entries (had {Count} total)", orgs.Count);
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "not.found", "Notation",
                    "Organisation does not exist or has been deleted", 404, "Not found");
            }

            if (activeOrgs.Count > 1)
            {
                _logger.LogInformation(
                    "Bolagsverket returnerte {Count} aktive registreringer (sole trader med flere trade names). Aggregerer navn og aktiviteter.",
                    activeOrgs.Count);
            }

            var firstOrg = activeOrgs[0];

            // Rule 3: aggreger navn — første navn per aktiv registrering, semikolon-separert
            var aggregatedName = string.Join(";", activeOrgs
                .Select(o => o.Organisationsnamn?.OrganisationsnamnLista?.FirstOrDefault()?.Namn)
                .Where(name => !string.IsNullOrWhiteSpace(name)));

            // Rule 6: adresse fra første aktive registrering
            var post = firstOrg.PostadressOrganisation?.Postadress;
            string fullAddress = null;
            if (post != null)
            {
                var built = BuildAddress(new[] { post.Utdelningsadress }, post.Postnummer, post.Postort, country: null);
                fullAddress = string.IsNullOrWhiteSpace(built) ? null : built;
            }

            // Rule 4: aggreger aktiviteter, deduplisert på NACE-kode
            var activities = new List<Activity>();
            var seenCodes = new HashSet<string>();
            foreach (var org in activeOrgs)
            {
                var snis = org.NaringsgrenOrganisation?.Sni;
                if (snis == null) continue;
                foreach (var sni in snis.Where(s => !string.IsNullOrWhiteSpace(s.Kod)))
                {
                    var digits = sni.Kod.Replace(".", "");
                    if (string.IsNullOrEmpty(digits)) continue;
                    var naceCode = digits.Length >= 4 ? digits.Substring(0, 4) : digits;
                    if (!seenCodes.Add(naceCode)) continue;
                    activities.Add(new Activity
                    {
                        code = naceCode,
                        InClassification = "http://data.europa.eu/ux2/nace2/nace2",
                        Reference = $"http://data.europa.eu/ux2/nace2/{naceCode}",
                        Sequence = activities.Count + 1
                    });
                }
            }

            // Rule 5: eldste aktive registreringsdato
            var oldestOrg = activeOrgs
                .OrderBy(o => DateTime.TryParse(o.Organisationsdatum?.Registreringsdatum, out var d) ? d : DateTime.MaxValue)
                .First();
            var registrationDate = oldestOrg.Organisationsdatum?.Registreringsdatum;

            // Rule 7: SOME_REGISTERED hvis noen aktiv registrering har pågående prosesser
            var hasOngoingProceedings = activeOrgs.Any(o =>
                o.PagandeAvvecklingsEllerOmstruktureringsforfarande
                    ?.PagandeAvvecklingsEllerOmstruktureringsforfarandeLista?.Any() == true);
            var legalStatusCode = hasOngoingProceedings ? "SOME_REGISTERED" : "NO_REGISTERED";
            var legalStatusName = legalStatusCode == "NO_REGISTERED"
                ? "No circumstances registered"
                : "Some circumstances registered";

            return new RegisteredInformationResponse
            {
                Name = string.IsNullOrEmpty(aggregatedName) ? null : aggregatedName,

                RegistrationDate = registrationDate,

                // Rule 2: legalIdentifier fra første aktive (samme for alle uansett)
                Identifier = new Identifier
                {
                    Notation = firstOrg.Organisationsidentitet?.Identitetsbeteckning,
                    IssuingAuthorityName = "The Swedish Tax Agency"
                },

                LegalForm = new Legalform
                {
                    Code = firstOrg.Organisationsform?.Kod != null ? "SE_" + firstOrg.Organisationsform.Kod : null,
                    Name = firstOrg.Organisationsform?.Klartext
                },

                LegalStatus = new Legalstatus
                {
                    Code = legalStatusCode,
                    Name = legalStatusName
                },

                PostalAddress = new Postaladdress
                {
                    FullAddress = fullAddress
                },

                RegisteredAddress = nsgbResponse?.RegisteredAddress,

                Activity = activities
            };
        }

    }
}
