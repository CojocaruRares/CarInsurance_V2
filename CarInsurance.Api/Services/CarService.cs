using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");
        var maxFutureDate = DateOnly.FromDateTime(DateTime.Now).AddYears(5);

        if (date.Year < 1900 || date > maxFutureDate)
            throw new ArgumentException("Invalid date provided");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            p.EndDate >= date
        );
    }

    public async Task<ClaimDto> CreateClaimAsync(long carId, CreateClaimDto dto)
    {
        var policy = await _db.Policies
            .Where(p => p.CarId == carId && p.StartDate <= dto.ClaimDate && p.EndDate >= dto.ClaimDate)
            .FirstOrDefaultAsync();

        if (policy == null)
            throw new KeyNotFoundException("No valid policy found for this car at the given date");

        var claim = new Claim
        {
            InsurancePolicyId = policy.Id,
            ClaimDate = dto.ClaimDate,
            Description = dto.Description,
            Amount = dto.Amount
        };

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();

        return new ClaimDto(
            claim.Id,
            claim.InsurancePolicyId,
            claim.ClaimDate,
            claim.Description,
            claim.Amount
        );
    }

    public async Task<ClaimDto> GetClaimAsync(long claimId)
    {
        var claim = await _db.Claims.FirstOrDefaultAsync(c => c.Id == claimId);
        if (claim == null) throw new KeyNotFoundException($"Claim {claimId} not found");

        return new ClaimDto(
            claim.Id,
            claim.InsurancePolicyId,
            claim.ClaimDate,
            claim.Description,
            claim.Amount
        );
    }

    public async Task<IEnumerable<PolicyHistoryDto>> GetCarHistoryAsync(long carId)
    {
        var policies = await _db.Policies
            .Where(p => p.CarId == carId)
            .Include(p => p.Claims)
            .OrderBy(p => p.StartDate)
            .ToListAsync();

        return policies.Select(p => new PolicyHistoryDto(
            p.Id,
            p.StartDate,
            p.EndDate,
            p.Provider ?? "Unknown",
            p.Claims
            .OrderBy(c => c.ClaimDate)
            .Select(c => new CreateClaimDto(
                c.ClaimDate,
                c.Description,
                c.Amount
            )).ToList()
        ));
    }

}