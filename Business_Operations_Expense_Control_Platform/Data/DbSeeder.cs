using Business_Operations_Expense_Control_Platform.Models;
using Microsoft.EntityFrameworkCore;

namespace Business_Operations_Expense_Control_Platform.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(DatabaseContext db)
        {
            if (await db.Departments.AnyAsync()) return; // already seeded — never runs twice

            var engineering = new UserDepartment { Name = "Engineering" };
            var marketing = new UserDepartment { Name = "Marketing" };
            db.Departments.AddRange(engineering, marketing);
            await db.SaveChangesAsync(); // save now so Ids exist for the users below

            string hash = BCrypt.Net.BCrypt.HashPassword("Demo123");

            db.Users.AddRange(
                new User { FirstName = "Ada", LastName = "Employee", Email = "employee1@demo.com", PasswordHash = hash, Role = UserRole.Employee, DepartmentId = engineering.Id },
                new User { FirstName = "Bayo", LastName = "Employee", Email = "employee2@demo.com", PasswordHash = hash, Role = UserRole.Employee, DepartmentId = marketing.Id },
                new User { FirstName = "Chika", LastName = "Manager", Email = "manager1@demo.com", PasswordHash = hash, Role = UserRole.Manager, DepartmentId = engineering.Id },
                new User { FirstName = "Dapo", LastName = "Manager", Email = "manager2@demo.com", PasswordHash = hash, Role = UserRole.Manager, DepartmentId = marketing.Id },
                new User { FirstName = "Efe", LastName = "Finance", Email = "finance@demo.com", PasswordHash = hash, Role = UserRole.Finance, DepartmentId = engineering.Id }
            );
            await db.SaveChangesAsync();
        }
    }
}