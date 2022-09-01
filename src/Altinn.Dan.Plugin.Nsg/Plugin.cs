using System;
using System.Collections.Generic;
using System.IO;
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

    public Plugin(IHttpClientFactory httpClientFactory, IEvidenceSourceMetadata evidenceSourceMetadata)
    {
        _evidenceSourceMetadata = evidenceSourceMetadata;
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
        ecb.AddEvidenceValue("default", JsonConvert.SerializeObject(companyInformation), EvidenceSourceMetadata.Source);

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
        // TODO! Underenheter
        var url = "https://data.brreg.no/enhetsregisteret/api/enheter/" + organizationNumber;
        var unit = await MakeRequest<NorUnit>(url);

        var ci = new CompanyInformation
        {
            Identifier = identifier,
            RegisteredOrganization = new RegisteredOrganization
            {
                LegalName = unit.Navn,
                Jurisdiction = "no"
            }
        };

        if (unit.Slettedato != DateTimeOffset.MinValue)
        {
            ci.RegisteredOrganization.DissolutionDate = unit.Slettedato.ToString("yyyy-MM-dd");
            ci.Address = new Address();
        }
        else
        {
            // Only available on non-deleted units
            ci.RegisteredOrganization.FoundingDate = unit.RegistreringsdatoEnhetsregisteret.ToString("yyyy-MM-dd");
            ci.Address = new Address
            {
                Thoroughfare = string.Join("\n", unit.Forretningsadresse.Adresse),
                PostCode = unit.Forretningsadresse.Postnummer,
                PostName = unit.Forretningsadresse.Poststed,
                AdminUnitL1 = unit.Forretningsadresse.Landkode.ToLowerInvariant()
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
        var firstAddress = sortedAdresses.FirstOrDefault(x => !x.EndDate.HasValue);
        var liquidations = unit.Liquidations?.OrderByDescending(x => x.RegistrationDate);
        var liquidation = liquidations?.FirstOrDefault();

        var ci = new CompanyInformation
        {
            Identifier = identifier,
            RegisteredOrganization = new RegisteredOrganization
            {
                LegalName = unit.Name,
                FoundingDate = unit.RegistrationDate.ToString("yyyy-MM-dd"),
                Jurisdiction = "fi",
            }
        };

        if (firstAddress != null)
        {
            ci.Address = new Address
            {
                Thoroughfare = firstAddress.Street,
                PostCode = firstAddress.PostCode.ToString(),
                PostName = firstAddress.City,
                AdminUnitL1 = firstAddress.Country ?? "fi" // always null in API?
            };
        }

        if (liquidation?.RegistrationDate != null)
        {
            ci.RegisteredOrganization.DissolutionDate = liquidation.RegistrationDate.Value.ToString("yyyy-MM-dd");
        }

        return ci;
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
