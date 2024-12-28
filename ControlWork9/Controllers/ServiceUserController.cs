using Microsoft.AspNetCore.Mvc;
using ControlWork9.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ControlWork9.Controllers;

public class ServiceUserController : Controller
{
    private readonly WalletContext _context;
    private readonly UserManager<User> _userManager;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["Error"] = "Пользователь не найден.";
            return RedirectToAction("Login", "Account");
        }

        var serviceAccounts = await _context.ServiceUsers
            .Include(su => su.SomeProvider)
            .Where(su => su.OwnerId == user.Id)
            .ToListAsync();

        return View(serviceAccounts);
    }
    
    public ServiceUserController(WalletContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> AddAccount(int providerId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index", "SomeProvider");
        }

        var providerExists = await _context.SomeProviders.AnyAsync(sp => sp.Id == providerId);
        if (!providerExists)
        {
            return RedirectToAction("Index", "SomeProvider");
        }

        var existingAccount = _context.ServiceUsers
            .FirstOrDefault(su => su.ServiceProviderId == providerId && su.OwnerId == user.Id);
        if (existingAccount != null)
        {
            return RedirectToAction("Index", "SomeProvider");
        }

        var serviceUser = new ServiceUser
        {
            AccountNumber = int.Parse($"{providerId}{user.Id:D6}"),
            Balance = 0,
            ServiceProviderId = providerId,
            OwnerId = user.Id
        };

        _context.ServiceUsers.Add(serviceUser);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "SomeProvider");
    }


    [HttpPost]
    public async Task<IActionResult> RemoveAccount(int providerId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["Error"] = "Пользователь не найден.";
            return RedirectToAction("Index", "SomeProvider");
        }

        var serviceUser = _context.ServiceUsers
            .FirstOrDefault(su => su.ServiceProviderId == providerId && su.OwnerId == user.Id);
        if (serviceUser == null)
        {
            TempData["Error"] = "Счет не найден.";
            return RedirectToAction("Index", "SomeProvider");
        }

        _context.ServiceUsers.Remove(serviceUser);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Счет удален.";
        return RedirectToAction("Index", "SomeProvider");
    }
    
    [HttpPost]
    public async Task<IActionResult> TransferToAccount(int accountId, decimal amount)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "Пользователь не найден." });
        }

        if (amount <= 0)
        {
            return Json(new { success = false, message = "Сумма должна быть больше 0." });
        }

        var serviceUser = await _context.ServiceUsers.FirstOrDefaultAsync(su => su.Id == accountId && su.OwnerId == user.Id);
        if (serviceUser == null)
        {
            return Json(new { success = false, message = "Счет не найден." });
        }

        if (user.Balance < amount)
        {
            return Json(new { success = false, message = "Недостаточно средств на счете." });
        }

        user.Balance -= amount;
        serviceUser.Balance += amount;

        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Перевод выполнен успешно." });
    }

}