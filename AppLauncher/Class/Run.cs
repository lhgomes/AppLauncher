using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AppLauncher.Class
{
    class Run
    {
        private Logging logToFile = null;
        private Mutex instanceMutex;
        private static readonly string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static readonly string localFolder = Path.Combine(appDataFolder, Properties.Settings.Default.AppName);

        public Run()
        {
            if (Properties.Settings.Default.LogDebug)
                logToFile = new Logging();
        }

        public void Execute()
        {
            if (!Properties.Settings.Default.MultiInstance && !AcquireInstanceMutex())
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

        private bool AcquireInstanceMutex()
        {
            string mutexName = BuildMutexName();
            bool createdNew;
            instanceMutex = new Mutex(true, mutexName, out createdNew);
            addNewLog("Instance mutex: " + mutexName + "; acquired: " + createdNew);

            if (!createdNew)
            {
                instanceMutex.Dispose();
                instanceMutex = null;
            }

            return createdNew;
        }

        private string BuildMutexName()
        {
            string appName = SanitizeFileNamePart(Properties.Settings.Default.AppName);
            int sessionId = Process.GetCurrentProcess().SessionId;
            return "Local\\AppLauncher_" + appName + "_Session_" + sessionId;
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

            if (Properties.Settings.Default.PersonalizeExecutableName)
            {
                try
                {
                    exeFileName = PreparePersonalizedExecutable(folderName, exeFileName);
                }
                catch (Exception ex)
                {
                    ShowError("The personalized application executable could not be prepared.\r\n\r\n" + ex.Message);
                    return false;
                }
            }

            addNewLog("Application EXE name: " + exeFileName);

            if (!File.Exists(exeFileName))
            {
                ShowError("Application executable doesn't exist: " + exeFileName);
                return false;
            }

            string appArguments = Properties.Settings.Default.AppArguments;
            addNewLog("Application EXE arguments: " + appArguments);

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exeFileName,
                    Arguments = appArguments,
                    WorkingDirectory = folderName,
                    UseShellExecute = true
                };

                Process process = Process.Start(startInfo);
                if (process == null)
                {
                    ShowError("Windows did not start the application process.");
                    return false;
                }

                addNewLog("Application started successfully. PID: " + process.Id);
                process.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                addNewLog("Application launch failed: " + ex);
                ShowError("The application could not be started.\r\n\r\n" + ex.Message);
                return false;
            }
        }

        private string PreparePersonalizedExecutable(string folderName, string originalExe)
        {
            if (!File.Exists(originalExe))
                throw new FileNotFoundException("Application executable doesn't exist.", originalExe);

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(originalExe);
            string version = string.IsNullOrWhiteSpace(versionInfo.FileVersion) ? "unknown" : versionInfo.FileVersion;
            string baseName = Path.GetFileNameWithoutExtension(originalExe);
            string personalizedName = string.Format("{0}_{1}_{2}.exe",
                SanitizeFileNamePart(baseName),
                SanitizeFileNamePart(version.Replace('.', '_')),
                SanitizeFileNamePart(Environment.UserName));

            string personalizedExe = Path.Combine(folderName, personalizedName);
            File.Copy(originalExe, personalizedExe, true);

            string originalConfig = originalExe + ".config";
            string personalizedConfig = personalizedExe + ".config";
            if (File.Exists(originalConfig))
                File.Copy(originalConfig, personalizedConfig, true);

            addNewLog("Personalized executable: " + personalizedExe);
            return personalizedExe;
        }

        private static string SanitizeFileNamePart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            char[] invalidChars = Path.GetInvalidFileNameChars();
            StringBuilder result = new StringBuilder(value.Length);
            foreach (char c in value)
                result.Append(invalidChars.Contains(c) || char.IsWhiteSpace(c) ? '_' : c);

            return result.ToString();
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
