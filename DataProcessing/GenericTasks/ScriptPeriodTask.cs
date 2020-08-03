using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commons.Extensions;

namespace DataProcessing.GenericTasks
{
    public class ScriptPeriodTask : IPeriodicTask
    {
        public const int MaximumExecuteTimeInMilliSeconds = 60 * 60 * 1000;

        public ScriptPeriodTask(ScriptPeriodicTaskDefinition definition)
        {
            Definition = definition;
        }

        public ScriptPeriodicTaskDefinition Definition { get; }
        public string DisplayName => Definition.DisplayName;
        public TimeSpan Period => Definition.Period;

        public async Task<ExecutionResult> Action(CancellationToken cancellationToken)
        {
            if(!File.Exists(Definition.ScriptPath))
                return new ExecutionResult(false, false, $"Script '{Definition.ScriptPath}' doesn't exist");
            try
            {
                var processInfo = new ProcessStartInfo(Definition.ScriptPath, Definition.Arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                var process = Process.Start(processInfo);
                var errorMessages = new List<string>();
                process.ErrorDataReceived += (sender, args) =>
                {
                    if(!string.IsNullOrEmpty(args.Data))
                        errorMessages.Add(args.Data);
                };
                process.BeginErrorReadLine();

                if(!await Task.Run(() => process.WaitForExit(MaximumExecuteTimeInMilliSeconds), cancellationToken))
                {
                    process.Kill();
                    return new ExecutionResult(false, false, $"{nameof(ScriptPeriodTask)} '{DisplayName}' didn't finish in time and was aborted");
                }

                var isSuccess = process.ExitCode == 0;
                var summary = $"{nameof(ScriptPeriodTask)} '{DisplayName}' finished with code {process.ExitCode}.";
                if (errorMessages.Any())
                {
                    summary += Environment.NewLine + "Errors:" + Environment.NewLine;
                    summary += errorMessages.Aggregate((a, b) => a + Environment.NewLine + b);
                }
                return new ExecutionResult(isSuccess, true, summary);
            }
            catch (Exception e)
            {
                return new ExecutionResult(false, false, 
                    $"{nameof(ScriptPeriodTask)} '{DisplayName}' failed: {e.InnermostException().Message}");
            }
        }
    }
}
