using Microsoft.EntityFrameworkCore;
using WebProject.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

/*app.MapGet("/setcookie", (HttpContext context) =>
{
    context.Response.Cookies.Append("Last Login", DateTime.UtcNow.ToString("o"), new CookieOptions
    {
        Expires = DateTimeOffset.Now.AddMinutes(30)
    });
    return "Cookie has been set";
});

app.MapGet("/getcookie", (HttpContext context) =>
{
    var loginTime = context.Request.Cookies["Last Login"];
    return loginTime is not null ? $"Hello {loginTime}" : "No cookie found";
});

app.MapGet("/deleteCookie", (HttpContext context) =>
{
    context.Response.Cookies.Delete("Last Login");
    return "Cookie Deleted!";
});*/

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
