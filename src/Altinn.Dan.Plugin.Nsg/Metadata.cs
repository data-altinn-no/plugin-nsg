using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Altinn.Dan.Plugin.Nsg.Models;
using Dan.Common;
using Dan.Common.Enums;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                        JsonSchemaDefintion = GetCompanyInformationSchema()
                    }
                }
            }
        };
    }

    // CompanyInformation.RegistrationDate is a DateTimeOffset serialized as a plain yyyy-MM-dd date
    // (see the DateFormatConverter on the property in CompanyInformation.cs), but reflection-based
    // schema generation from the CLR type defaults to "format": "date-time" for DateTimeOffset.
    // Override it here so the published schema matches the actual wire format.
    private static string GetCompanyInformationSchema()
    {
        var schema = JObject.Parse(EvidenceValue.SchemaFromObject<CompanyInformation>(Formatting.Indented));

        if (schema.SelectToken("properties.registrationDate") is JObject registrationDate)
        {
            registrationDate["format"] = "date";
        }

        return schema.ToString(Formatting.Indented);
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
}
