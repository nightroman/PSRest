
using System.Management.Automation;

namespace PSRest.Commands;

[Cmdlet("Set", "RestEnvironment")]
public sealed class SetEnvironmentCommand : PSCmdlet
{
    [Parameter(Position = 0)]
    public string? Name { get; set; } = null!;

    [Parameter]
    public string? Path { get; set; }

    protected override void BeginProcessing()
    {
        var dir = Path is { } ? GetUnresolvedProviderPathFromPSPath(Path) : SessionState.Path.CurrentFileSystemLocation.ProviderPath;
        var env = new RestEnvironment(dir, Name);
        SessionState.PSVariable.Set(new(Const.VarRestEnvironment, env));
    }
}
