using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudBudgetMVP.Services;
using StudBudgetMVP.Models;
using Microsoft.Maui.Storage;

namespace StudBudgetMVP.ViewModels;

public partial class BudgetViewModel : ObservableObject
{
    [ObservableProperty] public int userId;
    [ObservableProperty] public DateTime date = DateTime.Now;
    [ObservableProperty] public BudgetSummary summary;

    private readonly IDataService ds;

    public BudgetViewModel(IDataService dataService)
    {
        ds = dataService;
        userId = Preferences.Get("userId", 0);
        CalculateCommand = new AsyncRelayCommand(CalcAsync);
    }

    public IAsyncRelayCommand CalculateCommand { get; }

    async Task CalcAsync()
    {
        Summary = await ds.GetMonthlySummaryAsync(userId, date.Year, date.Month);
    }
}