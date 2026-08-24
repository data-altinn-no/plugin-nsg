using System;
using Altinn.Dan.Plugin.Nsg.Exceptions;
using Newtonsoft.Json;

public class NSGErrorModel
{
    [JsonProperty("type")]
    public string type { get; set; }

    [JsonProperty("instance")]
    public string instance { get; set; }

    [JsonProperty("status")]
    public int status { get; set; }

    [JsonProperty("timestamp")]
    public DateTime? timestamp { get; set; }

    [JsonProperty("requestId")]
    public string requestId { get; set; }

    [JsonProperty("title")]
    public string title { get; set; }

    [JsonProperty("detail")]
    public string detail { get; set; }

    [JsonProperty("code")]
    public string code { get; set; }

    [JsonProperty("source")]
    public string source { get; set; }

    public NSGErrorModel()
    {
    }

    public NSGErrorModel(NsgException ex, string requestIdValue)
    {
        code = ex.ErrorCode;
        detail = ex.ErrorDetail;
        instance = ex.ErrorInstance;
        requestId = requestIdValue;
        source = ex.ErrorSource;
        status = ex.ErrorStatus;
        timestamp = DateTime.UtcNow;
        title = ex.ErrorTitle;
        type = ex.ErrorType;
    }
}

public class VardefullaDatamangderErrorModel
{
    [JsonProperty("instance")]
    public string instance { get; set; }

    [JsonProperty("status")]
    public int status { get; set; }

    [JsonProperty("timestamp")]
    public DateTime? timestamp { get; set; }

    [JsonProperty("requestId")]
    public string requestId { get; set; }

    [JsonProperty("title")]
    public string title { get; set; }

    [JsonProperty("detail")]
    public string detail { get; set; }

    public VardefullaDatamangderErrorModel()
    {
    }

    public VardefullaDatamangderErrorModel(VardefullaDatamangderException ex, string requestIdValue)
    {
        detail = ex.Detail;
        instance = ex.Instance;
        requestId = requestIdValue;
        status = ex.Status;
        timestamp = DateTime.UtcNow;
        title = ex.Title;
    }
}
