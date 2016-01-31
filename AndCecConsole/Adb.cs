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


        public Adb(string address)
        {
            this.AdbShell("connect " + address);
            this.events = new List<string>();
            this.events.Add("Init");

        }

        public string GetEvent()
        {
            if (this.events.Count == 0) return "empty";

            return this.events[this.events.Count -1];
        }
        public List<string> GetEvents()
        {
            return this.events;
        }
        public void Dispose()
        {
            this.AdbShell("disconnect");
            this.AdbShell("kill-server");
        }
        public void Remove(int seen)
        {
            this.events.RemoveRange(0, seen);
        }


        private string AdbShell(string adbInput)
        {
            try
            {
                string result = string.Empty;
                string error = string.Empty;
                string output = string.Empty;
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo(@"C:\androidSDK\platform-tools\adb.exe");


                procStartInfo.Arguments = adbInput;
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                       //procStartInfo.WorkingDirectory = toolPath;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                proc.WaitForExit();
                result = proc.StandardOutput.ReadToEnd();
                error = proc.StandardError.ReadToEnd();  //Some ADB outputs use this
                if (result.Length > 1)
                {
                    output += result;
                }
                if (error.Length > 1)
                {
                    output += error;
                }
                return output;
            }
            catch (Exception objException)
            {
                throw objException;
            }
        }
        public void AdbShellGet()
        {
            string adbInput = "shell getevent -l";
            try
            {
                string result = string.Empty;
                string error = string.Empty;
                string output = string.Empty;
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo(@"C:\androidSDK\platform-tools\adb.exe");


                procStartInfo.Arguments = adbInput;
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                //procStartInfo.WorkingDirectory = toolPath;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                //proc.WaitForExit();
                while (true)
                {
                    result = proc.StandardOutput.ReadLine();
                    if(!result.Equals("")) events.Add(result);
                    
                    //Program.sr = proc.StandardOutput;
                    //Console.WriteLine(result);
                }
                /*error = proc.StandardError.ReadLine();  //Some ADB outputs use this
                if (result.Length > 1)
                {
                    output += result;
                }
                if (error.Length > 1)
                {
                    output += error;
                }*/
            }
            catch (Exception objException)
            {
                throw objException;
            }
        }
    }

}
