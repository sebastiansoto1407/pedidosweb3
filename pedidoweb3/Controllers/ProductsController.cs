using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pedidoweb3.Data;
using pedidoweb3.Models;

namespace pedidoweb3.Controllers
{
    // Admin y empleado gestionan productos. El listado público (Index) puede verse sin login.
    [Authorize(Roles = "admin,empleado")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        public ProductsController(AppDbContext context) => _context = context;

        // GET: Products
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? q, string? categoria, decimal? min, decimal? max)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Nombre.Contains(q) || (p.Descripcion ?? "").Contains(q));

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(p => p.Categoria == categoria);

            if (min.HasValue) query = query.Where(p => p.Precio >= min.Value);
            if (max.HasValue) query = query.Where(p => p.Precio <= max.Value);

            ViewBag.Categorias = await _context.Products
                .Select(p => p.Categoria).Where(c => c != null && c != "")
                .Distinct().OrderBy(c => c).ToListAsync();

            var lista = await query.OrderBy(p => p.Nombre).ToListAsync();
            return View(lista);
        }

        // GET: Products/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create() => View();

        // POST: Products/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Categoria,Precio,Stock")] Product product)
        {
            try
            {
                if (!ModelState.IsValid) return View(product);
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["ok"] = "Producto creado.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["err"] = "Error creando el producto.";
                return View(product);
            }
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Categoria,Precio,Stock")] Product product)
        {
            if (id != product.Id) return NotFound();

            try
            {
                if (!ModelState.IsValid) return View(product);
                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["ok"] = "Producto actualizado.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Products.AnyAsync(e => e.Id == product.Id))
                    return NotFound();
                TempData["err"] = "Conflicto al actualizar el producto.";
                return View(product);
            }
            catch
            {
                TempData["err"] = "Error actualizando el producto.";
                return View(product);
            }
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null) _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["ok"] = "Producto eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
