using ApprovalTests.Core;
using ApprovalTests.Writers;

namespace ApprovalTests.Excel
{
    public class ExcelApprovals
    {
        public static void VerifyXlsxFile(string xlsxFile, bool deleteOnSuccess = true)
        {
            var zipApprover = new ZipApprover(new ExistingFileWriter(xlsxFile), Approvals.GetDefaultNamer(), deleteOnSuccess);
            Approver.Verify(zipApprover, Approvals.GetReporter());
        }

        public static void VerifyXlsx(byte[] xlsxBytes)
        {
            var zipApprover = new ZipApprover(new ApprovalBinaryWriter(xlsxBytes, "xlsx"), Approvals.GetDefaultNamer(), true);
            Approver.Verify(zipApprover, Approvals.GetReporter());
        }
    }
}