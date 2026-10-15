using System;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface IExamDayRepo
{
    Task<ExamDay> AddAsync( int scheduleId,
         DateTime date, ExamDayType examDayType, bool usable);
    Task<ExamDay?> GetById(int id);
    Task<Schedule?> GetScheduleByIdAsync(int id);
    Task<List<ExamDay>> GetAll();
    Task<List<ExamDay>> GetAllByScheduleId(int id);
    Task<DateTime?> GetDateAsyncByIdAsync(int id);
    Task<ExamDayType?> GetExamDayTypeByIdAsync(int id);
    Task<bool> GetUsableByIdAsync(int id);
    Task<ExamDay?> UpdateByIdAsync(int id, int scheduleId,
         DateTime date, ExamDayType examDayType, bool usable);
    Task<ExamDay?> UpdateScheduleIdByIdAsync(int id, int scheduleId);
    Task<ExamDay?> UpdateDateByIdAsync(int id, DateTime date);
    Task<ExamDay?> UpdateExamTypeByIdAsync(int id, ExamDayType type);
    Task<ExamDay?> UpdateUsableByIdAsync(int id, bool usable);
    Task<bool> DeleteByIdAsync(int id);

    


    
}
