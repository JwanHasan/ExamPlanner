using System;
using System.Collections.Generic;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface IRuleSettingRepo
{
	Task<RuleSetting> CreateAsync(bool enabled, string severity);

	Task<RuleSetting?> GetById(int id);

	Task<List<RuleSetting>> GetAll();

	Task<RuleSetting> Update(int id, bool enabled, string severity);

	Task<bool> Delete(int id);
}
