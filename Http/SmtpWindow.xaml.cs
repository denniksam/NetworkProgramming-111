using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
using System.Windows.Shapes;

namespace Http
{
    /// <summary>
    /// Interaction logic for SmtpWindow.xaml
    /// </summary>
    public partial class SmtpWindow : Window
    {
        private dynamic? emailConfig;

        public SmtpWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /* Открываем файл конфигурации и пытаемся извлечь данные */
            String configFilename = "emailconfig.json";
            try
            {
                // для универсальности используем динамический тип для данных
                emailConfig = JsonSerializer.Deserialize<dynamic>(
                    System.IO.File.ReadAllText(configFilename)
                );
            }
            catch(System.IO.FileNotFoundException)
            {
                MessageBox.Show($"Не найден файл конфигурации '{configFilename}'");
                this.Close();
            }
            catch(JsonException ex)
            {
                MessageBox.Show($"Ошибка преобразования конфигурации '{ex.Message}'");
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка обработки конфигурации '{ex.Message}'");
                this.Close();
            }
            if( emailConfig is null )
            {
                MessageBox.Show("Ошибка получения конфигурации");
                this.Close();
            }
        }

        private SmtpClient GetSmtpClient()
        {
            if (emailConfig is null) { return null!; }
            /* Динамические объекты позволяют разыменовывать свои поля как
             * emailConfig.GetProperty("smtp").GetProperty("gmail").GetProperty("host").GetString();
             */
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");

            String host = gmail.GetProperty("host").GetString()!;
            int port = gmail.GetProperty("port").GetInt32();
            String mailbox = gmail.GetProperty("email").GetString()!;
            String password = gmail.GetProperty("password").GetString()!;
            bool ssl = gmail.GetProperty("ssl").GetBoolean();

            return new(host)
            {
                Port = port,
                EnableSsl = ssl,
                Credentials = new NetworkCredential(mailbox, password)
            };
        }

        private void SendTestButton_Click(object sender, RoutedEventArgs e)
        {
            using SmtpClient smtpClient = GetSmtpClient();
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");
            String mailbox = gmail.GetProperty("email").GetString()!;
            try
            {
                smtpClient.Send(
                    from: mailbox,
                    recipients: "denniksam@gmail.com",
                    subject: "Test Message",
                    body: "Test message from SmtpWindow");

                MessageBox.Show("Sent OK");
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Sent error '{ex.Message}'");
            }

        }

        private void SendHtmlButton_Click(object sender, RoutedEventArgs e)
        {
            using SmtpClient smtpClient = GetSmtpClient();
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");
            String mailbox = gmail.GetProperty("email").GetString()!;
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(mailbox),
                Body = "<u>Test</u> <i>message</i> from <b style='color:green'>SmtpWindow</b>",
                IsBodyHtml = true,
                Subject = "Test Message"
            };
            mailMessage.To.Add(new MailAddress("denniksam@gmail.com"));

            try
            {
                smtpClient.Send(mailMessage);
                MessageBox.Show("Sent OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sent error '{ex.Message}'");
            }
        }
    }
}
/* Д.З. Реализовать генератор кодов - случайных N-символьных строк,
 * состоящих из цифр и малых букв, не похожих на цифры (без о, l, z)
 * Использовать N=6 и отправить на почту письмо со случайным кодом
 * (добавить кнопку на SmtpWindow)
 */