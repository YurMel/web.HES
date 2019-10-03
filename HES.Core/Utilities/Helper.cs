using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HES.Core.Utilities
{
    public static class Hepler
    {
        public static string VerifyUrls(string urls)
        {
            List<string> verifiedUrls = new List<string>();
            foreach (var url in urls.Split(";"))
            {
                string uriString = url;
                string domain = string.Empty;

                if (string.IsNullOrWhiteSpace(uriString))
                {
                    throw new Exception("Not correct url");
                }

                if (!uriString.Contains(Uri.SchemeDelimiter))
                {
                    uriString = string.Concat(Uri.UriSchemeHttp, Uri.SchemeDelimiter, uriString);
                }

                domain = new Uri(uriString).Host;

                if (domain.StartsWith("www."))
                    domain = domain.Remove(0, 4);

                verifiedUrls.Add(domain);
            }

            var result = string.Join(";", verifiedUrls.ToArray());
            return result;
        }

        public static bool VerifyOtpSecret(string value)
        {
            return Regex.IsMatch(value.Replace(" ", ""), @"^[a-zA-Z0-9]+$");
        }
    }
}