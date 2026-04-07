using System.Runtime.CompilerServices;

namespace Contoso.TestHost;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Force load test assemblies so TUnit discovers tests in referenced projects
        _ = typeof(Contoso.Tests.StringUtilityTests);
    }
}
