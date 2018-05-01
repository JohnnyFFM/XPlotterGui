using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace XplotterGui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private int x1;
        private int x2;
        private List<Task> tasklist;
        AutoResetEvent[] autoEvents;

        public struct Task
        {
            public int file;
            public int fileLength;
            public int start;
            public int len;
        }

        //Plotter Thread
        static void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null)
                return;
            Console.WriteLine(e.Data);
        }

        void task1Status(string text)
        {
            if (textBox3.InvokeRequired){
                textBox3.Invoke(new MethodInvoker(() => { task1Status(text); }));
                return;
            }
            else
            {
                if (textBox3.TextLength > 1000) textBox3.Clear();
                textBox3.Text = text;
            }
            
        }
        void task2Status(string text)
        {
            if (textBox4.InvokeRequired)
            {
                textBox4.Invoke(new MethodInvoker(() => { task2Status(text); }));
                return;
            } else {
                if (textBox4.TextLength > 1000) textBox4.Clear();
                textBox4.Text = text;
            }
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null)
                return;
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new MethodInvoker(() => { p_OutputDataReceived(sender, e); }));
                return;
            }
            else
            {
                if (e.Data != null)
                {
                    textBox1.AppendText(e.Data+"\n");
                }
            }
        }
        void p_OutputDataReceived2(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null)
                return;
            if (textBox2.InvokeRequired)
            {
                textBox2.Invoke(new MethodInvoker(() => { p_OutputDataReceived2(sender, e); }));
                return;
            }
            else
            {
                if (e.Data != null)
                {
                    textBox2.AppendText(e.Data + "\n");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                xPlotter.Text = openFileDialog.FileName;
                Properties.Settings.Default.xplotter = openFileDialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void updateSSD_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                ssdCache.Text = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.cache = ssdCache.Text;
                Properties.Settings.Default.Save();
                UpdateSpace();

            }
        }

        private void UpdateSpace()
        {
            if (Directory.Exists(ssdCache.Text)){
                DriveInfo drive = new DriveInfo(ssdCache.Text);
                DriveInfo a = new DriveInfo(drive.Name);
                space.Text = "Using " + prettyBytes(a.AvailableFreeSpace * (long)cachepct.Value / 100) + " (" + (a.AvailableFreeSpace * (long)cachepct.Value / 100 / (2 << 17)).ToString("#,##0") + " Nonces)" + " out of " + prettyBytes(a.AvailableFreeSpace) + " (" + (a.AvailableFreeSpace / (2 << 17)).ToString("#,##0") + " Nonces)";
            }
        }
        private void UpdateSpace2()
        {
            if (Directory.Exists(target.Text))
            {
                DriveInfo drive = new DriveInfo(target.Text);
                DriveInfo a = new DriveInfo(drive.Name);
                space2.Text = prettyBytes(a.AvailableFreeSpace) + " (" + (a.AvailableFreeSpace *0.9999 / (2 << 17)).ToString("#,##0") + " Nonces)";
            }
        }
        private string prettyBytes(long bytes)
        {
            string result;
            if (bytes < 1024) { result = Math.Round((double)bytes, 1).ToString() + "B"; }
            else if (bytes < 1024 * 1024) { result = Math.Round((double)bytes / 1024, 1).ToString() + "kB"; }
            else if (bytes < 1024 * 1024 * 1024) { result = Math.Round((double)bytes / 1024 / 1024, 1).ToString() + "MB"; }
            else if (bytes < 1024L * 1024 * 1024 * 1024) { result = Math.Round((double)bytes / 1024 / 1024 / 1024, 1).ToString() + "GB"; }
            else { result = Math.Round((double)bytes / 1024 / 1024 / 1024 / 1024, 1).ToString() + "TB"; }
            return result;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadSettings();
            UpdateSpace();
            UpdateSpace2();
            updatesn();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                target.Text = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.target = target.Text;
                Properties.Settings.Default.Save();
                UpdateSpace2();
                updateNtp();
            }
        }

        private void loadSettings()
        {
            xPlotter.Text = Properties.Settings.Default.xplotter;
            numericID.Text = Properties.Settings.Default.ID;
            threads.Value = Properties.Settings.Default.threads;
            ram.Value = Properties.Settings.Default.ram;
            ssdCache.Text = Properties.Settings.Default.cache;
            cachepct.Value = Properties.Settings.Default.space;
            ntpValue.Checked = !Properties.Settings.Default.ntp;
            ntp.Value = Properties.Settings.Default.ntpvalue;
            target.Text = Properties.Settings.Default.target;
            automaticsn.Checked = Properties.Settings.Default.start;
            manualsn.Checked = !Properties.Settings.Default.start;
            drive.Value = Properties.Settings.Default.drive;
            offset.Value = Properties.Settings.Default.offset;
            ntpmax.Checked = Properties.Settings.Default.ntp;
     
            oneFile.Checked = Properties.Settings.Default.ftp;
            moreFiles.Checked = !Properties.Settings.Default.ftp;
            npf.Value = Properties.Settings.Default.ftpvalue;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ID = numericID.Text;
            Properties.Settings.Default.Save();
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.threads = (int)threads.Value;
            Properties.Settings.Default.Save();
        }

        private void ram_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ram = (int)ram.Value;
            Properties.Settings.Default.Save();

        }

        private void start_Click(object sender, EventArgs e)
        {
            int cachesize =0;
            //get cache
            if (Directory.Exists(ssdCache.Text))
            {
                DriveInfo drive = new DriveInfo(ssdCache.Text);
                DriveInfo a = new DriveInfo(drive.Name);
                cachesize = (int)(a.AvailableFreeSpace * (long)cachepct.Value / 100 / (2 << 18));
                Console.WriteLine(cachesize.ToString());
                cachesize = (cachesize / 8) * 8;
                Console.WriteLine(cachesize.ToString());
            }
            else { return; }
            x1 = 0;
            x2 = 0;
            //prepare tasklist
            tasklist = new List<Task>();
            //loop files

            for (int i = 0; i < (int)Math.Ceiling(((double)ntp.Value / (double)npf.Value)); i++)
            {
                //loop chunks                
                int length = Math.Min((int)npf.Value, -(int)npf.Value * i + (int)ntp.Value);

                for (int j = 0; j < (int)Math.Ceiling(((double)length / (double)cachesize)); j++) //todo replace with cache size in nonces
                {
                    //create Task
                    tasklist.Add(new Task { file = i, fileLength = length, start = i * (int)npf.Value + j * cachesize, len = Math.Min(cachesize, length - j * cachesize) });
                }
            }
            
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Control();
            }).Start();
        }

        //Controlthread
        private void Control()
        {
            autoEvents = new AutoResetEvent[]
              {
                    new AutoResetEvent(false),
                    new AutoResetEvent(false)
              };

            for (int i = 0; i < tasklist.Count+1; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(U1));
                ThreadPool.QueueUserWorkItem(new WaitCallback(U2));
                WaitHandle.WaitAll(autoEvents);
            }
        }
        
        //Plotter Thread
        public void U1(object stateInfo)
        {
            //Reset Status
            task1Status("");
            if (x1 == tasklist.Count)
            {
                task1Status("All tasks completed!");
                x1++;
                autoEvents[0].Set();
            }
            else
            {
                try
                {
                    using (Process p1 = new Process())
                    {
                        // set start info
                        Console.WriteLine("-id " + numericID.Text + " -sn " + (snonce.Value + tasklist[x1].start).ToString() + " -n " + tasklist[x1].len + " -t " + threads.Value.ToString() + " -mem " + ram.Value.ToString() + "G" + " -path " + ssdCache.Text);
                        p1.StartInfo = new ProcessStartInfo(xPlotter.Text, "-id "+numericID.Text+" -sn "+ (snonce.Value+tasklist[x1].start).ToString()+" -n "+tasklist[x1].len+" -t "+threads.Value.ToString() + " -mem " + ram.Value.ToString()+"G" + " -path "+ssdCache.Text)
                        {
                            WindowStyle = ProcessWindowStyle.Hidden,
                            //Arguments = "/A",
                            //RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            WorkingDirectory = @"c:\",
                            CreateNoWindow = true
                        };

                        // event handlers for output & error
                        p1.EnableRaisingEvents = true;
                        p1.OutputDataReceived += p_OutputDataReceived;
                        p1.ErrorDataReceived += p_ErrorDataReceived;
                        p1.Exited += new EventHandler(p1_threadExit);
                        // start process
                        task1Status("Running plotter task: " + (x1 + 1).ToString() + "/" + (tasklist.Count + 1).ToString());
                        p1.Start();
                        p1.BeginOutputReadLine();
                        p1.WaitForExit();
                         //task1Status("Task done! Waiting for Task 2, i.e. HDD is bottleneck!");
                        x1++;
                        autoEvents[0].Set();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public void p1_threadExit(object sender, System.EventArgs e)
        {
            task1Status("Task " + (x1 + 1).ToString() + " completed! ! Bottleneck is HDD.");
           // autoEvents[0].Set();
            Console.WriteLine("YYYYYYYYYY");
        }

        public void p2_threadExit(object sender, System.EventArgs e)
        {
            if (x2 == tasklist.Count)
            {
                task2Status("All tasks completed!");
            }
            else
            {
               task2Status("Task "+(x2+1).ToString()+" completed! Bottleneck is CPU.");
            }
          //  autoEvents[1].Set();
            Console.WriteLine("XXXXXXX");
        }

        //Mover Thread
        public void U2(object stateInfo)
        {
            //Reset Status
            task2Status("");
            if (x2 == 0)
            {
                task2Status("Warming up! Waiting for plotter task...");
                //Console.WriteLine("Task done! Waiting for Task 1, i.e. HDD is bottleneck!");
                x2++;
                autoEvents[1].Set();
            }
            else
            {
                try
                {
                    using (Process p2 = new Process())
                    {
                        // set start info
                        Console.WriteLine(ssdCache.Text + "\\" + numericID.Text + "_" + (tasklist[x2 - 1].start + snonce.Value).ToString() + "_" + tasklist[x2 - 1].len.ToString() + "_" + tasklist[x2 - 1].len.ToString() + " " + target.Text + "\\" + numericID.Text + "_" + (snonce.Value + npf.Value * tasklist[x2 - 1].file).ToString() + "_" + tasklist[x2 - 1].fileLength.ToString() + "_" + tasklist[x2 - 1].fileLength.ToString());
                        p2.StartInfo = new ProcessStartInfo(Application.StartupPath +"\\"+ "plotMerge.exe", ssdCache.Text + "\\" + numericID.Text + "_" + (tasklist[x2 - 1].start + snonce.Value).ToString() + "_" + tasklist[x2 - 1].len.ToString() + "_" + tasklist[x2 - 1].len.ToString() + " "+target.Text+ "\\" + numericID.Text + "_" + (snonce.Value+npf.Value * tasklist[x2 - 1].file).ToString() + "_" + tasklist[x2 - 1].fileLength.ToString() + "_" + tasklist[x2 - 1].fileLength.ToString())
                        {
                            WindowStyle = ProcessWindowStyle.Hidden,
                            //Arguments = "/A",
                            //RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            WorkingDirectory = @"c:\",
                            CreateNoWindow = true
                        };

                        // event handlers for output & error
                        p2.EnableRaisingEvents = true;
                        p2.OutputDataReceived += p_OutputDataReceived2;
                        p2.ErrorDataReceived += p_ErrorDataReceived;
                        p2.Exited += new EventHandler(p2_threadExit);
                        // start process
                        task2Status("Running transfer task: " + (x2+1).ToString() + "/" + (tasklist.Count + 1).ToString());
                        p2.Start();
                        p2.BeginOutputReadLine();
                        p2.WaitForExit();
                        // p2.Close();
                        File.Delete(ssdCache.Text + "\\" + numericID.Text + "_" + (tasklist[x2 - 1].start + snonce.Value).ToString() + "_" + tasklist[x2 - 1].len.ToString() + "_" + tasklist[x2 - 1].len.ToString());
                        //start_plot(x1, x2);
                        //task2Status("Task done! Waiting for Task 1, i.e. CPU is bottleneck!");
                        x2++;
                        autoEvents[1].Set();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void xPlotter_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.xplotter = xPlotter.Text;
            Properties.Settings.Default.Save();
        }

        private void ssdCache_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.cache = ssdCache.Text;
            Properties.Settings.Default.Save();
            UpdateSpace();
        }

        private void cachepct_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.space = cachepct.Value;
            Properties.Settings.Default.Save();
            UpdateSpace();
        }

        private void target_TextChanged(object sender, EventArgs e)
        {
                Properties.Settings.Default.target = target.Text;
                Properties.Settings.Default.Save();
                UpdateSpace2();
                updateNtp();
            
        }

        private void updatesn()
        { 
            if (automaticsn.Checked)
            {
                snonce.Value = (drive.Value-1)* offset.Value;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            updatesn();
            Properties.Settings.Default.drive = drive.Value;
            Properties.Settings.Default.Save();
        }

        private void automaticsn_CheckedChanged(object sender, EventArgs e)
        {
            updatesn();
            Properties.Settings.Default.start = automaticsn.Checked;
            Properties.Settings.Default.Save();
        }

        private void updateNtp()
        {
            if (ntpmax.Checked && Directory.Exists(target.Text))
                {
                DriveInfo drive = new DriveInfo(target.Text);
                DriveInfo a = new DriveInfo(drive.Name);
                ntp.Value = (decimal)((double)(a.AvailableFreeSpace / (2 << 17)) * 0.9999);
                Properties.Settings.Default.ntpvalue = ntp.Value;
                Properties.Settings.Default.Save();
            }
        }

        private void ntpmax_CheckedChanged(object sender, EventArgs e)
        {
            updateNtp();
            Properties.Settings.Default.ntp = ntpmax.Checked;
            Properties.Settings.Default.Save();
            

        }

        private void offset_ValueChanged(object sender, EventArgs e)
        {
            updatesn();
            Properties.Settings.Default.offset = offset.Value;
            Properties.Settings.Default.Save();
        }

        private void ntp_ValueChanged(object sender, EventArgs e)
        {
            if (!ntpmax.Checked)
            {
                Properties.Settings.Default.ntpvalue = ntp.Value;
                Properties.Settings.Default.Save();
            }
            displayPlotSize();
            if (oneFile.Checked)
            {
                npf.Value = ntp.Value;
            }
   
        }

        private void oneFile_CheckedChanged(object sender, EventArgs e)
        {
            npf.Value = ntp.Value;
            Properties.Settings.Default.ftp = oneFile.Checked;
            Properties.Settings.Default.Save();
            displayPlotSize();
        }

        private void npf_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ftpvalue = npf.Value;
            Properties.Settings.Default.Save();
            displayPlotSize();

        }
        private void displayPlotSize()
        {
            plotsize.Text = "Total size: " + prettyBytes((long)ntp.Value * (2 << 17));
            if (oneFile.Checked) {
                files.Text = "1 file(s)";
                    } else {
                if (npf.Value > 0)
                    files.Text = Math.Ceiling((double)ntp.Value/(double)npf.Value).ToString() + " file(s)";
            }
        }

        private void ntp_Enter(object sender, EventArgs e)
        {
                ntpmax.Checked = false;
                ntpValue.Checked = true;
                Properties.Settings.Default.ntp = ntpmax.Checked;
                Properties.Settings.Default.Save();
        }

        private void npf_Enter(object sender, EventArgs e)
        {
            oneFile.Checked = false;
            moreFiles.Checked = true;
            Properties.Settings.Default.ftp = oneFile.Checked;
            Properties.Settings.Default.Save();
        }

        private void snonce_Enter(object sender, EventArgs e)
        {
            automaticsn.Checked = false;
            manualsn.Checked = true;
            Properties.Settings.Default.start = automaticsn.Checked;
            Properties.Settings.Default.Save();
        }

        private void drive_Enter(object sender, EventArgs e)
        {
            automaticsn.Checked = true;
            manualsn.Checked = false;
            Properties.Settings.Default.start = automaticsn.Checked;
            Properties.Settings.Default.Save();
        }

        private void offset_Enter(object sender, EventArgs e)
        {
            automaticsn.Checked =true;
            manualsn.Checked = false;
            Properties.Settings.Default.start = automaticsn.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
