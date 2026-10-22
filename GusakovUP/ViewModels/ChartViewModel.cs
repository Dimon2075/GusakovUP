using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using GusakovUP.Models;

namespace GusakovUP.ViewModels
{
    public class ChartViewModel : BaseViewModel
    {
        // Используем стандартный список серий, который LiveCharts2 гарантированно прочитает в рантайме
        public List<ISeries> PieSeries { get; set; }

        public ChartViewModel(List<Tasks> tasks)
        {
            // Считаем точное количество задач на основе переданного списка из БД
            double todoCount = tasks.Count(t => t.StatusId == 1);
            double inProgressCount = tasks.Count(t => t.StatusId == 2);
            double testingCount = tasks.Count(t => t.StatusId == 3);
            double doneCount = tasks.Count(t => t.StatusId == 4);

            // Создаем коллекцию серий строго в формате PieSeries<double>
            PieSeries = new List<ISeries>
            {
                new PieSeries<double> { Values = new double[] { todoCount }, Name = "К выполнению" },
                new PieSeries<double> { Values = new double[] { inProgressCount }, Name = "В работе" },
                new PieSeries<double> { Values = new double[] { testingCount }, Name = "Тестирование" },
                new PieSeries<double> { Values = new double[] { doneCount }, Name = "Выполнено" }
            };
        }
    }
}
