using AppFin_Program.Models;
using AppFin_Program.Services;
using AppFin_Program.ViewModels.MainViewModels;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace AppFin_Program.ViewModels.WindowViewModels
{
    public class HomeViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
    {
        public string RouteKey => "home";
        private readonly TransactionService _transactionService;
        private readonly CategoryService _categoryService;
        private readonly UserSessionService _userSessionService;

        public ReactiveCommand<Unit, int> AddNoteCommand { get; }
        public ReactiveCommand<Unit, int> CancelAddNoteCommand { get; }
        public ReactiveCommand<Unit, Unit> ConfirmAddNoteCommand { get; }
        public ReactiveCommand<Unit, Unit> ReportCommand { get; }

        public ObservableCollection<TransactionDisplayModel> TransactionList { get; } = new();
        public ObservableCollection<ISeries> IncomeSeries { get; } = new();
        public ObservableCollection<ISeries> ExpenseSeries { get; } = new();
        public ObservableCollection<Category> Categories { get; }
        public ObservableCollection<TransactionType> Types { get; }
        public Category? SelectedCategory { get; set; }
        public TransactionType? SelectedTypes { get; set; }
        public string NewIncomeAmount { get; set; } = "0";
        public DateTimeOffset? SelectedDate { get; set; } = DateTimeOffset.Now;

        private bool _isExpenseEmpty;
        public bool IsExpenseEmpty
        {
            get => _isExpenseEmpty;
            private set => this.RaiseAndSetIfChanged(ref _isExpenseEmpty, value);
        }

        private bool _isIncomeEmpty;
        public bool IsIncomeEmpty
        {
            get => _isIncomeEmpty;
            private set => this.RaiseAndSetIfChanged(ref _isIncomeEmpty, value);
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
                LoadDataGrid();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }
        public HomeViewModel()
        {

        }
        public HomeViewModel(TransactionService transactionService, UserSessionService userSessionService, CategoryService categoryService, Action<string> navigateTo)
        {
            _transactionService = transactionService;
            _userSessionService = userSessionService;
            _categoryService = categoryService;

            Categories = new ObservableCollection<Category>(_categoryService.GetCategories());
            Types = new ObservableCollection<TransactionType>(_categoryService.GetTransactionTypes());

            AddNoteCommand = ReactiveCommand.Create(() => SelectedTabIndex = 2);
            CancelAddNoteCommand = ReactiveCommand.Create(() => SelectedTabIndex = 0);
            ConfirmAddNoteCommand = ReactiveCommand.Create(AddNote);
            ReportCommand = ReactiveCommand.Create(() => navigateTo("report"));

            LoadPieTransactions();
        }
        private void AddNote()
        {             
            if (!ValidateAddNote())
            {
                return;
            }
            try
            {
                _transactionService.AddTransaction(SelectedCategory!, SelectedTypes!, decimal.Parse(NewIncomeAmount), SelectedDate);
                SelectedTabIndex = 0;
                LoadPieTransactions();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка добавления записи: {ex.Message}";
            }
        }
        private bool ValidateAddNote()
        {
            if (SelectedCategory == null)
            {
                StatusMessage = "Не выбрана категория";
                return false;
            }
            if (SelectedTypes == null)
            {
                StatusMessage = "Не выбран тип транзакции";
                return false;
            }
            if (!decimal.TryParse(NewIncomeAmount, out var amount) || amount <= 0)
            {
                StatusMessage = "Не выбрана сумма или сумма некорректна";
                return false;
            }

            if (SelectedDate == null)
            {
                StatusMessage = "Не выбрана дата";
            }

            return true;
        }
        private void LoadPieTransactions()
        {
            try
            {
                var userId = _userSessionService.GetCurrentUserId();
                var transactions = _transactionService.LoadTransactions();
                var incomeType = _categoryService.GetIncomeTransactionType();
                var expenseType = _categoryService.GetExpenseTransactionType();

                var incomeTransactions = transactions
                    .Where(t => t.TransactionCategories.TransactionType.Id == incomeType.Id)
                    .ToList();

                var expenseTransactions = transactions
                    .Where(t => t.TransactionCategories.TransactionType.Id == expenseType.Id)
                    .ToList();

                IsIncomeEmpty = incomeTransactions.Count == 0;
                IsExpenseEmpty = expenseTransactions.Count == 0;

                IncomeSeries.Clear();
                ExpenseSeries.Clear();
                AddSeriesToCollection(IncomeSeries, incomeTransactions);
                AddSeriesToCollection(ExpenseSeries, expenseTransactions);

                TransactionList.Clear();
                TransactionList.AddRange(_transactionService.GetTransactionDisplayModels(transactions));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки транзакции: {ex.Message}";
            }
        }
        private static void AddSeriesToCollection(ObservableCollection<ISeries> seriesCollection, List<Transaction> transactions)
        {
            foreach (var group in transactions.SelectMany(t => t.TransactionCategories.Transactions)
                                               .GroupBy(tc => tc.TransactionCategories.Category.Name))
            {
                seriesCollection.Add(new PieSeries<decimal>
                {
                    Name = group.Key,
                    Values = new[] { group.Sum(tc => tc.Amount) }
                });
            }
        }
        private void LoadDataGrid()
        {
            string? transactionType = SelectedTabIndex == 0 ? "Доход" : SelectedTabIndex == 1 ? "Расход" : null;

            if (transactionType != null)
            {
                LoadDataGridByType(transactionType);
            }
        }
        private void LoadDataGridByType(string transactionType)
        {
            try
            {
                var transactionTypeObj = transactionType == "Доход" ?
                                  _categoryService.GetIncomeTransactionType() :
                                  _categoryService.GetExpenseTransactionType();
                var transactions = _transactionService.LoadTransactions()
                    .Where(t => t.TransactionCategories.TransactionType.Id == transactionTypeObj.Id)
                    .ToList();

                TransactionList.Clear();

                var transactionDisplayModels = _transactionService.GetTransactionDisplayModels(transactions);
                foreach (var transactionDisplayModel in transactionDisplayModels)
                {
                    TransactionList.Add(transactionDisplayModel);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки данных: {ex.Message}";
            }
        }
    }
}
