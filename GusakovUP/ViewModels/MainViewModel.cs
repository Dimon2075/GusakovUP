using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GusakovUP.Models;
using GusakovUP.Services;

namespace GusakovUP.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _searchText = "";
        private int _selectedStatusIndex = 0;
        private Projects _selectedProjectFilter; 
        private Tasks _selectedTask;            

        // Текущий авторизованный сотрудник (класс Users)
        public Users CurrentUser { get; private set; }
        public string UserRoleName => IsAdmin ? "Администратор" : "Пользователь";
        public bool IsAdmin { get; private set; }
        public bool IsAdminControlsVisible => IsAdmin;

        // Коллекции данных (классы во множественном числе)
        public ObservableCollection<Tasks> TasksList { get; set; }
        public ObservableCollection<Projects> ProjectsList { get; set; }
        public ICollectionView TasksView { get; set; }

        // Свойства для фильтрации и поиска
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; TasksView.Refresh(); OnPropertyChanged(); }
        }

        public int SelectedStatusIndex
        {
            get { return _selectedStatusIndex; }
            set { _selectedStatusIndex = value; TasksView.Refresh(); OnPropertyChanged(); }
        }

        public Projects SelectedProjectFilter
        {
            get { return _selectedProjectFilter; }
            set { _selectedProjectFilter = value; TasksView.Refresh(); OnPropertyChanged(); }
        }

        public Tasks SelectedTask
        {
            get { return _selectedTask; }
            set { _selectedTask = value; OnPropertyChanged(); }
        }

        // Команды управления
        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand ExportExcelCommand { get; }
        public ICommand ExportWordCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand EditTaskCommand { get; }

        private void ExecuteEditTask(object param)
        {
            var editWindow = new Views.EditTaskWindow(SelectedTask);
            editWindow.Owner = Application.Current.MainWindow;

            if (editWindow.ShowDialog() == true)
            {
                LoadData(); // Перезагружаем списки из БД
                TasksView.Refresh();
            }
        }


        public MainViewModel(Users authenticatedUser, bool isAdminRole)
        {
            EditTaskCommand = new RelayCommand(ExecuteEditTask, _ => SelectedTask != null);

            CurrentUser = authenticatedUser;
            IsAdmin = isAdminRole;

            TasksList = new ObservableCollection<Tasks>();
            ProjectsList = new ObservableCollection<Projects>();

            LoadData();

            TasksView = CollectionViewSource.GetDefaultView(TasksList);
            TasksView.Filter = FilterCriteria;

            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask, _ => SelectedTask != null);
            AddTaskCommand = new RelayCommand(ExecuteAddTask);

            ExportExcelCommand = new RelayCommand(_ => RunExport("excel"));
            ExportWordCommand = new RelayCommand(_ => RunExport("word"));
            ExportPdfCommand = new RelayCommand(_ => RunExport("pdf"));
        }


        private void LoadData()
        {
            try
            {
                using (UpEntities db = new UpEntities())
                {
                    // Загрузка проектов
                    var projects = db.Projects.Where(p => p.IsDeleted == false).ToList();
                    ProjectsList.Clear();
                    ProjectsList.Add(new Projects { ProjectId = 0, Name = "Все проекты" });
                    foreach (var p in projects)
                    {
                        ProjectsList.Add(p);
                    }

                    // Загрузка задач
                    var tasks = db.Tasks.Include(t => t.Projects).Where(t => t.IsDeleted == false).ToList();
                    TasksList.Clear();
                    foreach (var t in tasks)
                    {
                        TasksList.Add(t);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message, "Ошибка СУБД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool FilterCriteria(object obj)
        {
            Tasks task = obj as Tasks;
            if (task == null) return false;

            if (!string.IsNullOrWhiteSpace(SearchText) && task.Title.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            if (SelectedProjectFilter != null && SelectedProjectFilter.ProjectId != 0 && task.ProjectId != SelectedProjectFilter.ProjectId)
                return false;

            if (SelectedStatusIndex > 0 && task.StatusId != SelectedStatusIndex)
                return false;

            return true;
        }
        private void ExecuteAddTask(object param)
        {
            // Открываем диалоговое окно добавления задачи
            var addTaskWindow = new Views.AddTaskWindow(CurrentUser);
            addTaskWindow.Owner = Application.Current.MainWindow;

            // Если администратор успешно нажал "Сохранить", обновляем DataGrid на экране
            if (addTaskWindow.ShowDialog() == true)
            {
                LoadData(); // Перезагружает актуальные задачи из БД
                TasksView.Refresh();
            }
        }

        private void ExecuteDeleteTask(object param)
        {
            if (SelectedTask == null) return;

            var result = MessageBox.Show("Вы уверены, что хотите удалить задачу '" + SelectedTask.Title + "'?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (UpEntities db = new UpEntities())
                    {
                        var taskInDb = db.Tasks.Find(SelectedTask.TaskId);
                        if (taskInDb != null)
                        {
                            taskInDb.IsDeleted = true;
                            db.SaveChanges();
                        }
                    }
                    TasksList.Remove(SelectedTask);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось удалить задачу: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenChangePasswordDialog()
        {
            var changeWindow = new Views.ChangePasswordWindow(CurrentUser);
            changeWindow.Owner = Application.Current.MainWindow;
            changeWindow.ShowDialog();
        }

        private void RunExport(string format)
        {
            // Получаем из нашей TasksView только те задачи, которые сейчас отфильтрованы пользователем на экране
            var currentTasks = TasksView.Cast<GusakovUP.Models.Tasks>().ToList();

            if (currentTasks.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();

            if (format == "excel") { sfd.Filter = "Excel Списки (*.xlsx)|*.xlsx"; sfd.FileName = "Отчет_Задачи.xlsx"; }
            else if (format == "word") { sfd.Filter = "Документы Word (*.docx)|*.docx"; sfd.FileName = "Отчет_Задачи.docx"; }
            else if (format == "pdf") { sfd.Filter = "Документы PDF (*.pdf)|*.pdf"; sfd.FileName = "Отчет_Задачи.pdf"; }

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    if (format == "excel") ReportService.ExportToExcel(currentTasks, sfd.FileName);
                    else if (format == "word") ReportService.ExportToWord(currentTasks, sfd.FileName);
                    else if (format == "pdf") ReportService.ExportToPdf(currentTasks, sfd.FileName);

                    MessageBox.Show("Документ успешно сформирован и сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении отчета: " + ex.Message, "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
