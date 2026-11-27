using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace TesProject;

// The Opslagsark over HTTP, through the whole API: login, roles, upload, importers, database.
// The JSON is checked the way the website reads it, so a rename on either side fails here.
public class OpslagsarkApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string ImportUrl = "/api/v1/imports";

    // ---------- helpers ----------

    private async Task<string> TokenAsync(string email)
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = ApiFactory.Password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString()!;
    }

    private async Task<HttpResponseMessage> PostAsync(byte[] file, string token, string fileName = "upload.xlsx")
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var form = new MultipartFormDataContent { { new ByteArrayContent(file), "file", fileName } };
        return await client.PostAsync(ImportUrl, form);
    }

    private static async Task<JsonElement> JsonOf(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    // The planning elements a student row needs (the Opslagsark only imports rows whose element is in the database).
    private async Task ImportPlanningElementsAsync(string token, params object?[][] studentRows)
    {
        var column = Array.FindIndex(StudentWorkbook.Headers, h => h.StartsWith("Kaldenavn"));
        var elements = studentRows.Select(r => (string)r[column]!).Distinct()
            .Select(n => TestWorkbook.ValidRow(nickname: n)).ToArray();
        (await PostAsync(TestWorkbook.Build(elements).ToArray(), token, "export.xlsx")).EnsureSuccessStatusCode();
    }

    // ---------- importing ----------

    [Fact]
    public async Task Planner_ImportsAnOpslagsark_AndGetsTheCountersTheWebsiteShows()
    {
        var planner = await TokenAsync(ApiFactory.PlannerEmail);
        var rows = new[]
        {
            StudentWorkbook.UniqueRow(), StudentWorkbook.UniqueRow(), StudentWorkbook.UniqueRow()
        };
        await ImportPlanningElementsAsync(planner, rows);

        var response = await PostAsync(StudentWorkbook.Build(rows).ToArray(), planner, "opslagsark.xlsx");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await JsonOf(response);
        Assert.Equal("opslagsark.xlsx", json.GetProperty("fileName").GetString());
        Assert.Equal(3, json.GetProperty("totalRows").GetInt32());
        Assert.Equal(3, json.GetProperty("importedRows").GetInt32());
        Assert.Equal(0, json.GetProperty("skippedRows").GetInt32());

        // The names below are exactly what the website's ImportResult reads.
        var created = json.GetProperty("created");
        Assert.Equal(3, created.GetProperty("students").GetInt32());
        Assert.Equal(3, created.GetProperty("enrollments").GetInt32());
        Assert.Equal(0, created.GetProperty("courses").GetInt32());
        Assert.Equal(0, created.GetProperty("classes").GetInt32());

        // ...and the contract's own fields (API_CONTRACT.md 6.1).
        Assert.True(json.GetProperty("success").GetBoolean());
        Assert.Equal(3, json.GetProperty("studentsImported").GetInt32());

        var codes = rows.Select(r => (string)r[Array.FindIndex(StudentWorkbook.Headers, h => h == "Anonymt Student-ID")]!).ToList();
        var students = await factory.QueryDatabaseAsync(db =>
            db.Student.Include(s => s.User).Where(s => codes.Contains(s.User.Name)).ToListAsync());
        Assert.Equal(3, students.Count);
        Assert.Equal(3, await factory.QueryDatabaseAsync(db => db.Enrollment.CountAsync(e => students.Select(s => s.Id).Contains(e.StudentId))));
    }

    [Fact]
    public async Task OpslagsarkWithoutItsPlanningElements_IsOk_ButEverythingIsSkipped_WithAClearReason()
    {
        var planner = await TokenAsync(ApiFactory.PlannerEmail);

        var response = await PostAsync(StudentWorkbook.Build(StudentWorkbook.UniqueRow(), StudentWorkbook.UniqueRow()).ToArray(), planner);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await JsonOf(response);
        Assert.Equal(0, json.GetProperty("importedRows").GetInt32());
        Assert.Equal(2, json.GetProperty("skippedRows").GetInt32());
        Assert.False(json.GetProperty("success").GetBoolean());   // the website then says "Nothing was imported."

        var issue = Assert.Single(json.GetProperty("issues").EnumerateArray().Where(i => i.GetProperty("row").GetInt32() > 0)
            .GroupBy(i => i.GetProperty("message").GetString()!.Split('\'')[1]).First());
        Assert.Equal("error", issue.GetProperty("severity").GetString());
        Assert.Contains("planning-element export", issue.GetProperty("message").GetString());
    }

    [Fact]
    public async Task DisagreeingAttempts_ReachTheWebsiteAsOneWarningForTheWholeFile()
    {
        var planner = await TokenAsync(ApiFactory.PlannerEmail);
        var first = StudentWorkbook.ValidRow(student: "STU-200001", nickname: "KA61-ATT1-S26", attempts: 0);
        var second = StudentWorkbook.ValidRow(student: "STU-200001", nickname: "KA61-ATT2-S26", attempts: 2);
        await ImportPlanningElementsAsync(planner, first, second);

        var json = await JsonOf(await PostAsync(StudentWorkbook.Build(first, second).ToArray(), planner));

        var warning = Assert.Single(json.GetProperty("issues").EnumerateArray());
        Assert.Equal("warning", warning.GetProperty("severity").GetString());   // lowercase: the website's IssueSeverity
        Assert.Equal(0, warning.GetProperty("row").GetInt32());                  // row 0 = the file as a whole
        Assert.Equal("Antal forsøg", warning.GetProperty("column").GetString());
        Assert.Equal(0, json.GetProperty("skippedRows").GetInt32());
        Assert.StartsWith("File (Antal forsøg)", Assert.Single(json.GetProperty("warnings").EnumerateArray()).GetString());
    }

    [Fact]
    public async Task Teacher_CannotImportAnOpslagsark()
    {
        var teacher = await TokenAsync(ApiFactory.TeacherEmail);

        var response = await PostAsync(StudentWorkbook.Build(StudentWorkbook.UniqueRow()).ToArray(), teacher);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- security ----------

    [Fact]
    public async Task ImportedStudents_CannotLogIn()
    {
        var planner = await TokenAsync(ApiFactory.PlannerEmail);
        var row = StudentWorkbook.ValidRow(student: "STU-300001", nickname: "KA61-LOGIN-S26");
        await ImportPlanningElementsAsync(planner, row);
        (await PostAsync(StudentWorkbook.Build(row).ToArray(), planner)).EnsureSuccessStatusCode();
        var user = await factory.QueryDatabaseAsync(db => db.User.SingleAsync(u => u.Email == "stu-300001@students.invalid"));
        Assert.Equal(Role.Student, user.Role);   // the account exists...

        using var client = factory.CreateClient();
        foreach (var password in new[] { "x", "password", "STU-300001", ApiFactory.Password })
        {
            var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = user.Email, password });

            Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);   // ...but nothing opens it
            Assert.Equal("INVALID_CREDENTIALS", (await JsonOf(login)).GetProperty("code").GetString());
        }
    }

    // ---------- the real files over HTTP ----------

    [RealBothFilesFact]
    public async Task RealFiles_OverHttp_PlanningElementsFirstThenTheOpslagsark()
    {
        var planner = await TokenAsync(ApiFactory.PlannerEmail);
        await using (var export = new FileStream(ExcelTest.path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            var copy = new MemoryStream();
            await export.CopyToAsync(copy);
            (await PostAsync(copy.ToArray(), planner, "export.xlsx")).EnsureSuccessStatusCode();
        }

        var enrollmentsBefore = await factory.QueryDatabaseAsync(db => db.Enrollment.CountAsync()); // the database is shared by this class's tests
        var bytes = await File.ReadAllBytesAsync(OpslagsarkFile.Path);
        var response = await PostAsync(bytes, planner, "Opslagsark.xlsx");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await JsonOf(response);
        Assert.Equal(6411, json.GetProperty("totalRows").GetInt32());
        Assert.Equal(2506, json.GetProperty("importedRows").GetInt32());
        Assert.Equal(3905, json.GetProperty("skippedRows").GetInt32());
        Assert.Equal(623, json.GetProperty("created").GetProperty("students").GetInt32());
        Assert.Equal(2506, json.GetProperty("created").GetProperty("enrollments").GetInt32());
        Assert.True(json.GetProperty("success").GetBoolean());

        // 267 missing elements (one line each) and one warning about attempts.
        var issues = json.GetProperty("issues").EnumerateArray().ToList();
        Assert.Equal(268, issues.Count);
        Assert.Equal(267, issues.Count(i => i.GetProperty("severity").GetString() == "error"));
        Assert.Equal(1, issues.Count(i => i.GetProperty("severity").GetString() == "warning"));
        Assert.Equal(2506, await factory.QueryDatabaseAsync(db => db.Enrollment.CountAsync()) - enrollmentsBefore);
    }
}
