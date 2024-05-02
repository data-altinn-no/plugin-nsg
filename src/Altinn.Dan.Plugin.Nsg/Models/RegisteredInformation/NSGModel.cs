using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Dan.Plugin.Nsg.Models.RegisteredInformation
{
    public class RegisteredInformationRequest
    {
        [JsonProperty("country", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Country { get; set; }
        [JsonProperty("notation")]
        public string Notation { get; set; }
    }


    public class RegisteredInformationResponse
    {
        [JsonProperty("activity")]
        public List<Activity> Activity { get; set; }

        [JsonProperty("identifier")]
        public Identifier Identifier { get; set; }

        [JsonProperty("legalform")]
        public Legalform LegalForm { get; set; }

        [JsonProperty("legalStatus")]
        public Legalstatus LegalStatus { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("postalAddress")]
        public Postaladdress PostalAddress { get; set; }

        [JsonProperty("registeredAddress")]
        public Registeredaddress RegisteredAddress { get; set; }

        [JsonProperty("registrationDate")]
        public string RegistrationDate { get; set; }

    }

    public class Identifier
    {
        [JsonProperty("issuingAuthorityName")]
        public string IssuingAuthorityName { get; set; }

        [JsonProperty("notation")]
        public string Notation { get; set; }
    }

    public class Legalform
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Legalstatus
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Postaladdress
    {
        [JsonProperty("fullAddress")]
        public string FullAddress { get; set; }
    }

    public class Registeredaddress
    {
        [JsonProperty("fullAddress")]
        public string FullAddress { get; set; }
    }

    public class Activity
    {
        [JsonProperty("code")]
        public string code { get; set; }

        [JsonProperty("inClassification")]
        public string InClassification { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("sequence")]
        public int Sequence { get; set; }
    }

}
