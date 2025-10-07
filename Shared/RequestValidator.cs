using System;
using System.Collections.Generic;
using System.Text.Json;


namespace Assignment3.Shared
{
    public class RequestValidator
    {
        private readonly List<Category> _categories;

        public RequestValidator(List<Category> categories)
        {
            _categories = categories;
        }

        public RequestValidator() : this(new List<Category>())
        {
        }

        public Response ValidateRequest(Request request)
        {
            
            // 1. Validate Method
            if (string.IsNullOrWhiteSpace(request.Method))
                return new Response { Status = "4 missing method", Body = null };

            string method = request.Method.ToLower();
            string[] allowedMethods = { "create", "read", "update", "delete", "echo" };
            if (Array.IndexOf(allowedMethods, method) < 0)
                return new Response { Status = "4 illegal method", Body = null };

            // 2. Validate Path
            if (string.IsNullOrWhiteSpace(request.Path) && method != "echo")
                return new Response { Status = "4 missing path", Body = null };

            // 3. Validate Date
            if (method != "echo")
            {
                if (string.IsNullOrWhiteSpace(request.Date))
                    return new Response { Status = "4 missing date", Body = null };
                if (!long.TryParse(request.Date, out long dateValue) || dateValue <= 0)
                    return new Response { Status = "4 illegal date", Body = null };
            }
         

            // 4. Validate Body rules
            if ((method == "create" || method == "update") && string.IsNullOrWhiteSpace(request.Body))
                return new Response { Status = "4 missing body", Body = null };
            // Guard against null on Body and ensure it starts with '{' for JSON
            if ((method == "create" || method == "update") && !(request.Body?.TrimStart().StartsWith("{") ?? false))
                return new Response { Status = "4 illegal body", Body = null };
            if (method == "echo" && string.IsNullOrWhiteSpace(request.Body))
                return new Response { Status = "4 missing body", Body = null };

            // 5. Handle echo early so empty or missing path doesn't cause URL parsing to fail
            if (method == "echo")
            {
                return new Response { Status = "1 Ok", Body = request.Body ?? "" };
            }

            var urlParser = new UrlParser();
            bool urlOk = urlParser.ParseUrl(request.Path);

            // 6. Path parsing failed (invalid endpoint)
            if (!urlOk)
                return new Response { Status = "5 Not Found", Body = null };          

           
            return new Response { Status = "1 Ok", Body = null };
        }
    }
}
