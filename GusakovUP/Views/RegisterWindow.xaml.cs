using System;
using System.Linq;
using System.Windows;
using GusakovUP.Models;
using GusakovUP.Services;

namespace GusakovUP.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = TxtFullName.Text.Trim();
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;

            // Валидация полей (Пункт 4 ТЗ)
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля формы!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("Пароль должен содержать минимум 4 символа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (UpEntities db = new UpEntities())
                {
                    // Проверяем, нет ли уже пользователя с таким логином
                    if (db.Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует в системе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Хэшируем пароль в varbinary(64) через PBKDF2
                    byte[] secureHash = AuthService.HashPassword(password);

                    // Создаем объект
                    Users newUser = new Users
                    {
                        Username = username,
                        PasswordHash = secureHash,
                        FullName = fullName,
                        Role = "User", // Новые пользователи по умолчанию обычные сотрудники
                        IsActive = true,
                        IsDeleted = false
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();
                }

                MessageBox.Show("Регистрация успешно завершена! Теперь вы можете войти.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Возвращаемся на окно логина
                OpenLoginWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message, "Ошибка СУБД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackLink_Click(object sender, RoutedEventArgs e)
        {
            OpenLoginWindow();
        }

        private void OpenLoginWindow()
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
