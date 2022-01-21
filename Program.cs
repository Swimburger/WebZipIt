using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddFolderRouteModelConvention("/", pageRouteModel =>
    {
        foreach (var selectorModel in pageRouteModel.Selectors)
        {
            selectorModel.AttributeRouteModel.Template = "pages/" + selectorModel.AttributeRouteModel.Template;
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/mvc/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapGet("/", async context =>
{
    await context.Response.WriteAsync("Request /download-bots to download a zip file of .NET Bots");
});

app.MapGet("/download-bots", async context =>
{
    context.Response.ContentType = "application/octet-stream";
    context.Response.Headers.Add("Content-Disposition", "attachment; filename=\"Bots.zip\"");

    var botsFolderPath = Path.Combine(app.Environment.ContentRootPath, "bots");
    var botFilePaths = Directory.GetFiles(botsFolderPath);
    using (ZipArchive archive = new ZipArchive(context.Response.BodyWriter.AsStream(), ZipArchiveMode.Create))
    {
        foreach (var botFilePath in botFilePaths)
        {
            var botFileName = Path.GetFileName(botFilePath);
            var entry = archive.CreateEntry(botFileName);
            using (var entryStream = entry.Open())
            using (var fileStream = System.IO.File.OpenRead(botFilePath))
            {
                await fileStream.CopyToAsync(entryStream);
            }
        }
    }
});

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "mvc/{controller=Home}/{action=Index}/{id?}");

app.Run();
