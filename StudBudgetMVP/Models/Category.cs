using SQLite;

namespace StudBudgetMVP.Models
{
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool IsIncome { get; set; }
        public decimal? Limit { get; set; } // только для расходных
    }
}