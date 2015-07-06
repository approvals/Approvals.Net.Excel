using System.IO;
using ApprovalTests.Reporters;
using ApprovalUtilities.SimpleLogger;
using ApprovalUtilities.SimpleLogger.Writers;
using ApprovalUtilities.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApprovalTests.Excel.Tests

{
    [TestClass]
    [UseReporter(typeof (ExcelLauncherReporter))]
    public class ExcelTest
    {
        [TestMethod]
        public void TestFilesMatch()
        {
            Logger.Writer = new ConsoleWriter();
            ExcelApprovals.VerifyXlsxFile(PathUtilities.GetAdjacentFile("sample.xlsx"), deleteOnSuccess: false);
        }

        [TestMethod]
        public void TestBytes()
        {
            Logger.Writer = new ConsoleWriter();
            byte[] bytes = File.ReadAllBytes(PathUtilities.GetAdjacentFile("sample.xlsx"));
            ExcelApprovals.VerifyXlsx(bytes);
        }
    }
}