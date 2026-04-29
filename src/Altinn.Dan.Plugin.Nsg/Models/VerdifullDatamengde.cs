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
        public Organisasjonsidentitet Organisationsidentitet { get; set; }
        public string Namnskyddslopnummer { get; set; }
        public OrganisationsnamnWrapper Organisationsnamn { get; set; }
        public Registreringsland Registreringsland { get; set; }
        public Organisationsform Organisationsform { get; set; }
        public ReklameSparr Reklamsparr { get; set; }
        public JuridiskForm JuridiskForm { get; set; }
        public VerksamOrganisation VerksamOrganisation { get; set; }
        public PostadressOrganisation PostadressOrganisation { get; set; }
        public Verksamhetsbeskrivning Verksamhetsbeskrivning { get; set; }
        public Organisationsdatum Organisationsdatum { get; set; }
        public AvregistreradOrganisation AvregistreradOrganisation { get; set; }
        public Avregistreringsorsak Avregistreringsorsak { get; set; }
        public PagandeAvvecklingsEllerOmstruktureringsforfarande PagandeAvvecklingsEllerOmstruktureringsforfarande { get; set; }
        public NaringsgrenOrganisation NaringsgrenOrganisation { get; set; }
    }

    public class Organisasjonsidentitet
    {
        public string Identitetsbeteckning { get; set; }
    }

    public class OrganisationsnamnWrapper
    {
        public List<Organisationsnamn> OrganisationsnamnLista { get; set; }
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
    }

    public class Organisationsnamn
    {
        public string Registreringsdatum { get; set; }
        public string Namn { get; set; }
        public Organisationsnamntyp Organisationsnamntyp { get; set; }
        public string VerksamhetsbeskrivningSarskiltForetagsnamn { get; set; }
    }

    public class Fel
    {
        public string FelBeskrivning { get; set; }
        public string Typ { get; set; }
    }

    public class Organisationsnamntyp
    {
        public string Kod { get; set; }
        public string Klartext { get; set; }
    }

    public class Registreringsland
    {
        public string Kod { get; set; }
        public string Klartext { get; set; }
    }

    public class Organisationsform
    {
        public string Kod { get; set; }
        public string Klartext { get; set; }
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
    }

    public class ReklameSparr
    {
        public string Kod { get; set; }
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
    }

    public class JuridiskForm
    {
        public string Kod { get; set; }
        public string Klartext { get; set; }
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
    }

    public class VerksamOrganisation
    {
        public string Kod { get; set; }
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
    }

    public class PostadressOrganisation
    {
        public Postadress Postadress { get; set; }
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
    }

    public class Postadress
    {
        public string Postnummer { get; set; }
        public string Utdelningsadress { get; set; }
        public string Land { get; set; }
        public string CoAdress { get; set; }
        public string Postort { get; set; }
    }

    public class Verksamhetsbeskrivning
    {
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
        public string Beskrivning { get; set; }
    }

    public class Organisationsdatum
    {
        public string Registreringsdatum { get; set; }
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
        public string InfortHosScb { get; set; }
    }

    public class AvregistreradOrganisation
    {
        public DateTime? Avregistreringsdatum { get; set; }
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
    }

    public class Avregistreringsorsak
    {
        public string Kod { get; set; }
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
        public string Klartext { get; set; }
    }

    public class PagandeAvvecklingsEllerOmstruktureringsforfarande
    {
        public List<PagandeAvvecklingsEllerOmstruktureringsforfarandeItem> PagandeAvvecklingsEllerOmstruktureringsforfarandeLista { get; set; }
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
    }

    public class PagandeAvvecklingsEllerOmstruktureringsforfarandeItem
    {
        public string Kod { get; set; }
        public string Klartext { get; set; }
        public DateTime? FromDatum { get; set; }
    }

    public class NaringsgrenOrganisation
    {
        public Fel Fel { get; set; }
        public string Dataproducent { get; set; }
        public List<Sni> Sni { get; set; }
    }

    public class Sni
    {
        public string Kod { get; set; }
        public string Klartext { get; set; }
    }
}
