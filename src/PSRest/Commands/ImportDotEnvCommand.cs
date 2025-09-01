using DotNetEnv;
using DotNetEnv.Extensions;
using System.Management.Automation;

namespace PSRest.Commands;

[Cmdlet("Import", "RestDotEnv", DefaultParameterSetName = PsnMain)]
[OutputType(typeof(Dictionary<string, string>))]
[OutputType(typeof(KeyValuePair<string, string>))]
public sealed class ImportDotEnvCommand : PSCmdlet
{
    const string PsnMain = "Main";
    const string PsnAsDictionary = "AsDictionary";
    const string PsnAsKeyValue = "AsKeyValue";

    [Parameter(Position = 0)]
    public string? Path { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = PsnAsDictionary)]
    public SwitchParameter AsDictionary { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = PsnAsKeyValue)]
    public SwitchParameter AsKeyValue { get; set; }

    //! `Env.Load` does not fail on missing files, so we test `Path` if set.
    protected override void BeginProcessing()
    {
        // options
        bool opSetEnvVars = !AsDictionary && !AsKeyValue;
        bool opOnlyExactPath = !string.IsNullOrEmpty(Path);

        // ensure not null file to avoid DotNetEnv using `Directory.GetCurrentDirectory()`
        var file = GetUnresolvedProviderPathFromPSPath(opOnlyExactPath ? Path : ".env");

        // deny missing file
        if (opOnlyExactPath && !File.Exists(file))
            throw new ArgumentException($"Missing file: '{file}'.");

        // read
        var options = new LoadOptions(opSetEnvVars, true, opOnlyExactPath);
        var env = options.Load(file);

        //: write dictionary
        if (AsDictionary)
        {
            var dic = env.ToDotEnvDictionary(CreateDictionaryOption.TakeLast);
            WriteObject(dic);
            return;
        }

        //: write pairs
        if (AsKeyValue)
        {
            WriteObject(env, true);
            return;
        }
    }
}
