using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sessionhijack
{
  public class Processes : Form
  {
    private string username;
    private ImageList imageList = new ImageList();
    private IContainer components = (IContainer) null;
    public ListView listView1;

    public Processes(string procusername)
    {
      this.InitializeComponent();
      this.listView1.SmallImageList = this.imageList;
      this.imageList.ImageSize = new Size(20, 20);
      this.imageList.TransparentColor = Color.Transparent;
      this.listView1.View = View.SmallIcon;
      this.listView1.Columns.Add(nameof (Processes), this.listView1.Width - SystemInformation.VerticalScrollBarWidth - 4);
      this.username = procusername;
    }

    private async void Processes_Load(object sender, EventArgs e) => await this.GetProcess();

    private async Task GetProcess()
    {
      string currentuser = Environment.UserName;
      if (this.username == currentuser)
      {
        await Task.Run((Action) (() =>
        {
          foreach (Process process1 in Process.GetProcesses())
          {
            Process process = process1;
            try
            {
              Icon associatedIcon = Icon.ExtractAssociatedIcon(process.MainModule.FileName);
              this.imageList.Images.Add(process.ProcessName, associatedIcon);
              this.Invoke(new Action(() => this.listView1.Items.Add(new ListViewItem(process.ProcessName, this.imageList.Images.Count - 1))));
              }
            catch (Win32Exception ex)
            {
            }
          }
        }));
        currentuser = (string) null;
      }
      else
      {
        try
        {
          Process proc = new Process()
          {
            StartInfo = new ProcessStartInfo()
            {
              FileName = "cmd.exe",
              Arguments = " /c powershell.exe \"Get-Process -IncludeUserName | Select Processname,UserName\" | findstr \"" + this.username + "\"",
              UseShellExecute = false,
              RedirectStandardOutput = true,
              CreateNoWindow = true
            }
          };
          await Task.Run((Action) (() =>
          {
            proc.Start();
            proc.WaitForExit();
            string end = proc.StandardOutput.ReadToEnd();
            int exitCode = proc.ExitCode;
            proc.Close();
            if (exitCode != 0)
            {
              int num = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
              this.Invoke(new Action(() => this.Close()));

              }
              string str1 = end;
            char[] chArray = new char[1]{ '\n' };
            foreach (string str2 in str1.Split(chArray))
            {
              try
              {
                this.listView1.View = View.Details;
                int length = str2.IndexOf(' ');
                string str3 = str2.Substring(0, length);
                Icon processIcon = Processes.GetProcessIcon(str3);
                this.imageList.Images.Add(str3, processIcon);
                this.listView1.Items.Add(str3, this.imageList.Images.Count - 1);
              }
              catch
              {
              }
            }
          }));
          currentuser = (string) null;
        }
        catch
        {
          int num = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
          this.Close();
          currentuser = (string) null;
        }
      }
    }

    private static Icon GetProcessIcon(string processName)
    {
      Process[] processesByName = Process.GetProcessesByName(processName);
      return processesByName.Length != 0 ? Icon.ExtractAssociatedIcon(processesByName[0].MainModule.FileName) : (Icon) null;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Processes));
      this.listView1 = new ListView();
      this.SuspendLayout();
      this.listView1.GridLines = true;
      this.listView1.HideSelection = false;
      this.listView1.Location = new Point(9, 10);
      this.listView1.Margin = new Padding(2);
      this.listView1.Name = "listView1";
      this.listView1.Size = new Size(219, 355);
      this.listView1.TabIndex = 0;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = View.Details;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(239, 376);
      this.Controls.Add((Control) this.listView1);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.Margin = new Padding(2);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (Processes);
      this.Text = nameof (Processes);
      this.Load += new EventHandler(this.Processes_Load);
      this.ResumeLayout(false);
    }
  }
}
