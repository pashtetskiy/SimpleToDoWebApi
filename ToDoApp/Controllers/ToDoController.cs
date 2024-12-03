using Microsoft.AspNetCore.Mvc;
using ToDoApp.Domain.DTO;
using ToDoApp.Domain.Models;
using ToDoApp.Infrastructure.Repository;

namespace ToDoApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ToDoController : ControllerBase
{
	private readonly IRepository<ToDo> _repository;

	public ToDoController(IRepository<ToDo> repository)
	{
		_repository = repository;
	}

	// Endpoint to fetch all tasks
	[HttpGet("getAll")]
	public async Task<IActionResult> GetAll()
	{
		var todoList = await _repository.ToListAsync();
		if (todoList is null) return NoContent(); // Returns 204 if no content is found
		return Ok(todoList); // Returns 200 with the list of tasks
	}

	// Endpoint to fetch a task by its ID
	[HttpGet("getById{id:int}")]
	public async Task<IActionResult> GetById(int id)
	{
		if (id <= 0)
			return BadRequest("Invalid ID provided."); // Ensure ID is valid

		var todo = await _repository.GetByIdAsync(id);
		return todo is null ? NotFound() : Ok(todo); // Returns 404 if not found, else 200
	}

	// Endpoint to search for task by title and/or description
	[HttpGet("search")]
	public async Task<IActionResult> Search(string? titleName, string? description)
	{
		if (string.IsNullOrWhiteSpace(titleName) && string.IsNullOrWhiteSpace(description))
			return BadRequest("At least one search parameter (titleName or description) must be provided.");

		var todos = await _repository.GetAllWhereAsync(t =>
			(string.IsNullOrWhiteSpace(titleName) || t.Title.Contains(titleName)) &&
			(string.IsNullOrWhiteSpace(description) || t.Description.Contains(description))
		);

		return todos == null || !todos.Any()
			? NotFound("No items match the search criteria.")
			: Ok(todos);
	}

	// Endpoint to find tasks based on an incoming filter (today, next day, or week)
	[HttpGet("incoming")]
	public async Task<IActionResult> GetIncoming(string? filter = "today")
	{
		if (string.IsNullOrWhiteSpace(filter))
			return BadRequest("Filter cannot be empty."); // Validate the filter

		var today = DateTime.UtcNow.Date;
		var todos = filter.ToLower() switch
		{
			"today" => await _repository.GetAllWhereAsync(t => t.ExpiryDate.Date == today),
			"nextday" => await _repository.GetAllWhereAsync(t => t.ExpiryDate.Date == today.AddDays(1)),
			"week" => await _repository.GetAllWhereAsync(t =>
				t.ExpiryDate.Date >= today &&
				t.ExpiryDate.Date <= today.AddDays(7)),
			_ => null
		};

		if (todos == null || !todos.Any()) return NotFound("No ToDos match the specified filter.");

		return Ok(todos);
	}

	// Endpoint to create a new task item
	[HttpPost("create")]
	public async Task<IActionResult> Create(ToDoCreateDTO createDto)
	{
		var todo = new ToDo()
		{
			Title = createDto.Title,
			Description = createDto.Description,
			ExpiryDate = createDto.ExpiryDate,
			IsDone = false,
			PercentComplete = 0
		};

		var result = await _repository.AddAsync(todo);
		if (result) return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);

		return BadRequest("Failed to create the ToDo item.");
	}

	// Endpoint to mark a task as completed
	[HttpPost("markAsComplete")]
	public async Task<IActionResult> Update(int id)
	{
		if (id <= 0)
			return BadRequest("Invalid ID provided."); // Validate ID

		var todo = await _repository.GetByIdAsync(id);
		if (todo is null)
			return NotFound("ToDo item not found."); // Handle null case

		todo.PercentComplete = 100;
		todo.IsDone = true;

		var result = await _repository.UpdateAsync(todo);
		if (!result)
			return BadRequest("Failed to mark the ToDo item as complete.");

		return NoContent();
	}

	// Endpoint to update an existing task
	[HttpPut("update{id:int}")]
	public async Task<IActionResult> Update(int id, ToDoUpdateDTO updateDto)
	{
		if (id <= 0 || updateDto == null)
			return BadRequest("Invalid ID or update data provided."); // Validate input

		var todo = await _repository.GetByIdAsync(id);
		if (todo is null)
			return NotFound("ToDo item not found."); // Handle null case

		todo.Title = updateDto.Title;
		todo.Description = updateDto.Description;
		todo.ExpiryDate = updateDto.ExpiryDate;

		var result = await _repository.UpdateAsync(todo);
		if (!result)
			return BadRequest("Failed to update the ToDo item.");

		return NoContent();
	}

	// Endpoint to update the completion percentage of a task
	[HttpPut("setPercentComplete")]
	public async Task<IActionResult> Update(int id, int percentComplete)
	{
		if (id <= 0 || percentComplete < 0 || percentComplete > 100)
			return BadRequest("Invalid ID or percentage value provided."); // Validate inputs

		var todo = await _repository.GetByIdAsync(id);
		if (todo is null)
			return NotFound("ToDo item not found."); // Handle null case

		todo.PercentComplete = percentComplete;

		if (percentComplete == 100) todo.IsDone = true; // Marks the item as completed if 100% is set

		var result = await _repository.UpdateAsync(todo);
		if (!result)
			return BadRequest("Failed to update the percentage of the ToDo item.");

		return NoContent();
	}

	// Endpoint to delete a task by its ID
	[HttpDelete("Delete{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		if (id <= 0)
			return BadRequest("Invalid ID provided."); // Validate ID

		var todo = await _repository.GetByIdAsync(id);
		if (todo is null)
			return NotFound("ToDo item not found."); // Handle null case

		var result = await _repository.RemoveAsync(todo);

		if (!result)
			return BadRequest("Failed to delete the ToDo item.");

		return NoContent();
	}
}