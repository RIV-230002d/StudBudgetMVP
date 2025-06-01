using SQLite;
using System;

namespace StudBudgetMVP.Models;
public class Budget
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Category { get; set; }
    public decimal Limit { get; set; }
    public DateTime Month { get; set; }
}