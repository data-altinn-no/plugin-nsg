using Newtonsoft.Json;

namespace Altinn.Dan.Plugin.Nsg.Models
{
    public partial class CompanyInformation
    {
        [JsonProperty("Identifier")]
        public string Identifier { get; set; }

        [JsonProperty("RegisteredOrganization")]
        public RegisteredOrganization RegisteredOrganization { get; set; }

        [JsonProperty("Address")]
        public Address Address { get; set; }
    }

    public partial class Address
    {
        [JsonProperty("addressArea")]
        public string AddressArea { get; set; }

        [JsonProperty("adminUnitL1")]
        public string AdminUnitL1 { get; set; }

        [JsonProperty("adminUnitL2", NullValueHandling = NullValueHandling.Ignore)]
        public string AdminUnitL2 { get; set; }

        [JsonProperty("locatorDesignator")]
        public string LocatorDesignator { get; set; }

        [JsonProperty("postCode")]
        public string PostCode { get; set; }

        [JsonProperty("postName")]
        public string PostName { get; set; }

        [JsonProperty("thoroughfare")]
        public string Thoroughfare { get; set; }
    }

    public partial class RegisteredOrganization
    {
        [JsonProperty("dissolutionDate", NullValueHandling = NullValueHandling.Ignore)]
        public string DissolutionDate { get; set; }

        [JsonProperty("foundingDate")]
        public string FoundingDate { get; set; }

        [JsonProperty("jurisdiction", NullValueHandling = NullValueHandling.Ignore)]
        public string Jurisdiction { get; set; }

        [JsonProperty("legalName")]
        public string LegalName { get; set; }
    }
    
}
