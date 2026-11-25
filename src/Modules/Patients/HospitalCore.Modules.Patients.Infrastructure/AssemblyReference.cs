using System.Reflection;

namespace HospitalCore.Modules.Patients.Infrastructure;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
