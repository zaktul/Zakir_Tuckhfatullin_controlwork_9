using ControlWork9.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlWork9.Controllers;

public class TransactionController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly WalletContext _context;
    
    public TransactionController(WalletContext context, UserManager<User> userManager)
    {
        _userManager = userManager;
        _context = context;
    }
    
    [HttpPost]
    public async Task<IActionResult> Transfer([FromBody] Transaction transaction)
    {
        if (ModelState.IsValid)
        {
            var receiver = await _context.Users.SingleOrDefaultAsync(u => u.AccountNumber == transaction.AccountNumber);
        
            if (receiver == null)
            {
                return Json(new { success = false, message = "Получатель не найден." });
            }

            if (receiver.Id == transaction.SenderId)
            {
                receiver.Balance += transaction.Amount;
                transaction.RecieverId = receiver.Id;
                transaction.SenderId = receiver.Id; 
                transaction.Date = DateTime.UtcNow;
                await _context.AddAsync(transaction);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Счет пополнен успешно.", newBalance = receiver.Balance });
            }

            transaction.RecieverId = receiver.Id;
            receiver.Balance += transaction.Amount;
            transaction.Date = DateTime.UtcNow;

            if (User.Identity.IsAuthenticated)
            {
                var sender = await _userManager.GetUserAsync(User);
                transaction.SenderId = sender.Id;
                sender.Balance -= transaction.Amount;
                _context.Update(sender);
            }

            await _context.AddAsync(transaction);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Перевод средств выполнен успешно.", newBalance = receiver.Balance });
        }

        return Json(new { success = false, message = "Неверные данные." });
    }


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Send([FromBody] Transaction transaction)
    {
        if (ModelState.IsValid)
        {
            var sender = await _userManager.GetUserAsync(User);

            if (sender.Balance < transaction.Amount)
            {
                return Json(new { success = false, message = "Недостаточно средств на счете." });
            }

            var receiver = await _context.Users.SingleOrDefaultAsync(u => u.AccountNumber == transaction.AccountNumber);

            if (receiver == null)
            {
                return Json(new { success = false, message = "Получатель не найден." });
            }

            sender.Balance += transaction.Amount;
            receiver.Balance -= transaction.Amount;

            transaction.SenderId = sender.Id;
            transaction.RecieverId = receiver.Id;
            transaction.Date = DateTime.UtcNow;

            _context.Update(sender);
            _context.Update(receiver);
            await _context.AddAsync(transaction);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Перевод средств выполнен успешно.", newBalance = sender.Balance });
        }

        return Json(new { success = false, message = "Неверные данные." });
    }

    [Authorize]
    public async Task<IActionResult> History(DateTime? fromDate, DateTime? toDate)
    {
        var user = await _userManager.GetUserAsync(User);

        var transactions = _context.Transactions
            .Where(t => t.SenderId == user.Id || t.RecieverId == user.Id)
            .Include(t => t.Sender)
            .Include(t => t.Reciever)
            .OrderByDescending(t => t.Date)
            .ToList();

        if (fromDate.HasValue)
        {
            transactions = transactions.Where(t => t.Date > fromDate).ToList();
        }

        if (toDate.HasValue)
        {
            transactions = transactions.Where(t => t.Date < toDate).ToList();
        }

        return View(transactions);
    }
}