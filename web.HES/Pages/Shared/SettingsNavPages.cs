using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace web.HES.Pages
{
    public static class SettingsNavPages
    {
        public static string Administrators => "./Administrators/Index";

        public static string OrgStructure => "./OrgStructure/Index";

        public static string Positions => "./Positions/Index";


        public static string AdministratorsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Administrators);

        public static string OrgStructureNavClass(ViewContext viewContext) => PageNavClass(viewContext, OrgStructure);

        public static string PositionsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Positions);


        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["SettingsPage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}