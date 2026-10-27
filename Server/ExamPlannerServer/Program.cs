using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using DatabaseConnection.repo;
using ExamPlannerServer.Auth;
using ExamPlannerServer.Contracts;
using ExamPlannerServer.Import;
using ExamPlannerServer.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Invalid request bodies get the contract's error shape (API_CONTRACT.md section 4) instead of ASP.NET's default.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var fields = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToDictionary(entry => entry.Key, entry => entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new ApiError(ErrorCodes.InvalidRequest, "The request is not valid.", fields));
    };
});

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
));

// The teammates' repositories (DatabaseConnection/repo), so controllers and services can use them.
builder.Services.AddScoped<IAssessmentEventRepo, AssessmentEventRepo>();
builder.Services.AddScoped<IAssessmentHandInRepo, AssessmentHandInRepo>();
builder.Services.AddScoped<IClassRepo, ClassRepo>();
builder.Services.AddScoped<ICourseRepo, CourseRepo>();
builder.Services.AddScoped<IEnrollmentRepo, EnrollmentRepo>();
builder.Services.AddScoped<IExamDayRepo, ExamDayRepo>();
builder.Services.AddScoped<IExamSessionRepo, ExamSessionRepo>();
builder.Services.AddScoped<IPlanningEducationElementRepo, PlanningEducationElementRepo>();
builder.Services.AddScoped<IRoomRepo, RoomRepo>();
builder.Services.AddScoped<IScheduleRepo, ScheduleRepo>();
builder.Services.AddScoped<IScheduleReviewRepo, ScheduleReviewRepo>();
builder.Services.AddScoped<IStudentExamAssignmentRepo, StudentExamAssignmentRepo>();
builder.Services.AddScoped<IStudentRepo, StudentRepo>();
builder.Services.AddScoped<ITeacherAssignmentRepo, TeacherAssignmentRepo>();
builder.Services.AddScoped<ITeacherConstraintRepo, TeacherConstraintRepo>();
builder.Services.AddScoped<ITeacherRepo, TeacherRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IExcelImportParser, ExcelImportParser>();
builder.Services.AddScoped<IImportService, ImportService>();

// Authentication: JWT in the "Authorization: Bearer" header (API_CONTRACT.md 3.2) 
var jwt = builder.Configuration.GetSection(JwtSettings.Section).Get<JwtSettings>() ?? new JwtSettings();
jwt.Validate();

// Exam planners ("Admin" in the API) are listed by email, because the database has no Admin role.
var admins = builder.Configuration.GetSection(AdminSettings.Section).Get<AdminSettings>() ?? new AdminSettings();

builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton(admins);
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep "sub", "name" and "role" exactly as they are in the token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.SigningKey(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = AuthClaims.Name,
            RoleClaimType = AuthClaims.Role
        };

        options.Events = new JwtBearerEvents
        {
            // 401 and 403 with the contract's error body instead of an empty response.
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ApiError(ErrorCodes.Unauthorized, "You need to log in."));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(
                    new ApiError(ErrorCodes.Forbidden, "Your account does not have access to this."));
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // Outside development, never send stack traces to the browser.
    app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ApiError(ErrorCodes.InternalError, "Something went wrong on the server."));
    }));
}

await AdminSeeder.SeedAsync(app.Services, app.Configuration, app.Logger);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
