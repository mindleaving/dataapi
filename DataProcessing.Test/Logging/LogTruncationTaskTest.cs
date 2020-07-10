using System;
using System.Threading;
using System.Threading.Tasks;
using DataProcessing.Logging;
using NUnit.Framework;

namespace DataProcessing.Test.Logging
{
    [TestFixture]
    public class LogTruncationTaskTest : ApiTestBase
    {
        [Test]
        [Category("Tool")]
        //[Ignore("Tool")]
        public async Task Run()
        {
            var task = new LogTruncationTask(DataApiClient, TimeSpan.FromDays(3));
            var executionResult = await task.Action(CancellationToken.None);
            Console.WriteLine($"Success: {executionResult.IsSuccess}");
            Console.WriteLine($"Summary: {executionResult.Summary}");
        }
    }
}
