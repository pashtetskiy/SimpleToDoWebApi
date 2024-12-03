using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Models;
using ToDoApp.Infrastructure.Context;
using ToDoApp.Infrastructure.Repository;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ToDoApp.Tests;

public class RepositoryTests : IDisposable
{
	private readonly Repository<ToDo> _repository;
	private readonly ToDoDbContext _context;

	public RepositoryTests()
	{
		var options = new DbContextOptionsBuilder<ToDoDbContext>()
			.UseInMemoryDatabase(new Guid().ToString()).Options;

		_context = new ToDoDbContext(options);
		_repository = new Repository<ToDo>(_context, new LoggerFactory().CreateLogger<Repository<ToDo>>());
	}

	public void Dispose()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}

	[Fact]
	public async Task ToListAsync_ReturnsListOfToDos()
	{
		// Arrange
		_context.ToDos.Add(new ToDo
		{
			Title = "Test Task 1",
			Description = "Description 1",
			ExpiryDate = DateTime.UtcNow.AddDays(5)
		});
		_context.ToDos.Add(new ToDo
		{
			Title = "Test Task 2",
			Description = "Description 2",
			ExpiryDate = DateTime.UtcNow.AddDays(10)
		});
		await _context.SaveChangesAsync();

		// Act
		var result = await _repository.ToListAsync();

		// Assert
		Assert.Contains(result, t => t.Title == "Test Task 1");
		Assert.Contains(result, t => t.Title == "Test Task 2");
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsCorrectToDo()
	{
		// Arrange
		var todo = new ToDo
		{
			Id = 9,
			Title = "Test Task",
			Description = "Test Description",
			ExpiryDate = DateTime.UtcNow.AddDays(5)
		};
		_context.Add(todo);
		_context.SaveChanges();

		// Act
		var result = await _repository.GetByIdAsync(todo.Id);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(todo.Id, result?.Id);
		Assert.Equal("Test Task", result?.Title);
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsNull_WhenEntityDoesNotExist()
	{
		// Act
		var result = await _repository.GetByIdAsync(-1); // Invalid ID

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task AddAsync_ReturnsTrue_WhenEntityIsAdded()
	{
		// Arrange
		var todo = new ToDo
		{
			Title = "New Task",
			Description = "New Task Description",
			ExpiryDate = DateTime.UtcNow.AddDays(3)
		};

		// Act
		var result = await _repository.AddAsync(todo);

		// Assert
		Assert.True(result);
		Assert.Equal(1, _context.ToDos.Count());
	}

	[Fact]
	public async Task AddAsync_ReturnsFalse_WhenEntityAlreadyExists()
	{
		// Arrange
		var todo = new ToDo
		{
			Title = "New Task to add",
			Description = "New Task Description",
			ExpiryDate = DateTime.UtcNow.AddDays(5)
		};
		await _repository.AddAsync(todo); // Add once

		// Act
		var boolResult = await _repository.AddAsync(todo); // Try to add again
		var result = await _repository.ToListAsync();

		// Assert
		Assert.False(boolResult); // Should fail because same entity is being added again
	}

	[Fact]
	public async Task RemoveAsync_ReturnsTrue_WhenEntityIsRemoved()
	{
		// Arrange
		var todo = new ToDo
		{
			Title = "New Task to Remove!",
			Description = "Remove this task",
			ExpiryDate = DateTime.UtcNow.AddDays(7)
		};
		_context.ToDos.Add(todo);
		await _context.SaveChangesAsync();

		// Act
		var boolResult = await _repository.RemoveAsync(todo);

		// Assert
		Assert.True(boolResult);
		var result = await _repository.ToListAsync();
		Assert.DoesNotContain(result, t => t.Title == "New Task to Remove!");
	}

	[Fact]
	public async Task RemoveAsync_ReturnsFalse_WhenEntityDoesNotExist()
	{
		// Act
		var boolResult = await _repository.RemoveAsync(new ToDo
		{
			Id = -1,
			Title = "Non-existent Task",
			Description = "Non-existent Description",
			ExpiryDate = DateTime.UtcNow.AddDays(10),
			PercentComplete = 0,
			IsDone = false
		});

		// Assert
		Assert.False(boolResult); // Should fail because entity doesn't exist
	}

	[Fact]
	public async Task UpdateAsync_ReturnsTrue_WhenEntityIsUpdated()
	{
		// Arrange
		var todo = new ToDo
		{
			Id = 400,
			Title = "Old Task",
			Description = "Old Description",
			ExpiryDate = DateTime.UtcNow.AddDays(2),
			PercentComplete = 20,
			IsDone = false
		};
		_context.ToDos.Add(todo);
		await _context.SaveChangesAsync();

		var todoToUp = await _repository.GetByIdAsync(400);

		Assert.NotNull(todoToUp);

		todoToUp.Title = "Updated old Task";
		todoToUp.Description = "Updated old Description";
		todoToUp.ExpiryDate = DateTime.UtcNow.AddDays(1);
		todoToUp.PercentComplete = 90;
		todoToUp.IsDone = true;

		// Act
		var result = await _repository.UpdateAsync(todoToUp);

		// Assert
		Assert.True(result);
		var updatedTodo = await _context.ToDos.FindAsync(todoToUp.Id);
		Assert.Equal("Updated old Task", updatedTodo?.Title);
		Assert.Equal("Updated old Description", updatedTodo?.Description);
		Assert.Equal(90, updatedTodo?.PercentComplete);
		Assert.True(updatedTodo?.IsDone);
	}

	[Fact]
	public async Task UpdateAsync_ReturnsFalse_WhenEntityDoesNotExist()
	{
		// Arrange
		var todo = new ToDo
		{
			Id = -1,
			Title = "Non-existent Task",
			Description = "Non-existent Description",
			ExpiryDate = DateTime.UtcNow.AddDays(5),
			PercentComplete = 0,
			IsDone = false
		};

		// Act
		var result = await _repository.UpdateAsync(todo);

		// Assert
		Assert.False(result); // Should fail because entity doesn't exist
	}

	[Fact]
	public async Task GetAllWhereAsync_ReturnsFilteredToDos()
	{
		// Arrange
		_context.ToDos.Add(new ToDo
		{
			Title = "Task 1",
			Description = "Description 1",
			ExpiryDate = DateTime.UtcNow.AddDays(1),
			PercentComplete = 10,
			IsDone = false
		});
		_context.ToDos.Add(new ToDo
		{
			Title = "Task 2",
			Description = "Description 2",
			ExpiryDate = DateTime.UtcNow.AddDays(2),
			PercentComplete = 20,
			IsDone = true
		});
		_context.ToDos.Add(new ToDo
		{
			Title = "Task 3",
			Description = "Description 3",
			ExpiryDate = DateTime.UtcNow.AddDays(3),
			PercentComplete = 30,
			IsDone = false
		});
		await _context.SaveChangesAsync();

		// Act
		var result = await _repository.GetAllWhereAsync(t => t.Title.Contains("Task 1"));

		// Assert
		Assert.NotNull(result);
		Assert.Single(result);
		Assert.Equal("Task 1", result?.FirstOrDefault()?.Title);
	}

	[Fact]
	public async Task GetAllWhereAsync_ReturnsEmptyList_WhenNoMatch()
	{
		// Arrange
		_context.ToDos.Add(new ToDo
		{
			Title = "Task",
			Description = "Description 1",
			ExpiryDate = DateTime.UtcNow.AddDays(1),
			PercentComplete = 10,
			IsDone = false
		});
		await _context.SaveChangesAsync();

		// Act
		var result = await _repository.GetAllWhereAsync(t => t.Title.Contains("Non-existing Task"));

		// Assert
		Assert.NotNull(result);
		Assert.Empty(result);
	}
}