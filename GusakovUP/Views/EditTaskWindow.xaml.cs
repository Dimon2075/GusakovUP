using System;
using System.Linq;
using System.Windows;
using GusakovUP.Models;

namespace GusakovUP.Views
{
    public partial class EditTaskWindow : Window
    {
        private readonly Tasks _taskToEdit;

        public EditTaskWindow(Tasks task)
        {
            InitializeComponent();
            _taskToEdit = task;

            LoadProjects();
            PopulateData();
        }

        private void LoadProjects()
        {
            try
            {
                using (UpEntities db = new UpEntities())
                {
                    CboProjects.ItemsSource = db.Projects.Where(p => p.IsDeleted == false).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки проектов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateData()
        {
            TxtTitle.Text = _taskToEdit.Title;
            DpDueDate.SelectedDate = _taskToEdit.DueDate;

            // Заполнение комбобоксов текущими значениями
            if (CboProjects.ItemsSource is System.Collections.IList projects)
            {
                for (int i = 0; i < projects.Count; i++)
                {
                    if (((Projects)projects[i]).ProjectId == _taskToEdit.ProjectId)
                    {
                        CboProjects.SelectedIndex = i;
                        break;
                    }
                }
            }
            CboStatuses.SelectedIndex = _taskToEdit.StatusId - 1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtTitle.Text.Trim();
            Projects selectedProject = CboProjects.SelectedItem as Projects;

            if (string.IsNullOrWhiteSpace(title) || selectedProject == null)
            {
                MessageBox.Show("Заполните обязательные поля!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (UpEntities db = new UpEntities())
                {
                    var taskInDb = db.Tasks.Find(_taskToEdit.TaskId);
                    if (taskInDb != null)
                    {
                        taskInDb.Title = title;
                        taskInDb.ProjectId = selectedProject.ProjectId;
                        taskInDb.StatusId = CboStatuses.SelectedIndex + 1;
                        taskInDb.DueDate = DpDueDate.SelectedDate;

                        db.SaveChanges();
                    }
                }

                MessageBox.Show("Задача успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении задачи: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
