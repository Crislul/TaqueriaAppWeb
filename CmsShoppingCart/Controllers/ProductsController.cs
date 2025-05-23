using CmsShoppingCart.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Authorization;

namespace CmsShoppingCart.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly CmsShoppingCartContext context;
        public ProductsController(CmsShoppingCartContext context)
        {
            this.context = context;
        }
        //GET/Admin/Products/Index
        public async Task<IActionResult> Index(int p = 1)
        {
            int pageSize = 6;
            var products = context.Products.OrderByDescending(x => x.Id)
                                           .Skip((p - 1) * pageSize)
                                           .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)context.Products.Count() / pageSize);

            return View(await products.ToListAsync());
        }

        //GET/ProductsByCategory
        public async Task<IActionResult> ProductsByCategory(string categorySlug, int p = 1)
        {
            Category category = await context.Categories.Where(x => x.Slug == categorySlug).FirstOrDefaultAsync();
            if (category == null) return RedirectToAction("All");


            int pageSize = 6;
            var products = context.Products.OrderByDescending(x => x.Id)
                                           .Where(x => x.CategoryId == category.Id)
                                           .Skip((p - 1) * pageSize)
                                           .Take(pageSize);

            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)context.Products
                                 .Where(x => x.CategoryId == category.Id)
                                 .Count() / pageSize);
            ViewBag.CategoryName = category.Name;
            ViewBag.CategorySlug = category.Slug;

            return View(await products.ToListAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> All()
        {
            var products = await context.Products.ToListAsync();
            return View("AllProducts", products);
        }

        // GET: /Products/Details/{id}
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await context.Products
                .Include(p => p.Category) // Opcional: Si quieres mostrar la categoría del producto
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}
