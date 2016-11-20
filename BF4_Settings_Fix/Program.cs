using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using SimpleLogger;

namespace BF4_Settings_Fix
{
    class Program
    {
        static void Main(string[] args)
        {
            var temp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Temp\\BF4SettingsFix";

            SimpleLog.SetLogFile(logDir: temp, prefix: "MyLog_", writeText: false);

            SimpleLog.Info("Starting BF4_Settings_Fix at: " + DateTime.Now.ToString());

            Thread.CurrentThread.Priority = ThreadPriority.Lowest;

            //Modify files once on inital load
            OpenAndModifyFile();

            //Loop to always run to check for BF4
            while (true)
            {
                ModificationCheck();
            }

        }

        /// <summary>
        /// Determins if BF4 is running and if so then waits for it to exit to overwrite settings
        /// </summary>
        public static void ModificationCheck()
        {
            bool _needToFixSettings = false;

            var _currentProcess = Process.GetCurrentProcess();

            if (!ProcessRunning())
            {
                _currentProcess.PriorityClass = ProcessPriorityClass.Idle;    
                Thread.Sleep(60000);
            }
            else
            {
                _needToFixSettings = true;

                while (_needToFixSettings)
                {
                    if (ProcessRunning())
                    {
                        //if true then do nothing
                        //Not sure how BF saves settings
                        //(game exists, "apply", etc so
                        //wait for game to exit
                        SimpleLog.Info("BF4 Running at: " + DateTime.Now.ToString());
                        Thread.Sleep(60000);
                    }
                    else
                    {
                        //now we can modify settings
                        //read / edit file

                        SimpleLog.Info("BF4 Stopped Running at: " + DateTime.Now.ToString());

                        _currentProcess.PriorityClass = ProcessPriorityClass.Normal;

                        OpenAndModifyFile();

                        _needToFixSettings = false;

                        _currentProcess.PriorityClass = ProcessPriorityClass.Idle;
                    }
                }
            }
        }


        public static bool ProcessRunning()
        {
            //Check if process is running
            //In this case BF4
            Process[] pname = Process.GetProcessesByName("bf4");
            if (pname.Length == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Over writes BF4 settings profile
        /// </summary>
        public static void OpenAndModifyFile()
        {
            SimpleLog.Info("Modifying Settings " + DateTime.Now.ToString());

            try
            {
                

                //var userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var bf4Settings = userFolderPath + "\\Battlefield 4\\settings\\PROFSAVE_profile";
                var bf4SettingsNew = userFolderPath + "\\Battlefield 4\\settings\\PROFSAVE_profile_New";

                if (File.Exists(bf4Settings))
                {
                    using (var reader = new StreamReader(bf4Settings))
                    {
                        using (var output = new StreamWriter(bf4SettingsNew))
                        {
                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();

                                if (line != null)
                                {
                                    //check if line contains our screen setting
                                    if (line.Equals("GstRender.FullscreenMode 2") || line.Equals("GstRender.FullscreenMode 1"))
                                    {
                                        output.WriteLine("GstRender.FullscreenMode 0");
                                    }
                                    else
                                    {
                                        output.WriteLine(line);
                                    }
                                }
                            }
                        }
                    }
                }

                if (File.Exists(bf4SettingsNew))
                {
                    //Try to replace original file
                    File.Delete(bf4Settings);
                    File.Move(bf4SettingsNew, bf4Settings);
                }
            }
            catch(Exception ex)
            {
                SimpleLog.Error("Error occured in OpenAndModifyFile: \n\n" + ex.ToString());
            }
        }

    }
}
