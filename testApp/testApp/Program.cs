using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using testApp.Application;
using testApp.Application.Abstr;
using testApp.Broker;
using testApp.Infrastructure.Repositories;
using testApp.Presentation;

namespace testApp;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
		builder.Services.AddDbContext<TaskDBContext>(options => { options.UseNpgsql(connectionString); });
		builder.Services.AddMassTransit(x =>
		{
			x.UsingRabbitMq((context, cfg) =>
			{
				cfg.Host("localhost",
					"/",
					h =>
					{
						h.Username("guest");
						h.Password("guest");
					});
				
				cfg.UseRawJsonSerializer();
				
				cfg.ConfigureJsonSerializerOptions(o =>
				{
					o.Converters.Add(new JsonStringEnumConverter());
					return o;
				});
				cfg.Message<RabbitMqService.TaskCompletedMessage>(m => { m.SetEntityName("task.events"); });
				cfg.Publish<RabbitMqService.TaskCompletedMessage>(p => p.ExchangeType = "topic");
			});
		});

		builder.Services.Configure<HostOptions>(o => { o.ShutdownTimeout = TimeSpan.FromSeconds(5); });

		builder.Services.Configure<MassTransitHostOptions>(o => { o.StopTimeout = TimeSpan.FromSeconds(5); });

		builder.Services.ConfigureHttpJsonOptions(o =>
		{
			o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
		});

		builder.Services.AddScoped<ITaskService, TaskService>();
		builder.Services.AddScoped<IRepository, TaskRepository>();
		builder.Services.AddScoped<IBroker, RabbitMqService>();

		builder.Services.AddControllers();
		
		var app = builder.Build();

		app.UseHttpsRedirection();

		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<TaskDBContext>();
			if (db.Database.IsRelational())
				db.Database.Migrate();
		}

		
		app.MapMinimalApiEndpoints();
		app.Run();
	}
}