using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Altinn.Dan.Plugin.Nsg.Config;
using Altinn.Dan.Plugin.Nsg.Models;
using Altinn.Dan.Plugin.Nsg.RegisteredInformation.Models;
using Dan.Common;
using Dan.Common.Exceptions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Identifier = Altinn.Dan.Plugin.Nsg.RegisteredInformation.Models.Identifier;

namespace Altinn.Dan.Plugin.Nsg
{
    public class NSGv1
    {

        private readonly HttpClient _client;
        private readonly IEvidenceSourceMetadata _evidenceSourceMetadata;
        private readonly IEntityRegistryService _entityRegistryService;
        private readonly ApplicationSettings _settings;

        public NSGv1(IHttpClientFactory httpClientFactory, IEvidenceSourceMetadata evidenceSourceMetadata, IEntityRegistryService entityRegistryService, IOptions<ApplicationSettings> settings)
        {
            _evidenceSourceMetadata = evidenceSourceMetadata;
            _entityRegistryService = entityRegistryService;
            _client = httpClientFactory.CreateClient(Constants.SafeHttpClient);
            _settings = settings.Value;
        }

        [Function("Is-Alive")]
        public HttpResponseData CheckIsAlive(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req)
        {
            bool isAlive = true;

            if (isAlive)
            {
                return req.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("registered-organisations")]
        public async Task<HttpResponseData> RegisteredInformation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
        {
            var input = await req.ReadFromJsonAsync<RegisteredInformationRequest>();
            var info = await GetRegisteredInformation(input);

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(info);
            return response;
        }

        private async Task<RegisteredInformationResponse> GetRegisteredInformation(RegisteredInformationRequest input)
        {
            var url = string.Empty;

            switch (input.country)
            {
                case "NO": return await GetFromNorway(input.organisationNumber);
                case "SE": return await GetFromSweden(input.organisationNumber);
                case "FI": return await GetFromFinland(input.organisationNumber);
                case "IS":return await GetFromIceland(input.organisationNumber);
                case "DE":return await GetFromDenmark(input.organisationNumber);
                default: throw new EvidenceSourcePermanentClientException(1, "Invalid country code");
            }
        }

        private Task<RegisteredInformationResponse> GetFromDenmark(string organisationNumber)
        {
            throw new NotImplementedException();
        }

        private Task<RegisteredInformationResponse> GetFromIceland(string organisationNumber)
        {
            throw new NotImplementedException();
        }

        private Task<RegisteredInformationResponse> GetFromFinland(string organisationNumber)
        {
            throw new NotImplementedException();
        }

        private Task<RegisteredInformationResponse> GetFromSweden(string organisationNumber)
        {
            throw new NotImplementedException();
        }

        private async Task<RegisteredInformationResponse> GetFromNorway(string organizationNumber)
        {
            if (!Regex.IsMatch(organizationNumber, @"^\d{9}$"))
            {
                throw new EvidenceSourcePermanentClientException(
                    EvidenceSourceMetadata.ErrorInvalidInput, "Invalid company-id");
            }

            var unit = await _entityRegistryService.GetFull(organizationNumber, attemptSubUnitLookupIfNotFound: false);

            if (unit is null || unit.Slettedato is not null)
            {
                throw new EvidenceSourcePermanentClientException(
                    EvidenceSourceMetadata.ErrorOrganizationNotFound, "Could not find provided company-id");
            }

            var response = new RegisteredInformationResponse();

            response.registrationDate = unit.RegistreringsdatoEnhetsregisteret.Value.UtcDateTime;
            response.name = unit.Navn;
                //identifier = "",
                response.registeredAddress = new Registeredaddress()
                {
                    fullAddress = string.Join(',', unit.Forretningsadresse.Adresse)
                                  + ", " + unit.Forretningsadresse.Postnummer
                                  + ", " + unit.Forretningsadresse.Poststed
                                  + ", " + CountryCodesHelper.GetByCode(unit.Forretningsadresse.Landkode)
                };

                response.postalAddress = new Postaladdress()
                {
                    fullAddress = string.Join(',', unit.Postadresse.Adresse)
                                  + ", " + unit.Postadresse.Postnummer
                                  + ", " + unit.Postadresse.Poststed
                                  + ", " + CountryCodesHelper.GetByCode(unit.Postadresse.Landkode)
                };
                response.legalForm = new Legalform()
                {
                    name = unit.Organisasjonsform.Beskrivelse,
                    code = "NO_" + unit.Organisasjonsform.Kode
                };
                response.activity = new List<Activity>();

                if (unit.Naeringskode1 != null)
                    response.activity.Add(
                    new Activity()
                    {
                        code = unit.Naeringskode1.Kode.Replace(".", "").Substring(0, 4),
                        sequence = 1,
                        inClassification = "http://data.europa.eu/ux2/nace2/nace2",
                        reference = "http://data.europa.eu/ux2/nace2/5020",
                    });

                if (unit.Naeringskode2 != null)
                    response.activity.Add(
                        new Activity()
                        {
                            code = unit.Naeringskode2.Kode.Replace(".", "").Substring(0, 4),
                            sequence = 2,
                            inClassification = "http://data.europa.eu/ux2/nace2/nace2",
                            reference = "http://data.europa.eu/ux2/nace2/5020",
                        });

                if (unit.Naeringskode3 != null)
                    response.activity.Add(
                        new Activity()
                        {
                            code = unit.Naeringskode3.Kode.Replace(".", "").Substring(0, 4),
                            sequence = 3,
                            inClassification = "http://data.europa.eu/ux2/nace2/nace2",
                            reference = "http://data.europa.eu/ux2/nace2/5020",
                        });
        
                response.identifier = new Identifier()
                {
                    issuingAuthorityName = "Brønnøysundregistrene",
                    notation = unit.Organisasjonsnummer
                };
                response.legalStatus = new Legalstatus();
                response.legalStatus.code = unit.UnderTvangsavviklingEllerTvangsopplosning!.Value || unit.UnderAvvikling!.Value || unit.Konkurs!.Value
                    ? "NONE"
                    : "SOME";
                response.legalStatus.name = response.legalStatus.code == "NONE"
                        ? "No extraordinary circumstances registered"
                        : "Some extraordinary circumstances registered";

            return response;
        }

        private async Task<T> MakeRequest<T>(string target)
        {
            HttpResponseMessage result;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, target);
                result = await _client.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                throw new EvidenceSourceTransientException(EvidenceSourceMetadata.ErrorUpstreamError, "Error communicating with upstream source", ex);
            }

            if (!result.IsSuccessStatusCode)
            {
                throw result.StatusCode switch
                {
                    HttpStatusCode.NotFound => new EvidenceSourcePermanentClientException(
                        EvidenceSourceMetadata.ErrorOrganizationNotFound, "Upstream source could not find provided company-id"),
                    HttpStatusCode.BadRequest => new EvidenceSourcePermanentClientException(
                        EvidenceSourceMetadata.ErrorInvalidInput,
                        "Upstream source indicated an invalid company-id (400)"),
                    _ => new EvidenceSourceTransientException(EvidenceSourceMetadata.ErrorUpstreamError,
                        $"Upstream source retuned an HTTP error code ({(int)result.StatusCode})")
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
