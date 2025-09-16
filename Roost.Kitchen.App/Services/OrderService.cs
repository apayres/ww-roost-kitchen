using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Roost.Kitchen.App.Models;

namespace Roost.Kitchen.App.Services
{
    public class OrderService
    {
        private readonly string _connection;

        public OrderService(string connection)
        {
            _connection = connection;
        }

        public async Task<List<Order>> GetIncomingOrders()
        {
            var receiver = GetReceiver();

            var message = await receiver.ReceiveMessageAsync();
            if(message == null)
            {
                return new List<Order>();
            }

            var order = GetOrderFromMessage(message);
            if (order == null)
            {
                return new List<Order>();
            }

            return new List<Order> { order };
        }

        public async Task CompleteOrder(Order order)
        {
            var receiver = GetReceiver();
            await receiver.CompleteMessageAsync(order.OriginalMessage);
        }

        private static Order GetOrderFromMessage(ServiceBusReceivedMessage message)
        {
            var order = JsonConvert.DeserializeObject<Order>(message.Body.ToString());
            if (order == null)
            {
                return null;
            }

            order.OrderNumber = message.SequenceNumber;
            order.OrderDateTime = message.EnqueuedTime.ToLocalTime().DateTime;
            order.OriginalMessage = message;
            return order;
        }

        private ServiceBusReceiver GetReceiver()
        {
            var client = new ServiceBusClient(_connection);
            var receiver = client.CreateReceiver("orders");
            return receiver;
        }
    }
}
