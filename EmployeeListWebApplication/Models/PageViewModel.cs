using System.Collections.Generic;

namespace EmployeeListWebApplication.Models
{
	public class PageViewModel
	{
		public int Total { get; set;  }
		public IEnumerable<Employee> Rows { get; set; }
	}
}
