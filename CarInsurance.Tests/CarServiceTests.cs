using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Tests
{
    public class CarServiceTests
    {
        private static AppDbContext MakeDb()
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(opts);
        }

        private static AppDbContext AddOneCarWithPolicy(DateOnly start, DateOnly end)
        {
            var db = MakeDb();

            var owner = new Owner { Name = "Test Owner", Email = "test@test.com" };
            db.Owners.Add(owner);
            db.SaveChanges();

            var car = new Car
            {
                Vin = "VIN-TEST",
                Make = "Make",
                Model = "Model",
                YearOfManufacture = 2024,
                OwnerId = owner.Id
            };
            db.Cars.Add(car);
            db.SaveChanges();

            db.Policies.Add(new InsurancePolicy
            {
                CarId = car.Id,
                Provider = "Allianz",
                StartDate = start,
                EndDate = end
            });
            db.SaveChanges();

            return db;
        }

        private (CarService service, Car car, DateOnly start, DateOnly end) SetupTestData()
        {
            var start = new DateOnly(2025, 1, 1);
            var end = new DateOnly(2025, 12, 31);
            var db = AddOneCarWithPolicy(start, end);
            var service = new CarService(db);
            var car = db.Cars.First();
            return (service, car, start, end);
        }


        [Fact]
        public async Task IsInsuranceValid_True_ForValidDate()
        {
            var (service, car, start, end) = SetupTestData();

            Assert.True(await service.IsInsuranceValidAsync(car.Id, start));
            Assert.True(await service.IsInsuranceValidAsync(car.Id, end));
        }

        [Fact]
        public async Task IsInsuranceValid_False_ForDateOutsideInterval()
        {
            var (service, car, start, end) = SetupTestData();

            Assert.False(await service.IsInsuranceValidAsync(car.Id, new DateOnly(2024, 12, 31)));
            Assert.False(await service.IsInsuranceValidAsync(car.Id, new DateOnly(2026, 1, 1)));
        }

        [Fact]
        public async Task IsInsuranceValid_Throws_NoCarFound()
        {
            var (service, car, start, end) = SetupTestData();

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await service.IsInsuranceValidAsync(999, new DateOnly(2025, 1, 1));
            });
        }

        [Fact]
        public async Task IsInsuranceValid_Throws_ArgumentException()
        {
            var (service, car, start, end) = SetupTestData();
            var futureDate = DateOnly.FromDateTime(DateTime.Now).AddYears(6);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.IsInsuranceValidAsync(car.Id, new DateOnly(1899, 1, 1));
                await service.IsInsuranceValidAsync(car.Id, futureDate);
            });
        }

    }
}