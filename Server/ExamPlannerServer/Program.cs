using DatabaseConnection.DBContext;
using ExamPlannerServer.Import;
using ExamPlannerServer.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IExcelImportParser, ExcelImportParser>();

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
app.UseHttpsRedirection();


app.Run();

