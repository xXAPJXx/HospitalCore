using System.Reflection;

namespace HospitalCore.SharedKernel;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
