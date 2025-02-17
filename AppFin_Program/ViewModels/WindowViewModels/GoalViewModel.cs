using AppFin_Program.Models;
using AppFin_Program.Services;
using AppFin_Program.ViewModels.MainViewModels;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppFin_Program.ViewModels.WindowViewModels
{
    public partial class GoalViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
    {
        #region Dependency

        private readonly GoalService _goalService;
        private readonly UserSessionService _userSessionService;
        private readonly BudgetService _budgetService;

        #endregion

        #region Property
        public string RouteKey => "goal";
        public ObservableCollection<Goal> Goals { get; } = new();
        public ObservableCollection<GoalDisplayModel> GoalList { get; } = new();

        private Goal? _selectedGoal;
        public Goal? SelectedGoal
        {
            get => _selectedGoal;
            set => this.RaiseAndSetIfChanged(ref _selectedGoal, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        private string _goalProgressBar;
        public string GoalProgressBar
        {
            get => _goalProgressBar;
            set => this.RaiseAndSetIfChanged(ref _goalProgressBar, value);
        }

        private string _newGoalName;
        public string NewGoalName
        {
            get => _newGoalName;
            set => this.RaiseAndSetIfChanged(ref _newGoalName, value);
        }

        private int _newGoalTargetAmount;
        public int NewGoalTargetAmount
        {
            get => _newGoalTargetAmount;
            set => this.RaiseAndSetIfChanged(ref _newGoalTargetAmount, value);
        }

        private Budget? _budgets;
        public Budget? Budgets
        {
            get => _budgets;
            set => this.RaiseAndSetIfChanged(ref _budgets, value);
        }

        #endregion

        #region Command
        public ReactiveCommand<Unit, Unit> AddGoalCommand { get; }
        public ReactiveCommand<Unit, Unit> UpdateGoalCommand { get; }
        public ReactiveCommand<GoalDisplayModel, Unit> DeleteGoalCommand { get; }
        public ReactiveCommand<Unit, Unit> ReturnHomeCommand { get; }

        #endregion

        public GoalViewModel()
        {
        }
        public GoalViewModel(GoalService goalService,
                             UserSessionService userSessionService,
                             BudgetService budgetService,
                             Action<string> navigateTo)
        {
            _goalService = goalService;
            _userSessionService = userSessionService;
            _budgetService = budgetService;

            AddGoalCommand = ReactiveCommand.Create(AddGoal);
            UpdateGoalCommand = ReactiveCommand.Create(UpdateGoal);
            DeleteGoalCommand = ReactiveCommand.Create<GoalDisplayModel>(DeleteGoal);

            ReturnHomeCommand = ReactiveCommand.Create(() => navigateTo("home"));

            Initialize();
            
        }

        private void LoadDataGridGoal()
        {
            try
            {
                var goals = _goalService.LoadGoal();

                GoalList.Clear();
                

                var goalDisplayModels = GoalService.GetGoalDisplayModel(goals);

                foreach (var goalDisplayModel in goalDisplayModels)
                {
                    var progress = (Budgets.Amount / goalDisplayModel.GoalTargetAmount) * 100;
                    if (progress >= 100)
                        goalDisplayModel.GoalProgressBar = 100;
                    else
                        goalDisplayModel.GoalProgressBar = progress;

                    var goals2 = new GoalDisplayModel()
                    {
                        GoalName = goalDisplayModel.GoalName,
                        GoalTargetAmount = goalDisplayModel.GoalTargetAmount,
                        GoalProgressBar = goalDisplayModel.GoalProgressBar,
                    };


                    GoalList.Add(goalDisplayModel);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки целей: {ex.Message}";
            }
        }
        private async void Initialize()
        {
            await LoadBudgetAsync();
            LoadDataGridGoal();
        }
        private async Task LoadBudgetAsync()
        {
            Budgets = await _budgetService.GetCurrentBudgetAsync();

            this.RaiseAndSetIfChanged(ref _budgets, Budgets);
        }

        private void AddGoal()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewGoalName) || NewGoalTargetAmount <= 0)
                {
                    StatusMessage = $"Некорректное имя или сумма цели";
                    return;
                }

                _goalService.AddGoal(NewGoalName, NewGoalTargetAmount);
                GoalList.Clear();
                Initialize();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка добавления целей: {ex.Message}";
            }
        }

        private void UpdateGoal()
        {
            try
            {
                if (SelectedGoal == null)
                {
                    StatusMessage = $"Ошибка обновления цели";
                    return;
                }

                _goalService.UpdateGoal(SelectedGoal.Id, SelectedGoal.Name, SelectedGoal.TargetAmount);
                Initialize();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка обновления целей: {ex.Message}";
            }
        }

        private async void DeleteGoal(GoalDisplayModel goalDisplayModel)
        {
            try
            {
                if (goalDisplayModel == null)
                {
                    Debug.WriteLine(" SelectedGoal is null");
                    return;
                }
                await _goalService.Delete(SelectedGoal);
                GoalList.Remove(GoalList.FirstOrDefault(g => g.GoalName == SelectedGoal.Name));
                SelectedGoal = null;
                StatusMessage = $"Запись удалена!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка удаления цели: {ex.Message}";
            }
        }
    }
}