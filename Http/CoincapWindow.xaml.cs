using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Http
{
    /// <summary>
    /// Interaction logic for CoincapWindow.xaml
    /// </summary>
    public partial class CoincapWindow : Window
    {
        public ObservableCollection<Asset> Assets { get; set; }

        private HttpClient _httpClient = new();

        public CoincapWindow()
        {
            InitializeComponent();
            Assets = new();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAssets();
            DrawLine(10, 10, 100, 100);
        }

        private async void LoadAssets()
        {
            var assetsResponse = await _httpClient.GetFromJsonAsync<AssetsResponse>(
                "https://api.coincap.io/v2/assets");
            if (assetsResponse is null) 
            { 
                MessageBox.Show("JSON error"); 
                return;
            }
            Assets.Clear();
            foreach(Asset asset in assetsResponse.data)
            {
                Assets.Add(asset);
            }
        }

        private void DrawLine(double fromX, double fromY, double toX, double toY)
        {
            Line line = new()
            {
                X1 = fromX,
                Y1 = fromY,
                X2 = toX,
                Y2 = toY,
                Stroke = new SolidColorBrush(Colors.BlueViolet),
                StrokeThickness = 1
            };
            GraphCanvas.Children.Add(line);
        }
    }

    // ORM - используем инструмент https://json2csharp.com/
    public class Asset
    {
        public string id { get; set; }
        public string rank { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string supply { get; set; }
        public string maxSupply { get; set; }
        public string marketCapUsd { get; set; }
        public string volumeUsd24Hr { get; set; }
        public string priceUsd { get; set; }
        public string changePercent24Hr { get; set; }
        public string vwap24Hr { get; set; }
        public string explorer { get; set; }
    }

    public class AssetsResponse
    {
        public List<Asset> data { get; set; }
        public long timestamp { get; set; }
    }

}
