using GusakovUP.Models;
using GusakovUP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GusakovUP.Views;

namespace GusakovUP.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username = "";
        private string _captchaInput = "";
        private string _currentCaptchaText = "";
        private ImageSource _captchaImage; // Убран знак ?
        private bool _isCaptchaVisible;
        private int _failedAttempts;

        public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }
        public string CaptchaInput { get => _captchaInput; set { _captchaInput = value; OnPropertyChanged(); } }
        public ImageSource CaptchaImage { get => _captchaImage; set { _captchaImage = value; OnPropertyChanged(); } } // Убран знак ?
        public bool IsCaptchaVisible { get => _isCaptchaVisible; set { _isCaptchaVisible = value; OnPropertyChanged(); } }

        public ICommand LoginCommand { get; }
        public ICommand RefreshCaptchaCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            RefreshCaptchaCommand = new RelayCommand(_ => LoadCaptcha());
        }

        private void LoadCaptcha()
        {
            var captcha = CaptchaService.GenerateCaptcha(140, 45);
            _currentCaptchaText = captcha.Text;
            CaptchaImage = captcha.Image;
        }

        private void ExecuteLogin(object passwordParam)
        {
            // 1. Приводим параметр к типу PasswordBox
            System.Windows.Controls.PasswordBox passwordBox = passwordParam as System.Windows.Controls.PasswordBox;

            // 2. Достаем чистый пароль из элемента управления
            string password = passwordBox != null ? passwordBox.Password : "";

            // 3. Проверяем поля на заполненность
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните поля!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Здесь продолжается ваш стандартный код авторизации:
            using (UpEntities db = new UpEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == Username && u.IsActive == true && u.IsDeleted == false);

                if (user != null && Services.AuthService.VerifyPassword(password, user.PasswordHash))
                {
                    _failedAttempts = 0;
                    IsCaptchaVisible = false;

                    // Открываем главное окно
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.DataContext = new MainViewModel(user, user.Role == "Administrator");
                    mainWindow.Show();

                    // Закрываем окно авторизации
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is Views.LoginWindow)
                        {
                            window.Close();
                            break;
                        }
                    }
                }
                else
                {
                    _failedAttempts++;
                    MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                    if (_failedAttempts >= 3)
                    {
                        IsCaptchaVisible = true;
                        LoadCaptcha();
                    }
                }
            }
        }
    }
}
