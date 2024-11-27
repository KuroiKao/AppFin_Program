using AppFin_Program.Models;
using AppFin_Program.ViewModels.MainViewModels;
using Avalonia.Controls;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using static AppFin_Program.ViewModels.StartViewModels.AuthorizationViewModel;

namespace AppFin_Program.ViewModels.WindowViewModels
{
    public class HomeViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
    {
        public string RouteKey => "home";
        private readonly FinAppDataBaseContext _dbContext;
        private readonly SessionService _sessionService;

        public ReactiveCommand<Unit, int> AddNoteCommand { get; }
        public ReactiveCommand<Unit, int> CancelAddNoteCommand { get; }
        public ReactiveCommand<Unit, Unit> ConfirmAddNoteCommand { get; }


        public IEnumerable<ISeries> Series { get; set; }
        public ObservableCollection<Transaction> IncomeTransactions { get; } = new();
        public ObservableCollection<Transaction> ExpenseTransactions { get; } = new();
        public ObservableCollection<ISeries> IncomeSeries { get; } = new();
        public ObservableCollection<ISeries> ExpenseSeries { get; } = new();
        public ObservableCollection<Category> Categories { get; }
        public ObservableCollection<TransactionDisplayModel> TransactionList { get; } = new();
        public ObservableCollection<TransactionType> Types { get; }
        public Category? SelectedCategory { get; set; }
        public TransactionType? SelectedTypes { get; set; }
        public string NewIncomeAmount { get; set; } = "0";
        public DateTimeOffset? SelectedDate { get; set; } = DateTimeOffset.Now;
        public bool IsToday { get; set; }

        public class TransactionDisplayModel
        {
            public string CategoryName { get; set; }
            public decimal Amount { get; set; }
            public DateTimeOffset TransactionDate { get; set; }
        }
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

        public HomeViewModel(SessionService sessionService, Action<string> navigateTo)
        {
            _dbContext = new FinAppDataBaseContext();
            _sessionService = sessionService;
            if (_sessionService.CurrentUser != null)
                Debug.WriteLine($"Id current user: {_sessionService.CurrentUser.Id}");


            Categories = new ObservableCollection<Category>(_dbContext.Categories.ToList());
            Types = new ObservableCollection<TransactionType>(_dbContext.TransactionTypes.ToList());
            IncomeSeries = new ObservableCollection<ISeries>();
            ExpenseSeries = new ObservableCollection<ISeries>();

            AddNoteCommand = ReactiveCommand.Create(() => SelectedTabIndex = 2);
            CancelAddNoteCommand = ReactiveCommand.Create(() => SelectedTabIndex = 0);
            ConfirmAddNoteCommand = ReactiveCommand.Create(AddNote);

            LoadTransactions();
            LoadDataGrid();
        }

        public HomeViewModel()
        {
            throw new NotImplementedException();
        }

        private void AddNote()
        {
            if (!ValidateAddNote())
            {
                return;
            }

            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    if (SelectedCategory != null || SelectedTypes != null)
                    {
                        var transactionCategory = new TransactionCategory
                        {
                            CategoryId = SelectedCategory.Id,
                            TransactionTypeId = SelectedTypes.Id
                        };

                        _dbContext.TransactionCategories.Add(transactionCategory);
                        _dbContext.SaveChanges();

                        var transactionRecord = new Transaction
                        {
                            UserId = _sessionService.CurrentUser.Id,
                            Amount = decimal.Parse(NewIncomeAmount),
                            TransactionDate = IsToday ? DateTimeOffset.Now : SelectedDate ?? DateTimeOffset.Now,
                            TransactionCategoriesId = transactionCategory.Id,
                        };

                        _dbContext.Transactions.Add(transactionRecord);
                    }

                    _dbContext.SaveChanges();

                    transaction.Commit();
                }

                SelectedTabIndex = 0;
                LoadTransactions();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding note: {ex.Message}";
            }
        }

        private bool ValidateAddNote()
        {
            if (SelectedCategory == null)
            {
                StatusMessage = "Не выбрана категория";
                return false;
            }

            if (!decimal.TryParse(NewIncomeAmount, out var amount) || amount <= 0)
            {
                StatusMessage = "Не выбрана сумма или сумма некорректна";
                return false;
            }

            return true;
        }
        private void LoadTransactions()
        {
            try
            {
                var transactions = _dbContext.Transactions
                    .Include(t => t.TransactionCategories.TransactionType)
                    .Include(t => t.TransactionCategories.Category)
                    .Where(t => t.UserId == _sessionService.CurrentUser.Id)
                    .ToList();

                ProcessTransactionsByType(transactions, "Доход");
                ProcessTransactionsByType(transactions, "Расход");

                IncomeTransactions.Clear();
                ExpenseTransactions.Clear();
                IncomeTransactions.AddRange(transactions.Where(t => t.TransactionCategories.TransactionType.Name == "Доход"));
                ExpenseTransactions.AddRange(transactions.Where(t => t.TransactionCategories.TransactionType.Name == "Расход"));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading transactions: {ex.Message}";
            }
        }
        private void ProcessTransactionsByType(List<Transaction> transactions, string transactionType)
        {
            var filteredTransactions = transactions
                .Where(t => t.TransactionCategories.TransactionType.Name == transactionType)
                .ToList();

            bool isTypeEmpty = !filteredTransactions.Any();

            if (transactionType == "Доход")
            {
                IsIncomeEmpty = isTypeEmpty;
                IncomeSeries.Clear();
                if (!isTypeEmpty)
                {
                    AddSeriesToCollection(IncomeSeries, filteredTransactions);
                }
                else
                {
                    StatusMessage = "No income transactions found.";
                }
            }
            else if (transactionType == "Расход")
            {
                IsExpenseEmpty = isTypeEmpty;
                ExpenseSeries.Clear();
                if (!isTypeEmpty)
                {
                    AddSeriesToCollection(ExpenseSeries, filteredTransactions);
                }
                else
                {
                    StatusMessage = "No expense transactions found.";
                }
            }
        }
        private static void AddSeriesToCollection(ObservableCollection<ISeries> seriesCollection, List<Transaction> transactions)
        {
            seriesCollection.AddRange(
                transactions.SelectMany(t => t.TransactionCategories.Transactions)
                           .GroupBy(tc => tc.TransactionCategories.Category.Name)
                           .Select(g => new PieSeries<decimal>
                           {
                               Name = g.Key,
                               Values = new[] { g.Sum(tc => tc.Amount) }
                           })
            );
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
            var transactions = _dbContext.Transactions
                .Include(t => t.TransactionCategories)
                .ThenInclude(tc => tc.Category)
                .Where(t => t.UserId == _sessionService.CurrentUser.Id)
                .Where(t => t.TransactionCategories.TransactionType.Name == transactionType)
                .ToList();

            TransactionList.Clear();

            foreach (var transaction in transactions)
            {
                TransactionList.Add(new TransactionDisplayModel
                {
                    CategoryName = transaction.TransactionCategories.Category.Name,
                    Amount = transaction.Amount,
                    TransactionDate = transaction.TransactionDate.Date
                });
            }
        }
    }
}
