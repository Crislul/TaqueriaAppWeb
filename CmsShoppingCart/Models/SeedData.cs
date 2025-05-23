using CmsShoppingCart.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace CmsShoppingCart.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider) { 
            using (var context = new CmsShoppingCartContext(serviceProvider.GetRequiredService<DbContextOptions<CmsShoppingCartContext>>())) {
                if (context.pages.Any()) 
                {
                    return;
                }
                context.pages.AddRange(
                    new Page {
                       Title="Home",
                       Slug="home",
                       Content="home pages",
                       Sorting=0
                    },
                    new Page
                    {
                        Title = "About Us ",
                        Slug = "about-us",
                        Content = "about us page",
                        Sorting = 100
                    },
                    new Page
                    {
                        Title = "Services",
                        Slug = "services",
                        Content = "services pages",
                        Sorting = 100
                    },
                    new Page
                    {
                        Title = "Contact",
                        Slug = "contact",
                        Content = "contact pages",
                        Sorting = 100
                    }
                    );
                context.SaveChanges();
            }
        }
    }
}
