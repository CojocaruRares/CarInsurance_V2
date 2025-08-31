using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CarInsurance.Api.Dtos;
using Xunit;

public class CarControllerTests : IClassFixture<CarInsuranceWebApplicationFactory>
{
    private readonly CarInsuranceWebApplicationFactory _factory;

    public CarControllerTests(CarInsuranceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task InsuranceValid_Returns_400_On_Invalid_Date()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/cars/1/insurance-valid?date=12345"); 

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task CreateClaim_Returns_404_On_Inexistent_Policy()
    {
        var client = _factory.CreateClient();
        var claim = new CreateClaimDto(new DateOnly(1900, 11, 11), "Test", 10);
        var resp = await client.PostAsJsonAsync("/api/cars/1/claims",claim);

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task CreateClaim_Returns_201_On_Create()
    {
        var client = _factory.CreateClient();
        var claim = new CreateClaimDto(new DateOnly(2025, 11, 11), "Test", 10);
        var resp = await client.PostAsJsonAsync("/api/cars/1/claims", claim);

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }



}
