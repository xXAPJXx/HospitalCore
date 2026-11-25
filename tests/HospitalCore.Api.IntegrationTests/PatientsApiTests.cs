using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HospitalCore.Api.Controllers.Patients;
using HospitalCore.Modules.Patients.Application.GetPatientById;
using HospitalCore.Modules.Patients.Domain;

namespace HospitalCore.Api.IntegrationTests;

public sealed class PatientsApiTests(HospitalCoreApiFactory factory)
{
    private static readonly Uri PatientsUri = new("/api/patients", UriKind.Relative);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public async Task RegisterPatient_ThenGetById_ReturnsRegisteredPatient()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        var documentNumber = UniqueDocumentNumber();
        var request = ValidRequest(documentNumber.ToLowerInvariant());

        using var created = await client.PostAsJsonAsync(PatientsUri, request, JsonOptions, cancellationToken);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var registration = await created.Content.ReadFromJsonAsync<RegisterPatientResponse>(JsonOptions, cancellationToken);
        Assert.NotNull(registration);
        Assert.Equal($"/api/patients/{registration.Id}", created.Headers.Location?.AbsolutePath);

        using var fetched = await client.GetAsync(created.Headers.Location, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
        var patient = await fetched.Content.ReadFromJsonAsync<PatientResponse>(JsonOptions, cancellationToken);
        Assert.NotNull(patient);
        Assert.Equal(registration.Id, patient.Id);
        Assert.Equal("María", patient.GivenName);
        Assert.Equal("García López", patient.FamilyName);
        Assert.Equal(new DateOnly(1985, 3, 14), patient.DateOfBirth);
        Assert.Equal(AdministrativeSex.Female, patient.Sex);
        Assert.Equal(IdentityDocumentType.NationalId, patient.DocumentType);
        Assert.Equal(documentNumber, patient.DocumentNumber);
    }

    [Fact]
    public async Task RegisterPatient_WithDocumentAlreadyRegistered_ReturnsConflictProblem()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        var request = ValidRequest(UniqueDocumentNumber());
        using var first = await client.PostAsJsonAsync(PatientsUri, request, JsonOptions, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        using var second = await client.PostAsJsonAsync(PatientsUri, request with { GivenName = "Ana" }, JsonOptions, cancellationToken);

        await AssertProblemAsync(second, HttpStatusCode.Conflict, "Patients.DocumentAlreadyRegistered", cancellationToken);
    }

    [Fact]
    public async Task RegisterPatient_WithDateOfBirthInFuture_ReturnsValidationProblem()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        var request = ValidRequest(UniqueDocumentNumber()) with { DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddDays(2)) };

        using var response = await client.PostAsJsonAsync(PatientsUri, request, JsonOptions, cancellationToken);

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Patients.DateOfBirthInFuture", cancellationToken);
    }

    [Fact]
    public async Task RegisterPatient_WithMissingGivenName_ReturnsValidationProblem()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        var incompleteBody = new
        {
            familyName = "García López",
            dateOfBirth = "1985-03-14",
            sex = "Female",
            documentType = "NationalId",
            documentNumber = UniqueDocumentNumber(),
        };

        using var response = await client.PostAsJsonAsync(PatientsUri, incompleteBody, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFoundProblem()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri($"/api/patients/{Guid.NewGuid()}", UriKind.Relative), cancellationToken);

        await AssertProblemAsync(response, HttpStatusCode.NotFound, "Patients.NotFound", cancellationToken);
    }

    private static RegisterPatientRequest ValidRequest(string documentNumber) => new(
        GivenName: "María",
        FamilyName: "García López",
        DateOfBirth: new DateOnly(1985, 3, 14),
        Sex: AdministrativeSex.Female,
        DocumentType: IdentityDocumentType.NationalId,
        DocumentNumber: documentNumber);

    private static string UniqueDocumentNumber() => Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();

    private static async Task AssertProblemAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedCode,
        CancellationToken cancellationToken)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal(expectedCode, problem.RootElement.GetProperty("code").GetString());
        Assert.Equal((int)expectedStatus, problem.RootElement.GetProperty("status").GetInt32());
    }
}
