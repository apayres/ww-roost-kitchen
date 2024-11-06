using Roost.Kitchen.App.Models;
using Roost.Kitchen.App.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Roost.Kitchen.App
{
    public partial class MainWindow : Window
    {
        private readonly OrderRetriever _orderRetriever;
        
        public MainWindow()
        {
            _orderRetriever = new OrderRetriever(Properties.Settings.Default.AzureServiceBusConnectionString);
            
            InitializeComponent();

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);

            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            FillIncomingOrders(_orderRetriever.GetIncomingOrders());
        }
                
        private void FillIncomingOrders(List<Order> orders)
        {
            grdIncomingOrders.Children.Clear();
            grdIncomingOrders.RowDefinitions.Clear();

            foreach (var order in orders)
            {
                grdIncomingOrders.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(0, GridUnitType.Auto)
                });

                var lastRow = grdIncomingOrders.RowDefinitions.Count - 1;

                var orderControl = new TextBlock()
                {
                    Text = "Order #" + order.OrderNumber.ToString(),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(5),
                    Height = 25
                };

                grdIncomingOrders.Children.Add(orderControl);
                Grid.SetRow(orderControl, lastRow);
                Grid.SetColumn(orderControl, 0);

                var label = new Label()
                {
                    Content = order.OrderDateTime.ToString("MM/dd @ h:mm"),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(5),
                    Margin = new Thickness(0, 15, 0, 0),
                    FontSize = 10,
                    Foreground = Brushes.Gray
                };

                grdIncomingOrders.Children.Add(label);
                Grid.SetRow(label, lastRow);
                Grid.SetColumn(label, 0);

                var orderFillControl = new Button()
                {
                    Content = "Start",
                    Padding = new Thickness(5),
                    Margin = new Thickness(3),
                    Tag = order,
                    Style = (Style)FindResource("PrimaryButton")
                };

                orderFillControl.Click += new RoutedEventHandler(OrderFillControl_Click);

                grdIncomingOrders.Children.Add(orderFillControl);
                Grid.SetRow(orderFillControl, lastRow);
                Grid.SetColumn(orderFillControl, 1);

            }
        }

        private void OrderFillControl_Click(object sender, RoutedEventArgs e)
        {
            var order = (Order)((Button)sender).Tag;
            txtOrderNumber.Text = "#" + order.OrderNumber.ToString();
            lblName.Content = order.Name;
            btnOrderUp.IsEnabled = true;

            FillOrderGrid(order);
        }

        private void FillOrderGrid(Order order)
        {
            grdOrder.Children.Clear();
            grdOrder.RowDefinitions.Clear();

            foreach(var orderItem in order.orderItems)
            {
                grdOrder.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(0, GridUnitType.Auto)
                });

                var lastRow = grdOrder.RowDefinitions.Count - 1;
                                
                var imageSource = new BitmapImage(new Uri(orderItem.Item.Images[0].AbsoluteUri, UriKind.Absolute));

                var image = new Border()
                {
                    CornerRadius = new CornerRadius(300),
                    Background = new ImageBrush()
                    {
                        Stretch = Stretch.Fill,
                        ImageSource = imageSource,
                        AlignmentX = AlignmentX.Center,
                        AlignmentY = AlignmentY.Center
                    },
                    Height = 60,
                    Width = 60,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(5)
                };

                grdOrder.Children.Add(image);
                Grid.SetRow(image, lastRow);
                Grid.SetColumn(image, 0);

                var itemLabel = new Label()
                {
                    Content = orderItem.Item?.ItemName,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(5, 15, 5, 5),
                    FontSize = 14
                };

                grdOrder.Children.Add(itemLabel);
                Grid.SetRow(itemLabel, lastRow);
                Grid.SetColumn(itemLabel, 1);

                if (orderItem.Options != null && orderItem.Options.Any())
                {
                    var options = string.Join(", ", orderItem.Options.Select(x => $"{x.Name}: {x.Value}"));
                    var optionsLabel = new Label()
                    {
                        Content = options,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Padding = new Thickness(5),
                        Margin = new Thickness(0, 30, 0, 0),
                        FontSize = 10
                    };

                    grdOrder.Children.Add(optionsLabel);
                    Grid.SetRow(optionsLabel, lastRow);
                    Grid.SetColumn(optionsLabel, 1);
                }


                var qtyLabel = new Label()
                {
                    Content = orderItem.Quantity,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    Padding = new Thickness(5, 15, 15, 5),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };

                grdOrder.Children.Add(qtyLabel);
                Grid.SetRow(qtyLabel, lastRow);
                Grid.SetColumn(qtyLabel, 2);
            }

        }

        private void btnOrderUp_Click(object sender, RoutedEventArgs e)
        {
            grdOrder.Children.Clear();
            lblName.Content = "Customer";
            txtOrderNumber.Text = "";
        }
    }
}