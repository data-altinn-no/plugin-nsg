using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Dan.Plugin.Nsg.RegisteredInformation.Models
{
    public class RegisteredInformationRequest
    {
        public string country { get; set; }
        public string organisationNumber { get; set; }
    }


    public class RegisteredInformationResponse
    {
        public List<Activity> activity { get; set; }
        public Identifier identifier { get; set; }
        public Legalform legalForm { get; set; }
        public Legalstatus legalStatus { get; set; }
        public string name { get; set; }
        public Postaladdress postalAddress { get; set; }
        public Registeredaddress registeredAddress { get; set; }
        public DateTime registrationDate { get; set; }
    }

    public class Identifier
    {
        public string issuingAuthorityName { get; set; }
        public string notation { get; set; }
    }

    public class Legalform
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class Legalstatus
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class Postaladdress
    {
        public string fullAddress { get; set; }
    }

    public class Registeredaddress
    {
        public string fullAddress { get; set; }
    }

    public class Activity
    {
        public string code { get; set; }
        public string inClassification { get; set; }
        public string reference { get; set; }
        public int sequence { get; set; }
    }

}
