
using System;
using System.Collections.Generic;

namespace Assignment3.Shared
{ 
    public class UrlParser
        {
            public bool HasId { get; set; }
            public string Id { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;

        public bool ParseUrl(string url)
            {
                if (string.IsNullOrEmpty(url))
                {
                    return false;
                }

                // Remove leading and trailing slashes
                if (url.StartsWith("/"))
                {
                    url = url.Substring(1);
                }
                if (url.EndsWith("/"))
                {
                    url = url.Substring(0, url.Length - 1);
                }

                string[] parts = url.Split('/');

                // Must have at least two parts: "api" + "resource"
                if (parts.Length < 2)
                {
                    return false;
                }

                // Check if an ID is included
                if (parts.Length == 3)
                {
                    Path = "/" + parts[0] + "/" + parts[1];
                    Id = parts[2];
                    HasId = true;
                }
                else
                {
                    Path = "/" + parts[0] + "/" + parts[1];
                    HasId = false;
                    Id = "";
                }

            return true;
        }
    }
}

