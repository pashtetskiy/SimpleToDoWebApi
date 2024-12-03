using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Models;

namespace ToDoApp.Infrastructure.Context
{
	public class ToDoDbContext : DbContext
	{
		public ToDoDbContext(DbContextOptions options) : base(options)
		{
			
		}
	    public DbSet<ToDo> ToDos { get; set; } = default!;
 	}
}
