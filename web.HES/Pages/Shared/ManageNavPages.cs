using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace web.HES.Pages
{
    public static class ManageNavPages
    {
        public static string Users => "./Users/Index";

        public static string Devices => "./Devices/Index";

        public static string Settings => "./Settings/Index";


        public static string UsersNavClass(ViewContext viewContext) => PageNavClass(viewContext, Users);

        public static string DevicesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Devices);

        public static string SettingsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Settings);


        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}