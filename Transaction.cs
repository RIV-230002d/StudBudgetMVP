using SQLite;
using System;
namespace StudBudgetMVP.Data.Models;
public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}