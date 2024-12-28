using System.ComponentModel.DataAnnotations;

namespace ControlWork9.Models;

public class SomeProvider
{
    public int Id { get; set; }
        
    [Required]
    public string Name { get; set; }
        
    public List<ServiceUser>? ServiceUsers { get; set; }
}