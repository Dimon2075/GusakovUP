using System;
using System.Linq;
using System.Windows;
using GusakovUP.Models;

namespace GusakovUP.Views
{
    public partial class AddTaskWindow : Window
    {
        private readonly Users _currentAdmin;

        public AddTaskWindow(Users admin)
        {
            InitializeComponent();
            _currentAdmin = admin;
            LoadProjectsToComboBox();
        }

        // Подгружаем существующие проекты для выбора администратором
        private void LoadProjectsToComboBox()
        {
            try
            {
                using (UpEntities db = new UpEntities())
                {
                    var projects = db.Projects.Where(p => p.IsDeleted == false).ToList();
                    CboProjects.ItemsSource = projects;
                    if (projects.Count > 0) CboProjects.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки списка проектов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtTitle.Text.Trim();
            Projects selectedProject = CboProjects.SelectedItem as Projects;

            // Проверка на формат и обязательность заполнения (Пункт 8 ТЗ)
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Поле 'Название задачи' обязательно для заполнения!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedProject == null)
            {
                MessageBox.Show("Необходимо выбрать проект для привязки задачи!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (UpEntities db = new UpEntities())
                {
                    // Создаем объект Tasks во множественном числе по вашей EDMX схеме
                    Tasks newTask = new Tasks
                    {
                        ProjectId = selectedProject.ProjectId,
                        Title = title,
                        Description = "Добавлено администратором через интерфейс",
                        StatusId = 1,   // По умолчанию статус "К выполнению"
                        PriorityId = 2, // По умолчанию приоритет "Средний"
                        CreatedByUserId = _currentAdmin.UserId,
                        DueDate = DpDueDate.SelectedDate,
                        IsDeleted = false
                    };

                    db.Tasks.Add(newTask);
                    db.SaveChanges();
                }

                MessageBox.Show("Задача успешно добавлена в систему!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; 
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении задачи в СУБД: " + ex.Message, "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
