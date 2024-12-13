using AppFin_Program.Models;
using AppFin_Program.Services;
using AppFin_Program.ViewModels.MainViewModels;
using DynamicData;
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
using System.Threading.Tasks;

namespace AppFin_Program.ViewModels.WindowViewModels;

public class ReportViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
{
    public string RouteKey => "report";

    private readonly UserSessionService _userSessionService;
    private readonly TransactionService _transactionService;
    private readonly ReportService _reportService;
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
    public ReportViewModel(UserSessionService userSessionService, TransactionService transactionService, ReportService reportService, Action<string> navigateTo)
    {
        _userSessionService = userSessionService;
        _transactionService = transactionService;
        _reportService = reportService;


        GenerateReportCommand = ReactiveCommand.CreateFromTask(GenerateReportAsync);
        ReturnCommand = ReactiveCommand.Create(() => navigateTo("home"));
        MenuItem_Click_Open = ReactiveCommand.Create(ContextMenuSelect_Open);
        MenuItem_Click_Delete = ReactiveCommand.Create(ContextMenuSelect_Delete);

        _ = LoadDataGridReportsAsync();
    }
    private async Task LoadDataGridReportsAsync()
    {
        try
        {
            var reports = await _reportService.GetReportsByUserIdAsync(_userSessionService.GetCurrentUserId());

            Reports.Clear();
            if (reports.Count != 0)
            {
                Reports.AddRange(reports);
            }
            else
            {
                StatusMessage = "Отчеты не найдены.";
            }

        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}. Проверьте, выполнен ли вход в систему.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки отчетов: {ex.Message}";
        }
    }
    private async Task GenerateReportAsync()
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

        try
        {
            var transactions = await _transactionService.GetTransactionsAsync(StartDate.Value, EndDate.Value, IncludeExpenses, IncludeIncomes);
            var filePath = await _reportService.CreatePdfReportAsync(transactions);

            StatusMessage = "Отчет успешно сформирован.";
            await LoadDataGridReportsAsync();
            SelectedTabIndex = 0;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при генерации отчета: {ex.Message}";
        }
    }
    private async void ContextMenuSelect_Delete()
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

                await _reportService.DeleteReportAsync(SelectedReport.Id);

                Debug.WriteLine($"Файл {SelectedReport.FilePath} успешно удален.");

                await LoadDataGridReportsAsync();
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