﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AppLauncher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var run = new AppLauncher.Class.Run();
            run.Execute();
        }
    }
}
