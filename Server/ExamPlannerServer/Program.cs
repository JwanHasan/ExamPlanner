using DatabaseConnection.DBContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddScoped<IStudentRepo,StudentRepo>();

builder.Services.AddDbContext<AppDbContext>(options=> options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
));
var app = builder.Build();

app.MapControllers();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//app.UseHttpsRedirection();


app.Run();

