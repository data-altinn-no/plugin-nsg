using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dan.Common;
using Dan.Common.Enums;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Altinn.Dan.Plugin.Nsg;

public class EvidenceSourceMetadata : IEvidenceSourceMetadata
{
    public const string Source = "Nsg";

    public const int ErrorInvalidInput = 1;

    public const int ErrorOrganizationNotFound = 2;

    public const int ErrorUpstreamError = 3;

    public List<EvidenceCode> GetEvidenceCodes()
    {
        return new List<EvidenceCode>()
        {
            new()
            {
                EvidenceCodeName = "NsgCompanyBasicInformation",
                EvidenceSource = Source,
                ServiceContext = "Nordic Smart Government",
                IsPublic = true,
                Values = new List<EvidenceValue>()
                {
                    new()
                    {
                        EvidenceValueName = "default",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = GetSchemaDef()
                    }
                }
            }
        };
    }

    [Function(Constants.EvidenceSourceMetadataFunctionName)]
    public async Task<HttpResponseData> Metadata(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
        FunctionContext context)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(GetEvidenceCodes());
        return response;
    }


    private string GetSchemaDef()
    {
        var def = File.ReadAllText("Models/schema.json");
        return Regex.Replace(def, " {2,}|\r\n", "");
    }
}
