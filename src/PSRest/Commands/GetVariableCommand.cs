using System.Management.Automation;

namespace PSRest.Commands;

[Cmdlet("Get", "RestVariable")]
[OutputType(typeof(string))]
public sealed class GetVariableCommand : BaseEnvironmentCmdlet
{
    [Parameter(Position = 0, Mandatory = true)]
    public string Name { get; set; } = null!;

    [Parameter]
    public VariableType Type { get; set; }

    protected override void BeginProcessing()
    {
        WriteObject(GetCurrentEnvironment().GetVariable(Name, Type));
    }
}
