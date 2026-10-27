using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TesProject;

// All layers at once: HTTP request -> JWT authentication and roles -> ImportController ->
// ImportService -> ExcelImportParser -> database. The responses are checked the way the
// website reads them (JSON property names and values), so a rename on either side fails here.
public class ImportApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string ImportUrl = "/api/v1/imports";

    // ---------- helpers ----------

    private async Task<string> LoginAsync(string email, string password = ApiFactory.Password)
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("accessToken").GetString()!;
    }

    private async Task<HttpResponseMessage> PostImportAsync(byte[]? file, string fileName = "export.xlsx", string? token = null)
    {
        using var client = factory.CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var form = new MultipartFormDataContent();
        if (file is not null)
            form.Add(new ByteArrayContent(file), "file", fileName); // the field name is "file", as in the website
        else
            form.Add(new StringContent("hello"), "somethingElse");  // a form that has no "file" part

        return await client.PostAsync(ImportUrl, form);
    }

    // A request that is not a form at all (no body, no multipart boundary).
    private async Task<HttpResponseMessage> PostWithoutFormAsync(string token)
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.PostAsync(ImportUrl, new StringContent("", System.Text.Encoding.UTF8, "multipart/form-data"));
    }

    private static byte[] Workbook(params object?[][] rows) => TestWorkbook.Build(rows).ToArray();

    private static async Task<JsonElement> JsonOf(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    private Task<int> CountRowsAsync(IEnumerable<Guid> ids) =>
        factory.QueryDatabaseAsync(db => db.PlanningEducationElement.CountAsync(e => ids.Contains(e.Id)));

    private async Task<string> PlannerTokenAsync() => await LoginAsync(ApiFactory.PlannerEmail);

    // ---------- who may import ----------

    [Fact]
    public async Task WithoutLogin_Returns401_WithTheContractsErrorBody()
    {
        var response = await PostImportAsync(Workbook(TestWorkbook.ValidRow()), token: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("UNAUTHORIZED", (await JsonOf(response)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task WithAnInvalidToken_Returns401()
    {
        var response = await PostImportAsync(Workbook(TestWorkbook.ValidRow()), token: "not.a.token");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AsTeacher_Returns403_AndNothingIsSaved()
    {
        var id = Guid.NewGuid();
        var token = await LoginAsync(ApiFactory.TeacherEmail);

        var response = await PostImportAsync(Workbook(TestWorkbook.ValidRow(id)), token: token);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("FORBIDDEN", (await JsonOf(response)).GetProperty("code").GetString());
        Assert.Equal(0, await CountRowsAsync([id]));
    }

    // ---------- the import itself ----------

    [Fact]
    public async Task Planner_Imports_AndTheRowsEndUpInTheDatabase()
    {
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var token = await PlannerTokenAsync();

        var response = await PostImportAsync(Workbook(ids.Select(id => TestWorkbook.ValidRow(id)).ToArray()), token: token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await JsonOf(response);

        // The names below are exactly what the website's TypeScript types (ImportResult) read.
        Assert.Equal("export.xlsx", json.GetProperty("fileName").GetString());
        Assert.Equal(3, json.GetProperty("totalRows").GetInt32());
        Assert.Equal(3, json.GetProperty("importedRows").GetInt32());
        Assert.Equal(0, json.GetProperty("skippedRows").GetInt32());
        var created = json.GetProperty("created");
        Assert.Equal(3, created.GetProperty("classes").GetInt32());
        Assert.Equal(0, created.GetProperty("courses").GetInt32());
        Assert.Equal(0, created.GetProperty("students").GetInt32());
        Assert.Equal(0, created.GetProperty("enrollments").GetInt32());
        Assert.Equal(0, created.GetProperty("lecturers").GetInt32());
        Assert.Equal(0, created.GetProperty("exams").GetInt32());
        Assert.Equal(0, json.GetProperty("issues").GetArrayLength());

        // ...and the contract's own fields (API_CONTRACT.md 6.1) are there too.
        Assert.True(json.GetProperty("success").GetBoolean());
        Assert.Equal(3, json.GetProperty("classesImported").GetInt32());

        Assert.Equal(3, await CountRowsAsync(ids));
    }

    [Fact]
    public async Task ImportingTheSameFileTwice_SecondTimeCreatesNothing()
    {
        var rows = new[] { TestWorkbook.ValidRow(), TestWorkbook.ValidRow() };
        var ids = rows.Select(r => Guid.Parse((string)r[0]!)).ToArray();
        var token = await PlannerTokenAsync();

        var first = await JsonOf(await PostImportAsync(Workbook(rows), token: token));
        var second = await JsonOf(await PostImportAsync(Workbook(rows), token: token));

        Assert.Equal(2, first.GetProperty("created").GetProperty("classes").GetInt32());
        Assert.Equal(0, second.GetProperty("created").GetProperty("classes").GetInt32());
        Assert.Equal(2, second.GetProperty("importedRows").GetInt32());
        Assert.Equal(2, await CountRowsAsync(ids));
    }

    [Fact]
    public async Task BrokenRow_IsReported_InTheShapeTheWebsiteFiltersOn()
    {
        var good = TestWorkbook.ValidRow();
        var bad = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Startdato", "not a date");
        var token = await PlannerTokenAsync();

        var response = await PostImportAsync(Workbook(good, bad), token: token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // a partial import is still a 200
        var json = await JsonOf(response);
        Assert.Equal(2, json.GetProperty("totalRows").GetInt32());
        Assert.Equal(1, json.GetProperty("importedRows").GetInt32());
        Assert.Equal(1, json.GetProperty("skippedRows").GetInt32());

        var issue = Assert.Single(json.GetProperty("issues").EnumerateArray());
        Assert.Equal("error", issue.GetProperty("severity").GetString()); // lowercase: the website's IssueSeverity
        Assert.Equal("Data", issue.GetProperty("sheet").GetString());
        Assert.Equal(3, issue.GetProperty("row").GetInt32());
        Assert.Contains("Startdato", issue.GetProperty("message").GetString());

        // The same problem as a short text for the contract's "warnings" list.
        var warning = Assert.Single(json.GetProperty("warnings").EnumerateArray());
        Assert.StartsWith("Row 3", warning.GetString());
    }

    // ---------- errors the website must be able to show ----------

    [Fact]
    public async Task FormWithoutAFilePart_Returns400_FileRequired()
    {
        var response = await PostImportAsync(file: null, token: await PlannerTokenAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("FILE_REQUIRED", (await JsonOf(response)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task RequestThatIsNotAForm_Returns400()
    {
        var response = await PostWithoutFormAsync(await PlannerTokenAsync());

        // The code of this error comes from ASP.NET, before our controller runs (today: INVALID_REQUEST).
        // What matters to the website is that it is a clean 400 with an error body, not a crash.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace((await JsonOf(response)).GetProperty("code").GetString()));
    }

    [Fact]
    public async Task WrongExtension_Returns415_UnsupportedFileType()
    {
        var response = await PostImportAsync(Workbook(TestWorkbook.ValidRow()), fileName: "export.csv", token: await PlannerTokenAsync());

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.Equal("UNSUPPORTED_FILE_TYPE", (await JsonOf(response)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task XlsxThatIsNotReallyExcel_Returns422_WithAReadableMessage()
    {
        var response = await PostImportAsync("plain text, not an xlsx"u8.ToArray(), token: await PlannerTokenAsync());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var json = await JsonOf(response);
        Assert.Equal("INVALID_IMPORT_DATA", json.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("message").GetString()));
    }

    [Fact]
    public async Task ExcelFileWithoutTheRequiredColumns_Returns422_NamingTheMissingColumn()
    {
        var headers = TestWorkbook.Headers.Where(h => h != "Kaldenavn").ToArray();
        var row = TestWorkbook.ValidRow().Where((_, i) => i != 4).ToArray();
        var file = TestWorkbook.Build(headers, row).ToArray();

        var response = await PostImportAsync(file, token: await PlannerTokenAsync());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("Kaldenavn", (await JsonOf(response)).GetProperty("message").GetString());
    }

    // ---------- the real export, through the whole stack ----------

    [RealExportFact]
    public async Task RealExport_Imports108Rows_ThroughTheWholeStack()
    {
        var bytes = await ReadRealExportAsync();
        var token = await PlannerTokenAsync();
        var rowsBefore = await factory.QueryDatabaseAsync(db => db.PlanningEducationElement.CountAsync()); // the database is shared by this class's tests

        var response = await PostImportAsync(bytes, token: token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await JsonOf(response);
        Assert.Equal(108, json.GetProperty("importedRows").GetInt32());
        Assert.Equal(0, json.GetProperty("skippedRows").GetInt32());
        Assert.Equal(108, json.GetProperty("created").GetProperty("classes").GetInt32());
        var rowsAfter = await factory.QueryDatabaseAsync(db => db.PlanningEducationElement.CountAsync());
        Assert.Equal(108, rowsAfter - rowsBefore);
    }

    private static async Task<byte[]> ReadRealExportAsync()
    {
        await using var file = new FileStream(ExcelTest.path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var copy = new MemoryStream();
        await file.CopyToAsync(copy);
        return copy.ToArray();
    }
}

// The login that the import depends on (every import call needs a token).
public class LoginApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string LoginUrl = "/api/v1/auth/login";

    private async Task<HttpResponseMessage> LoginAsync(string email, string password)
    {
        using var client = factory.CreateClient();
        return await client.PostAsJsonAsync(LoginUrl, new { email, password });
    }

    [Fact]
    public async Task Planner_GetsAToken_AndTheAdminRole()
    {
        var response = await LoginAsync(ApiFactory.PlannerEmail, ApiFactory.Password);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("accessToken").GetString()));
        Assert.Equal("Admin", json.GetProperty("user").GetProperty("role").GetString());
    }

    [Fact]
    public async Task OrdinaryTeacher_GetsTheTeacherRole()
    {
        var response = await LoginAsync(ApiFactory.TeacherEmail, ApiFactory.Password);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Teacher", json.GetProperty("user").GetProperty("role").GetString());
    }

    [Fact]
    public async Task EmailIsMatchedIgnoringCapitalsAndSpaces()
    {
        var response = await LoginAsync("  PLANNER@Test.dk ", ApiFactory.Password);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WrongPassword_And_UnknownEmail_GiveTheSameAnswer()
    {
        var wrongPassword = await LoginAsync(ApiFactory.PlannerEmail, "wrong");
        var unknownEmail = await LoginAsync("nobody@test.dk", ApiFactory.Password);

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);

        // Same code and message for both, so the response does not reveal which emails have an account.
        var a = await wrongPassword.Content.ReadFromJsonAsync<JsonElement>();
        var b = await unknownEmail.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("INVALID_CREDENTIALS", a.GetProperty("code").GetString());
        Assert.Equal(a.GetProperty("code").GetString(), b.GetProperty("code").GetString());
        Assert.Equal(a.GetProperty("message").GetString(), b.GetProperty("message").GetString());
    }

    [Theory]
    [InlineData("", "x")]
    [InlineData("not-an-email", "x")]
    [InlineData("planner@test.dk", "")]
    public async Task IncompleteRequest_Returns400(string email, string password)
    {
        var response = await LoginAsync(email, password);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("INVALID_LOGIN_REQUEST", json.GetProperty("code").GetString());
    }
}
