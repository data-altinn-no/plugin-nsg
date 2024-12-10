using Altinn.Dan.Plugin.Nsg.Config;
using Altinn.Dan.Plugin.Nsg.Exceptions;
using Altinn.Dan.Plugin.Nsg.Models;
using Altinn.Dan.Plugin.Nsg.Models.RegisteredInformation;
using Dan.Common;
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
            //requestHeader = string.IsNullOrEmpty(requestHeader) ? "NOT SET" : requestHeader;
            _logger.LogInformation($"registered-organisations called with custom header {requestHeader}");

            var input = await req.ReadFromJsonAsync<RegisteredInformationRequest>();
            try
            {
                var info = await GetRegisteredInformation(input, requestHeader);
                var response = req.CreateResponse();
                await response.WriteAsJsonAsync(info);
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

                    var response = req.CreateResponse();
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
                case "IS":return await GetFromIceland(input.Notation);
                case "DE":return await GetFromDenmark(input.Notation);
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
                RequestUri = new Uri(string.Format(_settings.ProxyUrl, _settings.GetRegisteredInformationUrl("FI").Replace("https://","")))
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

        private async Task<TokenResponse> GetTokenSE(bool useCache = false)
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
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(clientCreds));

            var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("grant_type", grant_type),
                new("scope", _settings.ScopeSE)
            });

            HttpResponseMessage tokenResponse = await _client.PostAsync(baseAddress, formContent);

            var token = JsonConvert.DeserializeObject<TokenResponse>(await tokenResponse.Content.ReadAsStringAsync());

            await _tokenCacheProvider.Set("TokenSE", token,
                new TimeSpan(0, 0, Math.Max(0, token.ExpiresIn - 5)));

            return token;

        }

        private async Task<RegisteredInformationResponse> GetFromSweden(string organisationNumber, string header)
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
                var errorResponse = JsonConvert.DeserializeObject<NSGErrorModel>(await response.Content.ReadAsStringAsync());

                if (errorResponse == null)
                {
                    throw new NsgException("TBD", "urn:bronnoysundregistrene:error:unknown", "server.error", "",
                        "Could not process response from external api, " + response.ReasonPhrase, (int)response.StatusCode, "Remote server error");
                } else
                {
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
                    IssuingAuthorityName = "Brønnøysundregistrene",
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

        private async Task<T> MakeRequest<T>(HttpRequestMessage message)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                result = await _client.SendAsync(message);
            }
            catch (HttpRequestException ex)
            {
                throw new NsgException("TBD", "urn:bronnoysundregistrene:error:network", "network.error", "","Request to remote api failed", (int) result.StatusCode, "Network error" );
            }

            if (!result.IsSuccessStatusCode)
            {
                throw result.StatusCode switch
                {
                    HttpStatusCode.NotFound => new NsgException("TBD", "urn:bronnoysundregistrene:error:validation", "not.found", "Notation",
                        "Organisation does not exist or has been deleted", 404, "Not found"),
                    HttpStatusCode.BadRequest => new NsgException("TBD", "urn:bronnoysundregistrene:error:network", "server.error", "",
                        "Request to remote api failed unexpectedly", (int) HttpStatusCode.BadRequest, "Not found"),
                    _ => new NsgException("TBD", "urn:bronnoysundregistrene:error", "server.error", "", "Request to remote api failed unexpectedly", (int) result.StatusCode, "Error")
                };
            }

            var response = JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());
            if (response == null)
            {
                throw new EvidenceSourcePermanentServerException(EvidenceSourceMetadata.ErrorUpstreamError,
                    "Did not understand the data model returned from upstream source");
            }

            return response;
        }
    }
}
