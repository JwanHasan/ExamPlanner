using System;
using System.Diagnostics.CodeAnalysis;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class TeacherAssignmentRepo:ITeacherAssignmentRepo
{
    private readonly AppDbContext _context;

    public TeacherAssignmentRepo(AppDbContext context)
    {
        _context= context;
    }

    public async Task<TeacherAssignment> AddAsync(int examsessionId, int teacherId, TeacherRole role)
    {
        var add= new TeacherAssignment
        {
            ExamSessionId = examsessionId,
            TeacherId = teacherId,
            Role =role
        };
        await _context.TeacherAssignment.AddAsync(add);
        await _context.SaveChangesAsync();
        return add;
    }

    // delete from database Teacher Assignment based on teaher and exam session id return true if done
    // false if not, also be in mind this also mean it will everything,
    // that object has these 2 ids 
    public async Task<bool> DeleteByIdsAsync(int examsessionId, int teacherId)
    {
         var del=  await _context.TeacherAssignment
            .Where(x=> x.TeacherId == teacherId && x.ExamSessionId == examsessionId)
            .ExecuteDeleteAsync();
        return del>0;
    }

    // get list of Teacher assignment
    public async Task<List<TeacherAssignment>> GetAllAsync()
    {
        return await _context.TeacherAssignment.ToListAsync();
    }

    // get a list of teachers based on ExamSession Id
    public async Task<List<Teacher>> GetAllTeachersByExamSessionIdAsync(int id)
    {
        return await _context.TeacherAssignment
            .Where(x=> x.ExamSessionId == id)
            .Select(x=> x.Teacher)
            .ToListAsync();
        
    }
    // a method that  search and get Teacher Assignment by Teacher and Exam Session Id.
    // retrun obejct found, null if not found 
    public async Task<TeacherAssignment?> GetByIdAsync(int examsessionId, int teacherId)
    {
        return await _context.TeacherAssignment.FirstOrDefaultAsync(x=> x.TeacherId == teacherId && x.ExamSessionId == examsessionId);
    }

    //retrun exam session list based on Teacher Id
    public async Task<List<ExamSession>> GetExamSessionsByTeacherIdAsync(int id)
    {
        return await _context.TeacherAssignment
            .Where(x=> x.ExamSessionId == id)
            .Select(x=> x.ExamSession)
            .ToListAsync();
    }
        
    // return teacher role based on exam session and teacher id
    public async Task<TeacherRole?> GetRoleByIdsAsync(int examsessionId, int teacherId)
    {
         var role=await GetByIdAsync(examsessionId,teacherId);
         if(role is null ) return null;
         return role.Role;
    }

    // update teacher assignment 
    public async Task<TeacherAssignment?> UpdateAsync(int examsessionId, int teacherId, TeacherRole role)
    {
        var upt=await GetByIdAsync(examsessionId,teacherId);
         if(upt is null ) return null;
        upt.ExamSessionId = examsessionId;
        upt.TeacherId = teacherId;
        upt.Role = role;
        await _context.SaveChangesAsync();
        return upt;
    }

// update role based on teahcer and exam session id.
    public async  Task<TeacherAssignment?> UpdateRoleAsync(int examsessionId, int teacherId, TeacherRole role)
    {
        var upt=await GetByIdAsync(examsessionId,teacherId);
            if(upt is null ) return null;
        upt.Role = role;
        await _context.SaveChangesAsync();
        return upt;
    }
}
