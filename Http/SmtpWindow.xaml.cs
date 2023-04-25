using Http.Data;
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
        private readonly DataContext _dataContext;
        private Random rand = new();

        public SmtpWindow()
        {
            InitializeComponent();
            _dataContext = new();
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


            // проверяем, есть ли в БД пользователь с параметрами из полей окна
            NpUser? user = _dataContext.NpUsers.FirstOrDefault(
                    u => u.Name == UserNameTextbox.Text &&
                         u.Email == UserEmailTextbox.Text);
            if(user is null)
            {
                ConfirmDockPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ConfirmDockPanel.Visibility = Visibility.Visible;
                RegisterButton.IsEnabled = false;
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

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // заполняем шаблон письма данными
            String emailPattern = System.IO.File.ReadAllText("email.html");
            String confirmCode = RandomString(6);
            String emailBody = emailPattern
                .Replace("*name*", UserNameTextbox.Text)
                .Replace("*code*", confirmCode);

            // отправляем письмо с телом emailBody
            using SmtpClient smtpClient = GetSmtpClient();
            MailMessage mailMessage = new()
            {
                From = new MailAddress(emailConfig.GetProperty("smtp").GetProperty("gmail").GetProperty("email").GetString()),
                Body = emailBody,
                IsBodyHtml = true,
                Subject = "Code For Confirm"
            };
            mailMessage.To.Add(new MailAddress(UserEmailTextbox.Text));
            smtpClient.Send(mailMessage);

            // вносим запись в БД
            _dataContext.NpUsers.Add(new()
            {
                Id = Guid.NewGuid(),
                Name = UserNameTextbox.Text,
                Email = UserEmailTextbox.Text,
                ConfirmCode = confirmCode
            });
            _dataContext.SaveChanges();

            // отображаем поле для ввода кода
            ConfirmDockPanel.Visibility = Visibility.Visible;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            /* Д.З. Проверить код, введенный пользователем:
             * запросить в БД пользователя с данным именем и данной почтой
             * проверить код в поле ввода и код в БД
             * Если они совпадают, то "сбросить" код в БД - записать NULL
             * При старте приложения (Window_Loaded) добавить проверку
             * на то, что код равен NULL
             * Вывести сообщение о результате проверки (Подтверждено/или нет)
             */
        }
        private String RandomString(int l)
        {
            if (l <= 0) return String.Empty;
            string result = "";
            for (int i = 0; i < l; i++)
            {
                char nextC = (char)rand.Next('a', 'z');     // от a до z не включительно
                int nextI = rand.Next(0, 10);
                if (rand.Next(0, 2) == 0) result += nextI;  // следующий символ цифра или буква,
                                                            // 50/50
                else
                {
                    while (nextC == 'o' || nextC == 'l' || nextC == 'z')
                    {
                        nextC = (char)rand.Next('a', 'z');
                    }
                    result += nextC;
                }
            }
            return result;
        }
    }
}
/* Д.З. Реализовать генератор кодов - случайных N-символьных строк,
 * состоящих из цифр и малых букв, не похожих на цифры (без о, l, z)
 * Использовать N=6 и отправить на почту письмо со случайным кодом
 * (добавить кнопку на SmtpWindow)
 */