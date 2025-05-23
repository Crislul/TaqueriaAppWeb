using CmsShoppingCart.Infrastructure;
using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin,editor")]
    [Area("Admin")]
    public class PagesController : Controller
    {
        private readonly CmsShoppingCartContext context;
        public PagesController(CmsShoppingCartContext context)
        {
            this.context = context;
        }

        //GET/Admin/Pages/Index/
        public async Task<IActionResult> Index()
        {
            IQueryable<Page> pages = from p in context.pages
                                     orderby p.Sorting
                                     select p;

            List<Page> pagesList = await pages.ToListAsync();

            return View(pagesList);
        }

        //GET/Admin/Pages/Details/
        public async Task<IActionResult> Details(int id)
        {

            Page page = await context.pages.FirstOrDefaultAsync(x => x.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        //GET/Admin/Pages/Create/
        public IActionResult Create() => View();

        //POST/Admin/Pages/Create/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Page page)
        {
            if (ModelState.IsValid)
            {
                page.Slug = page.Title.ToLower().Replace(" ", "-");
                page.Sorting = 100;

                var slug = await context.pages.FirstOrDefaultAsync(x => x.Slug == page.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "La pagina ya existe!!!");
                    return View(page);
                }
                context.Add(page);
                await context.SaveChangesAsync();

                TempData["Success"] = "La pagina ha sido agregada!!!";

                return RedirectToAction("Index");
            }
            return View(page);
        }

        //GET/Admin/Pages/Details/
        public async Task<IActionResult> Edit(int id)
        {

            Page page = await context.pages.FindAsync(id);

            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        //POST/Admin/Pages/Create/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Page page)
        {
            if (ModelState.IsValid)
            {
                page.Slug = page.Id == 1 ? "home" : page.Title.ToLower().Replace(" ", "-");


                var slug = await context.pages.Where(x => x.Id != page.Id).FirstOrDefaultAsync(x => x.Slug == page.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "La pagina ya existe!!!");
                    return View(page);
                }
                context.Update(page);
                await context.SaveChangesAsync();

                TempData["Success"] = "La pagina ha sido editada!!!";

                return RedirectToAction("Edit", new { id = page.Id });
            }
            return View(page);
        }

        //GET/Admin/Pages/Delete/
        public async Task<IActionResult> Delete(int id)
        {

            Page page = await context.pages.FirstOrDefaultAsync(x => x.Id == id);

            if (page == null)
            {
                TempData["Error"] = "La pagina no existe!!!";
            }
            else
            {
                context.pages.Remove(page);
                await context.SaveChangesAsync();
                TempData["Success"] = "La pagina ha sido eliminada!!!";
            }

            return RedirectToAction("Index");
        }

        //POST/Admin/Pages/Reorder/
        [HttpPost]
        public async Task<IActionResult> Reorder(int[] id)
        {
                int count = 1;
                foreach (var pageId in id)
                {
                    Page page = await context.pages.FindAsync(pageId);
                    page.Sorting = count;
                    context.Update(page);
                    await context.SaveChangesAsync();
                    count++;
                }
                return Ok();
            }
        }
    }

 