
using System.Management.Automation;

namespace PSRest.Commands;

public abstract class BaseEnvironmentCmdlet : PSCmdlet
{
    RestEnvironment? _Environment;

    [Parameter]
    [ValidateNotNull]
    public RestEnvironment Environment
    {
        set { _Environment = value; }
        get => _Environment ??= (RestEnvironment)GetVariableValue(Const.VarRestEnvironment) ?? new(SessionState.Path.CurrentFileSystemLocation.ProviderPath, null);
    }
}
