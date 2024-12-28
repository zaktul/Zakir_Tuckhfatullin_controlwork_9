using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ControlWork9.Models;

public class User : IdentityUser<int>
{
    public int AccountNumber { get; set; }
    public decimal Balance { get; set; }

    public List<Transaction>? TransactionsU { get; set; }
    public List<Transaction>? TransactionsSendU { get; set; }
    public List<ServiceUser>? ServiceUsersU { get; set; }
}