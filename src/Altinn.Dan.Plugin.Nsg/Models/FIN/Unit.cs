using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Altinn.Dan.Plugin.Nsg.Models.FIN
{
    public partial class Unit
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("totalResults")]
        public long TotalResults { get; set; }

        [JsonProperty("resultsFrom")]
        public long ResultsFrom { get; set; }

        [JsonProperty("previousResultsUri")]
        public object PreviousResultsUri { get; set; }

        [JsonProperty("nextResultsUri")]
        public object NextResultsUri { get; set; }

        [JsonProperty("exceptionNoticeUri")]
        public object ExceptionNoticeUri { get; set; }

        [JsonProperty("results")]
        public Result[] Results { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("businessId")]
        public string BusinessId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("registrationDate")]
        public DateTimeOffset RegistrationDate { get; set; }

        [JsonProperty("companyForm")]
        public string CompanyForm { get; set; }

        [JsonProperty("detailsUri")]
        public object DetailsUri { get; set; }

        [JsonProperty("liquidations")]
        public Liquidation[] Liquidations { get; set; }

        [JsonProperty("names")]
        public AuxiliaryName[] Names { get; set; }

        [JsonProperty("auxiliaryNames")]
        public AuxiliaryName[] AuxiliaryNames { get; set; }

        [JsonProperty("addresses")]
        public Address[] Addresses { get; set; }

        [JsonProperty("companyForms")]
        public AuxiliaryName[] CompanyForms { get; set; }

        [JsonProperty("businessLines")]
        public AuxiliaryName[] BusinessLines { get; set; }

        [JsonProperty("languages")]
        public AuxiliaryName[] Languages { get; set; }

        [JsonProperty("registedOffices")]
        public AuxiliaryName[] RegistedOffices { get; set; }

        [JsonProperty("contactDetails")]
        public AuxiliaryName[] ContactDetails { get; set; }

        [JsonProperty("registeredEntries")]
        public RegisteredEntry[] RegisteredEntries { get; set; }

        [JsonProperty("businessIdChanges")]
        public BusinessIdChange[] BusinessIdChanges { get; set; }
    }

    public partial class Address
    {
        [JsonProperty("careOf")]
        public object CareOf { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("postCode")]
        public long PostCode { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("registrationDate")]
        public DateTimeOffset RegistrationDate { get; set; }

        [JsonProperty("endDate")]
        public DateTimeOffset? EndDate { get; set; }

        [JsonProperty("language")]
        public Language Language { get; set; }

        [JsonProperty("source")]
        public long Source { get; set; }
    }

    public partial class AuxiliaryName
    {
        [JsonProperty("order", NullValueHandling = NullValueHandling.Ignore)]
        public long? Order { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("registrationDate")]
        public DateTimeOffset RegistrationDate { get; set; }

        [JsonProperty("endDate")]
        public DateTimeOffset? EndDate { get; set; }

        [JsonProperty("source")]
        public long Source { get; set; }

        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public long? Code { get; set; }

        [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
        public Language? Language { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    public partial class BusinessIdChange
    {
        [JsonProperty("changeDate")]
        public DateTimeOffset ChangeDate { get; set; }

        [JsonProperty("change")]
        public string Change { get; set; }

        [JsonProperty("oldBusinessId")]
        public string OldBusinessId { get; set; }

        [JsonProperty("newBusinessId")]
        public string NewBusinessId { get; set; }

        [JsonProperty("language")]
        public object Language { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("source")]
        public long Source { get; set; }
    }

    public partial class RegisteredEntry
    {
        [JsonProperty("authority")]
        public long Authority { get; set; }

        [JsonProperty("register")]
        public long Register { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("registrationDate")]
        public DateTimeOffset RegistrationDate { get; set; }

        [JsonProperty("endDate")]
        public DateTimeOffset? EndDate { get; set; }

        [JsonProperty("statusDate")]
        public DateTimeOffset StatusDate { get; set; }

        [JsonProperty("language")]
        public Language Language { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public partial class Liquidation
    {
        [JsonProperty("registrationDate")]
        public DateTimeOffset? RegistrationDate { get; set; }
    }

    public enum Language { En, Fi, Se };
}
