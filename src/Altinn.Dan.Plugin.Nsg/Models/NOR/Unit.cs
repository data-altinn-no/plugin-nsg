using System;
using Newtonsoft.Json;

namespace Altinn.Dan.Plugin.Nsg.Models.NOR
{
    public class Unit
    {
        [JsonProperty("organisasjonsnummer")]
       // [JsonConverter(typeof(ParseStringConverter))]
        public long Organisasjonsnummer { get; set; }

        [JsonProperty("overordnetEnhet")]
       // [JsonConverter(typeof(ParseStringConverter))]
        public long OverordnetEnhet { get; set; }

        [JsonProperty("navn")]
        public string Navn { get; set; }

        [JsonProperty("organisasjonsform")]
        public Organisasjonsform Organisasjonsform { get; set; }

        [JsonProperty("registreringsdatoEnhetsregisteret")]
        public DateTimeOffset RegistreringsdatoEnhetsregisteret { get; set; }

        [JsonProperty("registrertIMvaregisteret")]
        public bool RegistrertIMvaregisteret { get; set; }

        [JsonProperty("naeringskode1")]
        public InstitusjonellSektorkode Naeringskode1 { get; set; }

        [JsonProperty("naeringskode2")]
        public InstitusjonellSektorkode Naeringskode2 { get; set; }

        [JsonProperty("naeringskode3")]
        public InstitusjonellSektorkode Naeringskode3 { get; set; }

        [JsonProperty("antallAnsatte")]
        public long AntallAnsatte { get; set; }

        [JsonProperty("forretningsadresse")]
        public EnhetAdresse Forretningsadresse { get; set; }

        [JsonProperty("institusjonellSektorkode")]
        public InstitusjonellSektorkode InstitusjonellSektorkode { get; set; }

        [JsonProperty("registrertIForetaksregisteret")]
        public bool RegistrertIForetaksregisteret { get; set; }

        [JsonProperty("registrertIStiftelsesregisteret")]
        public bool RegistrertIStiftelsesregisteret { get; set; }

        [JsonProperty("registrertIFrivillighetsregisteret")]
        public bool RegistrertIFrivillighetsregisteret { get; set; }

        [JsonProperty("konkurs")]
        public bool Konkurs { get; set; }

        [JsonProperty("underAvvikling")]
        public bool UnderAvvikling { get; set; }

        [JsonProperty("underTvangsavviklingEllerTvangsopplosning")]
        public bool UnderTvangsavviklingEllerTvangsopplosning { get; set; }

        [JsonProperty("maalform")]
        public string Maalform { get; set; }

        [JsonProperty("links")]
        public object[] Links { get; set; }

        [JsonProperty("hjemmeside")]
        public string Hjemmeside { get; set; }

        [JsonProperty("postadresse")]
        public EnhetAdresse Postadresse { get; set; }

        [JsonProperty("stiftelsesdato")]
        public DateTimeOffset Stiftelsesdato { get; set; }

        [JsonProperty("slettedato")]
        public DateTimeOffset Slettedato { get; set; }

        [JsonProperty("sisteInnsendteAarsregnskap")]
        public int SisteInnsendteAarsregnskap { get; set; }

        [JsonProperty("frivilligMvaRegistrertBeskrivelser")]
        public string[] FrivilligMvaRegistrertBeskrivelser { get; set; }
    }

    public partial class EnhetAdresse
    {
        [JsonProperty("land")]
        public string Land { get; set; }

        [JsonProperty("landkode")]
        public string Landkode { get; set; }

        [JsonProperty("postnummer")]
        public string Postnummer { get; set; }

        [JsonProperty("poststed")]
        public string Poststed { get; set; }

        [JsonProperty("adresse")]
        public string[] Adresse { get; set; }

        [JsonProperty("kommune")]
        public string Kommune { get; set; }

        [JsonProperty("kommunenummer")]
        public string Kommunenummer { get; set; }
    }

    public partial class InstitusjonellSektorkode
    {
        [JsonProperty("kode")]
        public string Kode { get; set; }

        [JsonProperty("beskrivelse")]
        public string Beskrivelse { get; set; }
    }

    public partial class Organisasjonsform
    {
        [JsonProperty("kode")]
        public string Kode { get; set; }

        [JsonProperty("beskrivelse")]
        public string Beskrivelse { get; set; }

        [JsonProperty("links")]
        public object[] Links { get; set; }
    }
}


