using DatabaseFirst.Data;
using DatabaseFirst.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Tracing;
using System.Threading.Tasks.Dataflow;

namespace DatabaseFirst
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var db = new SoftUniContext();
            var start = new StartUp(db);
            //start.GetEmployeesFullInformation();
            //start.GetEmployeesWithSalaryOver50000();
            start.DeleteProjectById();
        }
    }
    public class StartUp
    {
        private SoftUniContext _context;
        public StartUp(SoftUniContext context)
        {
            _context = context;
        }
        public void GetEmployeesFullInformation()
        {
            var res = _context.Employees.Select(x => new { x.FirstName, x.MiddleName, x.LastName, x.JobTitle,x.Salary});
            foreach (var item in res) 
            {
                Console.WriteLine($"Name: {item.FirstName} {item.MiddleName} {item.LastName}, Job:{item.JobTitle}, Salary:{item.Salary.ToString("F2")}");
            }
        }
        public void GetEmployeesWithSalaryOver50000()
        {
            var res = _context.Employees.Select(x => new { x.FirstName, x.Salary }).Where(x => x.Salary > 50000).OrderBy(x=>x.FirstName);
            foreach (var item in res)
            {
                Console.WriteLine($"Name: {item.FirstName}, Salaray: {item.Salary.ToString("F2")}");
            }
        }
        public void GetEmployeesFromResearchAndDevelopment()
        {
            var res = _context.Employees
                .Where(x => x.Department.Name == "Research and Development")
                .Select(x => new { x.FirstName, x.LastName, x.Department.Name, x.Salary })
                .OrderByDescending(x => x.FirstName)
                .OrderBy(x => x.Salary).ToList();
            foreach (var item in res)
            {
                Console.WriteLine($"{item.FirstName} {item.LastName} {item.Name} {item.Salary.ToString("F2")}");
            }

        }
        public void AddNewAddressToEmployee()
        {
            _context.Addresses.Add(new Models.Address
            {
                AddressText = "Vitoshka 15",
                TownId = 4,
            });
            _context.SaveChanges();
            var nakov = _context.Employees.Where(x=>x.LastName == "Nakov").FirstOrDefault();
            nakov.AddressId =  _context.Addresses.Where(x=>x.AddressText == "Vitoshka 15").Select(x=>x.AddressId).FirstOrDefault();
            _context.SaveChanges();
            var res = _context.Employees.Select(x => new { x.AddressId, x.Address.AddressText }).OrderByDescending(x => x.AddressId).Take(10).ToList();
            foreach (var item in res) 
            {
                Console.WriteLine(item.AddressText);
            }

        }
        public void GetEmployeesInPeriod()
        {
            var res = _context.Employees
                .Where(x => x.Projects.Any(y => y.StartDate >= new DateTime(2001, 1, 1) && y.EndDate <= new DateTime(2003, 12, 31)))
                .Select(e => new
                {
                    Employee = e,
                    Manager = e.Manager,
                    Projects = e.Projects.Where(p => p.StartDate >= new DateTime(2001, 1, 1) && p.EndDate <= new DateTime(2003, 12, 31))
                })
                .Take(10).ToList();
            foreach (var item in res) 
            {
                Console.WriteLine($"{item.Employee.FirstName} {item.Employee.LastName} - Manager: {item.Manager?.FirstName} {item.Manager?.LastName}");
                foreach(var project in item.Projects)
                {
                    string enddate = project.EndDate.HasValue ? project.EndDate.Value.ToString("yyyy-MM-dd") : "not finished";
                    Console.WriteLine($"--{project.Name} - {project.StartDate:yyyy-MM-dd} - {enddate}");
                }
            }
        }
        public void GetAddressesByTown()
        {
            var res = _context.Employees
                .Include(e => e.Address)
                .ThenInclude(a => a.Town)
                .GroupBy(x => x.AddressId)
                .Select(g => new
                {
                    AddressText = g.First().Address.AddressText,
                    AddressTown = g.First().Address.Town.Name,
                    Employees = g.ToList()


                })
                .ToList()
                .OrderByDescending(x => x.Employees.Count())
                .ThenBy(x => x.AddressTown)
                .ThenBy(x => x.AddressText)
                .Take(10);
                
            foreach (var group in res)
            {
                Console.WriteLine($"Address: {group.AddressText}, Town: {group.AddressTown}-Number of emp:{group.Employees.Count()}");
               
            }
        }
        public void GetEmployee147()
        {
            var res = _context.Employees.Where(x => x.EmployeeId == 147).Select(x => new
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                Job = x.JobTitle,
                Projects = x.Projects.OrderBy(x=>x.Name).ToList()
            }); 
            foreach(var person in res)
            {
                Console.WriteLine($"{person.FirstName} {person.LastName} {person.Job}");
                Console.WriteLine("Projects: ");
                foreach (var project in person.Projects)
                {
                    Console.WriteLine(project.Name);
                }
            }
        }
        public void GetDepartmentsWithMoreThan5Employees()
        {
            var res = _context.Departments
                .Where(x => x.Employees.Count > 5)
                .OrderBy(x => x.Employees.Count)
                .ThenBy(x => x.Name).ToList();
            foreach (var dep in res) 
            {
                Console.WriteLine($"Department: {dep.Name}, Manager:{dep.Manager?.FirstName} {dep.Manager?.LastName}");
                foreach (var item in dep.Employees.OrderBy(x=>x.FirstName).ThenBy(x=>x.LastName))
                {
                    Console.WriteLine($"{item.FirstName} {item.LastName} {item.JobTitle}");
                }
            }
        }
        public void GetLatestProjects()
        {
            var res = _context.Projects.OrderByDescending(x => x.StartDate).ThenBy(x => x.Name).Take(10)
                .Select(x => new
                {
                    Name = x.Name,
                    Desc = x.Description,
                    StartDate = x.StartDate,
                }).ToList();
            foreach(var pr in res)
            {
                Console.WriteLine($"Name:{pr.Name}, \nDescription:{pr.Desc}, \nStart Date: {pr.StartDate.ToString("M-d-yyyy")}");
            }
        }
        public void IncreaseSalaries()
        {
            var res = _context.Employees
                .Where(x => x.Department.Name == "Engineering" || x.Department.Name == "Tool Design" || x.Department.Name == "Marketing" || x.Department.Name == "Information Services")
                .OrderBy(x => x.FirstName).ThenByDescending(x => x.LastName);
            foreach (var emp in res) 
            {
                Console.WriteLine($"{emp.FirstName} {emp.LastName} with a salary {emp.Salary}:");
                emp.Salary = IncreaseByPercetage(emp.Salary, 12);
                Console.WriteLine($"New salary: {emp.Salary}");
            }
            _context.SaveChanges();
        }
        public static Func<decimal,decimal,decimal> IncreaseByPercetage = (decimal number, decimal percentage) => number * (1 + (percentage / 100));
        public void GetEmployeesByFirstNameStartingWithSa()
        {
            var res = _context.Employees.Where(x => x.FirstName.ToLower().StartsWith("sa"))
                .Select(x => new
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Job = x.JobTitle,
                    Salary = x.Salary.ToString("F2")
                }).OrderBy(x => x.FirstName).ThenBy(x => x.LastName);
            foreach (var emp in res) 
            {
                Console.WriteLine($"{emp.FirstName} {emp.LastName} - {emp.Job} - (${emp.Salary})");
            }
        }
        public void DeleteProjectById()
        {
            var res = _context.Projects.FirstOrDefault(x => x.ProjectId == 2);
            if (res == null)
            {
                Console.WriteLine("Project not found");
                return;
            }
            var employees = _context.Employees.Where(x => x.Projects.Any(x => x.ProjectId == 2)).ToList();
            foreach (var emp in employees)
            {
                var projectsToRemove = emp.Projects.Where(p => p.ProjectId == 2).ToList();
                foreach (var project in projectsToRemove)
                {
                    emp.Projects.Remove(project);
                }
            }
            _context.SaveChanges();
            _context.Projects.Remove(res);
            _context.SaveChanges();
            Console.WriteLine("Successfully deleted");
            var res2 = _context.Projects.Select(x => new
            {
                Name = x.Name,
            }).Take(10);
            foreach (var item in res2)
            {
                Console.WriteLine(item.Name);
            }
        }

    }
}
