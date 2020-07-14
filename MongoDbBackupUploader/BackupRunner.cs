using System;
using System.Diagnostics;
using System.IO;

namespace MongoDbBackupUploader
{
    public class BackupRunner
    {
        private readonly string backupDirectory;
        private readonly string username;
        private readonly string password;

        public BackupRunner(string backupDirectory, string username, string password)
        {
            this.backupDirectory = backupDirectory;
            this.username = username;
            this.password = password;
        }

        public string Run()
        {
            var utcNow = DateTime.UtcNow;

            var backupFilePath = Path.Combine(backupDirectory, $"{NamingConventions.BackupFilePrefix}{utcNow:yyyy-MM-dd_HHmm}.gz");
            RunBackup(backupFilePath);

            return backupFilePath;
        }

        private void RunBackup(string backupFilePath)
        {
            var startupInfo = new ProcessStartInfo
            {
                FileName = @"C:\mongodb\bin\mongodump.exe",
                Arguments = $"/gzip /archive:\"{backupFilePath}\" /u {username} /p {password}",
                UseShellExecute = true
            };
            var process = Process.Start(startupInfo);

            process.WaitForExit();
        }
    }
}
