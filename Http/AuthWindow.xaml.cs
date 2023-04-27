using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginTextBox.Focus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            String passwordHash = Hash(PasswordTextBox.Password);
            App.AuthUser = App.DataContext.NpUsers.FirstOrDefault(
                user =>
                    user.Login == LoginTextBox.Text
                    &&
                    user.Password == passwordHash
            );
            if(App.AuthUser is null)
            {
                MessageBox.Show("Вход отклонен");
            }
            else
            {
                this.Hide();
                new PortalWindow().ShowDialog();
                PasswordTextBox.Password = "";
                this.Show();
            }
        }

        private string Hash(string str)
        {
            using MD5 md5 = MD5.Create();
            return Convert.ToHexString(
                        md5.ComputeHash(
                            Encoding.UTF8.GetBytes(str)));
        }
    }
}
/* Аутентификация - подтверждение пользователя (проверка логина/пароля/...)
 * Авторизация - проверка прав доступа данного пользователя к данному ресурсу
 * 
 * Аутентификация. Особенность - в БД запрещено хранить пароли в открытом виде.
 * Один из простейших способов хранения - хеш образы пароля.
 * 
 * Хеши должны вычисляться при регистрации, мы используем стороннюю регистрацию
 * двух пользователей по известным хешам:
 * 
 * 202CB962AC59075B964B07152D234B70 (123)
 * CAF1A3DFB505FFED0D024130F58C5CFA (321)
 * 
 * INSERT INTO NpUsers VALUES ( UUID(), 'Content Moderator', 'denniksam@gmail.com', NULL, 'moder', '202CB962AC59075B964B07152D234B70' );
 * INSERT INTO NpUsers VALUES ( UUID(), 'Root Administrator', 'denniksam@gmail.com', NULL, 'admin', 'CAF1A3DFB505FFED0D024130F58C5CFA' );
 */
