using System;
using System.Linq;
using System.Windows;
using GusakovUP.Models;
using GusakovUP.Services;

namespace GusakovUP.Views
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly Users _currentUser;

        public ChangePasswordWindow(Users user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string oldPass = OldPasswordBox.Password;
            string newPass = NewPasswordBox.Password;
            string confirmPass = ConfirmPasswordBox.Password;

            // Валидация заполнения (Требование ТЗ)
            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirmPass))
            {
                MessageBox.Show("Заполните все поля формы!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass.Length < 4)
            {
                MessageBox.Show("Новый пароль должен содержать минимум 4 символа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Новый пароль и подтверждение не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (UpEntities db = new UpEntities())
                {
                    // Ищем пользователя в БД по его ID
                    var userInDb = db.Users.Find(_currentUser.UserId);
                    if (userInDb == null) return;

                    // Проверяем, совпадает ли старый пароль с хэшем в БД
                    if (!AuthService.VerifyPassword(oldPass, userInDb.PasswordHash))
                    {
                        MessageBox.Show("Неверно указан текущий пароль!", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Генерируем новый хэш PBKDF2 и сохраняем изменения
                    userInDb.PasswordHash = AuthService.HashPassword(newPass);
                    db.SaveChanges();
                }

                MessageBox.Show("Пароль успешно изменен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при смене пароля: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
