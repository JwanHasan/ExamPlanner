using System;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseConnection.repo;

public class ConflictAcknowledgementRepo:IConflictAcknowledgementRepo
{
    private readonly AppDbContext _context;
    public ConflictAcknowledgementRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<ConflictAcknowledgement> CreateAsync(int scheduleId, int ruleKeyId, int userId, string fingerPrint, string reason, DateTime createdAt)
    {
        var acknowledgement = new ConflictAcknowledgement
        {
            ScheduleId = scheduleId,
            RuleKeyId = ruleKeyId,
            UserId = userId,
            FingerPrint = fingerPrint,
            Reason = reason,
            CreatedAt = createdAt
        };

        _context.ConflictAcknowledgements.Add(acknowledgement);
        await _context.SaveChangesAsync();
        return acknowledgement;
    }

    public async Task<bool> Delete(int id)
    {
        var acknowledgement = await _context.ConflictAcknowledgements.FindAsync(id);
        if (acknowledgement is null)
            return false;

        _context.ConflictAcknowledgements.Remove(acknowledgement);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ConflictAcknowledgement>> GetAllAsync()
    {
        return await _context.ConflictAcknowledgements.ToListAsync();
    }

    public async Task<ConflictAcknowledgement?> GetByIdAsync(int id)
    {
        return await _context.ConflictAcknowledgements.FindAsync(id);
    }

    public async Task<ConflictAcknowledgement?> UpdateByIdAsync(int id ,int scheduleId, int ruleKeyId, int userId, string fingerPrint, string reason, DateTime createdAt)
    {
        var acknowledgement = await GetByIdAsync(id);

        if (acknowledgement is null)
            return null;
        acknowledgement.ScheduleId= scheduleId;
        acknowledgement.RuleKeyId= ruleKeyId;
        acknowledgement.UserId=userId;
        acknowledgement.FingerPrint = fingerPrint;
        acknowledgement.Reason = reason;
        acknowledgement.CreatedAt = createdAt;
        await _context.SaveChangesAsync();
        return acknowledgement;
    }
}
