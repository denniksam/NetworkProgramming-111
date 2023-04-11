using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Http
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient httpClient;

        public MainWindow()
        {
            InitializeComponent();
            httpClient = new HttpClient();
        }

        private async void get1Button_Click(object sender, RoutedEventArgs e)
        {
            String result = await httpClient.GetStringAsync(url1TextBox.Text);
            resultTextBlock.Text = result;
        }

        private async void get2Button_Click(object sender, RoutedEventArgs e)
        {
            HttpRequestMessage request = new(   // Более полная форма запроса
                HttpMethod.Get,                 // Метод
                url2TextBox.Text);              // URL

            HttpResponseMessage response =            // Более полная форма
                await httpClient.SendAsync(request);  // ответа

            resultTextBlock.Text = $"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}\n";
            
        }
    }
}
/* Современный подход к взаимодействию по НТТР - HttpClient
 * Объект этого класса есть смысл делать единственным и использовать
 * для разных запросов.
 * Выполнения запросов асинхронны, значит нужно применять соответствующие технологии
 * (async / await / ContinueWith)
 * httpClient.GetStringAsync - возвращает только тело
 */
