using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace QueueReceiver
{
    internal class Program
    {
        private static string connectionString = ConfigurationManager.AppSettings.Get("connection");
        private static string queueName = ConfigurationManager.AppSettings.Get("queue");
        private static ServiceBusClient client;
        private static ServiceBusProcessor processor;

        private static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");

            await args.CompleteMessageAsync(args.Message);
        }

        private static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private static async Task Main()
        {
            client = new ServiceBusClient(connectionString);

            processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            try
            {
                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;

                await processor.StartProcessingAsync();

                // Stop and terminate
                Console.ReadKey();

                await processor.StopProcessingAsync();
            }
            finally
            {
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}