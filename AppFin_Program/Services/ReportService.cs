using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Drawing;
using PdfSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using System.IO;
using System.Reflection;
using System.Text;

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
                    new XRect(0, 0, page.Width, 50), XStringFormats.TopCenter);

                int yPosition = 50;
                int i = 1;

                foreach (var transaction in transactions)
                {
                    var text = WrapText($"{i++}. {transaction.TransactionCategories.TransactionType.Name} " +
                        $"| {transaction.TransactionCategories.Category.Name} " +
                        $"| {transaction.Amount} " +
                        $"| {transaction.TransactionDate:dd.MM.yyyy}", 50);
                    var cleanedText = new string(text.Where(c => !char.IsControl(c) && c != '\uFFFD').ToArray());

                    graphics.DrawString(cleanedText, font, XBrushes.Black, new XRect(20, yPosition, page.Width - 40, 20), XStringFormats.TopLeft);
                    yPosition += 20;

                    if (yPosition > page.Height - 50)
                    {
                        page = document.AddPage();
                        graphics = XGraphics.FromPdfPage(page);
                        yPosition = 50;
                    }
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
        private static string WrapText(string text, int maxWidth)
        {
            var result = new StringBuilder();
            var words = text.Split(' ');

            int currentLineWidth = 0;
            foreach (var word in words)
            {
                if (currentLineWidth + word.Length > maxWidth)
                {
                    result.AppendLine();
                    currentLineWidth = 0;
                }

                result.Append(word + " ");
                currentLineWidth += word.Length + 1;
            }

            return result.ToString();
        }
    }
}
