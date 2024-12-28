using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ControlWork9.Models;

public class Transaction
{
    public int Id { get; set; }
    [Required]
    [Remote(action: "CheckAccountNumber", controller: "Validation", ErrorMessage = "Нет такого номера счёта!")]
    public int AccountNumber { get; set; }
    public DateTime? Date { get; set; }
    [Remote(action: "CheckAmount", controller: "Validation", ErrorMessage = "Нельзя пополнить счёт на такую сумму!")]
    public decimal Amount { get; set; }
    
    public int? SenderId { get; set; }
    public User? Sender { get; set; }
    
    public int? RecieverId { get; set; }
    public User? Reciever { get; set; }
}