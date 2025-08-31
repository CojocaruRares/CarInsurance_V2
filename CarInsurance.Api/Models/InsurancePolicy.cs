namespace CarInsurance.Api.Models;

public class InsurancePolicy
{
    public long Id { get; set; }

    public long CarId { get; set; }
    public Car Car { get; set; } = default!;
    public string? Provider { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsLogged { get; set; } = false;
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}