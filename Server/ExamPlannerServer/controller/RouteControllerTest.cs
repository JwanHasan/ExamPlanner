using DatabaseConnection.model;
using Microsoft.AspNetCore.Mvc;
using ExamPlannerServer.Dto;

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
    public async Task<IActionResult> CreateStudent([FromBody] CreateNewStudentDto studentDto)
    {
        var student = new Student{ Name = studentDto.Name, ViaId = studentDto.ViaId};

       var createdStudent = await  studentRepo.AddAsync(student);
       return Ok(createdStudent);
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents()
    {
        var students = await  studentRepo.GetAllAsync();
        return Ok(students);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateStudentAsync([FromBody] UpdateStudentDto updateStudentDto)
    {
        var student =await studentRepo.UpdateByIdAsync(updateStudentDto.Id,updateStudentDto.Name,updateStudentDto.ViaId);
        if(student!=null) return Ok(student);
        else return NotFound();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult>DeleteStudent(int id)
    {
        var status = await studentRepo.DeleteByIdAsync(id);
        if(!status) return NotFound();
        else return Ok(status);
    }
}
