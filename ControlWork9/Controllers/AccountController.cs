using ControlWork9.Models;
using ControlWork9.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace ControlWork9.Controllers;

public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(WalletContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult AccessDenied(string returnUrl = null)
        {
            return RedirectToAction("Login", new { returnUrl = returnUrl });
        }

        [Authorize(Roles = "admin")] 
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.userRole = userRoles.FirstOrDefault();
            return View(users);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = null;
                user = await _userManager.FindByEmailAsync(model.NumberOrEmail);
                if(user == null)
                {
                    if (int.TryParse(model.NumberOrEmail, out int accountNumber))
                    {
                        user = await _userManager.Users.SingleOrDefaultAsync(u => u.AccountNumber == accountNumber);
                    }
                }

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                            return Redirect(model.ReturnUrl);
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError("", "Неверный пароль или e-mail/номер кошелька");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userByEmail = await _userManager.FindByEmailAsync(model.Email);

                if (userByEmail != null)
                {
                    ModelState.AddModelError("", "Пользователь с таким email уже существует!");
                }
                else
                {
                    int accountNumber;
                    Random rand = new Random();

                    do
                    {
                        accountNumber = rand.Next(100000, 999999);
                    }
                    while (await _userManager.Users.AnyAsync(u => u.AccountNumber == accountNumber));

                    User user = new User()
                    {
                        Email = model.Email,
                        AccountNumber = accountNumber,
                        UserName = model.Email,
                        Balance = 100000
                    };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "user");
                        await _signInManager.SignInAsync(user, false);
                        return RedirectToAction("Index", "Home");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new EditViewModel
            {
                Email = user.Email
            };

            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user.Email != model.Email)
                {
                    var userByEmail = await _userManager.FindByEmailAsync(model.Email);
                    if (userByEmail != null)
                    {
                        ModelState.AddModelError("", "Пользователь с таким email уже существует!");
                        return View(model);
                    }
                }
                
                if (model.ChangeNumber)
                {
                    
                    Random rand = new Random();
                    int accountNumber;
                    do
                    {
                        accountNumber = rand.Next(100000, 999999);
                        user.AccountNumber = accountNumber;
                    } while (await _userManager.Users.AnyAsync(u => u.AccountNumber == accountNumber));
                }

                user.Email = model.Email;
                user.UserName = model.Email;
                

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    return RedirectToAction("Cabinet", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
        
        public async Task<IActionResult> Cabinet()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")] 
        public async Task<IActionResult> Delete(string? userid)
        {
            var userToDelete = await _userManager.FindByIdAsync(userid);
            if (userToDelete == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(userToDelete);
            if (userRoles.Contains("admin"))
            {
                return Forbid(); 
            }

            var result = await _userManager.DeleteAsync(userToDelete);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Account"); 
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Index", "Account"); 
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> DeleteSelf()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync(); 
                return RedirectToAction("Index", "Home"); 
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Cabinet"); 
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }