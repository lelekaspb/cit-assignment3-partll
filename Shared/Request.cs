using System;


namespace Assignment3.Shared
{
    public class Request
    {
        public string Path { get; set; } = string.Empty;       
        public string? Body { get; set; }
        public string Method { get; set; } = string.Empty;   
        public string Date { get; set; } = string.Empty;     
    }
}
