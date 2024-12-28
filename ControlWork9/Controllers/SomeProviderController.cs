using ControlWork9.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlWork9.Controllers;


public class SomeProviderController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly WalletContext _context;
    
    public SomeProviderController(WalletContext context, UserManager<User> userManager)
    {
        _userManager = userManager;
        _context = context;
    }
    
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);
        ViewBag.Role = roles.FirstOrDefault();

        var userProviders = new HashSet<int>(
            _context.ServiceUsers
                .Where(su => su.OwnerId == user.Id)
                .Select(su => su.ServiceProviderId)
        );

        ViewBag.UserProviders = userProviders;

        var someProviders = await _context.SomeProviders.ToListAsync();
        return View(someProviders);
    }


    public async Task<IActionResult> Detail(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var someProvider = await _context.SomeProviders
            .Include(u => u.ServiceUsers)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (someProvider == null)
        {
            return NotFound();
        }

        return View(someProvider);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SomeProvider someProvider)
    {
        if (ModelState.IsValid)
        {
            if (await _context.SomeProviders.AnyAsync(sp => sp.Name == someProvider.Name))
            {
                ModelState.AddModelError("", "Поставщик с таким названием уже существует.");
                return View(someProvider);
            }

            _context.Add(someProvider);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(someProvider);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var someProvider = await _context.SomeProviders.FindAsync(id);
        if (someProvider == null)
        {
            return NotFound();
        }
        return View(someProvider);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SomeProvider someProvider)
    {
        if (id != someProvider.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                if (await _context.SomeProviders.AnyAsync(sp => sp.Name == someProvider.Name && sp.Id != someProvider.Id))
                {
                    ModelState.AddModelError("", "Поставщик с таким названием уже существует.");
                    return View(someProvider);
                }

                _context.Update(someProvider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceProviderExists(someProvider.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        return View(someProvider);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var someProvider = await _context.SomeProviders
            .FirstOrDefaultAsync(m => m.Id == id);
        if (someProvider == null)
        {
            return NotFound();
        }

        _context.SomeProviders.Remove(someProvider);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ServiceProviderExists(int id)
    {
        return _context.SomeProviders.Any(e => e.Id == id);
    }
}
