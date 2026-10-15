using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace testApp.Application;

[Serializable]
public class TaskDto : IValidatableObject
{
	public Guid ID { get; set; }
	[Required] public string Title { get; set; }
	public bool IsCompleted { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? CompletedAt { get; set; }
	public Priority Priority { get; set; }

	[JsonConstructor]
	public TaskDto()
	{
	}

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (string.IsNullOrWhiteSpace(Title))
			yield return new ValidationResult("Title could not be empty", [nameof(Title)]);

		if (Title is {Length: > 200})
			yield return new ValidationResult("Title must be less then 200 chars", [nameof(Title)]);
	}

	public static TaskDto ToDto(TaskItem task)
	{
		return new TaskDto()
		{
			ID = task.Id,
			Title = task.Title,
			IsCompleted = task.IsCompleted,
			CreatedAt = task.CreatedAt,
			CompletedAt = task.CompletedAt,
			Priority = task.Priority,
		};
	}

	public static TaskItem ToItem(TaskDto taskDto)
	{
		return new TaskItem()
		{
			Id = taskDto.ID,
			Title = taskDto.Title,
			IsCompleted = taskDto.IsCompleted,
			CreatedAt = taskDto.CreatedAt,
			CompletedAt = taskDto.CompletedAt,
			Priority = taskDto.Priority,
		};
	}
}