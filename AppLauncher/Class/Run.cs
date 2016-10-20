using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;

namespace AppLauncher.Class
{
    class Run
    {
        private Logging logToFile = null;
        static string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static string localFolder = string.Format(@"{0}\{1}", appDataFolder, Properties.Settings.Default.AppName);

        public Run()
        {
            if (Properties.Settings.Default.LogDebug)
                logToFile = new Logging();
        }

        public void Execute()
        {
            if (!Properties.Settings.Default.MultiInstance)
            {
                var processName = Properties.Settings.Default.ExecFile.Split('.')[0];
                addNewLog(string.Concat("Searching for process: ", processName));

                var runningProcess = Process.GetProcessesByName(processName).Count();
                addNewLog(string.Concat("Running process: ", runningProcess));
                if (runningProcess > 0)
                {
                    System.Windows.Forms.MessageBox.Show(Properties.Settings.Default.MultiInstanceMessage, 
                        Properties.Settings.Default.AppName, 
                        System.Windows.Forms.MessageBoxButtons.OK, 
                        System.Windows.Forms.MessageBoxIcon.Exclamation);
                    return;
                }
            }

            string localFolder = GetLocalFolder();
            if (string.IsNullOrEmpty(localFolder))
            {
                var sourceFolder = string.Format(@"{0}\{1}",
                    new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName,
                    Properties.Settings.Default.VersionFolder);

                RunApp(sourceFolder);
            }
            else
            {
                string currentVersionFolder = string.Format(@"{0}\{1}", localFolder,
                    Properties.Settings.Default.VersionFolder);

                addNewLog(string.Concat("Current Version Folder: ", currentVersionFolder));

                if (!Directory.Exists(currentVersionFolder))
                {
                    addNewLog("Trying to create the current version folder.");
                    Directory.CreateDirectory(currentVersionFolder);

                    var sourceFolder = string.Format(@"{0}\{1}",
                        new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName,
                        Properties.Settings.Default.VersionFolder);
                    addNewLog(string.Concat("Source Folder: ", sourceFolder));

                    if (Directory.Exists(sourceFolder))
                    {
                        addNewLog("Copying files.");

                        using (var updateForm = new Update())
                        {
                            updateForm.Show();
                            updateForm.UpdateVersion(sourceFolder, currentVersionFolder);
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Source folder doesn't exist.",
                            Properties.Settings.Default.AppName, 
                            System.Windows.Forms.MessageBoxButtons.OK, 
                            System.Windows.Forms.MessageBoxIcon.Error);

                        addNewLog("Source folder doesn't exist.");
                        return;
                    }
                }
                else
                {
                    addNewLog("Current version folder already exists.");
                }

                RemoveOldVersions();
                RunApp(currentVersionFolder);
            }
        }

        private void RemoveOldVersions()
        {
            var di = new DirectoryInfo(localFolder);
            var directories = di.EnumerateDirectories()
                                .OrderBy(d => d.CreationTime)
                                .Select(d => d.Name)
                                .ToList();

            addNewLog(string.Concat("Old Versions founded: ", directories.Count));
            var removeDirectories = directories.Count - Properties.Settings.Default.KeepLastVersions;

            for (int i = 0; i < removeDirectories; i++)
            {
                addNewLog(string.Concat("Removing old Version Folder: ", directories[i]));
                Directory.Delete(directories[i]);
            }
        }

        private bool RunApp(string FolderName)
        {
            string exeFileName = string.Format(@"{0}\{1}", FolderName,
                Properties.Settings.Default.ExecFile);
            addNewLog(string.Concat("Application EXE name: ", exeFileName));

            if (File.Exists(exeFileName))
            {
                string appArguments = Properties.Settings.Default.AppArguments;
                addNewLog(string.Concat("Application EXE arguments: ", appArguments));

                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = exeFileName,
                    Arguments = appArguments
                };
                Process.Start(startInfo);
                return true;
            }
            else
            {
                addNewLog("Application EXE dosn't exists.");
                return false;
            }
        }

        private string GetLocalFolder()
        {
            addNewLog(string.Concat("Environment Application Data Folder: ", appDataFolder));
            addNewLog(string.Concat("Local Application Folder: ", localFolder));

            if (!Directory.Exists(localFolder))
            {
                addNewLog("Local Application Folder not exists.");
                var permissionSet = new PermissionSet(PermissionState.None);
                var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, appDataFolder);
                permissionSet.AddPermission(writePermission);

                if (permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
                {
                    addNewLog("Trying to create the Local Application Folder.");
                    Directory.CreateDirectory(localFolder);
                }
                else
                {
                    addNewLog("User don't have access to create the Local Application Folder.");
                    return null;
                }
            }
            else
            {
                addNewLog("Local Application Folder already exists.");
            }

            return localFolder;
        }

        private void addNewLog(string Message)
        {
            if (logToFile != null)
                logToFile.WriteToLog(Message);

        }
    }
}
