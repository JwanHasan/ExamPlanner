using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TesProject;

// The courses that the website's "Imported data" page shows: GET and PATCH /api/v1/courses, on data
// that an import just created. This is the whole path from the Excel file to that page.
public class CoursesApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string ImportUrl = "/api/v1/imports";
    private const string CoursesUrl = "/api/v1/courses";

    // ---------- helpers ----------

    private async Task<string> TokenAsync(string email)
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = ApiFactory.Password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString()!;
    }

    private HttpClient ClientWith(string? token)
    {
        var client = factory.CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static MultipartFormDataContent FileForm(object?[] row) =>
        new() { { new ByteArrayContent(TestWorkbook.Build(row).ToArray()), "file", "export.xlsx" } };

    // Imports one row as the planner and returns the course that was created for it.
    private async Task<JsonElement> ImportOneCourseAsync(string plannerToken, object?[] row)
    {
        using var client = ClientWith(plannerToken);
        using var form = FileForm(row);
        (await client.PostAsync(ImportUrl, form)).EnsureSuccessStatusCode();

        var courses = await client.GetFromJsonAsync<JsonElement>(CoursesUrl);
        var sueCode = ((string)row[3]!).Split(' ')[0]; // the course code is the first word of Navn
        return courses.EnumerateArray().Single(c => c.GetProperty("sueCode").GetString() == sueCode);
    }

    private static object EditBody(string name, string sueCode) =>
        new { name, sueCode, notes = (string?)null, classCount = 1 };

    // ---------- reading ----------

    [Fact]
    public async Task Courses_RequireALogin()
    {
        using var client = ClientWith(null);

        var response = await client.GetAsync(CoursesUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ImportedCourse_AppearsInTheCourseList_WithTheFieldsThePageShows()
    {
        var planner = await TokenAsync(ApiFactory.PlannerEmail);

        var course = await ImportOneCourseAsync(planner, TestWorkbook.UniqueRow());

        // The property names are what the website's Course type reads.
        Assert.StartsWith("Unique course", course.GetProperty("name").GetString());
        Assert.Equal(5, course.GetProperty("ects").GetInt32());
        Assert.Equal(1, course.GetProperty("classCount").GetInt32());
        Assert.True(course.GetProperty("id").GetInt32() > 0);
        Assert.True(course.TryGetProperty("semester", out _));
        Assert.True(course.TryGetProperty("notes", out _));
    }

    [Fact]
    public async Task Teacher_CanReadCourses_AndOneCourseById()
    {
        var planner = await TokenAsync(ApiFactory.PlannerEmail);
        var teacher = await TokenAsync(ApiFactory.TeacherEmail);
        var imported = await ImportOneCourseAsync(planner, TestWorkbook.UniqueRow());
        var id = imported.GetProperty("id").GetInt32();
        using var client = ClientWith(teacher);

        var one = await client.GetAsync($"{CoursesUrl}/{id}");

        Assert.Equal(HttpStatusCode.OK, one.StatusCode);
        Assert.Equal(id, (await one.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task UnknownCourse_Returns404_CourseNotFound()
    {
        using var client = ClientWith(await TokenAsync(ApiFactory.PlannerEmail));

        var response = await client.GetAsync($"{CoursesUrl}/987654");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("COURSE_NOT_FOUND", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    // ---------- editing (a planner corrects a course on the Imported data page) ----------

    [Fact]
    public async Task Planner_CanEditNameAndCode_AndTheCampusSurvives()
    {
        var planner = await TokenAsync(ApiFactory.PlannerEmail);
        var imported = await ImportOneCourseAsync(planner, TestWorkbook.UniqueRow());
        var id = imported.GetProperty("id").GetInt32();
        using var client = ClientWith(planner);

        var response = await client.PatchAsJsonAsync($"{CoursesUrl}/{id}", EditBody("Renamed by a planner", "NEW1"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Renamed by a planner", json.GetProperty("name").GetString());
        Assert.Equal("NEW1", json.GetProperty("sueCode").GetString());

        // What the planner did not change is still there. Campus is a required column, and the update
        // has to carry it along (it was missing from the call once, and the API did not even build).
        var course = await factory.QueryDatabaseAsync(db => db.Course.SingleAsync(c => c.Id == id));
        Assert.Equal("Horsens", course.Campus);
        Assert.Equal("IT", course.Prefix);
        Assert.Equal(5, course.ECTS);
    }

    [Fact]
    public async Task TheEditedCourse_IsNotDuplicatedByTheNextImport()
    {
        var planner = await TokenAsync(ApiFactory.PlannerEmail);
        var row = TestWorkbook.UniqueRow();
        var imported = await ImportOneCourseAsync(planner, row);
        var id = imported.GetProperty("id").GetInt32();
        using var client = ClientWith(planner);
        await client.PatchAsJsonAsync($"{CoursesUrl}/{id}", EditBody("Corrected", "FIXED1"));

        // The source changes the row, and the file is imported again.
        var changed = TestWorkbook.WithValue(row, "Kontrolsum for række", "changed-at-the-source");
        using var form = FileForm(changed);
        (await client.PostAsync(ImportUrl, form)).EnsureSuccessStatusCode();

        var courses = await client.GetFromJsonAsync<JsonElement>(CoursesUrl);
        var course = Assert.Single(courses.EnumerateArray().Where(c => c.GetProperty("id").GetInt32() == id));
        Assert.Equal("FIXED1", course.GetProperty("sueCode").GetString());   // the planner's edit survived
        Assert.Equal("Corrected", course.GetProperty("name").GetString());
        Assert.Equal(1, course.GetProperty("classCount").GetInt32());        // and no second class appeared
        Assert.DoesNotContain(courses.EnumerateArray(),                        // nor a second course under the old code
            c => c.GetProperty("sueCode").GetString() == ((string)row[3]!).Split(' ')[0]);
    }

    [Fact]
    public async Task Teacher_CannotEditCourses()
    {
        var planner = await TokenAsync(ApiFactory.PlannerEmail);
        var imported = await ImportOneCourseAsync(planner, TestWorkbook.UniqueRow());
        using var client = ClientWith(await TokenAsync(ApiFactory.TeacherEmail));

        var response = await client.PatchAsJsonAsync($"{CoursesUrl}/{imported.GetProperty("id").GetInt32()}", EditBody("Hacked", "X1"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EmptyName_Returns400()
    {
        var planner = await TokenAsync(ApiFactory.PlannerEmail);
        var imported = await ImportOneCourseAsync(planner, TestWorkbook.UniqueRow());
        using var client = ClientWith(planner);

        var response = await client.PatchAsJsonAsync($"{CoursesUrl}/{imported.GetProperty("id").GetInt32()}", EditBody("  ", "X1"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EditingAnUnknownCourse_Returns404()
    {
        using var client = ClientWith(await TokenAsync(ApiFactory.PlannerEmail));

        var response = await client.PatchAsJsonAsync($"{CoursesUrl}/987654", EditBody("Name", "X1"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
