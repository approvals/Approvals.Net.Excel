using System.IO;
using ApprovalTests.Core;
using ApprovalTests.Core.Exceptions;
using ApprovalUtilities.SimpleLogger;
using Ionic.Zip;

namespace ApprovalTests.Excel
{
    public class ZipApprover : IApprovalApprover
    {
        private readonly IApprovalNamer namer;
        private readonly bool deleteOnSuccess;
        private readonly IApprovalWriter writer;
        private string approved;
        private string received;
        private ApprovalException failure;

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

            return !Compare(receivedPath, approvedPath)
                ? new ApprovalMismatchException(receivedPath, approvedPath)
                : null;
        }

        private bool Compare(string receivedPath, string approvedPath)
        {
            using (var receivedZip = ZipFile.Read(receivedPath))
            {
                using (var approvedZip = ZipFile.Read(approvedPath))
                {
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
        }

        private bool AreEqual(ZipEntry approvedEntry, ZipEntry receivedEntry)
        {
            if (approvedEntry.IsDirectory && receivedEntry.IsDirectory)
            {
                return true;
            }
            using (var approvedStream = new MemoryStream())
            {
                using (var receivedStream = new MemoryStream())
                {
                    approvedEntry.Extract(approvedStream);
                    receivedEntry.Extract(receivedStream);
                    var approvedBytes = approvedStream.ToArray();
                    var receivedBytes = receivedStream.ToArray();
                    var areEqual = approvedBytes == receivedBytes;
                    for (int i = 0; i < receivedBytes.Length && i < approvedBytes.Length; i++)
                    {
                        if (receivedBytes[i] != approvedBytes[i])
                        {
                            Logger.Event("Failed on {0}[{1}]      '{2}' != '{3}'", receivedEntry.FileName, i,
                                (char) receivedBytes[i], (char) approvedBytes[i]);
                            return false;
                        }
                    }
                    return true;
                }
            }
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
            }
            doCleanUp(reporter);
        }

        public void doCleanUp(IApprovalFailureReporter reporter)
        {
            var withCleanUp = reporter as IApprovalReporterWithCleanUp;
            if (withCleanUp != null)
            {
                withCleanUp.CleanUp(this.approved, this.received);
            }
        }
    }
}