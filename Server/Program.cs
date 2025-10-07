using System;
using Assignment3.Shared;

namespace Assignment3.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebService(5000);
            server.Run();
        }
    }
}