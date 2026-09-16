using Microsoft.AspNetCore.Mvc;

namespace ExamPlannerServer.controller;

[ApiController]
[Route("api/[controller]")]
public class RouteControllerTest : ControllerBase

{
    private readonly  IStudentRepo studentRepo;

public RouteControllerTest(IStudentRepo repo)
    {
        studentRepo = repo;
    }
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var student = await studentRepo.GetByIdAsync(2);
        return Ok(student);
    }
}
