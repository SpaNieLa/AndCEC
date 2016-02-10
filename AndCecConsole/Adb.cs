using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AndCecConsole
{
    class Adb
    {
        private List<string> events;
        private static System.Diagnostics.Process proc;
        System.Diagnostics.ProcessStartInfo procStartInfo;

        // Constructor and initializer for new adb connection
        public Adb(string address)
        {
            this.AdbShell("connect " + address);
            this.events = new List<string>();
            this.events.Add("Init");

        }
        // returns last received event (not needed atm)
        public string GetEvent()
        {
            if (this.events.Count == 0) return "empty";

            return this.events[this.events.Count -1];
        }
        // Returns current list of unhandled events
        public List<string> GetEvents()
        {
            return this.events;
        }
        // Cleaning prosedures
        public void Dispose()
        {
            this.AdbShell("disconnect");
            this.AdbShell("kill-server");
        }
        // Remove seen events from queue
        public void Remove(int seen)
        {
            this.events.RemoveRange(0, seen);
        }

        // Initializing Adb connection and process info
        private void AdbShell(string adbInput)
        {
            try
            {
                string result = string.Empty;
                string error = string.Empty;
                string output = string.Empty;
                procStartInfo = new System.Diagnostics.ProcessStartInfo(@"C:\androidSDK\platform-tools\adb.exe");


                procStartInfo.Arguments = adbInput;
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();                
                proc.WaitForExit();
                            }
            catch (Exception objException)
            {
                throw objException;
            }
        }

        // For reading events with extra thread started in main Program
        public void AdbReadEvents()
        {

            try
            {
                string result = string.Empty;
                //string error = string.Empty;
                //string output = string.Empty;

                procStartInfo.Arguments = "shell getevent -l";
                
                proc.StartInfo = procStartInfo;
                proc.Start();

                while (true)
                {
                    result = proc.StandardOutput.ReadLine();
                    if(!result.Equals("")) events.Add(result);
                    
                    
                    //Program.sr = proc.StandardOutput;
                    //Console.WriteLine(result);
                }
             /*
                error = proc.StandardError.ReadLine();  //Some ADB outputs use this
                if (result.Length > 1)
                {
                    output += result;
                }
                if (error.Length > 1)
                {
                    output += error;
                }
             */
            }
            catch (Exception objException)
            {
                throw objException;
            }
        }
    }

}
