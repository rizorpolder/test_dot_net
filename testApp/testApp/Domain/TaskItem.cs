
namespace testApp;

public class TaskItem
{
	public Guid Id  = Guid.NewGuid();
	public string Title { get; set; }
	public bool IsCompleted { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? CompletedAt { get; set; }
	public Priority Priority { get; set; }
	public uint Version { get; set; }

	public TaskItem()
	{
		
	}

	public bool TryComplete(DateTimeOffset now)
	{
		if (IsCompleted)
			return false;

		IsCompleted = true;
		CompletedAt = now;
		return true;
	}
}

public enum Priority
{
	Low,
	Medium,
	High
}