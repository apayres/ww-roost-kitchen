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
        private readonly OrderService _orderService;
        private bool _isGettingIncomingOrders = false;
        private Order _activeOrder = null;
        
        public MainWindow()
        {
            _orderService = new OrderService(Properties.Settings.Default.AzureServiceBusConnectionString);
            
            InitializeComponent();

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);

            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);            
            dispatcherTimer.Start();
        }

        private async void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_isGettingIncomingOrders)
            {
                return;
            }

            _isGettingIncomingOrders = true;

            var orders = await _orderService.GetIncomingOrders();
            FillIncomingOrders(orders);
            
            _isGettingIncomingOrders = false;
        }
                
        private void FillIncomingOrders(List<Order> orders)
        {
            foreach (var order in orders)
            {
                grdIncomingOrders.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(0, GridUnitType.Auto),
                    Tag = order.OrderNumber.ToString()
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
            var button = (Button)sender;           

            var order = (Order)button.Tag;
            _activeOrder = order;

            txtOrderNumber.Text = "#" + order.OrderNumber.ToString();
            lblName.Content = order.Name;
            btnOrderUp.IsEnabled = true;
            btnOrderUp.Tag = order.OrderNumber.ToString();

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
                                
                var imageSource = string.IsNullOrWhiteSpace(orderItem.Item?.ImageUrl) ? null : new BitmapImage(new Uri(orderItem.Item.ImageUrl, UriKind.Absolute));

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

                var marginOffset = 30;
                if (orderItem.Options != null && orderItem.Options.Any())
                {
                    foreach(var option in orderItem.Options)
                    {
                        var optionsLabel = new Label()
                        {
                            Content = $"{option.Name}: {option.Value}",
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Padding = new Thickness(5),
                            Margin = new Thickness(0, marginOffset, 0, 0),
                            FontSize = 10
                        };

                        grdOrder.Children.Add(optionsLabel);
                        Grid.SetRow(optionsLabel, lastRow);
                        Grid.SetColumn(optionsLabel, 1);
                        marginOffset += 15;
                    }
                }

                if(orderItem.Recipe != null && orderItem.Recipe.Any())
                {
                    foreach (var ingredient in orderItem.Recipe)
                    {
                        var ingredientContent = $"{ingredient.ItemName}: {ingredient.Ratio}";

                        if(ingredient.UnitOfMeasure != null)
                        {
                            ingredientContent += $" ({ingredient.UnitOfMeasure.Name})";
                        }

                        var ingredientLabel = new Label()
                        {
                            Content = ingredientContent,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Padding = new Thickness(5),
                            Margin = new Thickness(0, marginOffset, 0, 0),
                            FontSize = 10
                        };

                        grdOrder.Children.Add(ingredientLabel);
                        Grid.SetRow(ingredientLabel, lastRow);
                        Grid.SetColumn(ingredientLabel, 1);
                        marginOffset += 15;
                    }
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

        private async void btnOrderUp_Click(object sender, RoutedEventArgs e)
        {
            if (_activeOrder != null)
            {
                await _orderService.CompleteOrder(_activeOrder);
            }

            var button = (Button)sender;

            for (var i = 0; i < grdIncomingOrders.RowDefinitions.Count(); i++)
            {
                var row = grdIncomingOrders.RowDefinitions[i];
                if(button.Tag == row.Tag)
                {
                    var children = grdIncomingOrders.Children.Cast<UIElement>().ToList();
                    foreach(var child in children.Where(x => Grid.GetRow(x) == i))
                    {
                        grdIncomingOrders.Children.Remove(child);
                    }

                    break;
                }
            }

            grdOrder.Children.Clear();
            lblName.Content = "Customer";
            txtOrderNumber.Text = "";
        }
    }
}