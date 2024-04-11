
using System;
using Altinn.Dan.Plugin.Nsg.Exceptions;

public class NSGErrorModel
{
    public string type { get; set; }
    public string instance { get; set; }
    public int status { get; set; }
    public DateTime timestamp { get; set; }
    public string requestId { get; set; }
    public string title { get; set; }
    public string detail { get; set; }
    public string code { get; set; }
    public string source { get; set; }

    public NSGErrorModel()
    {

    }

    public NSGErrorModel(NsgException ex, string requestIdValue)
    {
        code = ex.ErrorCode;
        detail = ex.ErrorDetail;
        instance = ex.ErrorInstance;
        requestId = requestId;
        source = ex.ErrorSource;
        status = ex.ErrorStatus;
        timestamp = DateTime.Now.ToUniversalTime();
        title = ex.ErrorTitle;
        type = ex.ErrorType;
    }
}

