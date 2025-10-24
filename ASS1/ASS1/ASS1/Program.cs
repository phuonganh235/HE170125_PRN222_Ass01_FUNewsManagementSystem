using ASS1.BLL.Service;
using ASS1.DAL.Models;
using ASS1.DAL.Repository;
using ASS1.Models;
using ASS1.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// Add configuration
builder.Services.Configure<AdminAccountConfig>(builder.Configuration.GetSection("DefaultAdmin"));

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Add repositories and services
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));

// Add admin account service
builder.Services.AddScoped<IAdminAccountService, AdminAccountService>();

// Add authentication service
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Add account management service
builder.Services.AddScoped<IAccountManagementService, AccountManagementService>();

// Add category management service
builder.Services.AddScoped<ICategoryManagementService, CategoryManagementService>();

// Add news article management service
builder.Services.AddScoped<INewsArticleManagementService, NewsArticleManagementService>();

// Add tag management service
builder.Services.AddScoped<ITagManagementService, TagManagementService>();

// Add news-tag link service
builder.Services.AddScoped<INewsTagLinkService, NewsTagLinkService>();

// Add reporting service
builder.Services.AddScoped<IReportingService, ReportingService>();

// Add related news service
builder.Services.AddScoped<IRelatedNewsService, RelatedNewsService>();

var app = builder.Build();

// Initialize default admin account
using (var scope = app.Services.CreateScope())
{
    try
    {
        var adminService = scope.ServiceProvider.GetRequiredService<IAdminAccountService>();
        await adminService.InitializeDefaultAdminAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the default admin account");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
