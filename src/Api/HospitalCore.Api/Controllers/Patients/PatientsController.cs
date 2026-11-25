using HospitalCore.Api.Errors;
using HospitalCore.Modules.Patients.Application.GetPatientById;
using HospitalCore.Modules.Patients.Application.RegisterPatient;
using HospitalCore.SharedKernel.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace HospitalCore.Api.Controllers.Patients;

[ApiController]
[Route("api/patients")]
public sealed class PatientsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<RegisterPatientResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> Register(
        RegisterPatientRequest request,
        [FromServices] ICommandHandler<RegisterPatientCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(handler);

        var command = new RegisterPatientCommand(
            request.GivenName,
            request.FamilyName,
            request.DateOfBirth,
            request.Sex,
            request.DocumentType,
            request.DocumentNumber);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new RegisterPatientResponse(result.Value))
            : this.ToProblem(result.Error);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PatientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] IQueryHandler<GetPatientByIdQuery, PatientResponse> handler,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var result = await handler.Handle(new GetPatientByIdQuery(id), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error);
    }
}
