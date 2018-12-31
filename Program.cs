using DNS.Server;
using System;

namespace DnsServerConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Proxy to google's DNS
            MasterFile masterFile = new MasterFile();
            DnsServer server = new DnsServer(masterFile, "8.8.8.8");

            server.Listening += (sender, e) => Console.WriteLine("Now listening on port 53");

            // Log every request
            server.Requested += (sender, e) => Console.WriteLine(e.Request);
            // On every successful request log the request and the response
            server.Responded += (sender, e) => Console.WriteLine("{0} => {1}", e.Request, e.Response);
            // Log errors
            server.Errored += (sender, e) => Console.WriteLine(e.Exception.Message);

            Console.WriteLine("Starting server...");

            try
            {
                server.Listen().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
