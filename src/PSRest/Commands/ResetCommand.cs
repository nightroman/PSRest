using System.Management.Automation;

namespace PSRest.Commands;

[Cmdlet("Reset", "Rest")]
public sealed class ResetCommand : AnyCmdlet
{
    protected override void MyBeginProcessing()
    {
        RestEnvironment.Reset();
    }
}
