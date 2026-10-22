using GusakovUP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GusakovUP.Views
{
    public partial class ChartWindow : Window
    {
        public ChartWindow()
        {
            InitializeComponent();
            this.Loaded += ChartWindow_Loaded;
        }

        private void ChartWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Получаем список переданных задач из MainViewModel
            var tasks = this.Tag as List<Tasks>;
            if (tasks == null || tasks.Count == 0) return;

            // 1. Считаем количество по каждому статусу
            double todo = tasks.Count(t => t.StatusId == 1);
            double inProgress = tasks.Count(t => t.StatusId == 2);
            double testing = tasks.Count(t => t.StatusId == 3);
            double done = tasks.Count(t => t.StatusId == 4);
            double total = todo + inProgress + testing + done;

            if (total == 0) return;

            // 2. Массив данных для секторов и фирменные цвета из вашего брендбука!
            double[] values = { todo, inProgress, testing, done };
            string[] colors = { "#0198AF", "#DD4B42", "#4DC6C5", "#233E76" };

            double startAngle = 0;
            double radius = 120;
            Point center = new Point(150, 150); // Центр холста 300х300

            ChartCanvas.Children.Clear();

            // Если все задачи в одном статусе — рисуем простой сплошной круг
            if (values.Any(v => v == total))
            {
                int index = Array.FindIndex(values, v => v == total);
                Ellipse circle = new Ellipse { Width = radius * 2, Height = radius * 2, Fill = (Brush)new BrushConverter().ConvertFromString(colors[index]) };
                Canvas.SetLeft(circle, center.X - radius);
                Canvas.SetTop(circle, center.Y - radius);
                ChartCanvas.Children.Add(circle);
                return;
            }

            // 3. Строим сектора круга математически через ArcSegment
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == 0) continue;

                double sweepAngle = (values[i] / total) * 360;
                double endAngle = startAngle + sweepAngle;

                // Перевод углов в радианы для вычисления координат
                double startRad = (Math.PI / 180) * (startAngle - 90);
                double endRad = (Math.PI / 180) * (endAngle - 90);

                Point pStart = new Point(center.X + radius * Math.Cos(startRad), center.Y + radius * Math.Sin(startRad));
                Point pEnd = new Point(center.X + radius * Math.Cos(endRad), center.Y + radius * Math.Sin(endRad));

                // Создание геометрии сектора
                PathGeometry geom = new PathGeometry();
                PathFigure fig = new PathFigure { StartPoint = center, IsClosed = true };
                fig.Segments.Add(new LineSegment(pStart, true));
                fig.Segments.Add(new ArcSegment(pEnd, new Size(radius, radius), 0, sweepAngle > 180, SweepDirection.Clockwise, true));
                geom.Figures.Add(fig);

                // Отрисовка фигуры на холсте
                Path sectorPath = new Path
                {
                    Data = geom,
                    Fill = (Brush)new BrushConverter().ConvertFromString(colors[i]),
                    ToolTip = $"Задач: {values[i]} ({Math.Round((values[i] / total) * 100, 1)}%)"
                };

                ChartCanvas.Children.Add(sectorPath);
                startAngle = endAngle;
            }
        }
    }
}
