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
            foreach (var question in request.Questions)
            {
                try
                {
                    await amazonDynamoDb.UpdateItemAsync(new UpdateItemRequest("DnsRequests", new Dictionary<string, AttributeValue>
                    {
                        { "RequestId", new AttributeValue(question.Name.ToString()) }
                    }, new Dictionary<string, AttributeValueUpdate>
                    {
                        { "Count", new AttributeValueUpdate { Action = "ADD", Value = new AttributeValue { N = "1" } } },
                        { "CurrentYear", new AttributeValueUpdate { Action = "PUT", Value = new AttributeValue(DateTime.UtcNow.ToString("yyyy")) } },
                        { "CurrentMonth", new AttributeValueUpdate { Action = "PUT", Value = new AttributeValue(DateTime.UtcNow.ToString("yyyy-MM")) } },
                        { "CurrentDate", new AttributeValueUpdate { Action = "PUT", Value = new AttributeValue(DateTime.UtcNow.ToString("yyyy-MM-dd")) } },
                        { "Expiry", new AttributeValueUpdate { Action = "PUT", Value = new AttributeValue { N = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() } } }
                    }));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}
