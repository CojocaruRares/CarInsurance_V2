using System;
using System.Linq;
using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class CarInsuranceWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbDescriptors = services.Where(
                d => d.ServiceType.FullName!.Contains("DbContextOptions"))
                .ToList();

            foreach (var d in dbDescriptors)
                services.Remove(d);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            var owner = new Owner { Name = "Ana", Email = "ana@example.com" };
            db.Owners.Add(owner);
            db.SaveChanges();

            var car = new Car
            {
                Id = 1,
                Vin = "VIN-IT-1",
                Make = "Dacia",
                Model = "Logan",
                YearOfManufacture = 2018,
                OwnerId = owner.Id
            };
            db.Cars.Add(car);
            db.SaveChanges();

            db.Policies.Add(new InsurancePolicy
            {
                CarId = car.Id,
                Provider = "Groupama",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 12, 31)
            });
            db.SaveChanges();
        });
    }
}
