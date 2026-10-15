using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using testApp.Application.Abstr;

namespace testApp.Infrastructure.Repositories;

public class TaskRepository(TaskDBContext dbContext) : IRepository
{
	public async Task AddAsync(TaskItem customEvent)
	{
		await dbContext.TaskItems.AddAsync(customEvent);
	}

	public Task<TaskItem?> GetByIdAsync(Guid id)
	{
		return dbContext.TaskItems.FirstOrDefaultAsync(x => x.Id.Equals(id));
	}

	public async Task<IReadOnlyList<TaskItem>> GetAllTasksAsync()
	{
		return await dbContext.TaskItems.AsNoTracking().ToListAsync();
	}

	public Task RemoveAsync(TaskItem customEvent)
	{
		dbContext.TaskItems.Remove(customEvent);
		return Task.CompletedTask;
	}

	public Task SaveChangesAsync()
	{
		return dbContext.SaveChangesAsync();
	}
	
}