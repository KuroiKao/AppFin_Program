using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AppFin_Program.Services
{
    public class ReportService
    {
        private readonly FinAppDataBaseContext _dbContext;
        private readonly UserSessionService _userSessionService;

        public ReportService(FinAppDataBaseContext dbContext, UserSessionService userSessionService)
        {
            _dbContext = dbContext;
            _userSessionService = userSessionService;
        }
        public async Task<List<Report>> GetReportsByUserIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.", nameof(userId));
            }

            return await _dbContext.Reports
                .Where(r => r.UserId == userId)
                .Select(r => new Report
                {
                    Id = r.Id,
                    CreatedAt = r.CreatedAt.DateTime,
                    FilePath = r.FilePath
                })
                .ToListAsync();
        }
        public async Task DeleteReportAsync(int reportId)
        {
            var report = await _dbContext.Reports.FindAsync(reportId);
            if (report != null)
            {
                _dbContext.Reports.Remove(report);
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task SaveReportPathAsync(string filePath)
        {
            try
            {
                var userId = _userSessionService.GetCurrentUserId();

                var report = new Report
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    FilePath = filePath
                };

                await _dbContext.Reports.AddAsync(report);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при сохранении отчета: {ex.Message}", ex);
            }
        }
        public async Task<string> CreatePdfReportAsync(List<Transaction> transactions)
        {
            try
            {
                var reportDirectory = GetReportDirectory();
                var fileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(reportDirectory, fileName);

                var document = new PdfDocument
                {
                    Info = { Title = "Отчет" }
                };

                var page = document.AddPage();
                page.Size = PageSize.A4;
                var graphics = XGraphics.FromPdfPage(page);


                var font = new XFont("Times New Roman", 16);
                graphics.DrawString("Отчет о транзакциях", font, XBrushes.Black,
                    new XRect(20, 20, page.Width, 50), XStringFormats.TopLeft);

                var fontHeader = new XFont("Times New Roman", 12, XFontStyleEx.Bold);
                var fontBody = new XFont("Times New Roman", 10);

                double columnNumb = 20;
                double columnType = 40;
                double columnCategory = 150;
                double columnAmount = 50;
                double columnDate = 90;

                double startX = 20;
                double startY = 70;
                graphics.DrawString("№", fontHeader, XBrushes.Black, startX, startY);
                graphics.DrawString("Тип", fontHeader, XBrushes.Black, startX + columnNumb, startY);
                graphics.DrawString("Категория", fontHeader, XBrushes.Black, startX + columnNumb + columnType, startY);
                graphics.DrawString("Сумма", fontHeader, XBrushes.Black, startX + columnNumb + columnType + columnCategory, startY);
                graphics.DrawString("Дата", fontHeader, XBrushes.Black, startX + columnNumb + columnType + columnCategory + columnAmount, startY);

                startY += 15;

                int i = 1;
                foreach (var transaction in transactions)
                {
                    graphics.DrawString(i.ToString(), fontBody, XBrushes.Black, startX, startY);
                    graphics.DrawString(transaction.TransactionCategories.TransactionType.Name, fontBody, XBrushes.Black, startX + columnNumb, startY);
                    graphics.DrawString(transaction.TransactionCategories.Category.Name, fontBody, XBrushes.Black, startX + columnNumb + columnType, startY);
                    graphics.DrawString(transaction.Amount.ToString("F2"), fontBody, XBrushes.Black, startX + columnNumb + columnType + columnCategory, startY);
                    graphics.DrawString(transaction.TransactionDate.ToString("dd.MM.yyyy"), fontBody, XBrushes.Black, startX + columnNumb + columnType + columnCategory + columnAmount, startY);

                    startY += 20;

                    if (startY > page.Height - 50)
                    {
                        page = document.AddPage();
                        graphics = XGraphics.FromPdfPage(page);
                        startY = 50;

                        graphics.DrawString("№", fontHeader, XBrushes.Black, startX, startY);
                        graphics.DrawString("Тип", fontHeader, XBrushes.Black, startX + columnNumb, startY);
                        graphics.DrawString("Категория", fontHeader, XBrushes.Black, startX + columnNumb + columnType, startY);
                        graphics.DrawString("Сумма", fontHeader, XBrushes.Black, startX + columnNumb + columnType + columnCategory, startY);
                        graphics.DrawString("Дата", fontHeader, XBrushes.Black, startX + columnNumb + columnType + columnCategory + columnAmount, startY);

                        startY += 15;
                    }
                    i++;
                }

                await Task.Run(() => document.Save(filePath));
                await SaveReportPathAsync(filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при создании отчета: {ex.Message}", ex);
            }
        }
        private static string GetReportDirectory()
        {
            var appName = Assembly.GetExecutingAssembly().GetName().Name;
            var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), appName!, "Reports");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }
        //private static string WrapText(string text, int maxWidth)
        //{
        //    var result = new StringBuilder();
        //    var words = text.Split(' ');

        //    int currentLineWidth = 0;
        //    foreach (var word in words)
        //    {
        //        if (currentLineWidth + word.Length > maxWidth)
        //        {
        //            result.AppendLine();
        //            currentLineWidth = 0;
        //        }

        //        result.Append(word + " ");
        //        currentLineWidth += word.Length + 1;
        //    }

        //    return result.ToString();
        //}
    }
}
