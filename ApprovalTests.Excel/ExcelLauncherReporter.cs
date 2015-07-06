using System.IO;
using ApprovalTests.Core;
using ApprovalTests.Reporters;
using ApprovalUtilities.Utilities;

namespace ApprovalTests.Excel
{
    public class ExcelLauncherReporter : IEnvironmentAwareReporter
    {
        public void Report(string approved, string received)
        {
            GenericDiffReporter.LaunchAsync(new LaunchArgs("excel", "/r \"{0}\"".FormatWith(approved)));
        }

        public bool IsWorkingInThisEnvironment(string forFile)
        {
            return File.Exists(GenericDiffReporter.GetActualProgramFile("excel")) && GenericDiffReporter.IsFileOneOf(forFile, new[]{".xlsx",".xls"});
        }
    }
}