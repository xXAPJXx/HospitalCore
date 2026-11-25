using System.Reflection;
using NetArchTest.Rules;
using TestResult = NetArchTest.Rules.TestResult;

namespace HospitalCore.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private const string ApiNamespace = "HospitalCore.Api";
    private const string ModulesNamespace = "HospitalCore.Modules";
    private const string PatientsApplicationNamespace = "HospitalCore.Modules.Patients.Application";
    private const string PatientsInfrastructureNamespace = "HospitalCore.Modules.Patients.Infrastructure";
    private const string EntityFrameworkCoreNamespace = "Microsoft.EntityFrameworkCore";
    private const string AspNetCoreNamespace = "Microsoft.AspNetCore";

    private static readonly Assembly PatientsDomain = Modules.Patients.Domain.AssemblyReference.Assembly;
    private static readonly Assembly PatientsApplication = Modules.Patients.Application.AssemblyReference.Assembly;
    private static readonly Assembly PatientsInfrastructure = Modules.Patients.Infrastructure.AssemblyReference.Assembly;
    private static readonly Assembly SharedKernel = HospitalCore.SharedKernel.AssemblyReference.Assembly;

    [Fact]
    public void PatientsDomain_ShouldNotDependOnOuterLayersOrFrameworks()
    {
        var result = Types.InAssembly(PatientsDomain)
            .ShouldNot()
            .HaveDependencyOnAny(
                PatientsApplicationNamespace,
                PatientsInfrastructureNamespace,
                ApiNamespace,
                EntityFrameworkCoreNamespace,
                AspNetCoreNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void PatientsApplication_ShouldNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(PatientsApplication)
            .ShouldNot()
            .HaveDependencyOnAny(PatientsInfrastructureNamespace, ApiNamespace, EntityFrameworkCoreNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void PatientsInfrastructure_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(PatientsInfrastructure)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void SharedKernel_ShouldNotDependOnModulesOrApi()
    {
        var result = Types.InAssembly(SharedKernel)
            .ShouldNot()
            .HaveDependencyOnAny(ModulesNamespace, ApiNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    private static void AssertSuccessful(TestResult result)
    {
        var failingTypes = result.FailingTypeNames ?? [];
        Assert.True(result.IsSuccessful, $"Forbidden dependencies found in: {string.Join(", ", failingTypes)}");
    }
}
