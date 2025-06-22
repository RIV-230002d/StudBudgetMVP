namespace StudBudgetMVP.ViewModels
{
    public class CategoryDisplay
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public bool IsIncome { get; init; }
        public decimal? Limit { get; init; }
        public decimal Total { get; init; }
        public bool IsOverspent => !IsIncome && Limit is decimal l && Total > l;
    }
}
