using CmsShoppingCart.Infrastructure;
using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly CmsShoppingCartContext context;
        public CategoriesController(CmsShoppingCartContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await context.Categories.OrderBy(x => x.Sorting).ToListAsync());
        }

        //GET/Admin/Categories/Create
        public IActionResult Create() => View();

        //POST/Admin/Pages/Create/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.ToLower().Replace(" ", "-");
                category.Sorting = 100;

                var slug = await context.Categories.FirstOrDefaultAsync(x => x.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "La categoria ya existe!!!");
                    return View(category);
                }
                context.Update(category);
                await context.SaveChangesAsync();

                TempData["Success"] = "La categoria ha sido editada!!!";

                return RedirectToAction("Index");
            }
            return View(category);
        }
        //GET/Admin/Categories/Edit/
        public async Task<IActionResult> Edit(int id)
        {
            Category category = await context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        //POST/Admin/Categories/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.ToLower().Replace(" ", "-"); // cuando haya un espacio se sustituye por un -


                var slug = await context.Categories.Where(x => x.Id != category.Id)
                    .FirstOrDefaultAsync(x => x.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "La categoría ya existe!!!");
                    return View(category);
                }
                context.Update(category);
                await context.SaveChangesAsync();

                TempData["Success"] = "La categoría ha sido editada!!!";

                return RedirectToAction("Edit", new { id });
            }
            return View(category);
        }

        //GET/Admin/Categories/Delete/
        public async Task<IActionResult> Delete(int id)
        {
            Category category = await context.Categories.FindAsync(id);

            if (category == null)
            {
                TempData["Error"] = "La categoría no existe!!!";
            }
            else
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                TempData["Success"] = "La categoría ha sido eliminada!!!";
            }

            return RedirectToAction("Index");
        }

        //POST/Admin/Categories/Reorder/
        [HttpPost]
        public async Task<IActionResult> Reorder(int[] id)
        {
            int count = 1;
            foreach (var categoryId in id)
            {
                Category category = await context.Categories.FindAsync(categoryId);
                category.Sorting = count;
                context.Update(category);
                await context.SaveChangesAsync();
                count++;
            }
            return Ok();
        }
    }
}



