using AppFin_Program.Models;
using AppFin_Program.Services;
using AppFin_Program.ViewModels.MainViewModels;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;

namespace AppFin_Program.ViewModels.WindowViewModels;

public class ReportViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
{
    #region Dependency

    private readonly UserSessionService _userSessionService;
    private readonly TransactionService _transactionService;
    private readonly ReportService _reportService;

    #endregion

    #region Property

    public string RouteKey => "report";

    public ObservableCollection<Report> Reports { get; } = new();

    private DateTimeOffset? _startDate;
    public DateTimeOffset? StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    private DateTimeOffset? _endDate;
    public DateTimeOffset? EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }

    private bool _includeExpenses;
    public bool IncludeExpenses
    {
        get => _includeExpenses;
        set => this.RaiseAndSetIfChanged(ref _includeExpenses, value);
    }

    private bool _includeIncomes;
    public bool IncludeIncomes
    {
        get => _includeIncomes;
        set => this.RaiseAndSetIfChanged(ref _includeIncomes, value);
    }

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

    #endregion 

    #region Command
    public ReactiveCommand<Unit, Unit> GenerateReportCommand { get; }
    public ReactiveCommand<Unit, int> CreatReportCommand { get; }
    public ReactiveCommand<Unit, int> CancelReportCommand { get; }
    public ReactiveCommand<Unit, Unit> ReturnCommand { get; }
    public ReactiveCommand<Unit, Unit> MenuItem_Click_Open { get; }
    public ReactiveCommand<Unit, Unit> MenuItem_Click_Delete { get; }

    #endregion

    public ReportViewModel()
    {
    }
    public ReportViewModel(UserSessionService userSessionService,
                           TransactionService transactionService,
                           ReportService reportService,
                           Action<string> navigateTo)
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
                Reports.AddRange(reports);

            else
                StatusMessage = "������ �� �������.";

        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"������: {ex.Message}. ���������, �������� �� ���� � �������.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"������ �������� �������: {ex.Message}";
        }
    }
    private async Task GenerateReportAsync()
    {
        if (StartDate == null || EndDate == null || StartDate > EndDate)
        {
            StatusMessage = "�������� ���������� �������� ���.";
            return;
        }

        if (!IncludeExpenses && !IncludeIncomes)
        {
            StatusMessage = "�������� ���� �� ���� ��� ������ ��� ��������� � ����� (������ ��� �����).";
            return;
        }

        try
        {
            var transactions = await _transactionService.GetTransactionsAsync(StartDate.Value, EndDate.Value, IncludeExpenses, IncludeIncomes);
            var filePath = await _reportService.CreatePdfReportAsync(transactions);

            StatusMessage = "����� ������� �����������.";
            await LoadDataGridReportsAsync();
            ResetReportParameters();
            SelectedTabIndex = 0;
        }
        catch (Exception ex)
        {
            StatusMessage = $"������ ��� ��������� ������: {ex.Message}";
        }
    }

    private void ResetReportParameters()
    {
        StartDate = null;
        EndDate = null;
        IncludeExpenses = false;
        IncludeIncomes = false;
    }

    private async void ContextMenuSelect_Delete()
    {
        if (SelectedReport == null || string.IsNullOrWhiteSpace(SelectedReport.FilePath))
            return;

        try
        {
            if (File.Exists(SelectedReport.FilePath))
            {
                File.Delete(SelectedReport.FilePath);

                await _reportService.DeleteReportAsync(SelectedReport.Id);
                await LoadDataGridReportsAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"������ ��� �������� �����: {ex.Message}";
        }
    }
    private void ContextMenuSelect_Open()
    {
        if (SelectedReport == null || string.IsNullOrWhiteSpace(SelectedReport.FilePath))
        {
            StatusMessage = "���� �� ������ ��� ����� �� ������.";
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
            StatusMessage = $"������ ��� �������� �����: {ex.Message}";
        }
    }
}