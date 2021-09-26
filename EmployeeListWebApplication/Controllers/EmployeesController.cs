using EmployeeListWebApplication.Data;
using EmployeeListWebApplication.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeListWebApplication.Controllers
{
	public class EmployeesController : Controller
	{
		private readonly EmployeeListDbContext _dbContext;

		public EmployeesController(EmployeeListDbContext context)
		{
			_dbContext = context;
		}

		//Get Employees By Pagination
		[HttpGet]
		public async Task<IActionResult> GetPagination(int page = 1, int rows = 10)
		{
			var count = await _dbContext.Employees.CountAsync();
			var employees = await _dbContext.Employees
				.Skip((page - 1) * rows)
				.Take(rows)
				.ToListAsync();

			return Json(new PageViewModel {
				Total = count,
				Rows = employees,
			});
		}

		// GET: Employees/Details/5
		[HttpGet]
		public async Task<IActionResult> Details(int id)
		{
			var employee = await _dbContext.Employees
				.FirstOrDefaultAsync(m => m.Id == id);
			if (employee == null)
			{
				return NotFound();
			}

			return Json(employee);
		}

		// POST: Employees/Create
		[HttpPost]
		public async Task<IActionResult> Create([Bind("Id,PersonnelNumber,FullName,Gender,DateOfBirth,IsStaff")] Employee employee)
		{
			if (ModelState.IsValid && await TryInsertOrUpdateAsync(employee))
			{
				return Ok();
			}

			return BadRequest(ModelState);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id, [Bind("Id,PersonnelNumber,FullName,Gender,DateOfBirth,IsStaff")] Employee employee)
		{
			if (id != employee.Id)
			{
				return BadRequest();
			}
			
			if (ModelState.IsValid && await TryInsertOrUpdateAsync(employee))
			{
				return Ok();
			}

			return BadRequest(ModelState);
		}

		[HttpPost]
		public async Task<IActionResult> UploadFromFile(IFormFile uploadedFile)
		{
			if (uploadedFile == null)
			{
				return BadRequest();
			}

			List<Employee> employees;

			using (var uploadStream = uploadedFile.OpenReadStream())
			using (var sr = new StreamReader(uploadStream))
			using (var jsonReader = new JsonTextReader(sr))
			{
				employees = new JsonSerializer().Deserialize<List<Employee>>(jsonReader);
			}

			using var memStream = new MemoryStream();
			using var tw = new StreamWriter(memStream);
			foreach (var employee in employees)
			{
				var employeeIdentity = $"{employee.Id} : {employee.PersonnelNumber ?? null} : {employee.FullName}";
				if (TryValidateModel(employee) && await TryInsertOrUpdateAsync(employee))
				{
					await tw.WriteLineAsync($"{employeeIdentity} : Loaded successfully");
				}
				else
				{
					await tw.WriteLineAsync($"{employeeIdentity} : Loaded with errors");
					foreach(var err in ModelState.Values.SelectMany(v => v.Errors))
					{
						await tw.WriteLineAsync(err.ErrorMessage);
					}

					ModelState.Clear();
				}

				await tw.WriteLineAsync();
				await tw.FlushAsync();
			}

			return File(memStream.GetBuffer(), "text/plain", $"{uploadedFile.FileName}_report.txt");
		}

		[HttpPost]
		public async Task<StatusCodeResult> Delete(int id)
		{
			var employee = await _dbContext.Employees.FindAsync(id);
			if (employee == null)
			{
				return NotFound();
			}

			_dbContext.Employees.Remove(employee);
			await _dbContext.SaveChangesAsync();
			return Ok();
		}

		private async Task<bool> TryInsertOrUpdateAsync(Employee empl)
		{
			try
			{
				var employee = await _dbContext.Employees.FirstOrDefaultAsync(e=> e.Id == empl.Id || e.PersonnelNumber == empl.PersonnelNumber);
				if (employee == null)
				{
					empl.Id = 0;
					await _dbContext.Employees.AddAsync(empl);
				} 
				else
				{
					empl.Id = employee.Id;
					_dbContext.Entry(employee).CurrentValues.SetValues(empl);
				}

				await _dbContext.SaveChangesAsync();
			}
			catch (DbUpdateException ex) when (ex.InnerException != null
					&& (ex.InnerException as SqlException)?.Number == 2601)
			{
				ModelState.AddModelError("", $"Employee number {empl.PersonnelNumber} is already registered");
				return false;
			}

			return true;
		}
	}
}
