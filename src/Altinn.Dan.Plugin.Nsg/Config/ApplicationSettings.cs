using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dan.Common.Exceptions;

namespace Altinn.Dan.Plugin.Nsg.Config
{
    public class ApplicationSettings
    {
        public ApplicationSettings() { }

        public string GetRegisteredInformationUrl(string country)
        {
            {
                if (country == "NO")
                {
                    return RegisteredInformationUrlNO;
                }
                else if (country == "SE")
                {
                    return RegisteredInformationUrlSE;
                } else if (country == "FI")
                {
                    return RegisteredInformationUrlFI;
                } else if (country == "DE")
                {
                    return RegisteredInformationUrlDE;
                } else if (country == "IS")
                {
                    return RegisteredInformationUrlIS;
                }
                else
                {
                    throw new EvidenceSourcePermanentClientException(1,"Invalid Country code");
                }
            }
        }

        public string RegisteredInformationUrlNO { get; set; }

        public string RegisteredInformationUrlSE { get; set; }

        public string RegisteredInformationUrlFI { get; set; }

        public string RegisteredInformationUrlDE { get; set; }

        public string RegisteredInformationUrlIS { get; set; }

        public string ClientSecretSE { get; set; }

        public string ClientIdSE { get; set; }

        public string ScopeSE { get; set; }

        public string TokenUrlSE { get; set; }

        public bool TokenCaching { get; set; }
    }
}
