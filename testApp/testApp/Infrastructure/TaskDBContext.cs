using Microsoft.EntityFrameworkCore;

namespace testApp;

public class TaskDBContext : DbContext
{
	public DbSet<TaskItem> TaskItems => Set<TaskItem>();

	public TaskDBContext(DbContextOptions<TaskDBContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.ApplyConfigurationsFromAssembly(typeof(TaskDBContext).Assembly);
	}
}