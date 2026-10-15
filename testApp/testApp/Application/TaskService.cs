using Microsoft.EntityFrameworkCore;
using testApp.Application.Abstr;
using testApp.Broker;

namespace testApp.Application;

public class TaskService(IRepository repository, IBroker broker, ILogger<TaskService> logger) : ITaskService
{
	public async Task<TaskDto> CreateTaskAsync(TaskDto taskDto)
	{
		var taskItem = TaskDto.ToItem(taskDto);
		await repository.AddAsync(taskItem);
		await repository.SaveChangesAsync();
		return taskDto;
	}

	public async Task<bool> RemoveTaskAsync(Guid taskId)
	{
		var result = await repository.GetByIdAsync(taskId);
		if (result == null)
			return false;

		await repository.RemoveAsync(result);
		await repository.SaveChangesAsync();

		return true;
	}

	public async Task<IReadOnlyList<TaskDto>> GetAllTasks()
	{
		var result = await repository.GetAllTasksAsync();

		var dtoTasks = result.Select(TaskDto.ToDto).ToList();

		return dtoTasks.AsReadOnly();
	}

	public async Task<int> MarkTaskCompleted(Guid id)
	{
		var task = await repository.GetByIdAsync(id);

		if (task is null)
			return 404;

		if (!task.TryComplete(DateTimeOffset.UtcNow))
			return 409;

		try
		{
			await repository.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return 409;
		}

		try
		{
			await broker.SendMessage(task);
		}
		catch (Exception e)
		{
			logger.LogError(e, "RabbitMQ publish failed for task {Id}", task.Id);
		}

		return 200;
	}
}