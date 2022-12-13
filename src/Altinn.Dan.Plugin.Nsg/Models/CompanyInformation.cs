namespace Altinn.Dan.Plugin.Nsg.Models
{
    using System;
    using System.Collections.Generic;

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
    public partial class CompanyInformation
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
    public partial class Addresses
    {
        /// <summary>
        /// The postal address is the address at which an agent wants to receive mail or other
        /// similar deliveries. For most companies the registered address is also the default postal
        /// address, but an agent can also declare a specific postal address separately.
        /// </summary>
        [JsonProperty("postalAddress", NullValueHandling = NullValueHandling.Ignore)]
        public PostalAddress PostalAddress { get; set; }

        /// <summary>
        /// The registered address is the address, where the agent or his or her representative is
        /// available for official matters. Most often this is the address of the main office, head
        /// office, registered office or headquarters of the agent.
        /// </summary>
        [JsonProperty("registeredAddress", NullValueHandling = NullValueHandling.Ignore)]
        public RegisteredAddress RegisteredAddress { get; set; }
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
    public partial class PostalAddress
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
    /// The registered address is the address, where the agent or his or her representative is
    /// available for official matters. Most often this is the address of the main office, head
    /// office, registered office or headquarters of the agent.
    ///
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
    public partial class RegisteredAddress
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
    public partial class Identifier
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
    public partial class LegalForm
    {
        /// <summary>
        /// The legal form code of an agent according to a national scheme based on national
        /// legislation. In this model the code is the nationally registered code with a country
        /// prefix. Example: "SE_AB".
        /// </summary>
        [JsonProperty("code")]
        public Code Code { get; set; }

        /// <summary>
        /// The national name of the legal form with a specific code expressed in the field
        /// legalForm.code. Name is in local language. Example "Aktiebolag"
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
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

    /// <summary>
    /// The legal form code of an agent according to a national scheme based on national
    /// legislation. In this model the code is the nationally registered code with a country
    /// prefix. Example: "SE_AB".
    /// </summary>
    public enum Code { FiAhve, FiAhvell, FiAoy, FiAsh, FiAsy, FiAy, FiAyh, FiElsyh, FiEsaa, FiEts, FiEty, FiEuokkt, FiEvl, FiEvlut, FiEyht, FiHyyh, FiKk, FiKonk, FiKoy, FiKp, FiKunt, FiKuntll, FiKuntlll, FiKuntyht, FiKvakyh, FiKvj, FiKvy, FiKy, FiLiy, FiMhy, FiMjuo, FiMohlo, FiMsaa, FiMtyh, FiMuu, FiMuukoy, FiMuve, FiMuyp, FiMyh, FiOk, FiOp, FiOrto, FiOy, FiOyj, FiPk, FiPy, FiSaa, FiSce, FiScp, FiSe, FiSl, FiSp, FiTeka, FiTyh, FiTyka, FiUlko, FiUyk, FiVakk, FiValt, FiValtll, FiVeyht, FiVoj, FiVoy, FiVy, FiYeh, FiYhme, FiYhte, FiYo, NoAafy, NoAdos, NoAnna, NoAns, NoAs, NoAsa, NoBa, NoBbl, NoBedr, NoBo, NoBrl, NoDa, NoEnk, NoEoefg, NoEsek, NoFkf, NoFli, NoFylk, NoGfs, NoIkjp, NoIks, NoKbo, NoKf, NoKirk, NoKomm, NoKs, NoKtrf, NoNuf, NoOpmv, NoOrgl, NoPers, NoPk, NoPre, NoSa, NoSaer, NoSam, NoSe, NoSf, NoSpa, NoStat, NoSti, NoTvam, NoVpfo, SeAb, SeBab, SeBf, SeBfl, SeBrf, SeE, SeEb, SeEeig, SeEgts, SeEk, SeFab, SeFl, SeFof, SeHb, SeI, SeKb, SeKhf, SeMb, SeOfb, SeS, SeSb, SeSce, SeSe, SeSf, SeTsf };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                LegalStatusConverter.Singleton,
                CodeConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
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

        public static readonly LegalStatusConverter Singleton = new LegalStatusConverter();
    }

    internal class CodeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Code) || t == typeof(Code?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "FI_AHVE":
                    return Code.FiAhve;
                case "FI_AHVELL":
                    return Code.FiAhvell;
                case "FI_AOY":
                    return Code.FiAoy;
                case "FI_ASH":
                    return Code.FiAsh;
                case "FI_ASY":
                    return Code.FiAsy;
                case "FI_AY":
                    return Code.FiAy;
                case "FI_AYH":
                    return Code.FiAyh;
                case "FI_ELSYH":
                    return Code.FiElsyh;
                case "FI_ESAA":
                    return Code.FiEsaa;
                case "FI_ETS":
                    return Code.FiEts;
                case "FI_ETY":
                    return Code.FiEty;
                case "FI_EUOKKT":
                    return Code.FiEuokkt;
                case "FI_EVL":
                    return Code.FiEvl;
                case "FI_EVLUT":
                    return Code.FiEvlut;
                case "FI_EYHT":
                    return Code.FiEyht;
                case "FI_HYYH":
                    return Code.FiHyyh;
                case "FI_KK":
                    return Code.FiKk;
                case "FI_KONK":
                    return Code.FiKonk;
                case "FI_KOY":
                    return Code.FiKoy;
                case "FI_KP":
                    return Code.FiKp;
                case "FI_KUNT":
                    return Code.FiKunt;
                case "FI_KUNTLL":
                    return Code.FiKuntll;
                case "FI_KUNTLLL":
                    return Code.FiKuntlll;
                case "FI_KUNTYHT":
                    return Code.FiKuntyht;
                case "FI_KVAKYH":
                    return Code.FiKvakyh;
                case "FI_KVJ":
                    return Code.FiKvj;
                case "FI_KVY":
                    return Code.FiKvy;
                case "FI_KY":
                    return Code.FiKy;
                case "FI_LIY":
                    return Code.FiLiy;
                case "FI_MHY":
                    return Code.FiMhy;
                case "FI_MJUO":
                    return Code.FiMjuo;
                case "FI_MOHLO":
                    return Code.FiMohlo;
                case "FI_MSAA":
                    return Code.FiMsaa;
                case "FI_MTYH":
                    return Code.FiMtyh;
                case "FI_MUU":
                    return Code.FiMuu;
                case "FI_MUUKOY":
                    return Code.FiMuukoy;
                case "FI_MUVE":
                    return Code.FiMuve;
                case "FI_MUYP":
                    return Code.FiMuyp;
                case "FI_MYH":
                    return Code.FiMyh;
                case "FI_OK":
                    return Code.FiOk;
                case "FI_OP":
                    return Code.FiOp;
                case "FI_ORTO":
                    return Code.FiOrto;
                case "FI_OY":
                    return Code.FiOy;
                case "FI_OYJ":
                    return Code.FiOyj;
                case "FI_PK":
                    return Code.FiPk;
                case "FI_PY":
                    return Code.FiPy;
                case "FI_SAA":
                    return Code.FiSaa;
                case "FI_SCE":
                    return Code.FiSce;
                case "FI_SCP":
                    return Code.FiScp;
                case "FI_SE":
                    return Code.FiSe;
                case "FI_SL":
                    return Code.FiSl;
                case "FI_SP":
                    return Code.FiSp;
                case "FI_TEKA":
                    return Code.FiTeka;
                case "FI_TYH":
                    return Code.FiTyh;
                case "FI_TYKA":
                    return Code.FiTyka;
                case "FI_ULKO":
                    return Code.FiUlko;
                case "FI_UYK":
                    return Code.FiUyk;
                case "FI_VAKK":
                    return Code.FiVakk;
                case "FI_VALT":
                    return Code.FiValt;
                case "FI_VALTLL":
                    return Code.FiValtll;
                case "FI_VEYHT":
                    return Code.FiVeyht;
                case "FI_VOJ":
                    return Code.FiVoj;
                case "FI_VOY":
                    return Code.FiVoy;
                case "FI_VY":
                    return Code.FiVy;
                case "FI_YEH":
                    return Code.FiYeh;
                case "FI_YHME":
                    return Code.FiYhme;
                case "FI_YHTE":
                    return Code.FiYhte;
                case "FI_YO":
                    return Code.FiYo;
                case "NO_AAFY":
                    return Code.NoAafy;
                case "NO_ADOS":
                    return Code.NoAdos;
                case "NO_ANNA":
                    return Code.NoAnna;
                case "NO_ANS":
                    return Code.NoAns;
                case "NO_AS":
                    return Code.NoAs;
                case "NO_ASA":
                    return Code.NoAsa;
                case "NO_BA":
                    return Code.NoBa;
                case "NO_BBL":
                    return Code.NoBbl;
                case "NO_BEDR":
                    return Code.NoBedr;
                case "NO_BO":
                    return Code.NoBo;
                case "NO_BRL":
                    return Code.NoBrl;
                case "NO_DA":
                    return Code.NoDa;
                case "NO_ENK":
                    return Code.NoEnk;
                case "NO_EOEFG":
                    return Code.NoEoefg;
                case "NO_ESEK":
                    return Code.NoEsek;
                case "NO_FKF":
                    return Code.NoFkf;
                case "NO_FLI":
                    return Code.NoFli;
                case "NO_FYLK":
                    return Code.NoFylk;
                case "NO_GFS":
                    return Code.NoGfs;
                case "NO_IKJP":
                    return Code.NoIkjp;
                case "NO_IKS":
                    return Code.NoIks;
                case "NO_KBO":
                    return Code.NoKbo;
                case "NO_KF":
                    return Code.NoKf;
                case "NO_KIRK":
                    return Code.NoKirk;
                case "NO_KOMM":
                    return Code.NoKomm;
                case "NO_KS":
                    return Code.NoKs;
                case "NO_KTRF":
                    return Code.NoKtrf;
                case "NO_NUF":
                    return Code.NoNuf;
                case "NO_OPMV":
                    return Code.NoOpmv;
                case "NO_ORGL":
                    return Code.NoOrgl;
                case "NO_PERS":
                    return Code.NoPers;
                case "NO_PK":
                    return Code.NoPk;
                case "NO_PRE":
                    return Code.NoPre;
                case "NO_SA":
                    return Code.NoSa;
                case "NO_SAER":
                    return Code.NoSaer;
                case "NO_SAM":
                    return Code.NoSam;
                case "NO_SE":
                    return Code.NoSe;
                case "NO_SF":
                    return Code.NoSf;
                case "NO_SPA":
                    return Code.NoSpa;
                case "NO_STAT":
                    return Code.NoStat;
                case "NO_STI":
                    return Code.NoSti;
                case "NO_TVAM":
                    return Code.NoTvam;
                case "NO_VPFO":
                    return Code.NoVpfo;
                case "SE_AB":
                    return Code.SeAb;
                case "SE_BAB":
                    return Code.SeBab;
                case "SE_BF":
                    return Code.SeBf;
                case "SE_BFL":
                    return Code.SeBfl;
                case "SE_BRF":
                    return Code.SeBrf;
                case "SE_E":
                    return Code.SeE;
                case "SE_EB":
                    return Code.SeEb;
                case "SE_EEIG":
                    return Code.SeEeig;
                case "SE_EGTS":
                    return Code.SeEgts;
                case "SE_EK":
                    return Code.SeEk;
                case "SE_FAB":
                    return Code.SeFab;
                case "SE_FL":
                    return Code.SeFl;
                case "SE_FOF":
                    return Code.SeFof;
                case "SE_HB":
                    return Code.SeHb;
                case "SE_I":
                    return Code.SeI;
                case "SE_KB":
                    return Code.SeKb;
                case "SE_KHF":
                    return Code.SeKhf;
                case "SE_MB":
                    return Code.SeMb;
                case "SE_OFB":
                    return Code.SeOfb;
                case "SE_S":
                    return Code.SeS;
                case "SE_SB":
                    return Code.SeSb;
                case "SE_SCE":
                    return Code.SeSce;
                case "SE_SE":
                    return Code.SeSe;
                case "SE_SF":
                    return Code.SeSf;
                case "SE_TSF":
                    return Code.SeTsf;
            }
            throw new Exception("Cannot unmarshal type Code");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Code)untypedValue;
            switch (value)
            {
                case Code.FiAhve:
                    serializer.Serialize(writer, "FI_AHVE");
                    return;
                case Code.FiAhvell:
                    serializer.Serialize(writer, "FI_AHVELL");
                    return;
                case Code.FiAoy:
                    serializer.Serialize(writer, "FI_AOY");
                    return;
                case Code.FiAsh:
                    serializer.Serialize(writer, "FI_ASH");
                    return;
                case Code.FiAsy:
                    serializer.Serialize(writer, "FI_ASY");
                    return;
                case Code.FiAy:
                    serializer.Serialize(writer, "FI_AY");
                    return;
                case Code.FiAyh:
                    serializer.Serialize(writer, "FI_AYH");
                    return;
                case Code.FiElsyh:
                    serializer.Serialize(writer, "FI_ELSYH");
                    return;
                case Code.FiEsaa:
                    serializer.Serialize(writer, "FI_ESAA");
                    return;
                case Code.FiEts:
                    serializer.Serialize(writer, "FI_ETS");
                    return;
                case Code.FiEty:
                    serializer.Serialize(writer, "FI_ETY");
                    return;
                case Code.FiEuokkt:
                    serializer.Serialize(writer, "FI_EUOKKT");
                    return;
                case Code.FiEvl:
                    serializer.Serialize(writer, "FI_EVL");
                    return;
                case Code.FiEvlut:
                    serializer.Serialize(writer, "FI_EVLUT");
                    return;
                case Code.FiEyht:
                    serializer.Serialize(writer, "FI_EYHT");
                    return;
                case Code.FiHyyh:
                    serializer.Serialize(writer, "FI_HYYH");
                    return;
                case Code.FiKk:
                    serializer.Serialize(writer, "FI_KK");
                    return;
                case Code.FiKonk:
                    serializer.Serialize(writer, "FI_KONK");
                    return;
                case Code.FiKoy:
                    serializer.Serialize(writer, "FI_KOY");
                    return;
                case Code.FiKp:
                    serializer.Serialize(writer, "FI_KP");
                    return;
                case Code.FiKunt:
                    serializer.Serialize(writer, "FI_KUNT");
                    return;
                case Code.FiKuntll:
                    serializer.Serialize(writer, "FI_KUNTLL");
                    return;
                case Code.FiKuntlll:
                    serializer.Serialize(writer, "FI_KUNTLLL");
                    return;
                case Code.FiKuntyht:
                    serializer.Serialize(writer, "FI_KUNTYHT");
                    return;
                case Code.FiKvakyh:
                    serializer.Serialize(writer, "FI_KVAKYH");
                    return;
                case Code.FiKvj:
                    serializer.Serialize(writer, "FI_KVJ");
                    return;
                case Code.FiKvy:
                    serializer.Serialize(writer, "FI_KVY");
                    return;
                case Code.FiKy:
                    serializer.Serialize(writer, "FI_KY");
                    return;
                case Code.FiLiy:
                    serializer.Serialize(writer, "FI_LIY");
                    return;
                case Code.FiMhy:
                    serializer.Serialize(writer, "FI_MHY");
                    return;
                case Code.FiMjuo:
                    serializer.Serialize(writer, "FI_MJUO");
                    return;
                case Code.FiMohlo:
                    serializer.Serialize(writer, "FI_MOHLO");
                    return;
                case Code.FiMsaa:
                    serializer.Serialize(writer, "FI_MSAA");
                    return;
                case Code.FiMtyh:
                    serializer.Serialize(writer, "FI_MTYH");
                    return;
                case Code.FiMuu:
                    serializer.Serialize(writer, "FI_MUU");
                    return;
                case Code.FiMuukoy:
                    serializer.Serialize(writer, "FI_MUUKOY");
                    return;
                case Code.FiMuve:
                    serializer.Serialize(writer, "FI_MUVE");
                    return;
                case Code.FiMuyp:
                    serializer.Serialize(writer, "FI_MUYP");
                    return;
                case Code.FiMyh:
                    serializer.Serialize(writer, "FI_MYH");
                    return;
                case Code.FiOk:
                    serializer.Serialize(writer, "FI_OK");
                    return;
                case Code.FiOp:
                    serializer.Serialize(writer, "FI_OP");
                    return;
                case Code.FiOrto:
                    serializer.Serialize(writer, "FI_ORTO");
                    return;
                case Code.FiOy:
                    serializer.Serialize(writer, "FI_OY");
                    return;
                case Code.FiOyj:
                    serializer.Serialize(writer, "FI_OYJ");
                    return;
                case Code.FiPk:
                    serializer.Serialize(writer, "FI_PK");
                    return;
                case Code.FiPy:
                    serializer.Serialize(writer, "FI_PY");
                    return;
                case Code.FiSaa:
                    serializer.Serialize(writer, "FI_SAA");
                    return;
                case Code.FiSce:
                    serializer.Serialize(writer, "FI_SCE");
                    return;
                case Code.FiScp:
                    serializer.Serialize(writer, "FI_SCP");
                    return;
                case Code.FiSe:
                    serializer.Serialize(writer, "FI_SE");
                    return;
                case Code.FiSl:
                    serializer.Serialize(writer, "FI_SL");
                    return;
                case Code.FiSp:
                    serializer.Serialize(writer, "FI_SP");
                    return;
                case Code.FiTeka:
                    serializer.Serialize(writer, "FI_TEKA");
                    return;
                case Code.FiTyh:
                    serializer.Serialize(writer, "FI_TYH");
                    return;
                case Code.FiTyka:
                    serializer.Serialize(writer, "FI_TYKA");
                    return;
                case Code.FiUlko:
                    serializer.Serialize(writer, "FI_ULKO");
                    return;
                case Code.FiUyk:
                    serializer.Serialize(writer, "FI_UYK");
                    return;
                case Code.FiVakk:
                    serializer.Serialize(writer, "FI_VAKK");
                    return;
                case Code.FiValt:
                    serializer.Serialize(writer, "FI_VALT");
                    return;
                case Code.FiValtll:
                    serializer.Serialize(writer, "FI_VALTLL");
                    return;
                case Code.FiVeyht:
                    serializer.Serialize(writer, "FI_VEYHT");
                    return;
                case Code.FiVoj:
                    serializer.Serialize(writer, "FI_VOJ");
                    return;
                case Code.FiVoy:
                    serializer.Serialize(writer, "FI_VOY");
                    return;
                case Code.FiVy:
                    serializer.Serialize(writer, "FI_VY");
                    return;
                case Code.FiYeh:
                    serializer.Serialize(writer, "FI_YEH");
                    return;
                case Code.FiYhme:
                    serializer.Serialize(writer, "FI_YHME");
                    return;
                case Code.FiYhte:
                    serializer.Serialize(writer, "FI_YHTE");
                    return;
                case Code.FiYo:
                    serializer.Serialize(writer, "FI_YO");
                    return;
                case Code.NoAafy:
                    serializer.Serialize(writer, "NO_AAFY");
                    return;
                case Code.NoAdos:
                    serializer.Serialize(writer, "NO_ADOS");
                    return;
                case Code.NoAnna:
                    serializer.Serialize(writer, "NO_ANNA");
                    return;
                case Code.NoAns:
                    serializer.Serialize(writer, "NO_ANS");
                    return;
                case Code.NoAs:
                    serializer.Serialize(writer, "NO_AS");
                    return;
                case Code.NoAsa:
                    serializer.Serialize(writer, "NO_ASA");
                    return;
                case Code.NoBa:
                    serializer.Serialize(writer, "NO_BA");
                    return;
                case Code.NoBbl:
                    serializer.Serialize(writer, "NO_BBL");
                    return;
                case Code.NoBedr:
                    serializer.Serialize(writer, "NO_BEDR");
                    return;
                case Code.NoBo:
                    serializer.Serialize(writer, "NO_BO");
                    return;
                case Code.NoBrl:
                    serializer.Serialize(writer, "NO_BRL");
                    return;
                case Code.NoDa:
                    serializer.Serialize(writer, "NO_DA");
                    return;
                case Code.NoEnk:
                    serializer.Serialize(writer, "NO_ENK");
                    return;
                case Code.NoEoefg:
                    serializer.Serialize(writer, "NO_EOEFG");
                    return;
                case Code.NoEsek:
                    serializer.Serialize(writer, "NO_ESEK");
                    return;
                case Code.NoFkf:
                    serializer.Serialize(writer, "NO_FKF");
                    return;
                case Code.NoFli:
                    serializer.Serialize(writer, "NO_FLI");
                    return;
                case Code.NoFylk:
                    serializer.Serialize(writer, "NO_FYLK");
                    return;
                case Code.NoGfs:
                    serializer.Serialize(writer, "NO_GFS");
                    return;
                case Code.NoIkjp:
                    serializer.Serialize(writer, "NO_IKJP");
                    return;
                case Code.NoIks:
                    serializer.Serialize(writer, "NO_IKS");
                    return;
                case Code.NoKbo:
                    serializer.Serialize(writer, "NO_KBO");
                    return;
                case Code.NoKf:
                    serializer.Serialize(writer, "NO_KF");
                    return;
                case Code.NoKirk:
                    serializer.Serialize(writer, "NO_KIRK");
                    return;
                case Code.NoKomm:
                    serializer.Serialize(writer, "NO_KOMM");
                    return;
                case Code.NoKs:
                    serializer.Serialize(writer, "NO_KS");
                    return;
                case Code.NoKtrf:
                    serializer.Serialize(writer, "NO_KTRF");
                    return;
                case Code.NoNuf:
                    serializer.Serialize(writer, "NO_NUF");
                    return;
                case Code.NoOpmv:
                    serializer.Serialize(writer, "NO_OPMV");
                    return;
                case Code.NoOrgl:
                    serializer.Serialize(writer, "NO_ORGL");
                    return;
                case Code.NoPers:
                    serializer.Serialize(writer, "NO_PERS");
                    return;
                case Code.NoPk:
                    serializer.Serialize(writer, "NO_PK");
                    return;
                case Code.NoPre:
                    serializer.Serialize(writer, "NO_PRE");
                    return;
                case Code.NoSa:
                    serializer.Serialize(writer, "NO_SA");
                    return;
                case Code.NoSaer:
                    serializer.Serialize(writer, "NO_SAER");
                    return;
                case Code.NoSam:
                    serializer.Serialize(writer, "NO_SAM");
                    return;
                case Code.NoSe:
                    serializer.Serialize(writer, "NO_SE");
                    return;
                case Code.NoSf:
                    serializer.Serialize(writer, "NO_SF");
                    return;
                case Code.NoSpa:
                    serializer.Serialize(writer, "NO_SPA");
                    return;
                case Code.NoStat:
                    serializer.Serialize(writer, "NO_STAT");
                    return;
                case Code.NoSti:
                    serializer.Serialize(writer, "NO_STI");
                    return;
                case Code.NoTvam:
                    serializer.Serialize(writer, "NO_TVAM");
                    return;
                case Code.NoVpfo:
                    serializer.Serialize(writer, "NO_VPFO");
                    return;
                case Code.SeAb:
                    serializer.Serialize(writer, "SE_AB");
                    return;
                case Code.SeBab:
                    serializer.Serialize(writer, "SE_BAB");
                    return;
                case Code.SeBf:
                    serializer.Serialize(writer, "SE_BF");
                    return;
                case Code.SeBfl:
                    serializer.Serialize(writer, "SE_BFL");
                    return;
                case Code.SeBrf:
                    serializer.Serialize(writer, "SE_BRF");
                    return;
                case Code.SeE:
                    serializer.Serialize(writer, "SE_E");
                    return;
                case Code.SeEb:
                    serializer.Serialize(writer, "SE_EB");
                    return;
                case Code.SeEeig:
                    serializer.Serialize(writer, "SE_EEIG");
                    return;
                case Code.SeEgts:
                    serializer.Serialize(writer, "SE_EGTS");
                    return;
                case Code.SeEk:
                    serializer.Serialize(writer, "SE_EK");
                    return;
                case Code.SeFab:
                    serializer.Serialize(writer, "SE_FAB");
                    return;
                case Code.SeFl:
                    serializer.Serialize(writer, "SE_FL");
                    return;
                case Code.SeFof:
                    serializer.Serialize(writer, "SE_FOF");
                    return;
                case Code.SeHb:
                    serializer.Serialize(writer, "SE_HB");
                    return;
                case Code.SeI:
                    serializer.Serialize(writer, "SE_I");
                    return;
                case Code.SeKb:
                    serializer.Serialize(writer, "SE_KB");
                    return;
                case Code.SeKhf:
                    serializer.Serialize(writer, "SE_KHF");
                    return;
                case Code.SeMb:
                    serializer.Serialize(writer, "SE_MB");
                    return;
                case Code.SeOfb:
                    serializer.Serialize(writer, "SE_OFB");
                    return;
                case Code.SeS:
                    serializer.Serialize(writer, "SE_S");
                    return;
                case Code.SeSb:
                    serializer.Serialize(writer, "SE_SB");
                    return;
                case Code.SeSce:
                    serializer.Serialize(writer, "SE_SCE");
                    return;
                case Code.SeSe:
                    serializer.Serialize(writer, "SE_SE");
                    return;
                case Code.SeSf:
                    serializer.Serialize(writer, "SE_SF");
                    return;
                case Code.SeTsf:
                    serializer.Serialize(writer, "SE_TSF");
                    return;
            }
            throw new Exception("Cannot marshal type Code");
        }

        public static readonly CodeConverter Singleton = new CodeConverter();
    }
}
