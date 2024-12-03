using Microsoft.EntityFrameworkCore;
using ToDoApp.Infrastructure.Context;

namespace ToDoApp.Extensions
{
	/// A static class containing extension methods for applying database migrations.
	public static class MigrationExtensions
	{
		/// Applies pending database migrations at application startup.
		public static void ApplyMigrations(this IApplicationBuilder app)
		{
			// Create a service scope to access scoped services from the service container.
			using IServiceScope scope = app.ApplicationServices.CreateScope();

			// Retrieve the database context (ToDoDbContext) from the service container.
			using ToDoDbContext dbContext = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();

			// Apply pending migrations to the database.
			// If the database does not exist, it will be created. If there are pending migrations, they will be applied.
			dbContext.Database.Migrate();
		}
	}
}
