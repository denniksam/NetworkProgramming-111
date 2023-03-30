using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            IPEndPoint endpoint;  // копия - как у сервера
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
            Socket clientSocket = new(        // создаем сокет подключения
                AddressFamily.InterNetwork,   // адресация IPv4
                SocketType.Stream,            // Двусторонний сокет (и читать, и писать)
                ProtocolType.Tcp);            // Протокол сокета - ТСР
            try
            {
                clientSocket.Connect(endpoint);
                clientSocket.Send(
                    Encoding.UTF8.GetBytes(
                        messageTextBox.Text));

                /* Д.З. ! Реализовать остановку сервера (если он включен)
                 * при закрытии окна.
                 * Реализовать отображение статуса сервера (ON | OFF)
                 * *использовать разные цвета для разных статусов
                 * **скопировать в кодах клиента процесс получения данных от 
                 * сервера (стр. 80-90 сервера) [получение у клиента должно быть
                 * после отправки]
                 */

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Dispose();
            }
            catch(Exception ex)
            {
                chatLogs.Text += ex.Message + "\n";
            }

        }
    }
}
