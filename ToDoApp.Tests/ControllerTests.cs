using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ToDoApp.Controllers;
using ToDoApp.Domain.DTO;
using ToDoApp.Domain.Models;
using ToDoApp.Infrastructure.Repository;
using Xunit;

namespace ToDoApp.Tests;

public class ToDoControllerTests
{
	private readonly Mock<IRepository<ToDo>> _mockRepository;
	private readonly ToDoController _controller;

	public ToDoControllerTests()
	{
		_mockRepository = new Mock<IRepository<ToDo>>();
		_controller = new ToDoController(_mockRepository.Object);
	}

	[Fact]
	public async Task GetAll_ReturnsOkResult_WithListOfToDos()
	{
		// Arrange
		var todos = new List<ToDo>
		{
			new() { Id = 1, Title = "Test 1", Description = "Description 1" },
			new() { Id = 2, Title = "Test 2", Description = "Description 2" }
		};
		_mockRepository.Setup(repo => repo.ToListAsync()).ReturnsAsync(todos);

		// Act
		var result = await _controller.GetAll();

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		var returnedTodos = Assert.IsType<List<ToDo>>(okResult.Value);
		Assert.Equal(2, returnedTodos.Count);
	}

	[Fact]
	public async Task GetById_ValidId_ReturnsOkResult_WithToDo()
	{
		// Arrange
		var todo = new ToDo { Id = 1, Title = "Test", Description = "Description" };
		_mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(todo);

		// Act
		var result = await _controller.GetById(1);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		var returnedTodo = Assert.IsType<ToDo>(okResult.Value);
		Assert.Equal(1, returnedTodo.Id);
	}

	[Fact]
	public async Task GetById_InvalidId_ReturnsNotFound()
	{
		// Arrange
		_mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ToDo?)null);

		// Act
		var result = await _controller.GetById(1);

		// Assert
		Assert.IsType<NotFoundResult>(result);
	}

	[Fact]
	public async Task Create_ValidToDo_ReturnsCreatedAtAction()
	{
		// Arrange
		var createDto = new ToDoCreateDTO
		{
			Title = "Test",
			Description = "Description",
			ExpiryDate = DateTime.UtcNow.AddDays(1)
		};

		var todo = new ToDo
		{
			Id = 1,
			Title = createDto.Title,
			Description = createDto.Description,
			ExpiryDate = createDto.ExpiryDate
		};

		_mockRepository.Setup(repo => repo.AddAsync(It.IsAny<ToDo>())).ReturnsAsync(true);

		// Act
		var result = await _controller.Create(createDto);

		// Assert
		var createdResult = Assert.IsType<CreatedAtActionResult>(result);
		var returnedTodo = Assert.IsType<ToDo>(createdResult.Value);
		Assert.Equal(createDto.Title, returnedTodo.Title);
	}

	[Fact]
	public async Task Create_Failure_ReturnsBadRequest()
	{
		// Arrange
		var createDto = new ToDoCreateDTO
		{
			Title = "Test",
			Description = "Description",
			ExpiryDate = DateTime.UtcNow.AddDays(1)
		};

		_mockRepository.Setup(repo => repo.AddAsync(It.IsAny<ToDo>())).ReturnsAsync(false);

		// Act
		var result = await _controller.Create(createDto);

		// Assert
		Assert.IsType<BadRequestObjectResult>(result);
	}

	[Fact]
	public async Task Delete_ValidId_ReturnsNoContent()
	{
		// Arrange
		var todo = new ToDo { Id = 1, Title = "Test", Description = "Description" };
		_mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(todo);
		_mockRepository.Setup(repo => repo.RemoveAsync(todo)).ReturnsAsync(true);

		// Act
		var result = await _controller.Delete(1);

		// Assert
		Assert.IsType<NoContentResult>(result);
	}

	[Fact]
	public async Task Delete_InvalidId_ReturnsNotFound()
	{
		// Arrange
		_mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ToDo?)null);

		// Act
		var result = await _controller.Delete(1);

		// Assert
		Assert.IsType<NotFoundObjectResult>(result);
	}

	[Fact]
	public async Task Search_NoMatch_ReturnsNotFound()
	{
		// Arrange
		_mockRepository.Setup(repo => repo.GetAllWhereAsync(It.IsAny<Expression<Func<ToDo, bool>>>()))
			.ReturnsAsync(new List<ToDo>());

		// Act
		var result = await _controller.Search("Nonexistent", null);

		// Assert
		Assert.IsType<NotFoundObjectResult>(result);
	}

	[Fact]
	public async Task GetIncoming_TodayFilter_ReturnsOkResult_WithMatchingToDos()
	{
		// Arrange
		var today = DateTime.UtcNow.Date;
		var todos = new List<ToDo>
		{
			new() { Id = 1, Title = "Test 1", ExpiryDate = today },
			new() { Id = 2, Title = "Test 2", ExpiryDate = today.AddDays(2) }
		};
		_mockRepository.Setup(repo => repo.GetAllWhereAsync(It.IsAny<Expression<Func<ToDo, bool>>>()))
			.ReturnsAsync(todos);

		// Act
		var result = await _controller.GetIncoming("week");

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		var returnedTodos = Assert.IsType<List<ToDo>>(okResult.Value);
		Assert.Equal(2, returnedTodos.Count);
		Assert.Equal("Test 1", returnedTodos[0].Title);
		Assert.Equal("Test 2", returnedTodos[1].Title);
	}

	[Fact]
	public async Task MarkAsComplete_ValidId_ReturnsNoContent()
	{
		// Arrange
		var todo = new ToDo
			{ Id = 1, Title = "Test", Description = "Description", PercentComplete = 0, IsDone = false };
		_mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(todo);
		_mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<ToDo>())).ReturnsAsync(true);

		// Act
		var result = await _controller.Update(1);

		// Assert
		Assert.IsType<NoContentResult>(result);
	}

	[Fact]
	public async Task MarkAsComplete_InvalidId_ReturnsNotFound()
	{
		// Arrange
		_mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ToDo?)null);

		// Act
		var result = await _controller.Update(1);

		// Assert
		Assert.IsType<NotFoundObjectResult>(result);
	}

	[Fact]
	public async Task Update_ValidData_ReturnsNoContent()
	{
		// Arrange
		var updateDto = new ToDoUpdateDTO
			{ Title = "Updated Title", Description = "Updated Description", ExpiryDate = DateTime.UtcNow.AddDays(1) };
		var todo = new ToDo
			{ Id = 1, Title = "Old Title", Description = "Old Description", ExpiryDate = DateTime.UtcNow };
		_mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(todo);
		_mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<ToDo>())).ReturnsAsync(true);

		// Act
		var result = await _controller.Update(1, updateDto);

		// Assert
		Assert.IsType<NoContentResult>(result);
	}

	[Fact]
	public async Task Update_InvalidId_ReturnsNotFound()
	{
		// Arrange
		var updateDto = new ToDoUpdateDTO
			{ Title = "Updated Title", Description = "Updated Description", ExpiryDate = DateTime.UtcNow.AddDays(1) };
		_mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ToDo?)null);

		// Act
		var result = await _controller.Update(1, updateDto);

		// Assert
		Assert.IsType<NotFoundObjectResult>(result);
	}
}