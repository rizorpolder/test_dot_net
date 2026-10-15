using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using testApp.Application;
using testApp.Application.Abstr;

namespace testApp.Presentation;

internal static class MinimalApi
{
	public static IEndpointRouteBuilder MapMinimalApiEndpoints(this IEndpointRouteBuilder builder)
	{
		var group = builder.MapGroup("/tasks");

		AddTaskItemEndpoint(group);
		GetAllTaskEndpoint(group);
		DeleteTaskEndpoint(group);
		UpdateTaskEndpoint(group);
		return group;
	}

	private static void AddTaskItemEndpoint(RouteGroupBuilder group)
	{
		group.MapPost("/",
				async (ITaskService service, [FromBody] string title, HttpContext ctx) =>
				{
					var dto = new TaskDto()
					{
						ID = Guid.NewGuid(),
						Title = title,
						CreatedAt = DateTimeOffset.UtcNow,
						IsCompleted = false,
						Priority = Priority.Medium
					};

					var validator = ctx.RequestServices.GetRequiredService<IObjectModelValidator>();
					var actionContext = new ActionContext(
						ctx,
						ctx.GetRouteData(),
						new ActionDescriptor(),
						new ModelStateDictionary()
					);
					validator.Validate(actionContext, null, string.Empty, dto);

					if (!actionContext.ModelState.IsValid)
						return Results.BadRequest();
					try
					{
						await service.CreateTaskAsync(dto);
					}
					catch (Exception e)
					{
						return Results.BadRequest(e.Message);
					}

					return Results.Ok(dto);
				})
			.Produces<TaskDto>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status400BadRequest);
	}

	private static void GetAllTaskEndpoint(RouteGroupBuilder group)
	{
		group.MapGet("/",
			async (ITaskService taskService) =>
			{
				try
				{
					var result = await taskService.GetAllTasks();
					return Results.Ok(result);
				}
				catch (Exception e)
				{
					return Results.InternalServerError(new {message = e.Message});
				}
			})
			.Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
	}

	private static void DeleteTaskEndpoint(RouteGroupBuilder group)
	{
		group.MapDelete("{id:guid}",
				async (ITaskService service, Guid id) =>
				{
					try
					{
						var result = await service.RemoveTaskAsync(id);
						return result ? Results.Ok() : Results.NotFound();
					}
					catch (Exception e)
					{
						return Results.InternalServerError(new {message = e.Message});
					}
				})
			.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
			.Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
	}

	private static void UpdateTaskEndpoint(RouteGroupBuilder group)
	{
		group.MapPut("{id:guid}/complete", async (ITaskService s, Guid id) =>
		{
			return Results.StatusCode(await s.MarkTaskCompleted(id));
		});
	}
}