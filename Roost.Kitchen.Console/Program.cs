using Roost.Kitchen.App.Services;

var retriever = new OrderRetriever("CONNECTION_STRING");

await retriever.MonitorIncomingOrders();