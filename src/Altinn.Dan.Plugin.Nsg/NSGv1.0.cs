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
            switch (input.Country)
            {
                case "":
                case "NO": return await GetFromNorway(input.Notation, headerValue);
                case "SE": return await GetFromSweden(input.Notation, headerValue);
                case "FI": return await GetFromFinland(input.Notation, headerValue);
                case "IS": return await GetFromIceland(input.Notation);
                case "DE": return await GetFromDenmark(input.Notation);
                default: throw new EvidenceSourcePermanentClientException(1, "Invalid Country code");
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
                return JsonConvert.DeserializeObject<RegisteredInformationResponse>(content);
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
                return JsonConvert.DeserializeObject<RegisteredInformationResponse>(content);
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
                var verdifullDatamengdeResponse = await GetFromVardefullaDatamangdeResponse(digits, header);

                RegisteredInformationResponse nsgbResponse = null;
                try
                {
                    nsgbResponse = await GetFromSwedenNSGB(digits, header);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "NSGB Sweden lookup failed for {Notation} — continuing with VDM only", digits);
                }

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

        private async Task<RegisteredInformationResponse> GetFromSwedenNSGB(string organisationNumber, string header)
        {
            //Get auth token
            organisationNumber = new string(organisationNumber.Where(char.IsDigit).ToArray());
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
                _logger.LogInformation($"Successfully retrieved from Sweden for Notation {organisationNumber}");
                return JsonConvert.DeserializeObject<RegisteredInformationResponse>(content);
            }
            else
            {
                var rawBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("NSGB Sweden returned non-success {Status} for Notation {Notation}. Body: {Body}",
                    (int)response.StatusCode, organisationNumber, rawBody);

                var errorResponse = JsonConvert.DeserializeObject<NSGErrorModel>(rawBody);

                if (errorResponse == null)
                {
                    throw new NsgException("TBD", "urn:bronnoysundregistrene:error:unknown", "server.error", "",
                        "Could not process response from external api, " + response.ReasonPhrase, (int)response.StatusCode, "Remote server error");
                }
                else
                {
                    _logger.LogWarning("404 source: NSGB Sweden API returned 404 (errorResponse parsed)");
                    throw new NsgException(errorResponse);
                }
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

            response.RegistrationDate = unit.RegistreringsdatoEnhetsregisteret!.Value.UtcDateTime.ToString("yyyy-MM-dd");
            response.Name = unit.Navn;
            //identifier = "",

            if (unit.Forretningsadresse != null)
            {
                response.RegisteredAddress = new Registeredaddress()
                {
                    FullAddress = string.Join(',', unit.Forretningsadresse!.Adresse)
                                  + ", " + unit.Forretningsadresse!.Postnummer
                                  + ", " + unit.Forretningsadresse!.Poststed
                                  + ", " + CountryCodesHelper.GetByCode(unit.Forretningsadresse!.Landkode)
                };
            }

            if (unit.Postadresse != null)
            {
                response.PostalAddress = new Postaladdress()
                {
                    FullAddress = string.Join(',', unit!.Postadresse!.Adresse)
                                  + ", " + unit!.Postadresse!.Postnummer
                                  + ", " + unit!.Postadresse!.Poststed
                                  + ", " + CountryCodesHelper.GetByCode(unit!.Postadresse!.Landkode)
                };
            }

            response.LegalForm = new Legalform()
            {
                Name = unit.Organisasjonsform.Beskrivelse,
                Code = "NO_" + unit.Organisasjonsform.Kode
            };
            response.Activity = new List<Activity>();

            if (unit.Naeringskode1 != null)
                response.Activity.Add(
                new Activity()
                {
                    code = unit.Naeringskode1.Kode.Replace(".", "").Substring(0, 4),
                    Sequence = 1,
                    InClassification = "http://data.europa.eu/ux2/nace2/nace2",
                    Reference = $"http://data.europa.eu/ux2/nace2/{unit.Naeringskode1.Kode.Replace(".", "").Substring(0, 4)}",
                });

            if (unit.Naeringskode2 != null)
                response.Activity.Add(
                    new Activity()
                    {
                        code = unit.Naeringskode2.Kode.Replace(".", "").Substring(0, 4),
                        Sequence = 2,
                        InClassification = "http://data.europa.eu/ux2/nace2/nace2",
                        Reference = $"http://data.europa.eu/ux2/nace2/{unit.Naeringskode2.Kode.Replace(".", "").Substring(0, 4)}",
                    });

            if (unit.Naeringskode3 != null)
                response.Activity.Add(
                    new Activity()
                    {
                        code = unit.Naeringskode3.Kode.Replace(".", "").Substring(0, 4),
                        Sequence = 3,
                        InClassification = "http://data.europa.eu/ux2/nace2/nace2",
                        Reference = $"http://data.europa.eu/ux2/nace2/{unit.Naeringskode3.Kode.Replace(".", "").Substring(0, 4)}",
                    });

            response.Identifier = new Identifier()
            {
                IssuingAuthorityName = "The Brønnøysund Register Centre",
                Notation = unit.Organisasjonsnummer
            };
            response.LegalStatus = new Legalstatus();
            response.LegalStatus.Code = unit.UnderTvangsavviklingEllerTvangsopplosning!.Value || unit.UnderAvvikling!.Value || unit.Konkurs!.Value
                ? "SOME"
                : "NONE";
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

            if (_settings.TokenCaching)
            {
                // Cache token for 59 minutes
                await _tokenCacheProvider.Set(CacheKey, token,
                    new TimeSpan(0, 0, Math.Max(0, token.ExpiresIn - 60)));
            }

            return token;
        }

        private async Task<RegisteredInformationResponse> MapOrgData(VerdifullDatamengdeResponse orgData, RegisteredInformationResponse nsgbResponse)
        {
            // Sole traders bruker personnummer som identifier og kan
            // ha flere "namnskyddslöpnummer" - ett per registrert virksomhetsnavn.
            // Vardefulla datamengder-APIet returnerer  en liste, så vi velger den oppføringen med siste registreringsdato (anbefalt fra Bolagsverket).
            var orgs = orgData?.Organisationer;
            if (orgs == null || orgs.Count == 0)
            {
                _logger.LogWarning("404 source: VDM returned 200 but empty Organisationer list");
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "not.found", "Notation",
                    "Organisation does not exist or has been deleted", 404, "Not found");
            }

            if (orgs.Count > 1)
            {
                _logger.LogInformation(
                    "Bolagsverket returnerte {Count} oppføringer (sole trader med flere namnskyddslöpnummer). Velger nyeste registreringsdato.",
                    orgs.Count);
            }

            var org = orgs
                .OrderByDescending(o => DateTime.TryParse(o.Organisationsdatum?.Registreringsdatum, out var d) ? d : DateTime.MinValue)
                .First();

            // Avregistrert organisasjon -> 404
            if (org.AvregistreradOrganisation?.Avregistreringsdatum.HasValue == true)
            {
                _logger.LogWarning("404 source: organisation has Avregistreringsdatum={Date}",
                    org.AvregistreradOrganisation.Avregistreringsdatum);
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "not.found", "Notation",
                    "Organisation does not exist or has been deleted", 404, "Not found");
            }

            var firstName = org.Organisationsnamn?.OrganisationsnamnLista?.FirstOrDefault();

            // Adresse — kommaseparert, hopp over tomme deler så vi ikke får ledende komma
            var post = org.PostadressOrganisation?.Postadress;
            string fullAddress = null;
            if (post != null)
            {
                var parts = new[] { post.Utdelningsadress, post.Postnummer, post.Postort }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();
                fullAddress = parts.Length > 0 ? string.Join(", ", parts) : null;
            }

            // Activities (SNI -> NACE: SNI er 5-sifret, NACE er 4-sifret. Kutt nasjonalt 5. siffer.)
            var activities = org.NaringsgrenOrganisation?.Sni?
                .Where(sni => !string.IsNullOrWhiteSpace(sni.Kod))
                .Select((sni, index) =>
                {
                    var digits = sni.Kod?.Replace(".", "");
                    var naceCode = digits.Length >= 4 ? digits.Substring(0, 4) : digits;
                    return new Activity
                    {
                        code = naceCode,
                        InClassification = "http://data.europa.eu/ux2/nace2/nace2",
                        Reference = $"http://data.europa.eu/ux2/nace2/{naceCode}",
                        Sequence = index + 1
                    };
                })
                .ToList();

            // LegalStatus: SOME_REGISTERED hvis det finnes pågående avvikling/omstrukturering.
            // (Hvis org var avregistrert ville vi allerede ha kastet 404 lenger oppe.)
            var hasOngoingProceedings = org.PagandeAvvecklingsEllerOmstruktureringsforfarande
                ?.PagandeAvvecklingsEllerOmstruktureringsforfarandeLista?.Any() == true;
            var legalStatusCode = hasOngoingProceedings
                ? "SOME_REGISTERED"
                : "NO_REGISTERED";
            var legalStatusName = legalStatusCode == "NO_REGISTERED"
                ? "No extraordinary circumstances registered"
                : "Some extraordinary circumstances registered";

            return new RegisteredInformationResponse
            {
                Name = firstName?.Namn,

                RegistrationDate = org.Organisationsdatum?.Registreringsdatum,

                Identifier = new Identifier
                {
                    Notation = org.Organisationsidentitet?.Identitetsbeteckning,
                    IssuingAuthorityName = "The Swedish Tax Agency"
                },

                LegalForm = new Legalform
                {
                    Code = org.Organisationsform?.Kod != null ? "SE_" + org.Organisationsform.Kod : null,
                    Name = org.Organisationsform?.Klartext
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

                Activity = activities ?? new List<Activity>()
            };           
        }

    }
}
