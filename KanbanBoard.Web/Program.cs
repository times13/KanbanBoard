using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.AccesDonnee.Implementations;
using KanbanBoard.LibrairieMetier.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using KanbanBoard.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();

// -- Enregistrement du DbContext (EF Core / SQL Server) --
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("KanbanBoardDb")
    )
);

// -- Repositories (Pattern Data Access) --
builder.Services.AddScoped<IBoardDA, BoardDA>();
builder.Services.AddScoped<IColumnDA, ColumnDA>();
builder.Services.AddScoped<ICardDA, CardDA>();
builder.Services.AddScoped<ICommentDA, CommentDA>();
builder.Services.AddScoped<ICardReadDA, CardReadDA>();

// -- Authentification par cookie (maison) --
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapHub<KanbanHub>("/kanbanHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
