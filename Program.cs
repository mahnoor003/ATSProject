using ATSProject.Data;
using ATSProject.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ATSContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<EmailService>();
//builder.Services.AddSingleton<EmailService>();

builder.Services.AddHostedService<EmailPollingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Read PORT from environment (Railway sets this automatically)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

// Make Kestrel listen on that port
builder.WebHost.UseUrls($"http://*:{port}");


// ? Only one Run at the end
app.Run();
