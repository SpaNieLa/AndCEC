using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Timers;
using System.Net.NetworkInformation;
using System.Net;
using System.Drawing;

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

        private static System.Timers.Timer mouseTimer;
        private static bool rightclick = false;

        // Adb and threading
        private static string address = "192.168.0.105";
        private static bool running = true;
        private static bool is_online;
      
        private static UDP_Server server;
        private static Thread eventing;
                
        // Initial ignoring state
        static Boolean ignoring = true;
        // Dictionary for qwerty mappings from layout.txt
        private static Dictionary<string, string> layout;
        private static bool shift_state = false;
        private static bool ctrl_state = false;
        // Mouse start point range 0-65535
        //private static int pos_x = 32768;
        //private static int pos_y = 49151;

        // Used to refresh desktop after drawing on it on CTRL_state changes
        //[System.Runtime.InteropServices.DllImport("Shell32.dll")]
        //private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        static void Main(string[] args)
        {
            // Exit handler
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            // Mapping keylayout
            InitLayout();

            

            //Waiting routine to make connection once device comes online
            System.Timers.Timer status_timer = new System.Timers.Timer(60000);
            status_timer.Elapsed += new ElapsedEventHandler(HandleTimer);
            status_timer.Start();

            UpdateStatus();
            
            server = new UDP_Server(9899);

            // Rightclick regognizition timer. Time to hold to establish mouse rightclick
            // TODO Convert to universal secondary button function timer
            mouseTimer = new System.Timers.Timer(1000);
            mouseTimer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            mouseTimer.Interval = 1000;

            // Thread to get constant stream of event in target Android
            StartEventing();
            

            /*// event execution loop
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

            }*/
        }
        
        // Starts thread to read events from adb stream
        private static void StartEventing()
        {
            while (running == true)
            {
                eventing = new Thread(new ThreadStart(server.RunServer));
                eventing.Start();
                while (!eventing.IsAlive) ;
                Thread.Sleep(2000);
               
             

                while (eventing.IsAlive)
                {
                    try
                    {                 
                        if (server.GetEvents().Count() > 0)
                        {
                            ParseEvent(server.GetEvents()[0]);
                            server.Remove(0);
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
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            rightclick = true;
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
                        //Console.WriteLine("Mouse move X " + value);
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
                        //Console.WriteLine("Mouse move Y " + value);
                        VirtualMouse.Move(0, (int)value*3);
                        return;
                        /*
                        pos_y = pos_y + ((int)value * 155);
                        if (pos_y > 65535) pos_y = 65535;
                        if (pos_y < 0) pos_y = 0;
                        break;
                        */
                    }
                case "ABS_Z":
                    {   // Mouse movement with relative information
                        //Console.WriteLine("Mouse move X " + value);
                        //Middle point 82 = 0100 0000 = 128
                        // "Dead-Zone" test
                        if (value < 135 && value > 121) return;
                        
                        VirtualMouse.Move((int)value -128, 0);
                        return;
                        /*
                        pos_x = pos_x + ((int)value * 90);
                        if (pos_x > 65535) pos_x = 65535;
                        if (pos_x < 0) pos_x = 0;

                        break;
                        */
                    }
                case "ABS_RZ":
                    {    // Mouse movement with relative information
                        //Console.WriteLine("Mouse move Y " + value);
                        //Middle point 82 = 0100 0000 = 128
                        // "Dead-Zone" test
                        if (value < 135 && value > 121) return;

                        VirtualMouse.Move(0, (int)value -128);
                        return;
                        /*
                        pos_y = pos_y + ((int)value * 155);
                        if (pos_y > 65535) pos_y = 65535;
                        if (pos_y < 0) pos_y = 0;
                        break;
                        */
                    }

                case "ABS_X":
                    {   // Mouse movement with relative information
                 
                        //Middle point 82 = 0100 0000 = 128
                        // "Dead-Zone" test
                        if (value < 135 && value > 121) return;
                        int xvalue = (int)value - 128;
                        if (xvalue < 0) SendKeys.SendWait("A");
                        else SendKeys.SendWait("D");
                        //VirtualMouse.Move((int)value - 128, 0);
                        return;
                        /*
                        pos_x = pos_x + ((int)value * 90);
                        if (pos_x > 65535) pos_x = 65535;
                        if (pos_x < 0) pos_x = 0;

                        break;
                        */
                    }
                case "ABS_Y":
                    {    // Mouse movement with relative information
           
                        //Middle point 82 = 0100 0000 = 128
                        // "Dead-Zone" test
                        
                        if (value < 135 && value > 121) return;
                        int yvalue = (int)value- 128;
                      
                        if (yvalue > 0) SendKeys.SendWait("S");
                        else SendKeys.SendWait("W");

                        // VirtualMouse.Move(0, (int)value - 128);
                        return;
                        /*
                        pos_y = pos_y + ((int)value * 155);
                        if (pos_y > 65535) pos_y = 65535;
                        if (pos_y < 0) pos_y = 0;
                        break;
                        */
                    }
                case "ABS_HAT0X":
                    {   // Mouse movement with relative information

                        int xvalue = (int)value;
                        if (xvalue > 0) SendKeys.SendWait(layout["KEY_RIGHT"]);
                        else if(xvalue < 0) SendKeys.SendWait(layout["KEY_LEFT"]);
                     
                        return;
                        /*
                        pos_x = pos_x + ((int)value * 90);
                        if (pos_x > 65535) pos_x = 65535;
                        if (pos_x < 0) pos_x = 0;

                        break;
                        */
                    }
                case "ABS_HAT0Y":
                    {    // Mouse movement with relative information

                        //Middle point 82 = 0100 0000 = 128
                        // "Dead-Zone" test
                        int yvalue = (int)value;
                        if (yvalue > 0) SendKeys.SendWait(layout["KEY_DOWN"]);
                        else if (yvalue < 0) SendKeys.SendWait(layout["KEY_UP"]);

                       
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
                        //Console.WriteLine("Received Relative-event from android: " + Regex.Split(x, @"\W+")[5] + " Value: " + Regex.Split(x, @"\W+")[6]);
                        ReplayMouse(Regex.Split(x, @"\W+")[5], Regex.Split(x, @"\W+")[6]);
                        return;
                    }
                    if (x.Contains("EV_ABS"))       //Max ingoming rate ~52 per second
                    {
                        //Console.WriteLine("Received ABS-event from android: " + Regex.Split(x, @"\W+")[5] + " Value: " + Regex.Split(x, @"\W+")[6]);
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
            if (key.Contains("01e5"))            // Changes the ctrl state
            {
                Point pt = new Point(10,100); // Point to draw CTRL overlay
              
                if (state.Contains("DOWN") && ctrl_state == false)
                {
                    ctrl_state = true;
                    // Draw to desktop to show CTRL state
                    using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        g.DrawString("CTRL", new Font(SystemFonts.DefaultFont.FontFamily,22,FontStyle.Bold), Brushes.LimeGreen, pt);
                       
                    }
                }
                else if (state.Contains("DOWN") && ctrl_state == true)
                {
                    ctrl_state = false;
                    // Refresh desktop to clear previous graphics
                    //SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
                    using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        g.DrawString("CTRL", new Font(SystemFonts.DefaultFont.FontFamily, 22, FontStyle.Bold), Brushes.Red, pt);

                    }

                }
                Console.WriteLine("Setting CTRL state to " + ctrl_state);
                return;
                
            }

           

            if (key.Contains("KEY_RED"))            // Changes the ignore state
            {
                if (state.Contains("DOWN"))
                {
                    
                    mouseTimer.Start();
                    return;
                }
                else
                {
                    if (rightclick == true)
                    {
                        //Secondary funcktion for KEY_RED
                        ReplayKey("KEY_SEC_RED", "UP");
                    }
                    else
                    {
                        if (ignoring == true) ignoring = false;
                        else ignoring = true;
                        
                    }

                    rightclick = false;
                    mouseTimer.Stop();
                }
                return;

                
            }

            if (state.Contains("DOWN") && key.Contains("KEY_")) return;     // At this point ignore "halfway"
            if (ignoring == true) return;          // Check if ignoring events!

            if (layout.ContainsKey(key) && state.Contains("UP"))           // These keys are from the layout file and read from the dictionary. SendKeys Method enabled keys only
            {
                //Check if key is used to execute path on layout file.
                if (System.Text.RegularExpressions.Regex.IsMatch(layout[key], @"\b\S*:"))
                {
                    Console.WriteLine("Launching set path");
                    System.Diagnostics.Process.Start(layout[key]);
                    return;
                }
                //Check if key is used to execute path on layout file. (With CTRL state on)
                if(layout.ContainsKey("CTRL_"+key) && ctrl_state == true)
                    if (System.Text.RegularExpressions.Regex.IsMatch(layout["CTRL_"+key], @"\b\S*:"))
                    {
                        Console.WriteLine("Launching set path");
                        System.Diagnostics.Process.Start(layout["CTRL_"+key]);
                        return;
                    }


                if (ctrl_state == true) SendKeys.SendWait('^'+layout[key]);
                else if (shift_state == false) SendKeys.SendWait(layout[key]);
                else if (ctrl_state == true) SendKeys.SendWait('^'+layout['+'+key]);
                else SendKeys.SendWait(layout['+' + key]);


            }

            switch (key)
            {
                case "BTN_LEFT":
                    {
                        Console.WriteLine("Executing " + key);
                        if (state.Contains("DOWN"))
                        {
                            VirtualMouse.LeftDown();
                            mouseTimer.Start();
                        }
                        else
                        {
                            if (rightclick == true)
                            {
                                VirtualMouse.RightClick();
                            }
                            else VirtualMouse.LeftUp();

                            rightclick = false;
                            mouseTimer.Stop();
                        }
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
                case "KEY_FASTFORWARD":
                    {
                        Console.WriteLine("Executing " + key);
                        AppCommand.Send(AppCommands.MediaNext);
                        return;
                    }
                case "KEY_REWIND":
                    {
                        Console.WriteLine("Executing " + key);
                        AppCommand.Send(AppCommands.MediaPrevious);
                        return;
                    }
                
                //DualShock R1
                case "BTN_Z":
                    {
                        Console.WriteLine("Executing " + key);
                        if (state.Contains("DOWN"))
                        {
                            VirtualMouse.LeftDown();
                            //mouseTimer.Start();
                        }

                        else VirtualMouse.LeftUp();

                        return;
                    }

            }

        }
        // Online status timer handler
        private static void HandleTimer(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("\nOnline-Status update via status_timer..");
            UpdateStatus();
        }

        // Checks if device is (still) online
        private static void UpdateStatus()
        {
            is_online = CheckOnline();
            while (is_online == false)
            {
                is_online = CheckOnline();
                if (is_online == false)
                {
                    System.Threading.Thread.Sleep(10000);
                    System.Console.WriteLine("waiting device..");
                }
                else
                {
                    System.Console.WriteLine("Found ya!");
                    Restart();
                    break;
                }
            }

        }

        private static void Restart()
        {
            try
            {
                eventing.Abort();
            }
            catch (NullReferenceException)
            {
                System.Console.WriteLine("eventing null");
            }

        }


        // Check if device is ´reachable
        private static bool CheckOnline()
        {
            // Ping's the local machine.
            Ping pingSender = new Ping();
            IPAddress address = IPAddress.Parse("192.168.0.105");

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            // Wait 10 seconds for a reply.
            int timeout = 10000;
            PingReply reply = pingSender.Send(address, timeout, buffer);

            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine("Address: {0}", reply.Address.ToString());
                Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                return true;
            }
            else
            {
                Console.WriteLine(reply.Status);
                return false;
            }

        }


        // Create mappings for keys with dictionary to use with Sendkeys()
        private static void InitLayout()
        {
            layout = new Dictionary<string, string>();

            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                layout.Add(key, ConfigurationManager.AppSettings[key]);  
            }
        }

        // exit handler for cleaning up
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                Console.WriteLine("Console window closing, death imminent");
                eventing.Abort();
            }
            return false;
        }
    }
}
