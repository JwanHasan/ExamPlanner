using System;
using System.Data.Common;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface IExamSessionRepo
{
    Task<ExamSession> AddAsyc(int assessmentEventId, int scheduleId, int roomId, DateOnly date, bool LockDate);
    Task<ExamSession?> GetByIdAsync(int id);
    Task<List<ExamSession>> GetAllAsync();
    Task<List<ExamSession>> GetAllByRoomId(int id);
    Task<List<ExamSession>> GetAllByScheduleId(int id);
    Task<List<ExamSession>> GetAllByAssessmentEventId(int id);
    Task<ExamSession?> UpdateAsync(int id,int assessmentEventId, int scheduleId, int roomId, DateOnly date, bool LockDate);
    Task<bool> DeleteByIdAsync(int id);
    Task SaveChangesAsync();

}
