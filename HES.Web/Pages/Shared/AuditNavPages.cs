using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HES.Web.Pages
{
    public static class AuditNavPages
    {
        public static string WorkstationEvents => "./WorkstationEvents/Index";

        public static string WorkstationSessions => "./WorkstationSessions/Index";

        public static string WorkstationSummaries => "./WorkstationSummaries/Index";


        public static string WorkstationEventsNavClass(ViewContext viewContext) => PageNavClass(viewContext, WorkstationEvents);

        public static string WorkstationSessionsNavClass(ViewContext viewContext) => PageNavClass(viewContext, WorkstationSessions);

        public static string WorkstationSummariesNavClass(ViewContext viewContext) => PageNavClass(viewContext, WorkstationSummaries);


        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["AuditPage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}