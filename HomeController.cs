using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using System;

  public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost] 
        public IActionResult tileSetSelected(String tileSelected)
        {
            Console.WriteLine($"< {tileSelected}");
            Program.tileSetExecution(tileSelected);
            return View();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(); 
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "tileSet",
                    pattern: "{controller=Home}/{action=tileSetSelected}/{tileSelected}");
            }); 
        }

    }
