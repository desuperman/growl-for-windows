using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Growl
{
    static class ApplicationMain
    {
        [Flags()]
        internal enum Signal
        {
            Silent = 1,
            ReloadDisplays = 2,
            UpdateLanguage = 4
        }

        static Program program;
        static bool appIsAlreadyRunning;
        static bool silentMode;
        static List<InternalNotification> queuedNotifications = new List<InternalNotification>();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Application.SetCompatibleTextRenderingDefault(false);

            SingleInstanceApplication app = new SingleInstanceApplication("GrowlForWindows");
            using (app)
            {
                Signal signalFlag = 0;
                int signalValue = 0;
                appIsAlreadyRunning = app.IsAlreadyRunning;

                // handle protocol-triggered operations
                if (args != null && args.Length == 1)
                {
                    string protocolArgument = args[0];
                    Installation.ProtocolHandler handler = new Growl.Installation.ProtocolHandler(appIsAlreadyRunning);
                    signalFlag = handler.Process(protocolArgument, ref queuedNotifications, ref signalValue);
                }

                // handle command line options
                try
                {
                    Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
                    if (args != null)
                    {
                        foreach (string arg in args)
                        {
                            Parameter p = GetParameterValue(arg);
                            if (p.Argument != null) parameters.Add(p.Argument, p);
                        }
                    }

                    bool enableLogging = false;
                    if (parameters.ContainsKey("/log"))
                    {
                        string log = parameters["/log"].Value.ToLower();
                        if (log == "true") enableLogging = true;
                        Properties.Settings.Default.EnableLogging = enableLogging;
                    }
                    bool debugMode = false;
                    if (parameters.ContainsKey("/debug"))
                    {
                        string debug = parameters["/debug"].Value.ToLower();
                        if (debug == "true") debugMode = true;
                        Utility.DebugMode = debugMode;
                        if (debugMode) MessageBox.Show("growl is now in debug mode");
                    }
                    if (parameters.ContainsKey("/silent"))
                    {
                        string silent = parameters["/silent"].Value.ToLower();
                        if (silent == "true") silentMode = true;
                        if (silentMode)
                            signalFlag = signalFlag | Signal.Silent;
                    }
                }
                catch (Exception ex)
                {
                    // dont fail on bad arguments
                    Utility.WriteDebugInfo("Bad arguments: " + ex.Message);
                }

                if (!appIsAlreadyRunning)
                {
                    app.AnotherInstanceStarted += new SingleInstanceApplication.AnotherInstanceStartedEventHandler(app_AnotherInstanceStarted);
                    try
                    {
                        program = new Program();
                        program.ProgramRunning += new EventHandler(program_ProgramRunning);

#if GTK

						System.Threading.ThreadStart gtkRun = new System.Threading.ThreadStart(()=>
						{
							if(!GLib.Thread.Supported) GLib.Thread.Init();
							Gdk.Threads.Init();
							Gtk.Application.Init();
							Gtk.Application.Run();
						});
						System.Threading.Thread gtkThread = new System.Threading.Thread(gtkRun);
						gtkThread.Start();
#endif						
                        app.Run(program);
                        program.Dispose();

                    }
                    catch (Exception ex)
                    {
                        string source = "Growl";
                        string logtext = String.Format("{0}\r\n\r\n{1}", ex.Message, ex.StackTrace);
                        string msgtext = "Growl encountered a fatal error and cannot continue.\r\n\r\nPlease see the Event Viewer for details.";
                        if (!System.Diagnostics.EventLog.SourceExists(source))
                        {
                            System.Diagnostics.EventLog.CreateEventSource(source, "Application");
                        }
                        System.Diagnostics.EventLog elog = new System.Diagnostics.EventLog();
                        elog.Source = source;
                        elog.WriteEntry(logtext, System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show(msgtext, "Growl - Fatal Exception", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
#if DEBUG
						Console.WriteLine(ex.ToString());
#endif
                    }
                }
                else
                {
                    InternalNotification.SaveToDisk(ref queuedNotifications);
                    app.SignalFirstInstance((int) signalFlag, signalValue);
                }
            }
        }

        static void program_ProgramRunning(object sender, EventArgs e)
        {
            HandleSystemNotifications();
        }

        static void app_AnotherInstanceStarted(int signalFlag, int signalValue)
        {
            // show notification that growl is already running...
            Console.WriteLine("INSTANCE: growl is already running");

            program.AlreadyRunning(signalFlag, signalValue);

            HandleSystemNotifications();
        }

        static void HandleSystemNotifications()
        {
            InternalNotification.ReadFromDisk(ref queuedNotifications);
            if (queuedNotifications != null)
            {
                foreach (InternalNotification n in queuedNotifications)
                {
                    Display display = (n.Display != null ? DisplayStyleManager.FindDisplayStyle(n.Display) : null);
                    program.SendSystemNotification(n.Title, n.Text, display);
                }

                queuedNotifications.Clear();
            }
            queuedNotifications = null;
        }

        static public bool HasProgramLaunchedYet
        {
            get
            {
                return (appIsAlreadyRunning || program != null);
            }
        }

        static public bool SilentMode
        {
            get
            {
                return silentMode;
            }
            set
            {
                silentMode = value;
            }
        }

        static public Program Program
        {
            get
            {
                return program;
            }
        }

        private static Parameter GetParameterValue(string argument)
        {
            if (argument.StartsWith("/"))
            {
                string val = "";
                string[] parts = argument.Split(new char[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    val = parts[1];
                    if (val.StartsWith("\"") && val.EndsWith("\""))
                    {
                        val = val.Substring(1, val.Length - 2);
                    }
                }
                return new Parameter(parts[0], val);
            }
            return Parameter.Empty;
        }

        private struct Parameter
        {
            public static Parameter Empty = new Parameter(null, null);

            public Parameter(string arg, string val)
            {
                this.Argument = arg;

                //if (val == null) val = String.Empty;
                //val = val.Replace("\\n", "\n");
                //val = val.Replace("\\\n", "\\n");
                this.Value = val;
            }

            public string Argument;
            public string Value;
        }
    }
}