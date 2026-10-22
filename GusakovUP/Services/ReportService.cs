using GusakovUP.Models;    
using iTextSharp.text;     
using iTextSharp.text.pdf;
using OfficeOpenXml;      
using System;
using System.Collections.Generic;
using System.IO;
using Xceed.Words.NET;    

namespace GusakovUP.Services
{
    public static class ReportService
    {
        // 1. Экспорт таблицы в MS Excel (.xlsx)
        public static void ExportToExcel(IEnumerable<Tasks> tasks, string filePath)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Отчет по задачам");

                // Заголовки столбцов
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Название задачи";
                worksheet.Cells[1, 3].Value = "Проект";
                worksheet.Cells[1, 4].Value = "Срок выполнения";

                using (ExcelRange range = worksheet.Cells["A1:D1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(35, 62, 118));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                int row = 2;
                foreach (Tasks task in tasks)
                {
                    worksheet.Cells[row, 1].Value = task.TaskId;
                    worksheet.Cells[row, 2].Value = task.Title;
                    worksheet.Cells[row, 3].Value = task.Projects != null ? task.Projects.Name : "Не указан";
                    worksheet.Cells[row, 4].Value = task.DueDate != null ? task.DueDate.Value.ToString("dd.MM.yyyy") : "-";
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                package.SaveAs(new FileInfo(filePath));
            }
        }

        // 2. Экспорт отчета в MS Word (.docx)
        public static void ExportToWord(IEnumerable<Tasks> tasks, string filePath)
        {
            using (Xceed.Words.NET.DocX document = Xceed.Words.NET.DocX.Create(filePath))
            {
                Xceed.Document.NET.Formatting titleFormat = new Xceed.Document.NET.Formatting();
                titleFormat.FontFamily = new Xceed.Document.NET.Font("Arial");
                titleFormat.Size = 16;
                titleFormat.Bold = true;

                document.InsertParagraph("ОТЧЕТ ПО УПРАВЛЕНИЮ КОМАНДНЫМИ ЗАДАЧАМИ", false, titleFormat).Alignment = Xceed.Document.NET.Alignment.center;
                document.InsertParagraph("Сформировано из системы автоматизации: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "\n").Alignment = Xceed.Document.NET.Alignment.right;

                Xceed.Document.NET.Table table = document.AddTable(1, 4);
                table.Alignment = Xceed.Document.NET.Alignment.center;

                table.Rows[0].Cells[0].Paragraphs[0].Append("ID").Bold();
                table.Rows[0].Cells[1].Paragraphs[0].Append("Задача").Bold();
                table.Rows[0].Cells[2].Paragraphs[0].Append("Проект").Bold();
                table.Rows[0].Cells[3].Paragraphs[0].Append("Срок сдачи").Bold();

                foreach (Tasks task in tasks)
                {
                    Xceed.Document.NET.Row row = table.InsertRow();
                    row.Cells[0].Paragraphs[0].Append(task.TaskId.ToString());
                    row.Cells[1].Paragraphs[0].Append(task.Title);
                    row.Cells[2].Paragraphs[0].Append(task.Projects != null ? task.Projects.Name : "-");
                    row.Cells[3].Paragraphs[0].Append(task.DueDate != null ? task.DueDate.Value.ToString("dd.MM.yyyy") : "-");
                }

                document.InsertTable(table);
                document.Save();
            }
        }


        // 3. Экспорт в PDF (.pdf) с гарантированной поддержкой кириллицы
        public static void ExportToPdf(IEnumerable<Tasks> tasks, string filePath)
        {
            Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                PdfWriter.GetInstance(doc, fs);
                doc.Open();

                // Подключаем стандартный системный шрифт Arial, чтобы русский текст не превратился в пустые квадраты
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                Font fontTitle = new Font(bf, 16, Font.BOLD);
                Font fontTable = new Font(bf, 10, Font.NORMAL);

                doc.Add(new Paragraph("Выгрузка текущих командных задач и проектов", fontTitle));
                doc.Add(new Paragraph("Дата генерации: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "\n\n", fontTable));

                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 10f, 50f, 25f, 15f });

                // Заголовки таблицы
                table.AddCell(new PdfPCell(new Phrase("ID", fontTable)));
                table.AddCell(new PdfPCell(new Phrase("Задача", fontTable)));
                table.AddCell(new PdfPCell(new Phrase("Проект", fontTable)));
                table.AddCell(new PdfPCell(new Phrase("Дедлайн", fontTable)));

                // Перенос строк из БД в PDF документ
                foreach (Tasks task in tasks)
                {
                    table.AddCell(new PdfPCell(new Phrase(task.TaskId.ToString(), fontTable)));
                    table.AddCell(new PdfPCell(new Phrase(task.Title, fontTable)));
                    table.AddCell(new PdfPCell(new Phrase(task.Projects != null ? task.Projects.Name : "-", fontTable)));
                    table.AddCell(new PdfPCell(new Phrase(task.DueDate != null ? task.DueDate.Value.ToString("dd.MM.yyyy") : "-", fontTable)));
                }

                doc.Add(table);
                doc.Close();
            }
        }
    }
}
