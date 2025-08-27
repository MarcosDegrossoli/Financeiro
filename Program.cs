using Microsoft.EntityFrameworkCore;
using Financeiro.Data;

var builder = WebApplication.CreateBuilder(args);

// Adicione os servi�os ao cont�iner.
builder.Services.AddControllersWithViews();

// Adicione o servi�o DbContext e configure-o para usar PostgreSQL
builder.Services.AddDbContext<OfxDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// Configure o pipeline de requisi��es HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();