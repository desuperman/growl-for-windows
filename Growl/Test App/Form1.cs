using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.Connector;
using Growl.CoreLibrary;

namespace Test_App
{
    public partial class Form1 : Form
    {
        private Growl.Connector.GrowlConnector growl;
        private Growl.Connector.Application app;

        public Form1()
        {
            InitializeComponent();

            growl = new GrowlConnector();
            //growl.Password = this.textBox2.Text;
            this.growl.OKResponse += new GrowlConnector.ResponseEventHandler(growl_OKResponse);
            this.growl.ErrorResponse += new GrowlConnector.ResponseEventHandler(growl_ErrorResponse);
            this.growl.NotificationCallback +=new GrowlConnector.CallbackEventHandler(growl_NotificationCallback);

            growl.KeyHashAlgorithm = Cryptography.HashAlgorithmType.SHA256;
            growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;
            //growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.DES;
            //growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.TripleDES;
            //growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.AES;

            this.app = new Growl.Connector.Application("SurfWriter");
            //app.Icon = "http://atomicbride.com/Apple.gif";
            //app.Icon = "http://www.thetroyers.com/images/Apple_Logo.jpg";
            //app.Icon = @"c:\apple.png";
            app.Icon = Properties.Resources.Apple;
            app.CustomTextAttributes.Add("Creator", "Apple Software");
            app.CustomTextAttributes.Add("Application-ID", "08d6c05a21512a79a1dfeb9d2a8f262f");
            //app.CustomBinaryAttributes.Add("Sound", "http://fake.net/app.wav");


            Growl.CoreLibrary.Detector detector = new Detector();
            if (detector.IsInstalled)
            {
                InvokeWrite(String.Format("Growl (v{0}; f{1}; a{2}) is installed at {3} ({4})", detector.FileVersion.ProductVersion, detector.FileVersion.FileVersion, detector.AssemblyVersion.ToString(), detector.InstallationFolder, detector.DisplaysFolder));
            }
            else
            {
                InvokeWrite("Growl is not available on this machine");
            }

            if (growl.IsGrowlRunning())
            {
                InvokeWrite("Growl is running");
            }
            else
            {
                InvokeWrite("Growl is not running");
            }
        }

        void growl_ErrorResponse(Response response)
        {
            InvokeWrite("Error - " + response.ErrorCode + " : " + response.ErrorDescription);
        }

        void growl_OKResponse(Response response)
        {
            InvokeWrite("OK - " + response.MachineName);
        }

        void growl_NotificationCallback(Response response, CallbackData callbackContext)
        {
            string s = String.Format("CALLBACK RECEIVED: {0} - {1} - {2} - {3}", callbackContext.NotificationID, callbackContext.Data, callbackContext.Type, callbackContext.Result);
            InvokeWrite(s);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            System.IO.FileStream fs1 = System.IO.File.Open(@"C:\Documents and Settings\brian\Desktop\f1.ico", System.IO.FileMode.Open);
            System.Drawing.Image i1 = System.Drawing.Bitmap.FromStream(fs1);

            System.IO.FileStream fs2 = System.IO.File.Open(@"C:\Documents and Settings\brian\Desktop\f2.ico", System.IO.FileMode.Open);
            System.Drawing.Image i2 = System.Drawing.Bitmap.FromStream(fs2);

            System.IO.FileStream fs3 = System.IO.File.Open(@"C:\Documents and Settings\brian\Desktop\f3.ico", System.IO.FileMode.Open);
            //System.Drawing.Image i3 = System.Drawing.Bitmap.FromStream(fs3);
            Icon icon = new Icon(fs3);
            Image i3 = icon.ToBitmap();

            System.Drawing.Image i4 = System.Drawing.Bitmap.FromFile(@"C:\Documents and Settings\brian\Desktop\f3.ico");

            System.Net.WebClient wc = new System.Net.WebClient();
            using (wc)
            {
                byte[] bytes = wc.DownloadData("http://haxe.org/favicon.ico");
                System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes, false);
                using (ms)
                {
                    ms.Position = 0;
                    System.Drawing.Image tempImage = System.Drawing.Bitmap.FromStream(ms, false, false);
                    // dont close stream yet, first create a copy
                    using (tempImage)
                    {
                        Image image = new Bitmap(tempImage);
                        Console.WriteLine(image);
                    }
                }
            }
            */

            NotificationType nt1 = new NotificationType("Download Complete");
            //nt1.Icon = new BinaryData(new byte[] { 65, 66, 67, 68 });
            nt1.Icon = "http://www.hamradio.pl/images/thumb/e/e5/Icon_48x48_star_01.png/48px-Icon_48x48_star_01.png";
            nt1.Enabled = false;
            nt1.CustomTextAttributes.Add("Language", "English");
            nt1.CustomTextAttributes.Add("Timezone", "PST");
            //nt1.CustomBinaryAttributes.Add("Sound", "http://fake.net/nt.wav");
            NotificationType nt2 = new NotificationType("Document Published", "Document successfully published", null, true);
            nt2.Icon = "http://coaching.typepad.com/EspressoPundit/feed-icon-legacy_blue_38.png";
            //nt2.CustomBinaryAttributes.Add("Sound", "http://fake.net/sound.wav");
            nt2.CustomBinaryAttributes.Add("Sound-Alt", new BinaryData(new byte[] { 70, 71, 72, 73 }));

            NotificationType[] types = new NotificationType[] { nt1, nt2 };

            //Growl.Connector.RequestData rd = new RequestData();
            //rd.Add("Return-To-Me", "some text value");
            //rd.Add("Return-To-Me2", "another value");
            Growl.Connector.RequestData rd = null;

            growl.Register(this.app, types, rd);
            this.textBox1.Text = "REGISTER sent";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Growl.Connector.Application app = new Growl.Connector.Application("SurfWriter");
            //app.Icon = "http://atomicbride.com/Apple.gif";
            //app.Icon = "http://www.thetroyers.com/images/Apple_Logo.jpg";
            app.Icon = @"c:\apple.png";
            app.CustomTextAttributes.Add("Creator", "Apple Software");
            app.CustomTextAttributes.Add("Application-ID", "08d6c05a21512a79a1dfeb9d2a8f262f");

            NotificationType nt1 = new NotificationType("Download Complete", "Download completed");
            //nt1.Icon = new BinaryData(new byte[] { 65, 66, 67, 68 });
            nt1.CustomTextAttributes.Add("Language", "English");
            nt1.CustomTextAttributes.Add("Timezone", "PST");

            Notification notification = new Notification(app.Name, nt1.Name, "123456", "You document was published", "File 'c:\\file.txt' was successfully published at 8:57pm.\n\nClick this notification to open the file.\n\nThis is a test of the expanding displays.");
            notification.Sticky = false;
            notification.Priority = Priority.Emergency;
            //notification.Icon = "http://atomicbride.com/Apple.gif";
            notification.Icon = "http://haxe.org/favicon.ico";
            notification.CustomTextAttributes.Add("Filename", @"c:\file.txt");
            notification.CustomTextAttributes.Add("Timestamp", "8:57pm");
            notification.CustomBinaryAttributes.Add("File", new BinaryData(new byte[] { 78, 78, 78, 78, 78, 78, 78, 78, 78, 78, 78, 78 }));

            //string url = "http://localhost/growl-callback.aspx";
            //string url = "mailto:brian@elementcodeproject.com";
            string url = "itpc:http://www.npr.org/rss/podcast.php?id=35";
            CallbackContext callback = new CallbackContext(url);

            Growl.Connector.RequestData rd = new RequestData();
            rd.Add("Return-To-Me", "some text value");
            rd.Add("Return-To-Me2", "this is some longer value, including a \n few \n line \n breaks");
            rd.Add("Can I have spaces?", "dont know");

            growl.Notify(notification, callback, rd);
            this.textBox1.Text = "NOTIFY sent";
        }

        private System.Timers.Timer timer;

        private void button4_Click(object sender, EventArgs e)
        {
            /*
            Growl.Connector.Application app = new Growl.Connector.Application("SurfWriter");

            string s = growl.Status(app);
            this.textBox1.Text = s;
             * */

            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer = null;
            }
            else
            {
                this.timer = new System.Timers.Timer(5 * 1000);
                this.timer.AutoReset = true;
                this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                this.timer.Start();
            }
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Growl.Connector.Notification notification = new Notification(this.app.Name, "Download Complete", String.Empty, "fake", "fake");
            this.growl.Notify(notification);
        }

        void InvokeWrite(string response)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DoWrite(Write), new object[] { response });
            }
            else
            {
                Write(response);
            }
        }

        private delegate void DoWrite(string message);

        private void Write(string message)
        {
            this.textBox1.Text = message;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Growl.Connector.Application app = new Growl.Connector.Application("SurfWriter");
            //app.Icon = "http://atomicbride.com/Apple.gif";
            //app.Icon = "http://www.thetroyers.com/images/Apple_Logo.jpg";
            app.Icon = @"c:\apple.png";
            app.CustomTextAttributes.Add("Creator", "Apple Software");
            app.CustomTextAttributes.Add("Application-ID", "08d6c05a21512a79a1dfeb9d2a8f262f");

            NotificationType nt1 = new NotificationType("Download Complete", "Download completed");
            nt1.Icon = new BinaryData(new byte[] { 65, 66, 67, 68 });
            nt1.CustomTextAttributes.Add("Language", "English");
            nt1.CustomTextAttributes.Add("Timezone", "PST");

            Notification notification = new Notification(app.Name, nt1.Name, "123456", "You document was published", @"File 'c:\file.txt' was successfully published at 8:57pm.");
            notification.Sticky = false;
            notification.Priority = Priority.Emergency;
            //notification.Icon = "http://atomicbride.com/Apple.gif";
            app.Icon = @"c:\apple.png";
            notification.CustomTextAttributes.Add("Filename", @"c:\file.txt");
            notification.CustomTextAttributes.Add("Timestamp", "8:57pm");
            notification.CustomBinaryAttributes.Add("File", new BinaryData(new byte[] { 78, 78, 78, 78, 78, 78, 78, 78, 78, 78, 78, 78 }));

            string data = "this is my context";
            string type = typeof(string).ToString();
            CallbackContext context = new CallbackContext(data, type);

            Growl.Connector.RequestData rd = new RequestData();
            rd.Add("Return-To-Me", "some text value");
            rd.Add("Return-To-Me2", "another value");

            growl.Notify(notification, context, rd);
            this.textBox1.Text = "NOTIFY sent";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //System.IO.File.WriteAllBytes(@"C:\Documents and Settings\brian\Desktop\growl protocol\tests\Request_FlashPolicyRequest.txt", System.Text.Encoding.UTF8.GetBytes("<policy-file-request/>\0"));


            string dir = @"C:\Documents and Settings\brian\Desktop\growl protocol\tests\";
            string resultsdir = dir + @"results\";

            if (System.IO.Directory.Exists(resultsdir))
                System.IO.Directory.Delete(resultsdir, true);
            System.IO.Directory.CreateDirectory(resultsdir);

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);

            System.IO.FileInfo[] files = di.GetFiles();

            foreach (System.IO.FileInfo f in files)
            {
                string filename = f.FullName;
                byte[] rb = new byte[4096];

                System.IO.StreamReader r = new System.IO.StreamReader(filename);
                string request = r.ReadToEnd();
                byte[] b = System.Text.Encoding.UTF8.GetBytes(request);

                System.Net.Sockets.TcpClient tcp = new System.Net.Sockets.TcpClient("127.0.0.1", Growl.Connector.GrowlConnector.TCP_PORT);    //local
                //System.Net.Sockets.TcpClient tcp = new System.Net.Sockets.TcpClient("superman", Growl.Connector.GrowlConnector.TCP_PORT);       //remote
                System.Net.Sockets.NetworkStream ns = tcp.GetStream();
                ns.Write(b, 0, b.Length);

                int count = ns.Read(rb, 0, rb.Length);
                byte[] a = new byte[count];
                Array.Copy(rb, a, count);

                System.IO.File.WriteAllBytes(resultsdir + f.Name, a);

            }

            

            string testName = null;

            // test
            testName = "Junk Data";

        }

        private void PasswordTest()
        {
            string password = "Password";
            byte[] saltBytes = new byte[] {1, 2, 3, 4};

            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            Console.WriteLine(passwordBytes);

            byte[] keyBasisBytes = new byte[passwordBytes.Length + saltBytes.Length];
            Array.Copy(passwordBytes, 0, keyBasisBytes, 0, passwordBytes.Length);
            Array.Copy(saltBytes, 0, keyBasisBytes, passwordBytes.Length, saltBytes.Length);
            Console.WriteLine(keyBasisBytes);

            byte[] keyBytes = Cryptography.ComputeHash(keyBasisBytes, Cryptography.HashAlgorithmType.MD5);
            Console.WriteLine(keyBytes);

            byte[] keyHashBytes = Cryptography.ComputeHash(keyBytes, Cryptography.HashAlgorithmType.MD5);
            string keyHash = Cryptography.HexEncode(keyHashBytes);
            Console.WriteLine(keyHash);

            string saltHash = Cryptography.HexEncode(saltBytes);
            Console.WriteLine(saltHash);

            Console.WriteLine(String.Format("MD5:{0}.{1}", keyHash, saltHash));
        }

        private void Test_Register(string testName, Growl.Connector.Application app, List<NotificationType> types, Cryptography.SymmetricAlgorithmType ea, Cryptography.HashAlgorithmType ha)
        {
            GrowlConnector g = new GrowlConnector(this.textBox2.Text);
            g.EncryptionAlgorithm = ea;
            g.KeyHashAlgorithm = ha;

            //string r = g.Register(app, types.ToArray());

            //WriteTestRequest(r);
        }

        private void WriteTestRequest(string r)
        {
            System.IO.StreamWriter writer = System.IO.File.CreateText(String.Format("Tests/Request-{0}.txt"));
            writer.Write(r);
            writer.Close();
        }

        private void WriteTestResponse(string r)
        {
            System.IO.StreamWriter writer = System.IO.File.CreateText(String.Format("Tests/Response-{0}.txt"));
            writer.Write(r);
            writer.Close();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            this.growl.Password = textBox2.Text;
        }
    }
}