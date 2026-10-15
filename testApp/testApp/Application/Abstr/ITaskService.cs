using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace testApp.Application.Abstr;

public interface ITaskService
{
	public Task<TaskDto> CreateTaskAsync(TaskDto taskDto);
	public Task<bool> RemoveTaskAsync(Guid taskId);
	public Task<IReadOnlyList<TaskDto>> GetAllTasks();
	public Task<int> MarkTaskCompleted(Guid id);
}