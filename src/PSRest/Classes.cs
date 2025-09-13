namespace PSRest;

public class VariableArgs
{
    public string? HttpFile { get; set; }
    public Dictionary<string, string>? Vars { get; set; }
    internal Action<string>? InvokeNamedRequest { get; set; }
}
