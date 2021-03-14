using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebZipIt
{
    public class Startup
    {
        private IWebHostEnvironment hostEnvironment;
        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            this.hostEnvironment = hostEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages(options =>
            {
                options.Conventions.AddFolderRouteModelConvention("/", pageRouteModel =>
                {
                    foreach (var selectorModel in pageRouteModel.Selectors)
                    {
                        selectorModel.AttributeRouteModel.Template = "pages/" + selectorModel.AttributeRouteModel.Template;
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/mvc/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Request /download-bots to download a zip file of .NET Bots");
                });
                
                endpoints.MapGet("/download-bots", async context =>
                {
                    context.Response.Headers.Add("Content-Type", "binary/octet-stream")  ;
                    context.Response.Headers.Add("Content-Disposition", "attachment; filename=\"Bots.zip\"");

                    var botsFolderPath = Path.Combine(hostEnvironment.ContentRootPath, "bots");
                    var botFilePaths = Directory.GetFiles(botsFolderPath);
                    using (var zipFileMemoryStream = new MemoryStream())
                    {
                        using (ZipArchive archive = new ZipArchive(zipFileMemoryStream, ZipArchiveMode.Update, leaveOpen: true))
                        {
                            foreach (var botFilePath in botFilePaths)
                            {
                                var botFileName = Path.GetFileName(botFilePath);
                                var entry = archive.CreateEntry(botFileName);
                                using (var entryStream = entry.Open())
                                using (var fileStream = System.IO.File.OpenRead(botFilePath))
                                {
                                    fileStream.CopyTo(entryStream);
                                }
                            }
                        }
                        zipFileMemoryStream.Seek(0, SeekOrigin.Begin);
                        await zipFileMemoryStream.CopyToAsync(context.Response.Body);
                    }
                });

                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "mvc/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
