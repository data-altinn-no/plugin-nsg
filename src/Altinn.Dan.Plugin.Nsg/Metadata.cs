using System.Collections.Generic;
using Nadobe.Common.Interfaces;
using Nadobe.Common.Models;
using Nadobe.Common.Models.Enums;

namespace Altinn.Dan.Plugin.Nsg
{
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
                            JsonSchemaDefintion = "{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"type\":\"object\",\"description\":\"Loosely based on STIRData business data model (https://stirdata.github.io/data-specification/), which is based on euBusinessGraph Semantic Data Model (https://docs.google.com/document/d/1dhMOTlIOC6dOK_jksJRX0CB-GIRoiYY6fWtCnZArUhU/edit)\",\"properties\":{\"Identifier\":{\"type\":\"string\",\"description\":\"ISO/IEC 6523 identifier\"},\"RegisteredOrganization\":{\"type\":\"object\",\"properties\":{\"legalName\":{\"type\":\"string\"},\"foundingDate\":{\"type\":\"string\",\"description\":\"ISO 8601 date\"},\"dissolutionDate\":{\"type\":\"string\",\"description\":\"ISO 8601 date\"},\"jurisdiction\":{\"type\":\"string\",\"description\":\"Three letter ISO 3166 (https://op.europa.eu/en/web/eu-vocabularies/dataset/-/resource?uri=http://publications.europa.eu/resource/dataset/country)\"}},\"required\":[\"legalName\",\"foundingDate\"]},\"Address\":{\"type\":\"object\",\"properties\":{\"addressArea\":{\"type\":\"string\",\"description\":\"Part of a city, village or neighborhood.\"},\"postCode\":{\"type\":\"string\",\"description\":\"Postal code of the address.\"},\"thoroughfare\":{\"type\":\"string\",\"description\":\"Street name and optionally number.\"},\"locatorDesignator\":{\"type\":\"string\",\"description\":\"Street number and/or building name.\"},\"postName\":{\"type\":\"string\",\"description\":\"Locality/City/Settlement of the address, free text.\"},\"adminUnitL1\":{\"type\":\"string\",\"description\":\"Country of the address\"},\"adminUnitL2\":{\"type\":\"string\",\"description\":\"NUTS1 (Macroregion) of the address.\"}},\"required\":[\"thoroughfare\",\"postName\",\"postCode\",\"adminUnitL1\"]}},\"required\":[\"Identifier\",\"RegisteredOrganization\",\"Address\"]}"
                        }
                    }
                }
            };
        }
    }
}
