using AppFin_Program.Models;
using AppFin_Program.ViewModels.MainViewModels;
using AppFin_Program.ViewModels.StartViewModels;

namespace AppFin_Program.ViewModels.WindowViewModels;

public class ReportViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
{
    public string RouteKey => "Report";
    
    private readonly FinAppDataBaseContext _dbContext;
    private readonly AuthorizationViewModel.SessionService _sessionService;
    
    
}