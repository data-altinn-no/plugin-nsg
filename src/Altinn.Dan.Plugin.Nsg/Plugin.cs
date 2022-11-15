using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.Dan.Plugin.Nsg.Models;
using Dan.Common;
using Dan.Common.Exceptions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using NorUnit = Altinn.Dan.Plugin.Nsg.Models.NOR.Unit;
using FinUnit = Altinn.Dan.Plugin.Nsg.Models.FIN.Unit;

namespace Altinn.Dan.Plugin.Nsg;

public class Plugin
{
    private readonly HttpClient _client;
    private readonly IEvidenceSourceMetadata _evidenceSourceMetadata;
    private readonly IEntityRegistryService _entityRegistryService;

    public Plugin(IHttpClientFactory httpClientFactory, IEvidenceSourceMetadata evidenceSourceMetadata, IEntityRegistryService entityRegistryService)
    {
        _evidenceSourceMetadata = evidenceSourceMetadata;
        _entityRegistryService = entityRegistryService;
        _client = httpClientFactory.CreateClient(Constants.SafeHttpClient);
    }

    [Function("NsgCompanyBasicInformation")]
    public async Task<HttpResponseData> NsgCompanyBasicInformation(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();

        return await EvidenceSourceResponse.CreateResponse(req,
            () => GetNsgCompanyBasicInformationDatasetName(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetNsgCompanyBasicInformationDatasetName(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        CompanyInformation companyInformation = await GetCompanyInformation(evidenceHarvesterRequest.OrganizationNumber);
        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, "NsgCompanyBasicInformation");
        ecb.AddEvidenceValue("default", JsonConvert.SerializeObject(companyInformation, Converter.Settings), EvidenceSourceMetadata.Source);

        return ecb.GetEvidenceValues();
    }

    private async Task<CompanyInformation> GetCompanyInformation(string identifier)
    {

        var parts = identifier.Split(':');
        if (parts.Length != 2)
        {
            throw new EvidenceSourcePermanentClientException(EvidenceSourceMetadata.ErrorInvalidInput, $"'{identifier}' has an unknown format. Expects a ISO/IEC 6523 identifier + \":\" + company-id ");
        }

        return parts[0] switch
        {
            "0192" => await GetFromNorway(parts[1], identifier),
            "0212" => await GetFromFinland(parts[1], identifier),
            _ => throw new EvidenceSourcePermanentClientException(
                EvidenceSourceMetadata.ErrorOrganizationNotFound,
                $"{parts[0]} is not a recognized ISO/IEC 6523 identifier. Supported ICDs are 0192 (Norway) and 0212 (Finland).")
        };
    }

    private async Task<CompanyInformation> GetFromNorway(string organizationNumber, string identifier)
    {
        var url = "https://data.brreg.no/enhetsregisteret/api/enheter/" + organizationNumber;
        var unit = await MakeRequest<NorUnit>(url);

        //var unit = await _entityRegistryService.GetFull(organizationNumber, attemptSubUnitLookupIfNotFound: false);

        if (unit.Slettedato != DateTimeOffset.MinValue)
        {
            throw new EvidenceSourcePermanentClientException(
                EvidenceSourceMetadata.ErrorOrganizationNotFound, "Upstream source could not find provided company-id");
        }

        var ci = new CompanyInformation
        {
            Identifier = new Identifier
            {
                Notation = organizationNumber,
                IssuingAuthorityName = "The Brønnøysund Register Centre"

            },
            Name = unit.Navn,
            RegistrationDate = unit.RegistreringsdatoEnhetsregisteret,
            LegalStatus = unit.UnderTvangsavviklingEllerTvangsopplosning || unit.UnderAvvikling || unit.Konkurs
                ? LegalStatus.SomeRegistered
                : LegalStatus.NoRegistered,
            Legalform = new LegalForm
            {
                Code = GetLegalBasisEnumValue("NO_" + unit.Organisasjonsform.Kode),
                Type = unit.Organisasjonsform.Beskrivelse,
            },
            Addresses = new Addresses()
        };

        if (unit.Postadresse != null)
        {
            ci.Addresses.PostalAddress = new PostalAddress
            {
                FullAddress = string.Join(';', unit.Postadresse.Adresse)
                              + ';' + unit.Postadresse.Postnummer
                              + ';' + unit.Postadresse.Poststed
                              + ';' + CountryCodesHelper.GetByCode(unit.Postadresse.Landkode)
            };
        }

        if (unit.Forretningsadresse != null)
        {
            ci.Addresses.RegisteredAddress = new RegisteredAddress()
            {
                FullAddress = string.Join(';', unit.Forretningsadresse.Adresse)
                              + ';' + unit.Forretningsadresse.Postnummer
                              + ';' + unit.Forretningsadresse.Poststed
                              + ';' + CountryCodesHelper.GetByCode(unit.Forretningsadresse.Landkode)
            };
        }

        return ci;
    }

    private async Task<CompanyInformation> GetFromFinland(string companyId, string identifier)
    {
        var url = "http://avoindata.prh.fi/bis/v1/" + companyId;
        var results = await MakeRequest<FinUnit>(url);
        var unit = results.Results[0];
        var sortedAdresses = unit.Addresses.OrderByDescending(x => x.RegistrationDate);
        var firstRegisteredAddress = sortedAdresses.FirstOrDefault(x => !x.EndDate.HasValue && x.Type == 1 /* street address */);
        var firstPostalAddress = sortedAdresses.FirstOrDefault(x => !x.EndDate.HasValue && x.Type == 2 /* postal address */);
        var liquidations = unit.Liquidations?.OrderByDescending(x => x.RegistrationDate);
        var liquidation = liquidations?.FirstOrDefault();

        var ci = new CompanyInformation
        {
            Identifier = new Identifier
            {
                Notation = companyId,
                IssuingAuthorityName = "Finnish Patent and Registration Office"
            },
            Name = unit.Name,
            RegistrationDate = unit.RegistrationDate,
            LegalStatus = liquidation == null ? LegalStatus.NoRegistered : LegalStatus.SomeRegistered,
            Legalform = new LegalForm
            {
                Code = GetLegalBasisEnumValue("FI_" + unit.CompanyForm)
            },
            Addresses = new Addresses()
        };

        if (firstPostalAddress != null)
        {
            ci.Addresses.PostalAddress = new PostalAddress()
            {
                FullAddress = (firstPostalAddress.CareOf == null ? "" : (string)firstPostalAddress.CareOf + ';')
                              + firstPostalAddress.Street
                              + ';' + firstPostalAddress.PostCode
                              + ';' + firstPostalAddress.City
                              + ';' + firstPostalAddress.Language
            };
        }

        if (firstRegisteredAddress != null)
        {
            ci.Addresses.RegisteredAddress = new RegisteredAddress()
            {
                FullAddress = (firstRegisteredAddress.CareOf == null ? "" : (string)firstRegisteredAddress.CareOf + ';')
                              + firstRegisteredAddress.Street
                              + ';' + firstRegisteredAddress.PostCode
                              + ';' + firstRegisteredAddress.City
                              + ';' + firstRegisteredAddress.Language
            };
        }



        return ci;
    }

    private Code GetLegalBasisEnumValue(string unitCompanyForm)
    {
        var json = "{\"legalForm\":{\"code\":\"" + unitCompanyForm + "\"}}";
        try
        {
            var ci = JsonConvert.DeserializeObject<CompanyInformation>(json, Converter.Settings)!;
            return ci.Legalform.Code;
        }
        catch (Exception)
        {
            throw new EvidenceSourcePermanentServerException(EvidenceSourceMetadata.ErrorUpstreamError,
                "Unable to recognize legalForm.Code '" + unitCompanyForm + "'");
        }
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
