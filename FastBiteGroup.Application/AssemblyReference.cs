using System.Runtime.CompilerServices;
using System.Reflection;

[assembly: InternalsVisibleTo("FastBiteGroup.Application.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]


namespace FastBiteGroup.Application;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
