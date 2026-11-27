using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using DatabaseConnection.model;
using ExamPlannerServer.Auth;
using ExamPlannerServer.controller;
using ExamPlannerServer.Import;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;

namespace TesProject;

// The access tiers over HTTP:
//   Admin   - read, edit and delete everything
//   Teacher - read everything, edit and delete teacher notes, nothing else
//   Student - only their own information
public class AccessTierTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string SomeElement = "11111111-2222-3333-4444-555555555555"; // an id nothing has

    // ---------- helpers ----------

    private async Task<HttpClient> ClientAsync(string? email, string password = ApiFactory.Password)
    {
        var client = factory.CreateClient();
        if (email is null)
            return client;

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private Task<HttpClient> AdminAsync() => ClientAsync(ApiFactory.PlannerEmail);
    private Task<HttpClient> TeacherAsync() => ClientAsync(ApiFactory.TeacherEmail);
    private Task<HttpClient> StudentAsync() => ClientAsync(ApiFactory.StudentEmail);

    private static async Task<JsonElement> JsonOf(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    private static HttpRequestMessage Request(string method, string url)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), "/api/v1" + url);
        // Bodies are valid, so that only the access rules decide the answer.
        if (method is "PATCH")
            request.Content = JsonContent.Create(new { name = "N", sueCode = "C", notes = (string?)null, classCount = 0 });
        if (method is "PUT")
            request.Content = JsonContent.Create(new { note = "A note" });
        return request;
    }

    // Adds a course with a class, a campus, a planning element (optionally with an exam) and one enrolled student.
    private async Task<(Guid ElementId, int CourseId, int StudentId, int UserId)> SeedAsync(
        string code, bool withExam = false, int studentNumber = 0)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseConnection.DBContext.AppDbContext>();

        var campus = await db.Campuses.FirstOrDefaultAsync() ?? new Campus { Name = "Horsens" };
        var course = new Course { SueCode = code, Prefix = "IT", Name = "Course " + code, ECTS = 5 };
        var element = new PlanningEducationElement
        {
            Id = Guid.NewGuid(), Course = course, Campus = campus, Name = code + " element", Nickmane = "IT-" + code,
            ECTS = 5, StartDate = new DateOnly(2026, 2, 1), EndDate = new DateOnly(2026, 6, 30)
        };
        db.PlanningEducationElement.Add(element);
        db.Class.Add(new Class { Course = course, ClassCode = "IT-" + code, NickName = "IT-" + code });
        if (withExam)
            db.AssessmentEvent.Add(new AssessmentEvent { PlanningEducationElement = element });

        var number = studentNumber != 0 ? studentNumber : Random.Shared.Next(100_000, 800_000);
        var user = new User { Email = $"s{Guid.NewGuid():N}@students.invalid", Name = "STU-" + number, Role = Role.Student };
        var student = new Student { Id = (await db.Student.MaxAsync(s => (int?)s.Id) ?? 0) + 1, User = user, SourceStudentId = number };
        db.Student.Add(student);
        db.Enrollment.Add(new Enrollment { StudentId = student.Id, PlanningElementId = element.Id, Hold = code + "-hold" });
        await db.SaveChangesAsync();

        return (element.Id, course.Id, student.Id, user.Id);
    }

    private Task<int> CountAsync(Func<DatabaseConnection.DBContext.AppDbContext, IQueryable<object>> query) =>
        factory.QueryDatabaseAsync(db => query(db).CountAsync());

    // ---------- the whole matrix: who may call what ----------

    // method, url, then the status for: nobody (not logged in), Admin, Teacher, Student.
    // 404 and 400 mean "allowed in, but there was nothing to do".
    public static TheoryData<string, string, int, int, int, int> Matrix => new()
    {
        { "GET",    "/auth/me",                                         401, 200, 200, 200 },
        { "GET",    "/courses",                                         401, 200, 200, 403 },
        { "GET",    "/courses/999999",                                  401, 404, 404, 403 },
        { "PATCH",  "/courses/999999",                                  401, 404, 403, 403 },
        { "DELETE", "/courses/999999",                                  401, 404, 403, 403 },
        { "POST",   "/courses/import",                                  401, 400, 403, 403 },
        { "POST",   "/imports",                                         401, 400, 403, 403 },
        { "GET",    "/planning-elements",                               401, 200, 200, 403 },
        { "DELETE", "/planning-elements/" + SomeElement,                401, 404, 403, 403 },
        { "PUT",    "/planning-elements/" + SomeElement + "/note",      401, 404, 404, 403 },
        { "DELETE", "/planning-elements/" + SomeElement + "/note",      401, 404, 404, 403 },
        { "GET",    "/students",                                        401, 200, 200, 403 },
        { "GET",    "/students/999999",                                 401, 404, 404, 403 },
        { "DELETE", "/students/999999",                                 401, 404, 403, 403 },
        { "GET",    "/students/me",                                     401, 403, 403, 200 },
    };

    [Theory]
    [MemberData(nameof(Matrix))]
    public async Task EveryEndpoint_AnswersEachRoleAsTheTiersSay(
        string method, string url, int nobody, int admin, int teacher, int student)
    {
        foreach (var (email, expected, who) in new[]
                 {
                     ((string?)null, nobody, "not logged in"), (ApiFactory.PlannerEmail, admin, "Admin"),
                     (ApiFactory.TeacherEmail, teacher, "Teacher"), (ApiFactory.StudentEmail, student, "Student")
                 })
        {
            using var client = await ClientAsync(email);
            var response = await client.SendAsync(Request(method, url));

            Assert.True((int)response.StatusCode == expected,
                $"{method} {url} as {who}: expected {expected}, got {(int)response.StatusCode}");
        }
    }

    [Fact]
    public void EveryControllerAction_RequiresLogin_ExceptTheLoginItself()
    {
        // Guards against a new endpoint that someone forgets to protect.
        var open = new List<string>();
        foreach (var controller in typeof(StudentsController).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ControllerBase))))
        foreach (var action in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .Where(m => m.GetCustomAttributes<HttpMethodAttribute>().Any()))
        {
            var anonymous = action.GetCustomAttribute<AllowAnonymousAttribute>() is not null;
            var protectedByLogin = controller.GetCustomAttribute<AuthorizeAttribute>() is not null
                                   || action.GetCustomAttribute<AuthorizeAttribute>() is not null;
            if (anonymous || !protectedByLogin)
                open.Add($"{controller.Name}.{action.Name}");
        }

        Assert.Equal(["AuthController.Login"], open);
    }

    // ---------- student: only their own information ----------

    [Fact]
    public async Task AStudent_SeesOnlyTheirOwnEnrollments()
    {
        // The factory's student gets one course; another student gets a different one.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DatabaseConnection.DBContext.AppDbContext>();
            var mine = await SeedAsync("MINE");
            db.Enrollment.Add(new Enrollment { StudentId = ApiFactory.StudentNumber, PlanningElementId = mine.ElementId, Hold = "my-hold" });
            await db.SaveChangesAsync();
        }
        await SeedAsync("OTHERS");
        using var student = await StudentAsync();

        var response = await student.GetAsync("/api/v1/students/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await JsonOf(response);
        Assert.Equal(ApiFactory.StudentNumber.ToString(), me.GetProperty("viaId").GetString());
        var enrollment = Assert.Single(me.GetProperty("enrollments").EnumerateArray());
        Assert.Equal("MINE", enrollment.GetProperty("courseCode").GetString());
        Assert.Equal("my-hold", enrollment.GetProperty("hold").GetString());
        Assert.DoesNotContain("OTHERS", await response.Content.ReadAsStringAsync());
        Assert.False(enrollment.TryGetProperty("note", out _)); // teacher notes are not the student's information
    }

    // ---------- teacher: read everything, edit and delete notes only ----------

    [Fact]
    public async Task ATeacher_ReadsEverything()
    {
        var seeded = await SeedAsync("READ", withExam: true);
        using var teacher = await TeacherAsync();

        var elements = await teacher.GetFromJsonAsync<JsonElement>("/api/v1/planning-elements");
        var students = await teacher.GetFromJsonAsync<JsonElement>("/api/v1/students");
        var one = await teacher.GetAsync($"/api/v1/students/{seeded.StudentId}");
        var course = await teacher.GetAsync($"/api/v1/courses/{seeded.CourseId}");

        var element = elements.EnumerateArray().Single(e => e.GetProperty("courseCode").GetString() == "READ");
        Assert.True(element.GetProperty("hasExam").GetBoolean());
        Assert.Equal("Horsens", element.GetProperty("campus").GetString());
        Assert.Contains(students.EnumerateArray(), s => s.GetProperty("id").GetInt32() == seeded.StudentId);
        Assert.Equal(HttpStatusCode.OK, one.StatusCode);
        Assert.Equal(HttpStatusCode.OK, course.StatusCode);
    }

    [Fact]
    public async Task ATeacher_CanWriteAndDeleteTheNoteOfAnExam()
    {
        var seeded = await SeedAsync("NOTE", withExam: true);
        using var teacher = await TeacherAsync();
        var url = $"/api/v1/planning-elements/{seeded.ElementId}/note";

        var written = await teacher.PutAsJsonAsync(url, new { note = "  Bring a calculator  " });

        Assert.Equal(HttpStatusCode.NoContent, written.StatusCode);
        Assert.Equal("Bring a calculator", await NoteAsync(seeded.ElementId)); // trimmed

        var deleted = await teacher.DeleteAsync(url);

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal("", await NoteAsync(seeded.ElementId));                    // emptied...
        Assert.Equal(1, await CountAsync(db => db.AssessmentEvent.Where(a => a.PlanningElementId == seeded.ElementId))); // ...but the exam stays
    }

    private Task<string> NoteAsync(Guid elementId) =>
        factory.QueryDatabaseAsync(db => db.AssessmentEvent.Where(a => a.PlanningElementId == elementId).Select(a => a.Note).SingleAsync());

    [Fact]
    public async Task AnElementWithoutAnExam_HasNoNoteToEdit_AndNoExamIsCreated()
    {
        var seeded = await SeedAsync("NOEXAM", withExam: false);
        using var teacher = await TeacherAsync();

        var response = await teacher.PutAsJsonAsync($"/api/v1/planning-elements/{seeded.ElementId}/note", new { note = "x" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("EXAM_NOT_FOUND", (await JsonOf(response)).GetProperty("code").GetString());
        Assert.Equal(0, await CountAsync(db => db.AssessmentEvent.Where(a => a.PlanningElementId == seeded.ElementId)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AnEmptyNote_IsRejected(string? note)
    {
        var seeded = await SeedAsync("EMPTY" + (note?.Length ?? 9), withExam: true);
        using var teacher = await TeacherAsync();

        var response = await teacher.PutAsJsonAsync($"/api/v1/planning-elements/{seeded.ElementId}/note", new { note });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ATooLongNote_IsRejected_AndNothingChanges()
    {
        var seeded = await SeedAsync("LONG", withExam: true);
        using var teacher = await TeacherAsync();
        var url = $"/api/v1/planning-elements/{seeded.ElementId}/note";

        Assert.Equal(HttpStatusCode.NoContent, (await teacher.PutAsJsonAsync(url, new { note = new string('a', 2000) })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await teacher.PutAsJsonAsync(url, new { note = new string('b', 2001) })).StatusCode);

        Assert.Equal(new string('a', 2000), await NoteAsync(seeded.ElementId));
    }

    [Fact]
    public async Task ATeacher_CannotDeleteOrEditAnythingElse()
    {
        var seeded = await SeedAsync("TKEEP", withExam: true);
        using var teacher = await TeacherAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await teacher.DeleteAsync($"/api/v1/courses/{seeded.CourseId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await teacher.DeleteAsync($"/api/v1/planning-elements/{seeded.ElementId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await teacher.DeleteAsync($"/api/v1/students/{seeded.StudentId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await teacher.PatchAsJsonAsync($"/api/v1/courses/{seeded.CourseId}",
            new { name = "Hacked", sueCode = "X", notes = (string?)null, classCount = 0 })).StatusCode);

        Assert.Equal(1, await CountAsync(db => db.Course.Where(c => c.Id == seeded.CourseId && c.Name == "Course TKEEP")));
        Assert.Equal(1, await CountAsync(db => db.PlanningEducationElement.Where(e => e.Id == seeded.ElementId)));
        Assert.Equal(1, await CountAsync(db => db.Student.Where(s => s.Id == seeded.StudentId)));
    }

    // ---------- admin: delete ----------

    [Fact]
    public async Task Admin_DeletingACourse_RemovesItsElementsClassesEnrollmentsAndExams_AndNothingElse()
    {
        var gone = await SeedAsync("CGONE", withExam: true);
        var kept = await SeedAsync("CKEPT", withExam: true);
        using var admin = await AdminAsync();

        var response = await admin.DeleteAsync($"/api/v1/courses/{gone.CourseId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await CountAsync(db => db.Course.Where(c => c.Id == gone.CourseId)));
        Assert.Equal(0, await CountAsync(db => db.Class.Where(c => c.CourseId == gone.CourseId)));
        Assert.Equal(0, await CountAsync(db => db.PlanningEducationElement.Where(e => e.Id == gone.ElementId)));
        Assert.Equal(0, await CountAsync(db => db.Enrollment.Where(e => e.PlanningElementId == gone.ElementId)));
        Assert.Equal(0, await CountAsync(db => db.AssessmentEvent.Where(a => a.PlanningElementId == gone.ElementId)));
        Assert.Equal(1, await CountAsync(db => db.Student.Where(s => s.Id == gone.StudentId))); // students are not part of a course
        Assert.Equal(1, await CountAsync(db => db.Enrollment.Where(e => e.PlanningElementId == kept.ElementId)));
        Assert.Equal(HttpStatusCode.NotFound, (await admin.DeleteAsync($"/api/v1/courses/{gone.CourseId}")).StatusCode);
    }

    [Fact]
    public async Task Admin_DeletingAPlanningElement_RemovesItsClassAndEnrollments_ButKeepsTheCourse()
    {
        var gone = await SeedAsync("EGONE", withExam: true);
        using var admin = await AdminAsync();

        var response = await admin.DeleteAsync($"/api/v1/planning-elements/{gone.ElementId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await CountAsync(db => db.PlanningEducationElement.Where(e => e.Id == gone.ElementId)));
        Assert.Equal(0, await CountAsync(db => db.Enrollment.Where(e => e.PlanningElementId == gone.ElementId)));
        Assert.Equal(0, await CountAsync(db => db.AssessmentEvent.Where(a => a.PlanningElementId == gone.ElementId)));
        Assert.Equal(0, await CountAsync(db => db.Class.Where(c => c.ClassCode == "IT-EGONE")));
        Assert.Equal(1, await CountAsync(db => db.Course.Where(c => c.Id == gone.CourseId)));
    }

    [Fact]
    public async Task Admin_DeletingOneOfTwoElementsWithTheSameClass_KeepsTheClass()
    {
        var first = await SeedAsync("SHARED");
        Guid secondId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DatabaseConnection.DBContext.AppDbContext>();
            var original = await db.PlanningEducationElement.AsNoTracking().SingleAsync(e => e.Id == first.ElementId);
            secondId = Guid.NewGuid();
            original.Id = secondId;
            db.PlanningEducationElement.Add(original);
            await db.SaveChangesAsync();
        }
        using var admin = await AdminAsync();

        await admin.DeleteAsync($"/api/v1/planning-elements/{first.ElementId}");

        Assert.Equal(1, await CountAsync(db => db.Class.Where(c => c.ClassCode == "IT-SHARED")));
        Assert.Equal(1, await CountAsync(db => db.PlanningEducationElement.Where(e => e.Id == secondId)));
    }

    [Fact]
    public async Task Admin_DeletingAStudent_RemovesTheirAccountAndEnrollments_AndTheyCannotLogInAfterwards()
    {
        var other = await SeedAsync("SKEPT");
        string email = $"delete-me-{Guid.NewGuid():N}@test.dk";
        int studentId, userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DatabaseConnection.DBContext.AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.IPasswordHasher<User>>();
            var user = new User { Email = email, Name = "Doomed", Role = Role.Student };
            user.HashedPassword = hasher.HashPassword(user, ApiFactory.Password);
            var student = new Student { Id = 700_000, User = user, SourceStudentId = 700_000 };
            db.Student.Add(student);
            db.Enrollment.Add(new Enrollment { StudentId = 700_000, PlanningElementId = other.ElementId, Hold = "h" });
            await db.SaveChangesAsync();
            (studentId, userId) = (student.Id, user.Id);
        }
        using var admin = await AdminAsync();

        var response = await admin.DeleteAsync($"/api/v1/students/{studentId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await CountAsync(db => db.Student.Where(s => s.Id == studentId)));
        Assert.Equal(0, await CountAsync(db => db.User.Where(u => u.Id == userId)));
        Assert.Equal(0, await CountAsync(db => db.Enrollment.Where(e => e.StudentId == studentId)));
        Assert.Equal(1, await CountAsync(db => db.Enrollment.Where(e => e.StudentId == other.StudentId)));
        using var anonymous = factory.CreateClient();
        var login = await anonymous.PostAsJsonAsync("/api/v1/auth/login", new { email, password = ApiFactory.Password });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    // ---------- the development accounts ----------

    private Task SeedDevAccountsAsync(string studentEmail, int studentNumber, string teacherEmail)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Seed:TeacherEmail"] = teacherEmail, ["Seed:TeacherPassword"] = "Seeded-Teacher1!",
            ["Seed:StudentEmail"] = studentEmail, ["Seed:StudentPassword"] = "Seeded-Student1!",
            ["Seed:StudentNumber"] = studentNumber.ToString()
        }).Build();
        return DevAccountSeeder.SeedAsync(factory.Services, config, NullLogger.Instance);
    }

    [Fact]
    public async Task DevelopmentAccounts_CanLogInWithTheirRoles_AndSeedingTwiceMakesNoDuplicates()
    {
        await SeedDevAccountsAsync("dev.student@test.dk", 880_001, "dev.teacher@test.dk");
        await SeedDevAccountsAsync("dev.student@test.dk", 880_001, "dev.teacher@test.dk");

        using var teacher = await ClientAsync("dev.teacher@test.dk", "Seeded-Teacher1!");
        using var student = await ClientAsync("dev.student@test.dk", "Seeded-Student1!");

        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync("/api/v1/students")).StatusCode);
        var me = await student.GetFromJsonAsync<JsonElement>("/api/v1/students/me");
        Assert.Equal("880001", me.GetProperty("viaId").GetString());
        Assert.Equal(1, await CountAsync(db => db.User.Where(u => u.Email == "dev.student@test.dk")));
        Assert.Equal(1, await CountAsync(db => db.User.Where(u => u.Email == "dev.teacher@test.dk")));
    }

    [Fact]
    public async Task DevelopmentStudent_TakesOverAnImportedStudent_WithoutCreatingASecondOne()
    {
        var imported = await SeedAsync("IMPORTED", studentNumber: 880_002); // a user without a password, as imports make

        await SeedDevAccountsAsync("takeover@test.dk", 880_002, "takeover.teacher@test.dk");

        Assert.Equal(1, await CountAsync(db => db.Student.Where(s => s.SourceStudentId == 880_002)));
        using var student = await ClientAsync("takeover@test.dk", "Seeded-Student1!");
        var me = await student.GetFromJsonAsync<JsonElement>("/api/v1/students/me");
        Assert.Equal(imported.StudentId, me.GetProperty("id").GetInt32());
        Assert.Equal("IMPORTED", Assert.Single(me.GetProperty("enrollments").EnumerateArray()).GetProperty("courseCode").GetString());
    }

    [Fact]
    public async Task DevelopmentStudent_GetsTheirEnrollmentsWhenTheOpslagsarkIsImportedLater()
    {
        await SeedDevAccountsAsync("later@test.dk", 880_003, "later.teacher@test.dk");
        var row = StudentWorkbook.ValidRow(student: "STU-880003", nickname: "IT-LATER");
        await SeedAsync("LATER"); // a planning element with Kaldenavn IT-LATER

        using (var scope = factory.Services.CreateScope())
        {
            var importer = scope.ServiceProvider.GetRequiredService<IImportService>();
            using var file = StudentWorkbook.Build(row);
            await importer.ImportAsync(new Microsoft.AspNetCore.Http.FormFile(file, 0, file.Length, "file", "o.xlsx"));
        }

        using var student = await ClientAsync("later@test.dk", "Seeded-Student1!");
        var me = await student.GetFromJsonAsync<JsonElement>("/api/v1/students/me");
        Assert.Equal("LATER", Assert.Single(me.GetProperty("enrollments").EnumerateArray()).GetProperty("courseCode").GetString());
        Assert.Equal(1, await CountAsync(db => db.Student.Where(s => s.SourceStudentId == 880_003)));
    }
}
