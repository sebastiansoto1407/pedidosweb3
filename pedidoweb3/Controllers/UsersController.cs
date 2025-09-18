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
        private static readonly string[] AllowedRoles = new[] { "admin", "cliente", "empleado" };

        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _hasher;

        public UsersController(AppDbContext context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var list = await _context.Users
                .OrderBy(u => u.Nombre)
                .ToListAsync();
            return View(list);
        }

        // GET: Users/Create
        public IActionResult Create() => View();

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Email,Rol")] User user, string password)
        {
            // Evita que la validacin exija PasswordHash(lo seteamos nosotros :))
            ModelState.Remove("PasswordHash"); 

            // Normalizar entradas
            user.Nombre = (user.Nombre ?? "").Trim();
            user.Email = (user.Email ?? "").Trim();
            user.Rol = (user.Rol ?? "").Trim().ToLowerInvariant();

            // Validaciones
            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("Password", "La contraseña es requerida.");

            if (!AllowedRoles.Contains(user.Rol))
                ModelState.AddModelError(nameof(user.Rol), "Rol inválido. Usa admin / cliente / empleado.");

            var emailLower = user.Email.ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailLower))
                ModelState.AddModelError(nameof(user.Email), "El email ya existe.");

            if (!ModelState.IsValid) return View(user);

            // Hash y guardar
            user.PasswordHash = _hasher.HashPassword(user, password);
            _context.Add(user);
            await _context.SaveChangesAsync();

            TempData["ok"] = "Usuario creado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var u = await _context.Users.FindAsync(id);
            if (u == null) return NotFound();
            return View(u);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Email,Rol")] User user, string? newPassword)
        {
            // Evita exigir PasswordHash en el model binding
            ModelState.Remove("PasswordHash");

            if (id != user.Id) return NotFound();

            //normalizar
            user.Nombre = (user.Nombre ?? "").Trim();
            user.Email = (user.Email ?? "").Trim();
            user.Rol = (user.Rol ?? "").Trim().ToLowerInvariant();

            try
            {
                var dbUser = await _context.Users.FindAsync(id);
                if (dbUser == null) return NotFound();

                if (!AllowedRoles.Contains(user.Rol))
                    ModelState.AddModelError(nameof(user.Rol), "Rol inválido. Usa admin / cliente / empleado.");

                var emailLower = user.Email.ToLowerInvariant();
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailLower && u.Id != id))
                    ModelState.AddModelError(nameof(user.Email), "El email ya existe.");

                if (!ModelState.IsValid)
                    return View(dbUser); // dejamos los datos actuales si hay error

                dbUser.Nombre = user.Nombre;
                dbUser.Email = user.Email;
                dbUser.Rol = user.Rol;

                if (!string.IsNullOrWhiteSpace(newPassword))
                    dbUser.PasswordHash = _hasher.HashPassword(dbUser, newPassword);

                await _context.SaveChangesAsync();
                TempData["ok"] = "Usuario actualizado.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["err"] = "Error de base de datos al actualizar el usuario.";
                return View(user);
            }
            catch
            {
                TempData["err"] = "Error actualizando el usuario.";
                return View(user);
            }
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var u = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (u == null) return NotFound();
            return View(u);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var u = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (u == null) return NotFound();
            return View(u);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var u = await _context.Users.FindAsync(id);
                if (u != null) _context.Users.Remove(u);
                await _context.SaveChangesAsync();
                TempData["ok"] = "Usuario eliminado.";
            }
            catch
            {
                TempData["err"] = "No es posible eliminar este usuario.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
