using DatabaseConnection.model;

namespace DatabaseConnection.repo;
public interface IAssessmentHandInRepo
{
    Task<AssessmentHandIn?> GetHandInByIdAsync(int id);
    Task<List<AssessmentHandIn>> GetAllEventAsync();
    Task<AssessmentEvent> GetAssessmentEventByIdAsync(int assessmentEvenId);
    Task<AssessmentHandIn> AddHandInAsync(int assessmentEvenId,
        DateTime handInDateTime,int part, string note);
    Task<AssessmentHandIn?> UpdateHandInAsync(int id,
        int assessmentEvenId,DateTime handInDateTime,
        int part,string note);
    Task<bool> DeleteHandInAsync(int id);

}