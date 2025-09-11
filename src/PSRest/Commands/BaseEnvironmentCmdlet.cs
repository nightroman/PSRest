using System.Management.Automation;

namespace PSRest.Commands;

public abstract class BaseEnvironmentCmdlet : AnyCmdlet
{
    RestEnvironment? _Environment;

    [Parameter]
    [ValidateNotNull]
    public RestEnvironment Environment
    {
        set { _Environment = value; }
    }

    /// <summary>
    /// Gets the current environment or fails.
    /// </summary>
    protected RestEnvironment GetCurrentEnvironment()
    {
        return _Environment ??=
            (RestEnvironment)GetVariableValue(Const.VarRestEnvironment) ??
            throw new InvalidOperationException("Invoke Set-RestEnvironment before this command.");
    }

    /// <summary>
    /// Gets the current environment or creates new with the directory.
    /// </summary>
    protected RestEnvironment GetOrCreateEnvironment(string dir)
    {
        return _Environment ??
            (RestEnvironment)GetVariableValue(Const.VarRestEnvironment) ??
            new RestEnvironment(new(dir));
    }
}
