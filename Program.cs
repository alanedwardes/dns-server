using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DNS.Protocol;
using DNS.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DnsServerConsole
{
    internal class Program
    {
        private static readonly IAmazonDynamoDB amazonDynamoDb = new AmazonDynamoDBClient();

        static void Main(string[] args)
        {
            try
            {
                MainAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        static async Task MainAsync()
        {
            MasterFile masterFile = new MasterFile();
            DnsServer server = new DnsServer(masterFile, "8.8.8.8");

            server.Listening += (sender, e) => Console.WriteLine("Now listening on port 53");
            server.Responded += (sender, e) => PersistRequest(e.Request);
            server.Errored += (sender, e) => Console.WriteLine(e.Exception.Message);

            Console.WriteLine("Starting server...");

            await server.Listen();
        }

        public static async Task PersistRequest(IRequest request)
        {
            var requestId = Guid.NewGuid().ToString();

            var requests = new List<PutRequest>();

            foreach (var question in request.Questions)
            {
                requests.Add(new PutRequest
                {
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "RequestId", new AttributeValue(requestId) },
                        { "Name", new AttributeValue(question.Name.ToString()) },
                        { "Expiry", new AttributeValue { N = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds().ToString() } }
                    }
                });
            }

            try
            {
                await amazonDynamoDb.BatchWriteItemAsync(new BatchWriteItemRequest
                {
                    RequestItems = new Dictionary<string, List<WriteRequest>>
                    {
                        { "DnsRequests", requests.Select(x => new WriteRequest(x)).ToList()}
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
