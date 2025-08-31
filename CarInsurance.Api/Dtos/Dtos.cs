namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);
public record CreateClaimDto(DateOnly ClaimDate, string Description, decimal Amount);
public record ClaimDto(long Id, long InsurancePolicyId, DateOnly ClaimDate, string Description, decimal Amount);
public record PolicyHistoryDto(
    long Id,
    DateOnly StartDate,
    DateOnly EndDate,
    string Provider,
    List<CreateClaimDto> Claims
);