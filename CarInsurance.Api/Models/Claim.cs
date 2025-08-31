namespace CarInsurance.Api.Models;

public class Claim
{
    public long Id { get; set; }

    public long InsurancePolicyId { get; set; }
    public InsurancePolicy Policy { get; set; } = default!;

    public DateOnly ClaimDate { get; set; }
    public string Description { get; set; } = default!;
    public decimal Amount { get; set; }
}