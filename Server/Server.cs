using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Assignment3.Shared;

namespace Assignment3.Server
{
    public class WebService
    {
        private readonly int _port;
        private TcpListener? _server;
        private readonly RequestValidator _validator;
        private readonly List<Category> _categories;

        public WebService(int port)
        {
            _port = port;
            // Populate with default categories
            var categoryService = new Assignment3.Shared.CategoryService();
            _categories = categoryService.GetCategories();
            _validator = new RequestValidator(_categories);
        }

        public void Run()
        {
            _server = new TcpListener(IPAddress.Loopback, _port);
            _server.Start();
            Console.WriteLine($"Server started on port {_port}");

            while (true)
            {
                TcpClient client = _server.AcceptTcpClient();
                Console.WriteLine("Client connected");

                // Handle each client in a separate thread
                ThreadPool.QueueUserWorkItem(_ => HandleClient(client));
            }
        }


        private void HandleClient(TcpClient client)
        {
            NetworkStream? stream = null;
            try
            {
                stream = client.GetStream();
                var buffer = new byte[4096];

                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    return;
                }

                var requestStr = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                var responseJson = HandleRequest(requestStr);

                var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                stream.Write(responseBytes, 0, responseBytes.Length);
            }
            catch (System.IO.IOException)
            {
                // Client disconnected abruptly; continue serving other clients.
            }
            catch (SocketException)
            {
                // Networking error; ignore this client and continue.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception in HandleClient: {ex}");
            }
            finally
            {
                try { stream?.Close(); } catch { }
                try { client.Close(); } catch { }
            }
        }

        
        private string HandleRequest(string requestStr)
        {
            Request? request;

   
            try
            {
                request = JsonSerializer.Deserialize<Request>(requestStr,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            catch (JsonException)
            {
                request = null;
            }

            // If deserialization failed or returned null, create a default Request
            request ??= new Request { Method = string.Empty, Path = string.Empty, Date = string.Empty };

            var validation = _validator.ValidateRequest(request);

            if (!validation.Status.StartsWith("1"))
            {
                if (validation.Status.ToLower().Contains("missing method") && string.IsNullOrWhiteSpace(request.Date))
                {
                    return JsonSerializer.Serialize(new Response { Status = "4 missing date", Body = null }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                }

                return JsonSerializer.Serialize(validation,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }

            // If this is an echo request, the validator already returned the body — return it directly
            string method = request.Method.ToLower();
            if (method == "echo")
            {
                return JsonSerializer.Serialize(validation, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }

            // For non-echo methods, parse the URL
            var urlParser = new UrlParser();
            bool urlOk = urlParser.ParseUrl(request.Path);
            if (!urlOk)
            {
                return JsonSerializer.Serialize(new Response { Status = "5 Not Found", Body = null }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }

            // Only /api/categories is a valid path for this server. Return Not Found for other resources.
            if (urlParser.Path != "/api/categories")
            {
                return JsonSerializer.Serialize(new Response { Status = "5 Not Found", Body = null }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }

            // At this point validation passed and resource is correct; perform CRUD operations based on method
            Response result = new Response { Status = "5 Not Found", Body = null };

            try
            {
                switch (method)
                {
                    case "create":
                        // Create must not contain an ID in the path
                        if (urlParser.HasId)
                        {
                            result = new Response { Status = "4 bad request", Body = null };
                            break;
                        }

                        // Deserialize incoming body
                        var catCreate = JsonSerializer.Deserialize<Category>(request.Body!, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        if (catCreate == null || string.IsNullOrWhiteSpace(catCreate.Name))
                        {
                            result = new Response { Status = "4 illegal body", Body = null };
                        }
                        else
                        {
                            int nextId = _categories.Count == 0 ? 1 : _categories.Max(c => c.Id) + 1;
                            catCreate.Id = nextId;
                            _categories.Add(catCreate);
                            result = new Response { Status = "1 Ok", Body = JsonSerializer.Serialize(catCreate, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) };
                        }
                        break;

                    case "read":
                        if (urlParser.HasId)
                        {
                            if (!int.TryParse(urlParser.Id, out int readId) || readId <= 0)
                            {
                                result = new Response { Status = "4 bad request", Body = null };
                            }
                            else
                            {
                                var found = _categories.Find(c => c.Id == readId);
                                if (found == null)
                                    result = new Response { Status = "5 Not Found", Body = null };
                                else
                                    result = new Response { Status = "1 Ok", Body = JsonSerializer.Serialize(found, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) };
                            }
                        }
                        else
                        {
                            if (_categories.Count == 0)
                                result = new Response { Status = "5 Not Found", Body = null };
                            else
                                result = new Response { Status = "1 Ok", Body = JsonSerializer.Serialize(_categories, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) };
                        }
                        break;

                    case "update":
                        if (!urlParser.HasId || !int.TryParse(urlParser.Id, out int updateId) || updateId <= 0)
                        {
                            result = new Response { Status = "4 bad request", Body = null };
                        }
                        else
                        {
                            var updateCat = _categories.Find(c => c.Id == updateId);
                            if (updateCat == null)
                                result = new Response { Status = "5 Not Found", Body = null };
                            else
                            {
                                var updated = JsonSerializer.Deserialize<Category>(request.Body!, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                                if (updated == null || string.IsNullOrWhiteSpace(updated.Name))
                                    result = new Response { Status = "4 illegal body", Body = null };
                                else
                                {
                                    updateCat.Name = updated.Name;
                                    result = new Response { Status = "3 Updated", Body = JsonSerializer.Serialize(updateCat, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) };
                                }
                            }
                        }
                        break;

                    case "delete":
                        if (!urlParser.HasId || !int.TryParse(urlParser.Id, out int deleteId) || deleteId <= 0)
                        {
                            result = new Response { Status = "4 bad request", Body = null };
                        }
                        else
                        {
                            var delCat = _categories.Find(c => c.Id == deleteId);
                            if (delCat == null)
                                result = new Response { Status = "5 Not Found", Body = null };
                            else
                            {
                                _categories.Remove(delCat);
                                result = new Response { Status = "1 Ok", Body = null };
                            }
                        }
                        break;
                }
            }
            catch
            {
                result = new Response { Status = "4 illegal body", Body = null };
            }

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
