using System.Management.Automation;

namespace PSRest.Commands;

[Cmdlet("Set", "RestEnvironment")]
public sealed class SetEnvironmentCommand : PSCmdlet
{
    [Parameter(Position = 0)]
    public string? Name { get; set; }

    [Parameter(Position = 1)]
    public string? Path { get; set; }

    [Parameter]
    public string? DotEnvFile { get; set; }

    [Parameter]
    public string? SettingsFile { get; set; }

    protected override void BeginProcessing()
    {
        var dir = Path is null ? SessionState.Path.CurrentFileSystemLocation.ProviderPath : GetUnresolvedProviderPathFromPSPath(Path);
        var env = new RestEnvironment(new(dir)
        {
            Name = Name,
            DotEnvFile = DotEnvFile is null ? null : GetUnresolvedProviderPathFromPSPath(DotEnvFile),
            SettingsFile = SettingsFile is null ? null : GetUnresolvedProviderPathFromPSPath(SettingsFile)
        });

        SessionState.PSVariable.Set(new(Const.VarRestEnvironment, env));
    }
}
