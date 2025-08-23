
using System.Management.Automation;

namespace PSRest.Commands;

[Cmdlet("Resolve", "RestVariable")]
[OutputType(typeof(string))]
public sealed class ResolveVariableCommand : BaseEnvironmentCmdlet
{
    [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
    [AllowNull]
    public string?[]? Value { get; set; }

    protected override void ProcessRecord()
    {
        if (Value is { })
        {
            foreach (var it in Value)
            {
                if (string.IsNullOrEmpty(it))
                    WriteObject(it);
                else
                    WriteObject(Environment.ExpandVariables(it));
            }
        }
    }
}
