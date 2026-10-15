using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using testApp.Broker;

namespace testApp.Tests;

public class FakeBroker : IBroker
{
	public ConcurrentBag<TaskItem> Sent { get; } = new();

	public Task SendMessage(TaskItem task)
	{
		Sent.Add(task);
		return Task.CompletedTask;
	}
}

public class CompleteTaskIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly FakeBroker _broker = new();
	private readonly HttpClient _client;

	public CompleteTaskIntegrationTest(WebApplicationFactory<Program> factory)
	{
		var dbName = Guid.NewGuid().ToString();

		_client = factory.WithWebHostBuilder(builder =>
		{
			builder.ConfigureTestServices(services =>
			{
				services.RemoveAll<DbContextOptions<TaskDBContext>>();
				services.RemoveAll<IDbContextOptionsConfiguration<TaskDBContext>>();
				services.AddDbContext<TaskDBContext>(o => o.UseInMemoryDatabase(dbName));

				services.RemoveAll<IBroker>();
				services.AddSingleton<IBroker>(_broker);
			});
		}).CreateClient();
	}

	[Fact]
	public async Task CompleteTask_SendsMessageToBroker_AndSecondCallReturns409()
	{
		var createResponse = await _client.PostAsJsonAsync("/tasks", "Buy milk");
		createResponse.EnsureSuccessStatusCode();

		var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
		var id = created.GetProperty("id").GetGuid();

		var items = await _client.GetAsync("/tasks");
		
		
		var completeResponse = await _client.PutAsync($"/tasks/{id}/complete", null);
		Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

		var sent = Assert.Single(_broker.Sent);
		Assert.Equal(id, sent.Id);
		Assert.Equal("Buy milk", sent.Title);
		Assert.NotNull(sent.CompletedAt);

		var secondResponse = await _client.PutAsync($"/tasks/{id}/complete", null);
		Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
		Assert.Single(_broker.Sent);
	}
	
}