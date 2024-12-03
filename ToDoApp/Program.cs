using Microsoft.EntityFrameworkCore;
using ToDoApp.Extensions;
using ToDoApp.Infrastructure.Context;
using ToDoApp.Infrastructure.Repository;

namespace ToDoApp;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();
		builder.Services.AddDbContext<ToDoDbContext>(
			options => options.UseNpgsql(builder.Configuration.GetConnectionString("ToDoDb"),
				sqlOptions => sqlOptions.MigrationsAssembly("ToDoApp.Infrastructure")
			)
		);
		builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

		builder.Services.AddLogging();

		builder.Logging.AddConsole();
		builder.Logging.AddDebug();

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
			app.ApplyMigrations();
		}

		app.UseHttpsRedirection();

		app.UseAuthorization();


		app.MapControllers();

		app.Run();
	}
}