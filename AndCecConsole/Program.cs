﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;



namespace AndCecConsole
{

    class Program
    {
        // Handler for closing adb on exit
        static ConsoleEventDelegate handler;
        // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        // Adb and threading
        private static Adb and1;
        private static Thread eventing;

        // Initial ignoring state
        static Boolean ignoring = true;
        // Dictionary for qwerty mappings from layout.txt
        private static Dictionary<string, string> layout;
        private static bool shift_state = false;
        // Mouse start point range 0-65535
        //private static int pos_x = 32768;
        //private static int pos_y = 49151;

        static void Main(string[] args)
        {
            // Exit handler
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            // Mapping keylayout
            InitLayout();

            // Connect to target Android
            and1 = new Adb("192.168.0.105");

            
            // Thread to get constant stream of event in target Android
            eventing = new Thread(new ThreadStart(and1.AdbReadEvents));
            eventing.Start();
            while (!eventing.IsAlive) ;
            Thread.Sleep(10);

            // event execution loop
            while (eventing.IsAlive)
            {
                try
                {
                    if (and1.GetEvents().Count() > 0)
                    {
                        ParseEvent(and1.GetEvents()[0]);
                        and1.Remove(0);
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
                catch (NullReferenceException)
                {
                    Thread.Sleep(1);
                    continue;
                }

            }
        }


        // Executes the matching keyevent from android on pc
        //  
        private static void ReplayMouse(string axis, string hexString)
        {
            uint value = uint.Parse(hexString, System.Globalization.NumberStyles.AllowHexSpecifier);
         
            switch (axis)
            {
                case "REL_X":
                    {   // Mouse movement with relative information
                        //Console.WriteLine("Mouse move X " + axis);
                        VirtualMouse.Move((int)value*3, 0);
                        return;
                        /*
                        pos_x = pos_x + ((int)value * 90);
                        if (pos_x > 65535) pos_x = 65535;
                        if (pos_x < 0) pos_x = 0;

                        break;
                        */
                    }
                case "REL_Y":
                    {    // Mouse movement with relative information
                        //Console.WriteLine("Mouse move Y " + axis);
                        VirtualMouse.Move(0, (int)value*3);
                        return;
                        /*
                        pos_y = pos_y + ((int)value * 155);
                        if (pos_y > 65535) pos_y = 65535;
                        if (pos_y < 0) pos_y = 0;
                        break;
                        */
                    }
            }
            // New calculated position for pointer. (Not needed with relative info)
            // VirtualMouse.MoveTo(pos_x, pos_y);
        }

        

        //  Finds the pressed key from the captured event
        //  Returns string that contains pressed key in "KEY_*" format or "notFound if there is no Key event.
        private static void ParseEvent(string x)
        {
            
            {
                try
                {
                    if (x.Contains("EV_KEY")) {
                        Console.WriteLine("Received Key-event from android: " + Regex.Split(x, @"\W+")[5] + " State: " + Regex.Split(x, @"\W+")[6]);
                        ReplayKey(Regex.Split(x, @"\W+")[5], Regex.Split(x, @"\W+")[6]);
                        return;
                    }
                    if (x.Contains("EV_REL"))       //Max ingoming rate ~52 per second
                    {
                        Console.WriteLine("Received Relative-event from android: " + Regex.Split(x, @"\W+")[5] + " Value: " + Regex.Split(x, @"\W+")[6]);
                        ReplayMouse(Regex.Split(x, @"\W+")[5], Regex.Split(x, @"\W+")[6]);
                        return;
                    }
                }
                catch (NullReferenceException)
                {
                    return;
                }
            }
            //Console.WriteLine("notFound");
            //return "notFound";
        }
        
        // Replay key event from android in computer
        private static void ReplayKey(string key, string state)
        {
            if (key.Contains("KEY_LEFTSHIFT"))            // Changes the shift state
            {
                if (state.Contains("DOWN")) shift_state = true;
                else shift_state = false;
                return;
            }


            if (state.Contains("DOWN") && key.Contains("KEY_")) return;     // At this point ignore "halfway"

            if (key.Contains("KEY_RED"))            // Changes the ignore state
            {
                if (ignoring == false) ignoring = true;
                else ignoring = false;
                Console.WriteLine("Setting ignoring state to " + ignoring);
                return;
            }
            if (ignoring == true) return;          // Check if ignoring events!

            if (layout.ContainsKey(key))
            {
                if(shift_state == false) SendKeys.SendWait(layout[key]);
                else SendKeys.SendWait(layout['+'+key]);
                 
                
            }

            switch (key)
            {
                case "BTN_LEFT":
                    {
                        Console.WriteLine("Executing " + key);
                        if (state.Contains("DOWN")) VirtualMouse.LeftDown();
                        else VirtualMouse.LeftUp();
                        return;
                    }

                case "KEY_GREEN":
                    {
                        Console.WriteLine("Launching DisplaySwitch.exe");
                        System.Diagnostics.Process.Start(@"C:\Windows\System32\DisplaySwitch.exe");
                        return;
                    }
                case "KEY_YELLOW":
                    {
                        Console.WriteLine("Launching DisplauSwitch.exe");
                        System.Diagnostics.Process.Start(@"F:\Program Files_HDD\Plex Home Theater\Plex Home Theater.exe");
                        return;
                    }
                          
                case "KEY_PLAYCD":
                    {
                        Console.WriteLine("Executing " + key);
                        AppCommand.Send(AppCommands.MediaPlay);
                        return;
                    }
                case "KEY_PAUSECD":
                    {
                        Console.WriteLine("Executing " + key);
                        AppCommand.Send(AppCommands.MediaPause);
                        return;
                    }
               
                case "KEY_VOLUMEUP":
                    {
                        Console.WriteLine("Executing " + key);
                        for (int i = 0; i < 2; i++)                         // loop to change 4units at a time
                        {
                            AppCommand.Send(AppCommands.VolumeUp);
                        }
                        return;
                    }
                case "KEY_VOLUMEDOWN":
                    {
                        Console.WriteLine("Executing " + key);
                        for (int i = 0; i < 2; i++)                         // loop to change 4units at a time
                        {
                            AppCommand.Send(AppCommands.VolumeDown);
                        }
                        return;
                    }
                case "KEY_MUTE":
                    {
                        Console.WriteLine("Executing " + key);
                        AppCommand.Send(AppCommands.VolumeMute);
                        return;
                    }
                case "KEY_NEXT":
                    {
                        Console.WriteLine("Executing " + key);
                        AppCommand.Send(AppCommands.MediaNext);
                        return;
                    }
                case "KEY_PREVIOUS":
                    {
                        Console.WriteLine("Executing " + key);
                        AppCommand.Send(AppCommands.MediaPrevious);
                        return;
                    }

            }

        }
        // Create mappings for keys with dictionary to use with Sendkeys()
        private static void InitLayout()
        {
            layout = new Dictionary<string, string>();

            foreach (string s in File.ReadLines(@"layout.txt"))
            {
                layout.Add(Regex.Split(s, @"\t")[0], Regex.Split(s, @"\t")[1]);
            }
        }

        // exit handler for cleaning up
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                Console.WriteLine("Console window closing, death imminent");
                eventing.Abort();
                and1.Dispose();
            }
            return false;
        }
    }
}
