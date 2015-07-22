namespace Dashing.CodeGeneration.Weaving {
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public class PEVerifier {
        private readonly TaskLoggingHelper log;

        private string windowsSdkDirectory;

        private bool foundPeVerify;

        private readonly string peVerifyPath;

        public PEVerifier(TaskLoggingHelper log) {
            this.log = log;
            var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            this.windowsSdkDirectory = Path.Combine(programFilesPath, @"Microsoft SDKs\Windows");
            if (!Directory.Exists(this.windowsSdkDirectory)) {
                this.foundPeVerify = false;
                this.log.LogMessage(MessageImportance.High, "Unable to find Peverify.exe");
                return;
            }

            this.peVerifyPath =
                Directory.EnumerateFiles(this.windowsSdkDirectory, "peverify.exe", SearchOption.AllDirectories)
                         .Where(x => !x.ToLowerInvariant().Contains("x64"))
                         .OrderByDescending(x => FileVersionInfo.GetVersionInfo(x).FileVersion)
                         .FirstOrDefault();

            if (this.peVerifyPath == null) {
                this.foundPeVerify = false;
                this.log.LogMessage(MessageImportance.High, "Unable to find Peverify.exe");
                return;
            }

            this.log.LogMessage(MessageImportance.Normal, "Found Peverify.exe!");
            this.foundPeVerify = true;
        }

        public bool Verify(string assemblyPath) {
            var processStartInfo = new ProcessStartInfo(this.peVerifyPath) {
                                                                               Arguments =
                                                                                   string.Format("\"{0}\" /hresult /ignore=0x80070002", assemblyPath),
                                                                               WorkingDirectory = Path.GetDirectoryName(assemblyPath),
                                                                               CreateNoWindow = true,
                                                                               UseShellExecute = false,
                                                                               RedirectStandardOutput = true
                                                                           };

            using (var process = Process.Start(processStartInfo)) {
                var output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                this.log.LogMessage(MessageImportance.Normal, output);
                if (process.ExitCode != 0) {
                    return false;
                }
            }
            return true;
        }
    }
}