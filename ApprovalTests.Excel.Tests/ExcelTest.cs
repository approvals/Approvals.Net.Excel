using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApprovalTests.Excel.Tests

{
    [TestClass]
    public class ExcelTest
    {
        [TestMethod]
        public void TestFilesMatch()
        {
            ExcelApprovals.VerifyXlsxFile()
        }
    }
}


