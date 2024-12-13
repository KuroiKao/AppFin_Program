using AppFin_Program.Models;
using AppFin_Program.ViewModels.MainViewModels;
using AppFin_Program.ViewModels.StartViewModels;
using DynamicData;
using Microsoft.EntityFrameworkCore;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text;

namespace AppFin_Program.ViewModels.WindowViewModels;

public class ReportViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
{
    public string RouteKey => "report";

    private readonly FinAppDataBaseContext _dbContext;
    private readonly AuthorizationViewModel.SessionService _sessionService;
    public ObservableCollection<Report> Reports { get; } = new();
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IncludeExpenses { get; set; }
    public bool IncludeIncomes { get; set; }

    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
            LoadDataGridReports();
        }
    }

    private Report _selectedReport;
    public Report SelectedReport
    {
        get => _selectedReport;
        set => this.RaiseAndSetIfChanged(ref _selectedReport, value);
    }

    public ReactiveCommand<Unit, Unit> GenerateReportCommand { get; }
    public ReactiveCommand<Unit, int> CreatReportCommand { get; }
    public ReactiveCommand<Unit, int> CancelReportCommand { get; }
    public ReactiveCommand<Unit, Unit> ReturnCommand { get; }
    public ReactiveCommand<Unit, Unit> MenuItem_Click_Open { get; }
    public ReactiveCommand<Unit, Unit> MenuItem_Click_Delete { get; }
    public ReportViewModel()
    {
    }
    public ReportViewModel(FinAppDataBaseContext dbContext, AuthorizationViewModel.SessionService sessionService, Action<string> navigateTo)
    {
        _dbContext = dbContext;
        _sessionService = sessionService;

        GenerateReportCommand = ReactiveCommand.Create(GenerateReport);
        ReturnCommand = ReactiveCommand.Create(() => navigateTo("home"));
        MenuItem_Click_Open = ReactiveCommand.Create(ContextMenuSelect_Open);
        MenuItem_Click_Delete = ReactiveCommand.Create(ContextMenuSelect_Delete);

        if (SelectedTabIndex == 0)
        {
            LoadDataGridReports();
        }
    }
    private void LoadDataGridReports()
    {
        if (SelectedTabIndex == 0)
        {
            try
            {
                string? transactionType = SelectedTabIndex == 0 ? "Отчеты" : null;

                var reports = _dbContext.Reports
                    .Where(t => t.UserId == _sessionService.CurrentUser!.Id)
                    .ToList();
                bool isReportEmpty = reports.Count == 0;
                if (!isReportEmpty)
                {
                    Reports.Clear();
                    Reports.AddRange(reports);

                    foreach (var report in reports)
                    {
                        Reports.Add(new Report
                        {
                            Id = report.Id,
                            CreatedAt = report.CreatedAt.DateTime
                        });
                    }
                }
                else
                {
                    StatusMessage = "Отчеты не найдены";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки отчетов: {ex.Message}";
            }
            Reports.Clear();
            Reports.AddRange(_dbContext.Reports.ToList());
        }
        else
            StatusMessage = "";
    }
    private void GenerateReport()
    {

        if (StartDate == null || EndDate == null || StartDate > EndDate)
        {
            StatusMessage = "Выберите корректный интервал дат.";
            return;
        }
        if (!IncludeExpenses && !IncludeIncomes)
        {
            StatusMessage = "Выберите хотя бы один тип данных для включения в отчет (Расход или Доход).";
            return;
        }

        var transactionsQuery = _dbContext.Transactions
                .Include(t => t.TransactionCategories.Category)
                .Include(t => t.TransactionCategories.TransactionType)
                .Where(t => t.TransactionDate >= StartDate && t.TransactionDate <= EndDate);

        if (IncludeExpenses && !IncludeIncomes)
            transactionsQuery = transactionsQuery.Where(t => t.TransactionCategories.TransactionType.Name == "Расход");

        if (!IncludeExpenses && IncludeIncomes)
            transactionsQuery = transactionsQuery.Where(t => t.TransactionCategories.TransactionType.Name == "Доход");

        var transactions = transactionsQuery.ToList();

        CreatePdfReport(transactions);
        StatusMessage = "Отчет успешно сформирован.";
        LoadDataGridReports();
        SelectedTabIndex = 0;
    }
    private void CreatePdfReport(List<Transaction> transactions)
    {
        try
        {
            var reportDirectory = GetReportDirectory();
            var fileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(reportDirectory, fileName);

            var document = new PdfDocument();
            document.Info.Title = "Отчет";


            var page = document.AddPage();
            page.Size = PageSize.A4;
            var graphics = XGraphics.FromPdfPage(page);

            var font = new XFont("Arial", 16);
            graphics.DrawString("Отчет о транзакциях", font, XBrushes.Black,
                new XRect(0, 0, page.Width, 50), XStringFormats.TopCenter);

            int yPosition = 50;
            int i = 1;
            foreach (var transaction in transactions)
            {
                var text = WrapText($"{i++}. {transaction.TransactionCategories.TransactionType.Name} | {transaction.TransactionCategories.Category.Name} | {transaction.Amount} | {transaction.TransactionDate:dd.MM.yyyy}", 50);

                graphics.DrawString(text, font, XBrushes.Black, new XRect(20, yPosition, page.Width - 40, 20), XStringFormats.TopLeft);
                yPosition += 20;

                if (yPosition > page.Height - 50)
                {
                    page = document.AddPage();
                    graphics = XGraphics.FromPdfPage(page);
                    yPosition = 50;
                }
            }

            document.Save(filePath);
            SaveReportPathToDatabase(filePath);
            StatusMessage = $"Отчет успешно сохранен: {filePath}";
        }

        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при сохранении отчета: {ex.Message}";
        }
    }
    private static string GetReportDirectory()
    {
        var appName = Assembly.GetExecutingAssembly().GetName().Name;
        var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), appName, "Reports");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        return directoryPath;
    }
    private void SaveReportPathToDatabase(string filePath)
    {
        var report = new Report
        {
            UserId = _sessionService.CurrentUser.Id,
            CreatedAt = DateTime.Now,
            FilePath = filePath
        };

        _dbContext.Reports.Add(report);
        _dbContext.SaveChanges();
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
    private void ContextMenuSelect_Delete()
    {
        if (SelectedReport == null || string.IsNullOrWhiteSpace(SelectedReport.FilePath))
        {
            Debug.WriteLine("Файл не найден или отчет не выбран.");
            return;
        }

        try
        {
            if (File.Exists(SelectedReport.FilePath))
            {
                File.Delete(SelectedReport.FilePath);

                _dbContext.Reports.Remove(SelectedReport);
                _dbContext.SaveChanges();

                Debug.WriteLine($"Файл {SelectedReport.FilePath} успешно удален.");
                LoadDataGridReports();
            }
            else
            {
                Debug.WriteLine("Файл не найден.");
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при удалении файла: {ex.Message}");
        }
    }
    private void ContextMenuSelect_Open()
    {
        if (SelectedReport == null || string.IsNullOrWhiteSpace(SelectedReport.FilePath))
        {
            Debug.WriteLine("Файл не найден или отчет не выбран.");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = SelectedReport.FilePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при открытии файла: {ex.Message}");
        }
    }
}