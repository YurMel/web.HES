using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HES.Web.Pages
{
    public static class AuditNavPages
    {
        public static string WorkstationsEvents => "./WorkstationsEvents/Index";

        public static string WorkstationsSessions => "./WorkstationsSessions/Index";


        public static string WorkstationsEventsNavClass(ViewContext viewContext) => PageNavClass(viewContext, WorkstationsEvents);

        public static string WorkstationsSessionsNavClass(ViewContext viewContext) => PageNavClass(viewContext, WorkstationsSessions);


        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["AuditPage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}