using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pedidoweb3.Data;
using pedidoweb3.Models;

namespace pedidoweb3.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _hasher;

        public UsersController(AppDbContext context, IPasswordHasher<User> hasher)
        { _context = context; _hasher = hasher; }

        public async Task<IActionResult> Index() =>
            View(await _context.Users.OrderBy(u => u.Nombre).ToListAsync());

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Email,Rol")] User user, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    ModelState.AddModelError("", "La contraseña es requerida.");
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                    ModelState.AddModelError("Email", "El email ya existe.");
                if (!ModelState.IsValid) return View(user);

                user.PasswordHash = _hasher.HashPassword(user, password);
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["ok"] = "Usuario creado.";
                return RedirectToAction(nameof(Index));
            }
            catch { TempData["err"] = "Error creando el usuario."; return View(user); }
        }

        public async Task<IActionResult> Edit(int? id)
        { if (id == null) return NotFound(); var u = await _context.Users.FindAsync(id); return u == null ? NotFound() : View(u); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Email,Rol")] User user, string? newPassword)
        {
            if (id != user.Id) return NotFound();
            var dbUser = await _context.Users.FindAsync(id); if (dbUser == null) return NotFound();

            if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != id))
                ModelState.AddModelError("Email", "El email ya existe.");
            if (!ModelState.IsValid) return View(dbUser);

            dbUser.Nombre = user.Nombre; dbUser.Email = user.Email; dbUser.Rol = user.Rol;
            if (!string.IsNullOrWhiteSpace(newPassword))
                dbUser.PasswordHash = _hasher.HashPassword(dbUser, newPassword);

            await _context.SaveChangesAsync();
            TempData["ok"] = "Usuario actualizado.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        { if (id == null) return NotFound(); var u = await _context.Users.FirstOrDefaultAsync(m => m.Id == id); return u == null ? NotFound() : View(u); }

        public async Task<IActionResult> Delete(int? id)
        { if (id == null) return NotFound(); var u = await _context.Users.FirstOrDefaultAsync(m => m.Id == id); return u == null ? NotFound() : View(u); }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var u = await _context.Users.FindAsync(id);
                if (u != null) _context.Users.Remove(u);
                await _context.SaveChangesAsync();
                TempData["ok"] = "Usuario eliminado.";
            }
            catch { TempData["err"] = "No es posible eliminar este usuario."; }
            return RedirectToAction(nameof(Index));
        }
    }
}
