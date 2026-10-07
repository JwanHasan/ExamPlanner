using ExamPlannerServer.Import;
using Microsoft.AspNetCore.Mvc;

namespace ExamPlannerServer.controller;

[ApiController]
[Route("api/import")]
public class ImportController : ControllerBase
{
    private readonly IImportService _import;

    public ImportController(IImportService import) => _import = import;
    
    [HttpPost]
    [RequestSizeLimit(25_000_000)]
    public async Task<IActionResult> Import(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file was uploaded.");

        try
        {
            return Ok(await _import.ImportAsync(file));
        }
        catch (ImportFileException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}