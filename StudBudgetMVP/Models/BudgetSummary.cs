namespace StudBudgetMVP.Models;
public class CategorySummary
{
    public string Category { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
}

public class BudgetSummary
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public bool IsOverBudget { get; set; }
    public List<CategorySummary> CategorySummaries { get; set; } = new();
    public bool IsOverspent => TotalExpense > TotalIncome;
}