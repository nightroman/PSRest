using System.Management.Automation;

namespace PSRest.Commands;

public class AnyCmdlet : PSCmdlet
{
    protected ErrorRecord CreateErrorRecord(Exception ex)
    {
        return new(ex, MyInvocation.MyCommand.Name, ErrorCategory.InvalidOperation, null);
    }

    protected virtual void MyBeginProcessing() { }
    protected sealed override void BeginProcessing()
    {
        try
        {
            MyBeginProcessing();
        }
        catch (Exception ex)
        {
            ThrowTerminatingError(CreateErrorRecord(ex));
        }
    }

    protected virtual void MyEndProcessing() { }
    protected sealed override void EndProcessing()
    {
        try
        {
            MyEndProcessing();
        }
        catch (Exception ex)
        {
            ThrowTerminatingError(CreateErrorRecord(ex));
        }
    }

    protected virtual void MyProcessRecord() { }
    protected sealed override void ProcessRecord()
    {
        try
        {
            MyProcessRecord();
        }
        catch (Exception ex)
        {
            ThrowTerminatingError(CreateErrorRecord(ex));
        }
    }
}
