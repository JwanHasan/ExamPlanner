using System;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface IScheduleReviewRepo
{
    Task<ScheduleReview> AddAsync(int scheduleId,int teacherId,bool approved, bool completed,string remarks);

    Task<ScheduleReview?> UpdateAsync(int scheduleId,int teacherId,bool approved, bool completed,string remarks);

    Task<ScheduleReview?> GetbyIdAsync(int scheduleId,int teacherId);

    Task<List<ScheduleReview>> GetAllAsync();
    Task<List<ScheduleReview>> GetAllByTeacherIdAsync(int id);
    Task<List<ScheduleReview>> GetAllByScheduleIdAsync(int id);
    Task<List<Schedule>> GetAllScheduleByTeacherIdAsync(int id);
    Task<List<Teacher>> GetAllTeacherByScheduleIdAsync(int id);

    Task<bool> DeleteAsync(int scheduleId,int teacherId);
    Task SaveAsync();
}
