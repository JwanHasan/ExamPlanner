using ExamPlannerServer.Contracts;
using ExamPlannerServer.controller;
using ExamPlannerServer.Dto;
using ExamPlannerServer.Import;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace TesProject;

// Controller layer on its own: a fake IImportService stands behind it, so these tests only
// check what the controller decides (status code, error code, response shape).
// Authentication (401/403) is not the controller's code, so it is tested in ImportApiTests.
public class ImportControllerTests
{
    // ---------- helpers ----------

    private sealed class FakeImportService(Func<IFormFile, Task<ImportResponse>> handler) : IImportService
    {
        public int Calls { get; private set; }

        public Task<ImportResponse> ImportAsync(IFormFile file)
        {
            Calls++;
            return handler(file);
        }
    }

    // The controller looks the service up through HttpContext.RequestServices, so that is what we fill.
    private static ImportController CreateController(IImportService? service)
    {
        var services = new ServiceCollection();
        if (service is not null)
            services.AddSingleton(service);

        return new ImportController(NullLogger<ImportController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() }
            }
        };
    }

    private static IFormFile FormFile(string fileName, string content = "some bytes")
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName);
    }

    private static ApiError ErrorOf(IActionResult result) => Assert.IsType<ApiError>(Assert.IsAssignableFrom<ObjectResult>(result).Value);

    private static int StatusOf(IActionResult result) => Assert.IsAssignableFrom<ObjectResult>(result).StatusCode!.Value;

    private static ImportResponse SomeResponse() => new()
    {
        FileName = "export.xlsx",
        TotalRows = 10,
        ImportedRows = 8,
        SkippedRows = 2,
        Created = new ImportCounts { Classes = 5 },
        Issues =
        [
            new ImportIssue("error", "Data", 4, "ECTS", "ECTS 7.5 has decimals."),
            new ImportIssue("error", "Data", 9, null, "Startdato: 'x' is not a date")
        ]
    };

    // ---------- the request is checked before the service is called ----------

    [Fact]
    public async Task NoFile_Returns400_FileRequired_AndDoesNotCallTheService()
    {
        var service = new FakeImportService(_ => Task.FromResult(SomeResponse()));

        var result = await CreateController(service).Import(null);

        Assert.Equal(400, StatusOf(result));
        Assert.Equal(ErrorCodes.FileRequired, ErrorOf(result).Code);
        Assert.Equal(0, service.Calls);
    }

    [Fact]
    public async Task EmptyFile_Returns400_FileRequired()
    {
        var service = new FakeImportService(_ => Task.FromResult(SomeResponse()));

        var result = await CreateController(service).Import(FormFile("export.xlsx", content: ""));

        Assert.Equal(400, StatusOf(result));
        Assert.Equal(ErrorCodes.FileRequired, ErrorOf(result).Code);
        Assert.Equal(0, service.Calls);
    }

    [Theory]
    [InlineData("notes.txt")]
    [InlineData("export.csv")]
    [InlineData("export.xls")]
    [InlineData("export")]
    public async Task WrongFileType_Returns415_UnsupportedFileType(string fileName)
    {
        var service = new FakeImportService(_ => Task.FromResult(SomeResponse()));

        var result = await CreateController(service).Import(FormFile(fileName));

        Assert.Equal(415, StatusOf(result));
        Assert.Equal(ErrorCodes.UnsupportedFileType, ErrorOf(result).Code);
        Assert.Equal(0, service.Calls);
    }

    [Theory]
    [InlineData("export.xlsx")]
    [InlineData("EXPORT.XLSX")]   // extension check ignores capitalisation
    [InlineData("macros.xlsm")]
    public async Task ExcelFileTypes_AreAccepted(string fileName)
    {
        var service = new FakeImportService(_ => Task.FromResult(SomeResponse()));

        var result = await CreateController(service).Import(FormFile(fileName));

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, service.Calls);
    }

    // ---------- what the controller does with the service's outcome ----------

    [Fact]
    public async Task ServiceNotRegistered_Returns500_ImportFailed()
    {
        var result = await CreateController(service: null).Import(FormFile("export.xlsx"));

        Assert.Equal(500, StatusOf(result));
        Assert.Equal(ErrorCodes.ImportFailed, ErrorOf(result).Code);
    }

    [Fact]
    public async Task UnusableData_Returns422_InvalidImportData_WithTheServicesMessage()
    {
        var service = new FakeImportService(_ => throw new ImportFileException("Required column(s) missing: Kaldenavn"));

        var result = await CreateController(service).Import(FormFile("export.xlsx"));

        Assert.IsType<UnprocessableEntityObjectResult>(result);
        var error = ErrorOf(result);
        Assert.Equal(ErrorCodes.InvalidImportData, error.Code);
        Assert.Equal("Required column(s) missing: Kaldenavn", error.Message);
    }

    [Fact]
    public async Task UnexpectedFailure_Returns500_AndDoesNotLeakTheExceptionText()
    {
        var service = new FakeImportService(_ => throw new InvalidOperationException("secret: Host=db;Password=hunter2"));

        var result = await CreateController(service).Import(FormFile("export.xlsx"));

        Assert.Equal(500, StatusOf(result));
        var error = ErrorOf(result);
        Assert.Equal(ErrorCodes.ImportFailed, error.Code);
        Assert.DoesNotContain("hunter2", error.Message);
        Assert.DoesNotContain("secret", error.Message);
    }

    [Fact]
    public async Task Success_Returns200_WithTheSummaryTheWebsiteExpects()
    {
        var service = new FakeImportService(_ => Task.FromResult(SomeResponse()));

        var result = await CreateController(service).Import(FormFile("export.xlsx"));

        var summary = Assert.IsType<ImportSummaryDto>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.Equal("export.xlsx", summary.FileName);
        Assert.Equal(10, summary.TotalRows);
        Assert.Equal(8, summary.ImportedRows);
        Assert.Equal(2, summary.SkippedRows);
        Assert.Equal(5, summary.Created.Classes);
        Assert.Equal(2, summary.Issues.Count);
    }

    // ---------- the summary DTO (the shape of the JSON) ----------

    [Fact]
    public void Summary_CopiesTheContractFields_AndDescribesIssuesAsWarnings()
    {
        var summary = ImportSummaryDto.From(SomeResponse());

        Assert.True(summary.Success);              // something was imported
        Assert.Equal(5, summary.ClassesImported);  // API_CONTRACT.md: classesImported
        Assert.Equal(0, summary.StudentsImported);
        Assert.Equal(0, summary.ExamsImported);
        Assert.Null(summary.ImportId);             // there is no import history in the database yet
        Assert.Equal(
            ["Row 4 (ECTS): ECTS 7.5 has decimals.", "Row 9: Startdato: 'x' is not a date"],
            summary.Warnings);
    }

    [Fact]
    public void Summary_IsNotSuccessful_WhenNothingWasImported()
    {
        var summary = ImportSummaryDto.From(new ImportResponse { TotalRows = 3, ImportedRows = 0, SkippedRows = 3 });

        Assert.False(summary.Success);
    }
}
