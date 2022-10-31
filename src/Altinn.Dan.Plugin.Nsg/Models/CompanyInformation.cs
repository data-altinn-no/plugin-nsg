namespace Altinn.Dan.Plugin.Nsg.Models;

using System;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Exchanging information about the broader concept of Agents instead of Companies or
/// Registered Organizations or Persons acting as Sole Traders.
///
/// The Agent class represents both registered organizations ("companies") and persons who
/// are doing business without being registered organizations, usually as sole traders (sole
/// proprietors).
/// </summary>
public class CompanyInformation
{
    /// <summary>
    /// The addresses registered on the company.
    /// </summary>
    [JsonProperty("addresses", NullValueHandling = NullValueHandling.Ignore)]
    public Addresses Addresses { get; set; }

    /// <summary>
    /// Identifier identifies an Agent.
    /// </summary>
    [JsonProperty("identifier")]
    public Identifier Identifier { get; set; }

    /// <summary>
    /// The legal form of the agent.
    /// </summary>
    [JsonProperty("legalform")]
    public LegalForm Legalform { get; set; }

    /// <summary>
    /// The nationally registered legal status of the agent. In the context of this model, the
    /// national values for legal status are mapped to the codes: * NO_REGISTERED - no
    /// extraordinary circumstances registered. The business register does not have anything
    /// registered but note that this doesn't say anything about for example financial status of
    /// the agent at this point in time. * SOME_REGISTERED - some extraordinary circumstances
    /// registered. The business register have registered information on for example bankruptcy.
    /// To know the exact nationally registered legal status you need to check that national
    /// register.
    /// </summary>
    [JsonProperty("legalStatus")]
    public LegalStatus LegalStatus { get; set; }

    /// <summary>
    /// The name of an agent, which can be a legal name for a registered organization or a full
    /// name for a person doing business as a sole trader.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// The date when a national registration authority has registered the agent in the relevant
    /// registry. Response in format YYYY-MM-DD.
    /// </summary>
    [JsonProperty("registrationDate")]
    [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
    public DateTimeOffset RegistrationDate { get; set; }
}

/// <summary>
/// The addresses registered on the company.
///
/// Addresses can be of different types, in this API we use both Registered Address and
/// Postal Address. For both addresses, we use the attribute "Full Address".
/// </summary>
public class Addresses
{
    /// <summary>
    /// The postal address is the address at which an agent wants to receive mail or other
    /// similar deliveries. For most companies the registered address is also the default postal
    /// address, but an agent can also declare a specific postal address separately.
    /// </summary>
    [JsonProperty("postalAddress", NullValueHandling = NullValueHandling.Ignore)]
    public PostalAddress PostalAddress { get; set; }
}

/// <summary>
/// The postal address is the address at which an agent wants to receive mail or other
/// similar deliveries. For most companies the registered address is also the default postal
/// address, but an agent can also declare a specific postal address separately.
///
/// An address representation as conceptually defined by the INSPIRE Address Representation
/// data type: Representation of an address spatial object for use in external application
/// schemas that need to include the basic, address information in a readable way.
///
/// The representation of Addresses varies widely from one country's postal system to
/// another. Even within countries, there are almost always examples of Addresses that do not
/// conform to the stated national standard. However, ISO 19160-1 provides a method through
/// which different Addresses can be converted from one conceptual model to another.
///
/// In this first version of the API we decided to only use - full address (the complete
/// address as a formatted string)
/// </summary>
public class PostalAddress
{
    /// <summary>
    /// The complete address written as a string. Use of this property is recommended as it will
    /// not suffer any misunderstandings that might arise through the breaking up of an address
    /// into its component parts. This property is analogous to vCards label property but with
    /// two important differences: (1) formatting is not assumed so that, unlike vCard label, it
    /// may not be suitable to print this on an address label, (2) vCards label property has a
    /// domain of vCard Address; the fullAddress property has no such restriction. The delimiter
    /// between address lines is semicolon ";". Example: 'Testroad 11; 852 94 Sundsvall; Sweden'
    /// </summary>
    [JsonProperty("fullAddress")]
    public string FullAddress { get; set; }
}

/// <summary>
/// Identifier identifies an Agent.
///
/// Identifier is a structured reference that identifies an entity or in this model an agent.
/// </summary>
public class Identifier
{
    /// <summary>
    /// The name of the authority that registered the agent.
    /// </summary>
    [JsonProperty("issuingAuthorityName")]
    public string IssuingAuthorityName { get; set; }

    /// <summary>
    /// The attribute notation refers to a national organization number like "5522334567" for a
    /// Swedish registered agent. In case of "-" is included in the identifier like
    /// "552233-4567", that is not included in the response.
    /// </summary>
    [JsonProperty("notation")]
    public string Notation { get; set; }
}

/// <summary>
/// The legal form of the agent.
///
/// Legal form of the agent.
/// </summary>
public class LegalForm
{
    /// <summary>
    /// The legal form code of an agent according to a national scheme based on national
    /// legislation. In this model the code is the nationally registered code with a country
    /// prefix. Example: "SE_AB".
    /// </summary>
    [JsonProperty("code")]
    public string Code { get; set; }
}

/// <summary>
/// The nationally registered legal status of the agent. In the context of this model, the
/// national values for legal status are mapped to the codes: * NO_REGISTERED - no
/// extraordinary circumstances registered. The business register does not have anything
/// registered but note that this doesn't say anything about for example financial status of
/// the agent at this point in time. * SOME_REGISTERED - some extraordinary circumstances
/// registered. The business register have registered information on for example bankruptcy.
/// To know the exact nationally registered legal status you need to check that national
/// register.
/// </summary>
public enum LegalStatus { NoRegistered, SomeRegistered };

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
        {
            LegalStatusConverter.Instance,
            new IsoDateTimeConverter
            {
                DateTimeStyles = DateTimeStyles.AssumeUniversal
            }
        },
    };
}

internal class DateFormatConverter : IsoDateTimeConverter
{
    public DateFormatConverter(string format)
    {
        DateTimeFormat = format;
    }
}

internal class LegalStatusConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(LegalStatus) || t == typeof(LegalStatus?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        switch (value)
        {
            case "NO_REGISTERED":
                return LegalStatus.NoRegistered;
            case "SOME_REGISTERED":
                return LegalStatus.SomeRegistered;
        }
        throw new Exception("Cannot unmarshal type LegalStatus");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (LegalStatus)untypedValue;
        switch (value)
        {
            case LegalStatus.NoRegistered:
                serializer.Serialize(writer, "NO_REGISTERED");
                return;
            case LegalStatus.SomeRegistered:
                serializer.Serialize(writer, "SOME_REGISTERED");
                return;
        }
        throw new Exception("Cannot marshal type LegalStatus");
    }

    public static readonly LegalStatusConverter Instance = new LegalStatusConverter();
}
