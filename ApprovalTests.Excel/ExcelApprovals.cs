using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApprovalTests.Core;
using ApprovalTests.Core.Exceptions;
using ApprovalTests.Writers;
using Ionic.Zip;

namespace ApprovalTests.Excel
{
    public class ExcelApprovals
    {
        public static void VerifyXlsxFile(string xlsxFile, bool deleteOnSuccess = true)
        {
            Approver.Verify(
                new ZipApprover(new ExistingFileWriter(xlsxFile), Approvals.GetDefaultNamer(), deleteOnSuccess),
                Approvals.GetReporter());
        }
    }

    public class ZipApprover : IApprovalApprover
    {
        private readonly IApprovalNamer namer;
        private readonly bool deleteOnSuccess;
        private readonly IApprovalWriter writer;
        private string approved;
        private ApprovalException failure;
        private string received;

        public ZipApprover(IApprovalWriter writer, IApprovalNamer namer, bool deleteOnSuccess)
        {
            this.writer = writer;
            this.namer = namer;
            this.deleteOnSuccess = deleteOnSuccess;
        }

        public virtual bool Approve()
        {
            string basename = Path.Combine(this.namer.SourcePath, this.namer.Name);
            this.approved = Path.GetFullPath(this.writer.GetApprovalFilename(basename));
            this.received = Path.GetFullPath(this.writer.GetReceivedFilename(basename));
            this.received = this.writer.WriteReceivedFile(this.received);

            this.failure = this.Approve(this.approved, this.received);
            return this.failure == null;
        }

        public virtual ApprovalException Approve(string approvedPath, string receivedPath)
        {
            if (!File.Exists(approvedPath))
            {
                return new ApprovalMissingException(receivedPath, approvedPath);
            }

            return !Compare(receivedPath,approvedPath)
                ? new ApprovalMismatchException(receivedPath, approvedPath)
                : null;
        }

        private bool Compare(string receivedPath, string approvedPath)
        {


            {
                var receivedZip = ZipFile.Read(receivedPath);

   
                var approvedZip = ZipFile.Read(approvedPath);
                if (approvedZip.Count != receivedZip.Count)
                {
                    return false;
                }

                foreach (var approvedEntry in approvedZip.EntriesSorted)
                {
                    var receivedEntry = receivedZip[approvedEntry.FileName];
                    if (!AreEqual(approvedEntry, receivedEntry))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private bool AreEqual(ZipEntry approvedEntry, ZipEntry receivedEntry)
        {
            throw new NotImplementedException();
        }

        public void Fail()
        {
            throw this.failure;
        }

        public void ReportFailure(IApprovalFailureReporter reporter)
        {
            reporter.Report(this.approved, this.received);
        }

        public void CleanUpAfterSuccess(IApprovalFailureReporter reporter)
        {
            if (deleteOnSuccess)
            {
                File.Delete(this.received);
                var withCleanUp = reporter as IApprovalReporterWithCleanUp;
                if (withCleanUp != null)
                {
                    withCleanUp.CleanUp(this.approved, this.received);
                }
            }
        }

  
    }
}