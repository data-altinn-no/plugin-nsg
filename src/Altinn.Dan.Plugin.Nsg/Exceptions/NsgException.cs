using System;

namespace Altinn.Dan.Plugin.Nsg.Exceptions
{
    public class NsgException : Exception
    {
        //The error as a pre-defined code. A possible use case of this field, is for different logic in the client.
        public string ErrorCode { get; set; }

        public string ErrorType { get; set; }

        public string ErrorInstance { get; set; }

        //Name of the parameter causing the error.
        public string ErrorSource { get; set; }

        //Detailed description of the error
        public string ErrorDetail { get; set; }

        //HTTP status
        public int ErrorStatus { get; set; }

        public string ErrorTitle { get; set; }

        public NsgException(string errorCode, string errorType, string errorInstance, string errorSource, string errorDetail, int errorStatus, string errorTitle)
        {
            ErrorCode = errorCode;
            ErrorType = errorType;
            ErrorInstance = errorInstance;
            ErrorSource = errorSource;
            ErrorDetail = errorDetail;
            ErrorStatus = errorStatus;
            ErrorTitle = errorTitle;
        }

        public NsgException(NSGErrorModel error)
        {
            ErrorCode = error.code;
            ErrorType = error.type;
            ErrorInstance = error.instance;
            ErrorSource = error.source;
            ErrorDetail = error.detail;
            ErrorStatus = error.status;
            ErrorTitle = error.title;
        }

        public override string ToString()
        {
            return
                $"ErrorCode = {ErrorCode}, ErrorType = {ErrorType}, ErrorInstance = {ErrorInstance}, ErrorSource = {ErrorSource}, ErrorDetail = {ErrorDetail}, ErrorStatus = {ErrorStatus}, ErrorTitle = {ErrorTitle}";
        }
    }
}
