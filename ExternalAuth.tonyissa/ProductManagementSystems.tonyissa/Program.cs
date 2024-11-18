using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductManagementSystems.tonyissa.Data;
using ProductManagementSystems.tonyissa.Models;
using ProductManagementSystems.tonyissa.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<ZohoSettings>(builder.Configuration.GetSection("ZohoSettings"));
builder.Services.AddSingleton<IEmailSender, EmailSender>();

builder.Services.AddAuthentication()
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["FacebookSettings:AppId"]!;
        options.AppSecret = builder.Configuration["FacebookSettings:AppSecret"]!;
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    })
    .AddTwitter(options =>
    {
        options.ConsumerKey = builder.Configuration["TwitterSettings:ClientId"]!;
        options.ConsumerSecret = builder.Configuration["TwitterSettings:ClientSecret"]!;
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["GoogleSettings:ClientId"]!;
        options.ClientSecret = builder.Configuration["GoogleSettings:ClientSecret"]!;
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["MicrosoftSettings:ClientId"]!;
        options.ClientSecret = builder.Configuration["MicrosoftSettings:ClientSecret"]!;
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    })
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["GithubSettings:ClientId"]!;
        options.ClientSecret = builder.Configuration["GithubSettings:ClientSecret"]!;
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));

builder.Services.AddRazorPages(options => 
{
    options.Conventions.AuthorizeFolder("/Games");
    options.Conventions.AuthorizeFolder("/Admin", "AdminPolicy");
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    SeedData.InitializeData(context);

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.InitializeRolesAsync(roleManager);
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();