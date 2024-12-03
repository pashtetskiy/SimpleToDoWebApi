using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Domain.Models
{
    public class ToDo : BaseEntity
	{
		[Required]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Description { get; set; } = String.Empty;

		[DataType(DataType.DateTime)]
		public DateTime ExpiryDate { get; set; }

		[Range(0, 100)]
		public int PercentComplete { get; set; }
		public bool IsDone { get; set; }
	}
}
