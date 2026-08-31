using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Altinn.Dan.Plugin.Nsg.Models
{
    public class OrganisationerRequest
    {
        [JsonProperty("identitetsbeteckning")]
        public string Identitetsbeteckning { get; set; }
    }

    public class VerdifullDatamengdeResponse
    {
        [JsonProperty("organisationer")]
        public List<Organisasjon> Organisationer { get; set; }
    }

    public class Organisasjon
    {
        [JsonProperty("organisationsidentitet")]
        public Organisasjonsidentitet Organisationsidentitet { get; set; }

        [JsonProperty("namnskyddslopnummer")]
        public string Namnskyddslopnummer { get; set; }

        [JsonProperty("organisationsnamn")]
        public OrganisationsnamnWrapper Organisationsnamn { get; set; }

        [JsonProperty("registreringsland")]
        public Registreringsland Registreringsland { get; set; }

        [JsonProperty("organisationsform")]
        public Organisationsform Organisationsform { get; set; }

        [JsonProperty("reklamsparr")]
        public ReklameSparr Reklamsparr { get; set; }

        [JsonProperty("juridiskForm")]
        public JuridiskForm JuridiskForm { get; set; }

        [JsonProperty("verksamOrganisation")]
        public VerksamOrganisation VerksamOrganisation { get; set; }

        [JsonProperty("postadressOrganisation")]
        public PostadressOrganisation PostadressOrganisation { get; set; }

        [JsonProperty("verksamhetsbeskrivning")]
        public Verksamhetsbeskrivning Verksamhetsbeskrivning { get; set; }

        [JsonProperty("organisationsdatum")]
        public Organisationsdatum Organisationsdatum { get; set; }

        [JsonProperty("avregistreradOrganisation")]
        public AvregistreradOrganisation AvregistreradOrganisation { get; set; }

        [JsonProperty("avregistreringsorsak")]
        public Avregistreringsorsak Avregistreringsorsak { get; set; }

        // Bolagsverket sender "pagaende..." (med ekstra 'e' etter 'a'), ikke "Pagande...".
        // Uten JsonProperty ville feltet aldri bli populert, og konkurs-detekteringen ville alltid feile.
        [JsonProperty("pagaendeAvvecklingsEllerOmstruktureringsforfarande")]
        public PagandeAvvecklingsEllerOmstruktureringsforfarande PagandeAvvecklingsEllerOmstruktureringsforfarande { get; set; }

        [JsonProperty("naringsgrenOrganisation")]
        public NaringsgrenOrganisation NaringsgrenOrganisation { get; set; }
    }

    public class Organisasjonsidentitet
    {
        [JsonProperty("identitetsbeteckning")]
        public string Identitetsbeteckning { get; set; }
    }

    public class OrganisationsnamnWrapper
    {
        [JsonProperty("organisationsnamnLista")]
        public List<Organisationsnamn> OrganisationsnamnLista { get; set; }

        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }
    }

    public class Organisationsnamn
    {
        [JsonProperty("registreringsdatum")]
        public string Registreringsdatum { get; set; }

        [JsonProperty("namn")]
        public string Namn { get; set; }

        [JsonProperty("organisationsnamntyp")]
        public Organisationsnamntyp Organisationsnamntyp { get; set; }

        [JsonProperty("verksamhetsbeskrivningSarskiltForetagsnamn")]
        public string VerksamhetsbeskrivningSarskiltForetagsnamn { get; set; }
    }

    public class Fel
    {
        [JsonProperty("felBeskrivning")]
        public string FelBeskrivning { get; set; }

        [JsonProperty("typ")]
        public string Typ { get; set; }
    }

    public class Organisationsnamntyp
    {
        [JsonProperty("kod")]
        public string Kod { get; set; }

        [JsonProperty("klartext")]
        public string Klartext { get; set; }
    }

    public class Registreringsland
    {
        [JsonProperty("kod")]
        public string Kod { get; set; }

        [JsonProperty("klartext")]
        public string Klartext { get; set; }
    }

    public class Organisationsform
    {
        [JsonProperty("kod")]
        public string Kod { get; set; }

        [JsonProperty("klartext")]
        public string Klartext { get; set; }

        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }
    }

    public class ReklameSparr
    {
        [JsonProperty("kod")]
        public string Kod { get; set; }

        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }
    }

    public class JuridiskForm
    {
        [JsonProperty("kod")]
        public string Kod { get; set; }

        [JsonProperty("klartext")]
        public string Klartext { get; set; }

        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }
    }

    public class VerksamOrganisation
    {
        [JsonProperty("kod")]
        public string Kod { get; set; }

        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }
    }

    public class PostadressOrganisation
    {
        [JsonProperty("postadress")]
        public Postadress Postadress { get; set; }

        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }
    }

    public class Postadress
    {
        [JsonProperty("postnummer")]
        public string Postnummer { get; set; }

        [JsonProperty("utdelningsadress")]
        public string Utdelningsadress { get; set; }

        [JsonProperty("land")]
        public string Land { get; set; }

        [JsonProperty("coAdress")]
        public string CoAdress { get; set; }

        [JsonProperty("postort")]
        public string Postort { get; set; }
    }

    public class Verksamhetsbeskrivning
    {
        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }

        [JsonProperty("beskrivning")]
        public string Beskrivning { get; set; }
    }

    public class Organisationsdatum
    {
        [JsonProperty("registreringsdatum")]
        public string Registreringsdatum { get; set; }

        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }

        [JsonProperty("infortHosScb")]
        public string InfortHosScb { get; set; }
    }

    public class AvregistreradOrganisation
    {
        [JsonProperty("avregistreringsdatum")]
        public DateTime? Avregistreringsdatum { get; set; }

        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }
    }

    public class Avregistreringsorsak
    {
        [JsonProperty("kod")]
        public string Kod { get; set; }

        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }

        [JsonProperty("klartext")]
        public string Klartext { get; set; }
    }

    public class PagandeAvvecklingsEllerOmstruktureringsforfarande
    {
        // Samme spelling-avvik gjelder listen inni: "pagaende..." fra API, "Pagande..." i C#.
        [JsonProperty("pagaendeAvvecklingsEllerOmstruktureringsforfarandeLista")]
        public List<PagandeAvvecklingsEllerOmstruktureringsforfarandeItem> PagandeAvvecklingsEllerOmstruktureringsforfarandeLista { get; set; }

        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }
    }

    public class PagandeAvvecklingsEllerOmstruktureringsforfarandeItem
    {
        [JsonProperty("kod")]
        public string Kod { get; set; }

        [JsonProperty("klartext")]
        public string Klartext { get; set; }

        [JsonProperty("fromDatum")]
        public DateTime? FromDatum { get; set; }
    }

    public class NaringsgrenOrganisation
    {
        [JsonProperty("fel")]
        public Fel Fel { get; set; }

        [JsonProperty("dataproducent")]
        public string Dataproducent { get; set; }

        [JsonProperty("sni")]
        public List<Sni> Sni { get; set; }
    }

    public class Sni
    {
        [JsonProperty("kod")]
        public string Kod { get; set; }

        [JsonProperty("klartext")]
        public string Klartext { get; set; }
    }
}
