using ExamPlannerServer.Auth;
using ExamPlannerServer.Contracts;
using ExamPlannerServer.Dto;
using ExamPlannerServer.Import;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamPlannerServer.controller;

/// <summary>
/// Excel import (API_CONTRACT.md 6.1). Only the exam planners may import: the accounts listed in
/// Auth:AdminEmails, which get the role "Admin" in their token.
///
/// The work itself is done by IImportService. Until an ImportService is registered in Program.cs,
/// this endpoint answers with a clear IMPORT_FAILED error instead of crashing.
/// </summary>
[ApiController]
[Route("api/v1/imports")]
[Authorize(Roles = AdminSettings.AdminRole)]
public class ImportController : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".xlsx", ".xlsm"];

    private readonly ILogger<ImportController> _logger;

    public ImportController(ILogger<ImportController> logger)
    {
        _logger = logger;
    }

    // POST /api/v1/imports   (multipart/form-data, field name "file")
    [HttpPost]
    [RequestSizeLimit(25_000_000)]
    public async Task<IActionResult> Import(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ApiError(ErrorCodes.FileRequired, "Choose an Excel file to import."));

        if (!AllowedExtensions.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase))
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, new ApiError(ErrorCodes.UnsupportedFileType,
                "Only Excel workbooks (.xlsx) can be imported. Open the file in Excel and save it as .xlsx."));

        // Looked up here rather than in the constructor, so the controller still works
        // while no ImportService is registered yet.
        var importService = HttpContext.RequestServices.GetService<IImportService>();
        if (importService is null)
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(ErrorCodes.ImportFailed,
                "The import is not set up on the server yet: no IImportService is registered in Program.cs."));

        try
        {
            var result = await importService.ImportAsync(file);
            return Ok(ImportSummaryDto.From(result));
        }
        catch (ImportFileException ex)
        {
            // The file was read, but its data cannot be used.
            return UnprocessableEntity(new ApiError(ErrorCodes.InvalidImportData, ex.Message));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Import of {File} failed", file.FileName);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiError(ErrorCodes.ImportFailed, "The import failed unexpectedly."));
        }
    }
}
