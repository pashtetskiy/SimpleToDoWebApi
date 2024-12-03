using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToDoApp.Domain.Models;
using ToDoApp.Infrastructure.Context;

namespace ToDoApp.Infrastructure.Repository;

// Generic repository implementation for managing database operations
public class Repository<T> : IRepository<T> where T : BaseEntity
{
	private readonly ToDoDbContext _context; // Database context
	private readonly ILogger<Repository<T>> _logger; // Logger for capturing errors and debugging information

	// Constructor to initialize database context and logger
	public Repository(ToDoDbContext context, ILogger<Repository<T>> logger)
	{
		_context = context;
		_logger = logger;
	}

	// Fetches all records of type T from the database
	public async Task<List<T>> ToListAsync()
	{
		var result = new List<T>();
		try
		{
			result = await _context.Set<T>().ToListAsync(); // Fetches all records asynchronously
		}
		catch (Exception e)
		{
			_logger.LogError(e, e.Message); // Logs the exception if any error occurs
			return result;
		}

		return result;
	}

	// Fetches a record by its ID
	public async Task<T?> GetByIdAsync(long? id)
	{
		if (id == null || id <= 0)
		{
			_logger.LogWarning("Invalid ID provided for GetByIdAsync."); // Logs a warning for invalid ID
			return null;
		}

		T? result = null;
		try
		{
			result = await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id); // Finds the record by ID
		}
		catch (Exception e)
		{
			_logger.LogError(e, e.Message); // Logs the exception if any error occurs
			return result;
		}

		return result;
	}

	// Fetches all records matching the given predicate
	public async Task<List<T>?> GetAllWhereAsync(Expression<Func<T, bool>> predicate)
	{
		List<T>? result = null;

		try
		{
			result = await _context.Set<T>().Where(predicate).ToListAsync(); // Filters records based on the predicate
		}
		catch (Exception e)
		{
			_logger.LogError(e, e.Message); // Logs the exception if any error occurs
			return result;
		}

		return result;
	}

	// Adds a new record to the database
	public async Task<bool> AddAsync(T entity)
	{
		var result = false;
		try
		{
			await _context.AddAsync(entity); // Adds the entity to the database
			result = await _context.SaveChangesAsync() > 0; // Persists the changes to the database
		}
		catch (Exception e)
		{
			_logger.LogError(e, e.Message); // Logs the exception if any error occurs
			return result;
		}

		return result;
	}

	// Updates an existing record in the database
	public async Task<bool> UpdateAsync(T entity)
	{
		var result = false;
		try
		{
			_context.Update(entity); // Marks the entity as updated
			result = await _context.SaveChangesAsync() > 0; // Persists the changes to the database
		}
		catch (Exception e)
		{
			_logger.LogError(e, e.Message); // Logs the exception if any error occurs
			return result;
		}

		return result;
	}

	// Removes a record from the database
	public async Task<bool> RemoveAsync(T entity)
	{
		var result = false;
		try
		{
			// Checks if the entity exists in the database
			if (await _context.Set<T>().FindAsync(entity.Id) is null)
				throw new Exception("Entity not found"); // Throws an exception if entity does not exist

			_context.Remove(entity); // Marks the entity for deletion
			result = await _context.SaveChangesAsync() > 0; // Persists the changes to the database
		}
		catch (Exception e)
		{
			_logger.LogError(e, e.Message); // Logs the exception if any error occurs
			return result;
		}

		return result;
	}
}