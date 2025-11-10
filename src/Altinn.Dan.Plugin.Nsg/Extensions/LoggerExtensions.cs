using Dan.Common.Enums;
using Dan.Common.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Altinn.Dan.Plugin.Nsg.Extensions
{
    //Copied from Dan.Core - TODO: Consider moving to Dan.Common
    public static class LoggerExtensions
    {
        private const string LogString = "{action}:{callingClass}.{callingMethod},a={accreditationId},c={consentReference},e={externalReference},o={owner},r={requestor},s={subject},d={evidenceCodeName},t={dateTime},sc={serviceContext}";

        public static void DanLog(
            this ILogger logger,
            Accreditation accreditation,
            LogAction action,
            string? dataSetName = null,
            [CallerFilePath] string callingClass = "",
            [CallerMemberName] string callingMethod = "")
        {

            logger.LogInformation(LogString, Enum.GetName(typeof(LogAction), action),
                Path.GetFileNameWithoutExtension(callingClass), callingMethod, accreditation.AccreditationId,
                accreditation.ConsentReference, accreditation.ExternalReference, accreditation.Owner,
                accreditation.RequestorParty?.ToString(), accreditation.SubjectParty?.ToString(), dataSetName,
                DateTime.UtcNow, accreditation.ServiceContext);
        }

        public static void DanLog(
            this ILogger logger,
            string subject,
            string evidenceCodeName,
            string serviceContext,
            LogAction action,
            [CallerFilePath] string callingClass = "",
            [CallerMemberName] string callingMethod = "")
        {
            logger.LogInformation(LogString, Enum.GetName(typeof(LogAction), action),
                Path.GetFileNameWithoutExtension(callingClass), callingMethod, null, "", "", "", "", subject,
                evidenceCodeName, DateTime.UtcNow, serviceContext);
        }

        public static void DanLog(
            this ILogger logger,
            LogAction action,
            string owner,
            string requestor,
            string serviceContext,
            [CallerFilePath] string callingClass = "",
            [CallerMemberName] string callingMethod = "")
        {
            logger.LogInformation(LogString, Enum.GetName(typeof(LogAction), action),
                Path.GetFileNameWithoutExtension(callingClass), callingMethod, null, "", "", owner, requestor, "", "", DateTime.UtcNow, serviceContext);
        }
    }
}
