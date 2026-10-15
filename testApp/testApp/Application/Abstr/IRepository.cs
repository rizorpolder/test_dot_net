using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace testApp.Application.Abstr;

public interface IRepository
{
	public Task AddAsync(TaskItem taskItem);
	public Task<TaskItem?> GetByIdAsync(Guid id);
	public Task<IReadOnlyList<TaskItem>> GetAllTasksAsync();

	public Task RemoveAsync(TaskItem taskItem);
	public Task SaveChangesAsync();
}