using Dan.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Altinn.Dan.Plugin.Nsg.Exceptions
{
    public class VardefullaDatamangderException : Exception
    {
        public string Instance { get; set; }
        public int Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string RequestId { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }

        public VardefullaDatamangderException(string instance, int status, DateTime timestamp, string requestId, string title, string detail)
        {
            Instance = instance;
            Status = status;
            Timestamp = timestamp;
            RequestId = requestId;
            Title = title;
            Detail = detail;
        }

        public VardefullaDatamangderException(VardefullaDatamangderErrorModel error)
        {
            Instance = error.instance;
            Status = error.status;
            Timestamp = error.timestamp;
            RequestId= error.requestId;
            Title = error.title;
            Detail = error.detail;
        }
    }
}
