using TaskTrackingApi.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true; // optional, for readable JSON
    });

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<ICompanyCodeGenerator, CompanyCodeGenerator>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<TaskTrackingDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("prod", p =>
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .AllowAnyOrigin());
});
var app = builder.Build();
app.UseCors("prod");






// This makes Swagger available in Production/Render
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Tracking API v1");
    c.RoutePrefix = string.Empty; // This makes Swagger the home page!
});


app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");

