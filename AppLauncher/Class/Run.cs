using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AppLauncher.Class
{
    class Run
    {
        private Logging logToFile = null;
        private static readonly string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static readonly string localFolder = Path.Combine(appDataFolder, Properties.Settings.Default.AppName);

        public Run()
        {
            if (Properties.Settings.Default.LogDebug)
                logToFile = new Logging();
        }

        public void Execute()
        {
            if (!Properties.Settings.Default.MultiInstance && IsRunningInCurrentSession())
            {
                System.Windows.Forms.MessageBox.Show(Properties.Settings.Default.MultiInstanceMessage,
                    Properties.Settings.Default.AppName,
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Exclamation);
                return;
            }

            string sourceRoot = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName;
            string versionFolder = ResolveVersionFolder(sourceRoot);
            if (string.IsNullOrEmpty(versionFolder))
                return;

            string sourceFolder = Path.Combine(sourceRoot, versionFolder);
            string userFolder = GetLocalFolder();

            if (string.IsNullOrEmpty(userFolder))
            {
                RunApp(sourceFolder);
                return;
            }

            string currentVersionFolder = Path.Combine(userFolder, versionFolder);
            addNewLog("Current Version Folder: " + currentVersionFolder);

            if (!Directory.Exists(currentVersionFolder))
            {
                if (!InstallVersion(sourceFolder, currentVersionFolder))
                    return;
            }

            RemoveOldVersions(currentVersionFolder);
            RunApp(currentVersionFolder);
        }

        private bool IsRunningInCurrentSession()
        {
            string processName = Path.GetFileNameWithoutExtension(Properties.Settings.Default.ExecFile);
            int currentSessionId = Process.GetCurrentProcess().SessionId;
            addNewLog("Searching for process in session " + currentSessionId + ": " + processName);

            int runningProcess = Process.GetProcessesByName(processName)
                .Count(p => GetSessionIdSafely(p) == currentSessionId);

            addNewLog("Running process in current session: " + runningProcess);
            return runningProcess > 0;
        }

        private static int GetSessionIdSafely(Process process)
        {
            try
            {
                return process.SessionId;
            }
            catch
            {
                return -1;
            }
            finally
            {
                process.Dispose();
            }
        }

        private string ResolveVersionFolder(string sourceRoot)
        {
            string configuredVersion = Properties.Settings.Default.VersionFolder;
            if (!string.IsNullOrWhiteSpace(configuredVersion))
            {
                string configuredPath = Path.Combine(sourceRoot, configuredVersion);
                if (Directory.Exists(configuredPath))
                    return configuredVersion;

                ShowError("Configured source version folder doesn't exist: " + configuredVersion);
                return null;
            }

            var latest = new DirectoryInfo(sourceRoot)
                .EnumerateDirectories()
                .Where(d => !d.Name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (latest == null)
            {
                ShowError("No application release folder was found.");
                return null;
            }

            addNewLog("Automatically selected version folder: " + latest.Name);
            return latest.Name;
        }

        private bool InstallVersion(string sourceFolder, string destinationFolder)
        {
            if (!Directory.Exists(sourceFolder))
            {
                ShowError("Source folder doesn't exist: " + sourceFolder);
                return false;
            }

            string temporaryFolder = destinationFolder + ".tmp";
            try
            {
                if (Directory.Exists(temporaryFolder))
                    Directory.Delete(temporaryFolder, true);

                Directory.CreateDirectory(temporaryFolder);
                addNewLog("Copying release to temporary folder: " + temporaryFolder);

                using (var updateForm = new Update())
                {
                    updateForm.Show();
                    updateForm.UpdateVersion(sourceFolder, temporaryFolder);
                }

                string executable = Path.Combine(temporaryFolder, Properties.Settings.Default.ExecFile);
                if (!File.Exists(executable))
                    throw new FileNotFoundException("Application executable was not found after copying the release.", executable);

                Directory.Move(temporaryFolder, destinationFolder);
                addNewLog("Release installation completed: " + destinationFolder);
                return true;
            }
            catch (Exception ex)
            {
                addNewLog("Release installation failed: " + ex);
                try
                {
                    if (Directory.Exists(temporaryFolder))
                        Directory.Delete(temporaryFolder, true);
                }
                catch (Exception cleanupEx)
                {
                    addNewLog("Temporary folder cleanup failed: " + cleanupEx.Message);
                }

                ShowError("The application update could not be installed.\r\n\r\n" + ex.Message);
                return false;
            }
        }

        private void RemoveOldVersions(string currentVersionFolder)
        {
            try
            {
                var directories = new DirectoryInfo(localFolder)
                    .EnumerateDirectories()
                    .Where(d => !d.Name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
                    .Where(d => !string.Equals(d.FullName, currentVersionFolder, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(d => d.CreationTimeUtc)
                    .ToList();

                int keepOldVersions = Math.Max(0, Properties.Settings.Default.KeepLastVersions - 1);
                foreach (DirectoryInfo directory in directories.Skip(keepOldVersions))
                {
                    try
                    {
                        addNewLog("Removing old version folder: " + directory.FullName);
                        directory.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        // Cleanup must never prevent the current application from starting.
                        addNewLog("Unable to remove old version " + directory.FullName + ": " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                addNewLog("Old version cleanup failed: " + ex.Message);
            }
        }

        private bool RunApp(string folderName)
        {
            string exeFileName = Path.Combine(folderName, Properties.Settings.Default.ExecFile);
            addNewLog("Application EXE name: " + exeFileName);

            if (!File.Exists(exeFileName))
            {
                ShowError("Application executable doesn't exist: " + exeFileName);
                return false;
            }

            string appArguments = Properties.Settings.Default.AppArguments;
            addNewLog("Application EXE arguments: " + appArguments);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exeFileName,
                Arguments = appArguments,
                WorkingDirectory = folderName,
                UseShellExecute = true
            };
            Process.Start(startInfo);
            return true;
        }

        private string GetLocalFolder()
        {
            addNewLog("Environment Local Application Data Folder: " + appDataFolder);
            addNewLog("Local Application Folder: " + localFolder);

            try
            {
                Directory.CreateDirectory(localFolder);
                return localFolder;
            }
            catch (Exception ex)
            {
                addNewLog("Unable to create local application folder: " + ex.Message);
                return null;
            }
        }

        private void ShowError(string message)
        {
            addNewLog(message);
            System.Windows.Forms.MessageBox.Show(message,
                Properties.Settings.Default.AppName,
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);
        }

        private void addNewLog(string message)
        {
            if (logToFile != null)
                logToFile.WriteToLog(message);
        }
    }
}
