using GusakovUP.ViewModels;
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
using System.Windows.Shapes;

namespace GusakovUP.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            this.DataContext = new LoginViewModel();
        }
        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            // Создаем и открываем окно регистрации
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();

            // Закрываем текущее окно авторизации
            this.Close();
        }

    }
}
