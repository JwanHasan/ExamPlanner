using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface IConflictAcknowledgementRepo
{
	Task<ConflictAcknowledgement> CreateAsync(int scheduleId, int ruleKey, int userId, string fingerPrint, string reason, DateTime createdAt);
	Task<ConflictAcknowledgement?> GetByIdAsync(int id);
	Task<List<ConflictAcknowledgement>> GetAllAsync();
	Task<ConflictAcknowledgement?>  UpdateByIdAsync(int id, int scheduleId, int ruleKey, int userId, string fingerPrint, string reason, DateTime createdAt);

	Task<bool> Delete(int id);
}
