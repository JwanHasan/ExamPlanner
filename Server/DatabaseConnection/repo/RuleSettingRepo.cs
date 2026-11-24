using System;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class RuleSettingRepo:IRuleSettingRepo
{
    private readonly AppDbContext _context;
    public RuleSettingRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<RuleSetting> CreateAsync(bool enabled, string severity)
    {
        var ruleSetting = new RuleSetting
        {
            Enabled = enabled,
            Severity = severity
        };

        _context.RuleSettings.Add(ruleSetting);
        await _context.SaveChangesAsync();

        return ruleSetting;
    }

    public async Task<bool> Delete(int id)
    {
        var ruleSetting = await _context.RuleSettings.FindAsync(id);
        if (ruleSetting is null)
        {
            return false;
        }

        _context.RuleSettings.Remove(ruleSetting);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<RuleSetting>> GetAll()
    {
        return await _context.RuleSettings.ToListAsync();
    }

    public async Task<RuleSetting?> GetById(int id)
    {
        return await _context.RuleSettings.FindAsync(id);
    }

    public async Task<RuleSetting> Update(int id, bool enabled, string severity)
    {
        var ruleSetting = await _context.RuleSettings.FindAsync(id);
        if (ruleSetting is null)
        {
            throw new KeyNotFoundException($"Rule setting with ID {id} was not found.");
        }

        ruleSetting.Enabled = enabled;
        ruleSetting.Severity = severity;
        await _context.SaveChangesAsync();

        return ruleSetting;
    }
}
