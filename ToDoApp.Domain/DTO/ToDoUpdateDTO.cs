using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Domain.DTO
{
	public class ToDoUpdateDTO
	{
		[Required]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Description { get; set; } = string.Empty;

		[DataType(DataType.DateTime)]
		public DateTime ExpiryDate { get; set; }

	}
}
