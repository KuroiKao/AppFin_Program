using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AppFin_Program.Services
{
    public partial class GoalService : DbContextService
    {
        private readonly UserSessionService _userSessionService;
        public GoalService(IDbContextFactory<FinAppDataBaseContext> dbContextFactory, UserSessionService userSessionService) : base(dbContextFactory)
        {
            _userSessionService = userSessionService;
        }

        public List<Goal> LoadGoal()
        {
            return DbContext.Goals.Where(g => g.UserId == _userSessionService.GetCurrentUserId())
                .OrderByDescending(g => g.Name)
                .ToList();
        }
        public static List<GoalDisplayModel> GetGoalDisplayModel(List<Goal> goals)
        {
            return goals.Select(goal => new GoalDisplayModel
            {
                GoalName = goal.Name,
                GoalTargetAmount = goal.TargetAmount
            }).ToList();
        }

        public void AddGoal(string newGoalName, int newGoalTargetAmount)
        {
            Goal goal = new()
            {
                Name = newGoalName,
                TargetAmount = newGoalTargetAmount,
                UserId = _userSessionService.GetCurrentUserId()
            };

            DbContext.Goals.Add(goal);
            DbContext.SaveChanges();
        }

        public void UpdateGoal(int id, string name, decimal targetAmount)
        {
            var goal = DbContext.Goals.Where(g => g.UserId == _userSessionService.GetCurrentUserId()).Where(g => g.Id == id);

            foreach (var item in goal)
            {
                item.Name = name;
                item.TargetAmount = targetAmount;

                DbContext.Goals.Update(item);
            }

        }
        public async Task Delete(Goal selectedGoal)
        {
            if (selectedGoal == null)
                return;
            await using var transaction = await DbContext.Database.BeginTransactionAsync();

            try
            {
                var goal = DbContext.Goals.FirstOrDefault(g => g.Id == selectedGoal.Id);
                if (goal == null)
                    return;
                DbContext.Remove(goal);
                await DbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка: " + ex);
            }
        }
    }
}
