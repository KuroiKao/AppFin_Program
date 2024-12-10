using System;
using AppFin_Program.Models;
using AppFin_Program.ViewModels.MainViewModels;
using AppFin_Program.ViewModels.StartViewModels;

namespace AppFin_Program.ViewModels.WindowViewModels;

public class ReportViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
{
    public string RouteKey => "report";
    
    private readonly FinAppDataBaseContext _dbContext;
    private readonly AuthorizationViewModel.SessionService _sessionService;
    public string Greeteng => "Hello";

    public ReportViewModel(FinAppDataBaseContext dbContext, AuthorizationViewModel.SessionService sessionService)
    {
        _dbContext = dbContext;
        
        
    }
    
}