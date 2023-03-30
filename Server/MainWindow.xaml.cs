using System;
using System.Collections.Generic;
using System.Linq;
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

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket? listenSocket;   // слушающий сокет - постоянно активный при вкл сервере
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            IPEndPoint endpoint;
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
                MessageBox.Show("Check start network parameters");
                return;
            }
            listenSocket = new(               // создаем слушающий сокет
                AddressFamily.InterNetwork,   // адресация IPv4
                SocketType.Stream,            // Двусторонний сокет (и читать, и писать)
                ProtocolType.Tcp);            // Протокол сокета - ТСР
            
            // постоянно активный слушающий сокет заблокирует UI если его не
            // отделить в свой поток
            new Thread(StartServerMethod).Start(endpoint);
        }
        private void StartServerMethod(object? param)
        {
            if (listenSocket is null) return;
            IPEndPoint? endpoint = param as IPEndPoint;
            if (endpoint is null) return;

            try
            {
                listenSocket.Bind(endpoint);   // "привязываем" сервер к endpoint
                listenSocket.Listen(100);      // стартуем прослушку, разрешаем очередь из 100 сообщений
                Dispatcher.Invoke(() =>        // Добавляем  к логам
                    serverLogs.Text +=         // информацию о старте сервера. 
                        "Server started\n");   //
                byte[] buf = new byte[1024];   // буфер приема данных
                while (true)                   // бесконечный цикл прослушки порта
                {                              // 
                    Socket socket =            // Ожидание подключения и создание 
                        listenSocket.Accept(); // обменного сокета с клиентом
                    // Начинаем прием данных
                    StringBuilder sb = new();
                    do
                    {
                        int n =                      // сохраняем "порцию" данных в буфер
                            socket.Receive(buf);     // и получаем кол-во переданных байт (n) 
                        sb.Append(                   // Переводим полученные байты в 
                            Encoding.UTF8            // строку согласно кодировке UTF8
                            .GetString(buf, 0, n));  // и накапливаем в StringBuilder
                    } while (socket.Available > 0);  // Пока есть данные в сокете
                    
                    String str = sb.ToString();      // собираем все фрагменты в одну строку
                    Dispatcher.Invoke(() =>          // Добавляем полученные данные к логам
                        serverLogs.Text +=           // сервера. Используем Dispatcher
                            str + "\n");             // для доступа к UI

                    // Отправляем клиенту ответ - отчет о получении сообщения
                    str = "Received at " +           // В обратном порядке - 
                        DateTime.Now;                // сначала строка
                    socket.Send(                     // затем переводим в байты
                        Encoding.UTF8                // по заданной кодировке
                        .GetBytes(str));             // и отправляем в сокет

                    
                    socket.Shutdown(                 // Закрываем соединение - 
                        SocketShutdown.Both);        // отключаем сокет c уведомлением клиента
                    socket.Dispose();                // Освобождаем ресурс
                }                
            }
            catch(Exception ex) 
            {
                Dispatcher.Invoke(() =>              // Логируем исключение
                    serverLogs.Text +=               // и уведомляем об остановке
                    "Server stopped "                // сервера
                    + ex.Message + "\n");            // 
            }
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            // Остановить бесконечный цикл можно только выбросом исключения
            listenSocket?.Close();
        }
    }
}
