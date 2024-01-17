using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sessionhijack
{
  public class Password : Form
  {
    private string username;
    private IContainer components = (IContainer) null;
    private Label label1;
    private Button button1;
    private TextBox textBox1;

    public Password(string sentusername)
    {
      this.InitializeComponent();
      this.username = sentusername;
    }

    private async void button1_Click(object sender, EventArgs e)
    {
      try
      {
        string newpassword = this.textBox1.Text;
        char[] notAllowedCharacters = new char[5]
        {
          '<',
          '>',
          '\\',
          '/',
          '|'
        };
        char[] chArray = notAllowedCharacters;
        for (int index = 0; index < chArray.Length; ++index)
        {
          char c = chArray[index];
          if (newpassword.IndexOf(c) != -1)
          {
            int num = (int) MessageBox.Show("Password cannot contain: <, >, \\, /, |", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return;
          }
        }
        chArray = (char[]) null;
        if (newpassword == null)
        {
          int num1 = (int) MessageBox.Show(this.username + "'s password will be blank.", "Sessionsploit", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
        }
        if (MessageBox.Show("Change " + this.username + "'s password to " + newpassword + "?", "Sessionsploit", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
        {
          Process process = new Process();
          process.StartInfo.FileName = "cmd.exe";
          process.StartInfo.Arguments = " /c net user " + this.username + " " + newpassword;
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
          if (errorCode != 0)
          {
            int num2 = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return;
          }
          int num3 = (int) MessageBox.Show(this.username + "'s password successfully changed.", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
        newpassword = (string) null;
        notAllowedCharacters = (char[]) null;
      }
      catch
      {
        int num = (int) MessageBox.Show("Error occurred", "Sessionsploit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Password));
      this.label1 = new Label();
      this.button1 = new Button();
      this.textBox1 = new TextBox();
      this.SuspendLayout();
      this.label1.AutoSize = true;
      this.label1.Location = new Point(12, 26);
      this.label1.Name = "label1";
      this.label1.Size = new Size(81, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "New Password:";
      this.button1.Location = new Point(47, 65);
      this.button1.Name = "button1";
      this.button1.Size = new Size(122, 45);
      this.button1.TabIndex = 1;
      this.button1.Text = "Execute";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new EventHandler(this.button1_Click);
      this.textBox1.Location = new Point(100, 23);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new Size(100, 20);
      this.textBox1.TabIndex = 2;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(219, 122);
      this.Controls.Add((Control) this.textBox1);
      this.Controls.Add((Control) this.button1);
      this.Controls.Add((Control) this.label1);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (Password);
      this.Text = nameof (Password);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
