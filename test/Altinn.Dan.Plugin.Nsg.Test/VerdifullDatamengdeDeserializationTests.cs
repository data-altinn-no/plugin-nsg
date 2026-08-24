using Altinn.Dan.Plugin.Nsg.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Altinn.Dan.Plugin.Nsg.Test
{
    /// <summary>
    /// Verifiserer at Bolagsverkets Vardefulla Datamangder-respons deserialiserer korrekt til våre C#-modeller.
    /// Dette er første forsvarslinje mot stavefeil-mismatch mellom JSON-nøkler og C#-properties
    /// </summary>
    [TestClass]
    public class VerdifullDatamengdeDeserializationTests
    {
        /// <summary>
        /// Et fullstendig eksempelsvar med alle feltene som forekommer i produksjon,
        /// inkludert edge-cases: sole trader med Namnskyddslopnummer, ongoing konkurs, NACE-koder, og avregistreringsårsak.
        /// </summary>
        private const string SampleResponse = @"{
  ""organisationer"": [
    {
      ""organisationsidentitet"": { ""identitetsbeteckning"": ""5590719539"" },
      ""namnskyddslopnummer"": ""1"",
      ""organisationsnamn"": {
        ""organisationsnamnLista"": [
          {
            ""registreringsdatum"": ""2020-01-15"",
            ""namn"": ""Test Bolag AB"",
            ""organisationsnamntyp"": { ""kod"": ""FORETAGSNAMN"", ""klartext"": ""Företagsnamn"" },
            ""verksamhetsbeskrivningSarskiltForetagsnamn"": null
          }
        ],
        ""fel"": null,
        ""dataproducent"": ""Bolagsverket""
      },
      ""registreringsland"": { ""kod"": ""SE"", ""klartext"": ""Sverige"" },
      ""organisationsform"": {
        ""kod"": ""AB"",
        ""klartext"": ""Aktiebolag"",
        ""fel"": null,
        ""dataproducent"": ""Bolagsverket""
      },
      ""reklamsparr"": { ""kod"": ""N"", ""fel"": null, ""dataproducent"": ""Bolagsverket"" },
      ""juridiskForm"": {
        ""kod"": ""AB"",
        ""klartext"": ""Aktiebolag"",
        ""fel"": null,
        ""dataproducent"": ""Bolagsverket""
      },
      ""verksamOrganisation"": { ""kod"": ""J"", ""fel"": null, ""dataproducent"": ""Bolagsverket"" },
      ""postadressOrganisation"": {
        ""postadress"": {
          ""postnummer"": ""85181"",
          ""utdelningsadress"": ""Testgatan 1"",
          ""land"": ""SE"",
          ""coAdress"": null,
          ""postort"": ""SUNDSVALL""
        },
        ""fel"": null,
        ""dataproducent"": ""Bolagsverket""
      },
      ""verksamhetsbeskrivning"": {
        ""fel"": null,
        ""dataproducent"": ""Bolagsverket"",
        ""beskrivning"": ""Bolaget skal drive konsulentvirksomhet.""
      },
      ""organisationsdatum"": {
        ""registreringsdatum"": ""2020-01-15"",
        ""fel"": null,
        ""dataproducent"": ""Bolagsverket"",
        ""infortHosScb"": ""2020-01-20""
      },
      ""avregistreradOrganisation"": {
        ""avregistreringsdatum"": null,
        ""fel"": null,
        ""dataproducent"": ""Bolagsverket""
      },
      ""avregistreringsorsak"": {
        ""kod"": null,
        ""fel"": null,
        ""dataproducent"": ""Bolagsverket"",
        ""klartext"": null
      },
      ""pagaendeAvvecklingsEllerOmstruktureringsforfarande"": {
        ""dataproducent"": ""Bolagsverket"",
        ""fel"": null,
        ""pagaendeAvvecklingsEllerOmstruktureringsforfarandeLista"": [
          {
            ""kod"": ""KK"",
            ""fromDatum"": ""2026-08-18"",
            ""klartext"": ""Konkurs""
          }
        ]
      },
      ""naringsgrenOrganisation"": {
        ""fel"": null,
        ""dataproducent"": ""Bolagsverket"",
        ""sni"": [
          { ""kod"": ""70220"", ""klartext"": ""Konsultvirksomhet"" },
          { ""kod"": ""62010"", ""klartext"": ""Dataprogrammering"" }
        ]
      }
    }
  ]
}";

        private static VerdifullDatamengdeResponse Deserialize() =>
            JsonConvert.DeserializeObject<VerdifullDatamengdeResponse>(SampleResponse);

        [TestMethod]
        public void Deserialize_TopLevel_OrganisationerListIsPopulated()
        {
            var result = Deserialize();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Organisationer);
            Assert.AreEqual(1, result.Organisationer.Count);
        }

        [TestMethod]
        public void Deserialize_BasicOrgIdentity_MapsCorrectly()
        {
            var org = Deserialize().Organisationer[0];

            Assert.AreEqual("5590719539", org.Organisationsidentitet?.Identitetsbeteckning);
            Assert.AreEqual("1", org.Namnskyddslopnummer);
            Assert.AreEqual("Test Bolag AB", org.Organisationsnamn?.OrganisationsnamnLista?[0].Namn);
        }

        [TestMethod]
        public void Deserialize_OrganisationsForm_MapsCorrectly()
        {
            var org = Deserialize().Organisationer[0];

            Assert.AreEqual("AB", org.Organisationsform?.Kod);
            Assert.AreEqual("Aktiebolag", org.Organisationsform?.Klartext);
            Assert.AreEqual("Bolagsverket", org.Organisationsform?.Dataproducent);
        }

        [TestMethod]
        public void Deserialize_PostalAddress_MapsCorrectly()
        {
            var post = Deserialize().Organisationer[0].PostadressOrganisation?.Postadress;

            Assert.IsNotNull(post);
            Assert.AreEqual("Testgatan 1", post.Utdelningsadress);
            Assert.AreEqual("85181", post.Postnummer);
            Assert.AreEqual("SUNDSVALL", post.Postort);
            Assert.AreEqual("SE", post.Land);
        }

        [TestMethod]
        public void Deserialize_OrganisationsDatum_MapsRegistreringsdatum()
        {
            var org = Deserialize().Organisationer[0];

            Assert.AreEqual("2020-01-15", org.Organisationsdatum?.Registreringsdatum);
        }

        [TestMethod]
        public void Deserialize_NaringsgrenSni_MapsMultipleCodes()
        {
            var sni = Deserialize().Organisationer[0].NaringsgrenOrganisation?.Sni;

            Assert.IsNotNull(sni);
            Assert.AreEqual(2, sni.Count);
            Assert.AreEqual("70220", sni[0].Kod);
            Assert.AreEqual("Konsultvirksomhet", sni[0].Klartext);
            Assert.AreEqual("62010", sni[1].Kod);
        }

        /// <summary>
        /// Bolagsverket sender feltet som "pagaende..." (med ekstra 'e'), mens C#-klassen
        /// heter "Pagande...". Uten JsonProperty-attributt vil dette silently mislykkes,
        /// og konkurs-detekteringen blir alltid false → feil legal status returneres.
        /// </summary>
        [TestMethod]
        public void Deserialize_PagaendeAvvecklings_MapsDespiteSpellingDifference()
        {
            var org = Deserialize().Organisationer[0];

            Assert.IsNotNull(org.PagandeAvvecklingsEllerOmstruktureringsforfarande,
                "FEIL-006: pagaende...-feltet må mappes til Pagande...-property via JsonProperty-attributt");

            var lista = org.PagandeAvvecklingsEllerOmstruktureringsforfarande.PagandeAvvecklingsEllerOmstruktureringsforfarandeLista;
            Assert.IsNotNull(lista);
            Assert.AreEqual(1, lista.Count);
            Assert.AreEqual("KK", lista[0].Kod);
            Assert.AreEqual("Konkurs", lista[0].Klartext);
        }

        [TestMethod]
        public void Deserialize_PagaendeAvvecklings_ListaAnyReturnsTrueForKonkurs()
        {
            // Dette er det logikk-uttrykket MapOrgData bruker for å avgjøre SOME_REGISTERED.
            // Hvis .Any() returnerer false her, betyr det at hasOngoingProceedings blir false,
            // og legal status blir feilaktig NO_REGISTERED.
            var org = Deserialize().Organisationer[0];
            var hasOngoingProceedings = org.PagandeAvvecklingsEllerOmstruktureringsforfarande
                ?.PagandeAvvecklingsEllerOmstruktureringsforfarandeLista?.Count > 0;

            Assert.IsTrue(hasOngoingProceedings,
                "En org med konkurs-oppføring i pagaende-listen skal gi hasOngoingProceedings=true, som mapper til SOME_REGISTERED");
        }

        [TestMethod]
        public void Deserialize_EmptyPagaendeList_TreatedAsNoOngoingProceedings()
        {
            // Kontrolltest — en org uten pågående prosesser skal ha tom (eller null) liste
            const string noOngoingResponse = @"{
              ""organisationer"": [{
                ""organisationsidentitet"": { ""identitetsbeteckning"": ""5560000001"" },
                ""pagaendeAvvecklingsEllerOmstruktureringsforfarande"": {
                  ""dataproducent"": ""Bolagsverket"",
                  ""fel"": null,
                  ""pagaendeAvvecklingsEllerOmstruktureringsforfarandeLista"": []
                }
              }]
            }";

            var org = JsonConvert.DeserializeObject<VerdifullDatamengdeResponse>(noOngoingResponse).Organisationer[0];
            var hasOngoingProceedings = org.PagandeAvvecklingsEllerOmstruktureringsforfarande
                ?.PagandeAvvecklingsEllerOmstruktureringsforfarandeLista?.Count > 0;

            Assert.IsFalse(hasOngoingProceedings, "Tom liste skal ikke gi SOME_REGISTERED");
        }

        [TestMethod]
        public void Deserialize_AvregistreradOrganisation_NullDateForActiveOrg()
        {
            var org = Deserialize().Organisationer[0];

            Assert.IsNotNull(org.AvregistreradOrganisation);
            Assert.IsFalse(org.AvregistreradOrganisation.Avregistreringsdatum.HasValue,
                "Aktiv org skal ha null Avregistreringsdatum");
        }

        /// <summary>
        /// Regresjonstest for "not found shell"-oppførselen.
        /// Bolagsverket returnerer 200 med et objekt der subfeltene har fel.typ =
        /// "ORGANISATION_FINNS_EJ" istedenfor 404 når org ikke eksisterer.
        /// Uten den detekteringen returnerte plugin-en tomt 200-svar i stedet for 404.
        /// </summary>
        [TestMethod]
        public void Deserialize_NotFoundShell_HasOrganisationFinnsEjFelType()
        {
            const string notFoundResponse = @"{
              ""organisationer"": [{
                ""avregistreradOrganisation"": {
                  ""avregistreringsdatum"": null,
                  ""dataproducent"": ""Bolagsverket"",
                  ""fel"": {
                    ""typ"": ""ORGANISATION_FINNS_EJ"",
                    ""felBeskrivning"": ""Begärd organisation finns inte registrerad i sökbar form...""
                  }
                },
                ""avregistreringsorsak"": {
                  ""kod"": null,
                  ""klartext"": null,
                  ""dataproducent"": ""Bolagsverket"",
                  ""fel"": {
                    ""typ"": ""ORGANISATION_FINNS_EJ"",
                    ""felBeskrivning"": ""Begärd organisation finns inte registrerad i sökbar form...""
                  }
                }
              }]
            }";

            var org = JsonConvert.DeserializeObject<VerdifullDatamengdeResponse>(notFoundResponse).Organisationer[0];

            Assert.AreEqual("ORGANISATION_FINNS_EJ", org.AvregistreradOrganisation?.Fel?.Typ,
                "fel-strukturen inni avregistreradOrganisation må mappes korrekt så MapOrgData kan oppdage 'ikke funnet'-signalet");
            Assert.AreEqual("ORGANISATION_FINNS_EJ", org.Avregistreringsorsak?.Fel?.Typ);
        }

        [TestMethod]
        public void Deserialize_AvregistreradOrganisation_ParsedDateForDeregisteredOrg()
        {
            const string deregisteredResponse = @"{
              ""organisationer"": [{
                ""organisationsidentitet"": { ""identitetsbeteckning"": ""5562820745"" },
                ""avregistreradOrganisation"": {
                  ""avregistreringsdatum"": ""2025-06-18T00:00:00"",
                  ""dataproducent"": ""Bolagsverket"",
                  ""fel"": null
                }
              }]
            }";

            var org = JsonConvert.DeserializeObject<VerdifullDatamengdeResponse>(deregisteredResponse).Organisationer[0];

            Assert.IsTrue(org.AvregistreradOrganisation.Avregistreringsdatum.HasValue);
            Assert.AreEqual(2025, org.AvregistreradOrganisation.Avregistreringsdatum.Value.Year);
            Assert.AreEqual(6, org.AvregistreradOrganisation.Avregistreringsdatum.Value.Month);
            Assert.AreEqual(18, org.AvregistreradOrganisation.Avregistreringsdatum.Value.Day);
        }
    }
}
