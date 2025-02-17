using ReactiveUI;

namespace AppFin_Program.Models
{
    public class GoalDisplayModel : ReactiveObject
    {
        public string? GoalName { get; set; }
        public decimal GoalTargetAmount { get; set; }
        public decimal GoalProgressBar { get; set; }
    }
}
