using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Roost.Kitchen.App.Models;

namespace Roost.Kitchen.App.Services
{
    public class OrderRetriever
    {
        private readonly string _connection;

        public OrderRetriever(string connection)
        {
            _connection = connection;
        }

        public List<Order> GetIncomingOrders()
        {
            var client = new ServiceBusClient(_connection);

            var reciever = client.CreateReceiver("orders");

            var messages = reciever.PeekMessagesAsync(10).Result;

            return messages.Select(GetOrderFromMessage).ToList();
        }

        public Order GetOrderFromMessage(ServiceBusReceivedMessage message)
        {
            var order = JsonConvert.DeserializeObject<Order>(message.Body.ToString());
            order.OrderNumber = message.SequenceNumber;
            order.OrderDateTime = message.EnqueuedTime.ToLocalTime().DateTime;

            return order;
        }
    }
}
