using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace testApp.Infrastructure.Configs;

public class TaskItemConfiguration: IEntityTypeConfiguration<TaskItem>
{
	public void Configure(EntityTypeBuilder<TaskItem> builder)
	{
		builder.ToTable("tasks");
		builder.HasKey(x => x.Id);
		builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
		builder.Property(x => x.Priority).HasConversion<string>();
		builder.Property(x => x.Version).IsRowVersion();
	}
}