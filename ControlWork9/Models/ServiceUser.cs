using System.ComponentModel.DataAnnotations;

namespace ControlWork9.Models;

public class ServiceUser
{
    public int Id { get; set; }
    
    [Required]
    public int AccountNumber { get; set; }
    [Required]
    public decimal Balance { get; set; }
        
    public int ServiceProviderId { get; set; }
    public SomeProvider SomeProvider { get; set; }

    public int OwnerId { get; set; }
    public User Owner { get; set; }
}