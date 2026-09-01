using Microsoft.AspNetCore.Mvc;

namespace ExamPlannerServer.controller;

[ApiController]
[Route("api/[controller]")]
public class RouteControllerTest : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("it is working");
    }
}
