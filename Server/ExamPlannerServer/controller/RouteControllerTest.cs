using DatabaseConnection.model;
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
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var student = await studentRepo.GetByIdAsync(id);
        if(student is null)
        {
            return NotFound();
        }
         else return Ok(student);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent([FromBody] string username)
    {
       await  studentRepo.AddAsync(new Student{ Name = username});
       return Ok(username);
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents()
    {
        var students = studentRepo.GetAllAsync();
        return Ok(students);
    }
    
}
