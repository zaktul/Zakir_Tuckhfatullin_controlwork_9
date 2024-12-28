using System.ComponentModel.DataAnnotations;

namespace ControlWork9.ViewModels;

public class LoginViewModel

{
    [Required(ErrorMessage = "Не указан Email или номер кошелька")]
    public string NumberOrEmail { get; set; }
    
    [Required(ErrorMessage = "Не указан пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Display(Name = "Запомнить меня?")]
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}