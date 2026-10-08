using System.Text;
using DatabaseConnection.DBContext;
using ExamPlannerServer.Auth;
using ExamPlannerServer.Import;
using ExamPlannerServer.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
));

builder.Services.AddScoped<IStudentRepo, StudentRepo>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IExcelImportParser, ExcelImportParser>();
//builder.Services.AddScoped<IImportService, ImportService>();

// Authentication (JWT)
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await DevSeeder.SeedAsync(app.Services, app.Configuration, app.Logger);
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();