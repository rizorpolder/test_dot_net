using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using testApp.Broker;

namespace testApp;

public class RabbitMqService(IPublishEndpoint publishEndpoint) : IBroker
{
	public record TaskCompletedMessage(Guid TaskId, string Title, DateTimeOffset CompletedAt, Priority Priority);
	
	public async Task SendMessage(TaskItem task)
	{
		var message = new TaskCompletedMessage(task.Id, task.Title, task.CompletedAt!.Value, task.Priority);

		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
		await publishEndpoint.Publish(message, ctx => ctx.SetRoutingKey("task.completed"), cts.Token);
	}

}