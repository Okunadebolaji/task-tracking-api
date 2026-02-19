using TaskTrackingApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true; 
    });

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICompanyCodeGenerator, CompanyCodeGenerator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Register DbContext
builder.Services.AddDbContext<TaskTrackingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 3. Define the CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("NetlifyPolicy", p =>
        p.WithOrigins("https://your-app-name.netlify.app") // <--- REPLACE WITH YOUR ACTUAL NETLIFY URL
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

// 4. Configure Middleware Order (CRITICAL)
// UseCors must come before Authorization
app.UseCors("NetlifyPolicy");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Tracking API v1");
    c.RoutePrefix = string.Empty; 
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");