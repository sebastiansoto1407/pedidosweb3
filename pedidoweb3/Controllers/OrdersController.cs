using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pedidoweb3.Data;
using pedidoweb3.Models;

namespace pedidoweb3.Controllers
{
    [Authorize(Roles = "admin,empleado")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context) => _context = context;

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var data = await _context.Orders
                .Include(o => o.Cliente)
                .OrderByDescending(o => o.Fecha)
                .ToListAsync();
            return View(data);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.Cliente)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Clientes = await _context.Users
                .Where(u => u.Rol == "cliente")
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            ViewBag.Productos = await _context.Products
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View();
        }

        // POST: Orders/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int clienteId, int productId, int cantidad)
        {
            if (clienteId <= 0 || productId <= 0 || cantidad <= 0)
            {
                TempData["err"] = "Datos invalidos para crear el pedido.";
                await CargarCombos();
                return View();
            }

            var cliente = await _context.Users.FindAsync(clienteId);
            var producto = await _context.Products.FindAsync(productId);

            if (cliente == null || producto == null)
            {
                TempData["err"] = "Cliente o producto no encontrado.";
                await CargarCombos();
                return View();
            }

            if (producto.Stock < cantidad)
            {
                TempData["err"] = "Stock insuficiente.";
                await CargarCombos();
                return View();
            }

            var order = new Order
            {
                ClienteId = clienteId,
                Fecha = DateTime.UtcNow,
                Estado = OrderStatus.Pendiente
            };

            var item = new OrderItem
            {
                ProductId = productId,
                Cantidad = cantidad,
                Subtotal = producto.Precio * cantidad
            };

            order.Items.Add(item);
            order.Total = order.Items.Sum(i => i.Subtotal);

            producto.Stock -= cantidad;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["ok"] = "Pedido creado correctamente.";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderStatus estado)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Estado = estado;
            await _context.SaveChangesAsync();
            TempData["ok"] = "Estado del pedido actualizado.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            foreach (var it in order.Items)
            {
                var p = await _context.Products.FindAsync(it.ProductId);
                if (p != null) p.Stock += it.Cantidad;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            TempData["ok"] = "Pedido eliminado y stock devuelto.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCombos()
        {
            ViewBag.Clientes = await _context.Users
                .Where(u => u.Rol == "cliente")
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            ViewBag.Productos = await _context.Products
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
    }
}
