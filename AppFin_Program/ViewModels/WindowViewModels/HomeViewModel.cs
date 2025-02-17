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
using System.Threading.Tasks;

namespace AppFin_Program.ViewModels.WindowViewModels
{
    public partial class HomeViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
    {
        #region Dependency

        private readonly TransactionService _transactionService;
        private readonly CategoryService _categoryService;
        private readonly UserSessionService _userSessionService;
        private readonly BudgetService _budgetService;

        #endregion

        #region Property
        public string RouteKey => "home";
        public ObservableCollection<TransactionDisplayModel> TransactionList { get; } = new();
        public ObservableCollection<ISeries> IncomeSeries { get; } = new();
        public ObservableCollection<ISeries> ExpenseSeries { get; } = new();
        public ObservableCollection<Category> Categories { get; }
        public ObservableCollection<TransactionType> Types { get; }
        public ObservableCollection<Goal> Goals { get; }
        public Category? SelectedCategory { get; set; }
        public TransactionType? SelectedTypes { get; set; }
        public string NewIncomeAmount { get; set; } = "0";
        public DateTimeOffset? SelectedDate { get; set; } = DateTimeOffset.Now;

        private Budget? _budgets;
        public Budget? Budgets
        {
            get => _budgets;
            set => this.RaiseAndSetIfChanged(ref _budgets, value);
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
                LoadDataGridAsync();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage = "";
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        #endregion

        #region Command
        public ReactiveCommand<Unit, int> AddTransactionCommand { get; }
        public ReactiveCommand<Unit, int> CancelAddTransactionCommand { get; }
        public ReactiveCommand<Unit, Unit> ConfirmAddTransactionCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToReportsCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToGoalsCommand { get; }

        #endregion

        public HomeViewModel()
        {

        }
        public HomeViewModel(TransactionService transactionService,
                             UserSessionService userSessionService,
                             CategoryService categoryService,
                             BudgetService budgetService,
                             Action<string> navigateTo)
        {
            _transactionService = transactionService;
            _userSessionService = userSessionService;
            _categoryService = categoryService;
            _budgetService = budgetService;

            Categories = new ObservableCollection<Category>(_categoryService.GetCategories());
            Types = new ObservableCollection<TransactionType>(_categoryService.GetTransactionTypes());
            Budgets = new Budget();

            AddTransactionCommand = ReactiveCommand.Create(() => SelectedTabIndex = 2);
            CancelAddTransactionCommand = ReactiveCommand.Create(() => SelectedTabIndex = 0);
            ConfirmAddTransactionCommand = ReactiveCommand.Create(AddTransaction);

            NavigateToReportsCommand = ReactiveCommand.Create(() => navigateTo("report"));
            NavigateToGoalsCommand = ReactiveCommand.Create(() => navigateTo("goal"));

            Initialize();
            LoadPieTransactions();
        }

        #region Method
        
        /// <summary>
        ///     Инициализация таблицы и диаграммы
        /// </summary>
        private async void Initialize()
        {
            await LoadDataGridAsync();
            await LoadBudgetAsync();
        }

        /// <summary>
        ///     Загрузка диаграммы
        /// </summary>
        private void LoadPieTransactions()
        {
            try
            {
                var userId = _userSessionService.GetCurrentUserId();
                var transactions = _transactionService.LoadTransactions();

                var incomeType = _categoryService.GetIncomeTransactionType();
                var expenseType = _categoryService.GetExpenseTransactionType();

                var incomeTransactions = transactions
                    .Where(t => t.TransactionCategories.TransactionTypeId == incomeType.Id)
                    .ToList();

                var expenseTransactions = transactions
                    .Where(t => t.TransactionCategories.TransactionTypeId == expenseType.Id)
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
        
        /// <summary>
        ///     Добавление данных в диаграмму
        /// </summary>
        /// <param name="seriesCollection">Коллекция транзакций</param>
        /// <param name="transactions">Транзакции</param>
        private static void AddSeriesToCollection(ObservableCollection<ISeries> seriesCollection, List<Transaction> transactions)
        {
            foreach (var group in transactions.SelectMany(t => t.TransactionCategories.Transactions)
                                               .GroupBy(tc => tc.TransactionCategories.Category.Name))
                seriesCollection.Add(new PieSeries<decimal> { Name = group.Key, Values = new[] { group.Sum(tc => tc.Amount) } });
        }
        
        /// <summary>
        ///     Загрузка данных таблицы
        /// </summary>
        /// <returns></returns>
        private async Task LoadDataGridAsync()
        {
            string? transactionType = SelectedTabIndex == 0 ? "Доход" : SelectedTabIndex == 1 ? "Расход" : null;

            if (transactionType != null)
            {
                LoadDataGridByType(transactionType);
                await LoadBudgetAsync();
            }
        }
        
        /// <summary>
        ///     Загрузка данных по типу
        /// </summary>
        /// <param name="transactionType">Тип транзакции</param>
        private void LoadDataGridByType(string transactionType)
        {
            try
            {
                var transactionTypeObj = transactionType == "Доход" ?
                                  _categoryService.GetIncomeTransactionType() :
                                  _categoryService.GetExpenseTransactionType();

                if (transactionTypeObj == null)
                {
                    StatusMessage = "Ошибка: Тип транзакции не найден.";
                    return;
                }

                var transactions = _transactionService.LoadTransactions()
                    .Where(t => t.TransactionCategories?.TransactionTypeId == transactionTypeObj.Id)
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

        /// <summary>
        ///     Загрузка бюджета
        /// </summary>
        /// <returns></returns>
        private async Task LoadBudgetAsync()
        {
            Budgets = await _budgetService.GetCurrentBudgetAsync();

            this.RaiseAndSetIfChanged(ref _budgets, Budgets);
        }

        /// <summary>
        ///     Перерасчет бюджета
        /// </summary>
        /// <returns></returns>
        public async Task RecalculateBudgetAsync()
        {
            await _budgetService.RecalculateBudgetAsync();
            await LoadBudgetAsync();
        }

        /// <summary>
        ///     Добавление бюджета
        /// </summary>
        private async void AddTransaction()
        {
            if (!ValidateAddNote())
                return;

            try
            {
                SelectedTabIndex = 0;

                await _transactionService.AddTransactionAsync(SelectedCategory!, SelectedTypes!, decimal.Parse(NewIncomeAmount), SelectedDate);
                LoadPieTransactions();
                await LoadDataGridAsync();
                await LoadBudgetAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка добавления записи: {ex.Message}";
            }
        }

        /// <summary>
        ///     Валидация при добавлении транзакции
        /// </summary>
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
                return false;
            }

            return true;
        }

        #endregion
    }
}
