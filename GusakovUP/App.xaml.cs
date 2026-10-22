using System;
using System.Linq;
using System.Windows;
using GusakovUP.Models; // Проверьте, что папка с моделями EDMX называется именно так

namespace GusakovUP
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Автоматическое создание первого администратора через ADO.NET, если таблица пуста
            try
            {
                using (UpEntities db = new UpEntities())
                {
                    if (!db.Users.Any())
                    {
                        // Хэшируем пароль "admin123" с помощью нашего AuthService
                        byte[] secureHash = Services.AuthService.HashPassword("admin123");

                        Users admin = new Users
                        {
                            Username = "admin",
                            PasswordHash = secureHash,
                            FullName = "Администратор Системы",
                            Role = "Administrator",
                            IsActive = true,
                            IsDeleted = false
                        };

                        db.Users.Add(admin);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                // Если база данных ещё не создана или строка подключения неверна,
                // приложение не упадет при запуске, а выведет ошибку в отладку
                System.Diagnostics.Debug.WriteLine("Ошибка инициализации БД: " + ex.Message);
            }
        }

        public static void ChangeTheme(string themeName)
        {
            ResourceDictionary currentTheme = Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"));

            if (currentTheme != null)
                Current.Resources.MergedDictionaries.Remove(currentTheme);

            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"Styles/{themeName}Theme.xaml", UriKind.Relative)
            });
        }
    }
}
