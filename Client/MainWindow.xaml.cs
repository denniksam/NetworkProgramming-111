using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] buffer = new byte[1024];
        IPEndPoint? endpoint;
        DateTime lastSyncMoment;

        public MainWindow()
        {
            InitializeComponent();
            lastSyncMoment = DateTime.MinValue;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (endpoint is null)   // первое нажатие - определяем сервер
            {
                try
                {
                    IPAddress ip =               // На окне IP - это текст ("127.0.0.1")
                        IPAddress.Parse(         // Для его перевода в число используется
                            serverIp.Text);      // IPAddress.Parse
                    int port =                   // Аналогично - порт
                        Convert.ToInt32(         // парсим число из текста
                            serverPort.Text);    // 
                    endpoint =                   // endpoint - комбинация IP и порта
                        new(ip, port);           // 
                }
                catch
                {
                    MessageBox.Show("Check server network parameters");
                    return;
                }
            }

            ChatMessage chatMessage = new()
            {
                Author = authorTextBox.Text,
                Text = messageTextBox.Text,
                Moment = DateTime.Now
            };
            SendMessage(chatMessage);
        }

        private void SendMessage(ChatMessage chatMessage)
        {
            if (endpoint is null) return;

            Socket clientSocket = new(        // создаем сокет подключения
                AddressFamily.InterNetwork,   // адресация IPv4
                SocketType.Stream,            // Двусторонний сокет (и читать, и писать)
                ProtocolType.Tcp);            // Протокол сокета - ТСР
            try
            {
                clientSocket.Connect(endpoint);
                // --------------------- соединение установлено ----------------------
                // сервер начинает с приема данных, поэтому клиент начинает с посылки
                // формируем объект-запрос
                ClientRequest request = new()
                {
                    Action = "Message",
                    Author = chatMessage.Author,
                    Text = chatMessage.Text,
                    Moment = chatMessage.Moment
                };
                // преобразуем объект в JSON
                String json = JsonSerializer.Serialize(request,           // Для Юникода в JSON
                    new JsonSerializerOptions()
                    {                         // используются \uXXXX
                        Encoder = System.Text.Encodings.Web               // выражения. Чтобы 
                        .JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });  // был обычный текст - Encoder
                // отправляем на сервер
                clientSocket.Send(Encoding.UTF8.GetBytes(json));

                // после приема сервер отправляет подтверждение, клиент - получает
                MemoryStream stream = new();               // Другой способ получения
                do                                         // данных - собирать части
                {                                          // бинарного потока в 
                    int n = clientSocket.Receive(buffer);  // память.
                    stream.Write(buffer, 0, n);            // Затем создать строку
                } while (clientSocket.Available > 0);      // один раз пройдя
                String str = Encoding.UTF8.GetString(      // все полученные байты.
                    stream.ToArray());                     // 

                chatLogs.Text += str + "\n";

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Dispose();
            }
            catch (Exception ex)
            {
                chatLogs.Text += ex.Message + "\n";
            }
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверить есть ли новые сообщения
            if (endpoint is null) return;

            // новый запрос начинается с нового соединения
            Socket clientSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(endpoint);
                ClientRequest request = new()
                {
                    Action = "Get",
                    Author = authorTextBox.Text,
                    Moment = lastSyncMoment   // момент последней сверки сообщений
                };
                lastSyncMoment = DateTime.Now;   // обновляем момент последней сверки сообщений

                // преобразуем объект в JSON
                String json = JsonSerializer.Serialize(request,
                    new JsonSerializerOptions()
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                // отправляем на сервер
                clientSocket.Send(Encoding.UTF8.GetBytes(json));

                // получаем ответ
                MemoryStream stream = new();
                do
                {
                    int n = clientSocket.Receive(buffer);
                    stream.Write(buffer, 0, n);
                } while (clientSocket.Available > 0);
                String str = Encoding.UTF8.GetString(stream.ToArray());

                // Декодируем его из JSON
                var response = JsonSerializer.Deserialize<ServerResponse>(str);
                if (response is not null && response.Messages is not null)
                {
                    foreach (var message in response.Messages)
                    {
                        chatLogs.Text += $"{message.Moment.ToShortTimeString()} {message.Author}: {message.Text}\n";

                    }
                }
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Dispose();
            }
            catch (Exception ex)
            {
                chatLogs.Text += ex.Message + "\n";
            }
        }
    }
}
