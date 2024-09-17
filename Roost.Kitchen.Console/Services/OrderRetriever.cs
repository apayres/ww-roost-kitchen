using Azure.Messaging.ServiceBus;

namespace Roost.Kitchen.App.Services
{
    public class OrderRetriever
    {
        private readonly string _connection;

        public OrderRetriever(string connection)
        {
            _connection = connection;
        }

        public async Task MonitorIncomingOrders()
        {
            var client = new ServiceBusClient(_connection);

            var options = new ServiceBusProcessorOptions()
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 10
            };

            await using ServiceBusProcessor processor = client.CreateProcessor("orders", options);
            processor.ProcessMessageAsync += MessageHander;
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync();
            Console.ReadKey();

            await processor.StopProcessingAsync();
        }

        private async Task MessageHander(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Recieved: Order #{args.Message.SequenceNumber}");
        }

        private Task ErrorHandler(ProcessErrorEventArgs args) {
            // DO SOMETHING ON ERROR
            return Task.CompletedTask;
        }
    }
}
