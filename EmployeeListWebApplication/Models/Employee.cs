using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmployeeListWebApplication.Models
{
	public class Employee : IValidatableObject
	{
		[Key]
		public int Id { get; set; }
		public string PersonnelNumber { get; set; }
		public string FullName { get; set; }
		public Gender Gender { get; set; }
		public DateTime DateOfBirth { get; set; }
		public bool IsStaff { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			switch (this.IsStaff)
			{
				case true when string.IsNullOrEmpty(PersonnelNumber):
					yield return new ValidationResult("Для штатного сотрудніка обязятелено заполнение табельного номера");
					break;
				case false when !string.IsNullOrEmpty(PersonnelNumber):
					yield return new ValidationResult("У внештатного сотрудника не должно быть табельного номера");
					break;
			}

			yield break;
		}
	}
}
