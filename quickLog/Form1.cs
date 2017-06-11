using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Hotkeys;

namespace quickLog
{
	public partial class formQuick : Form
	{
		string LogPath = "C:\\logs\\";	//put the log files here.  We will load this from a config file later
		string LogExtension = ".csv";	//file extension... should prolly be an enum with various handlers
		bool dualLog = true;			//log to both weekly roll-ups and category based log files
		

		private Hotkeys.GlobalHotkey showUI;
		//private System.Windows.Forms.NotifyIcon myNotificationIcon;

		public formQuick()
		{
			InitializeComponent();
			this.txtLog.KeyPress +=new KeyPressEventHandler(txtLog_KeyPress);
			showUI = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.Space, this);
			
			//this.components = new System.ComponentModel.Container();

			//this.myNotificationIcon = new System.Windows.Forms.NotifyIcon(this.components);
			//this.myNotificationIcon.Click += new EventHandler(myNotificationIcon_Click);
			//myNotificationIcon.Icon = new Icon("Mimetypes-text-x-log.ico");

			//// The Text property sets the text that will be displayed,
			//// in a tooltip, when the mouse hovers over the systray icon.
			//myNotificationIcon.Text = "Show QuickLogger";
			//myNotificationIcon.Visible = true;

		}

		void myNotificationIcon_Click(object sender, EventArgs e)
		{
			HandleHotkey();
		}

		private void HandleHotkey()
		{
			if (this.WindowState == FormWindowState.Normal)
				this.WindowState = FormWindowState.Minimized;
			else
			{
				this.WindowState = FormWindowState.Normal;
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == Hotkeys.Constants.WM_HOTKEY_MSG_ID)
				HandleHotkey();
			base.WndProc(ref m);
		}

		/// <summary>
		/// Loads the apps! 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void formQuick_Load(object sender, EventArgs e)
		{
			//register our global show/hide hotkey
			if (!showUI.Register())
				MessageBox.Show("Failed to register key");

			//Make the UI stretch the width of the primary screen
			this.Width = Screen.PrimaryScreen.WorkingArea.Width;
			txtLog.Width = this.Width - 34;
			txtLog.Left = 17;

			this.KeyPreview = true;
			this.KeyDown += new KeyEventHandler(formQuick_KeyDown);
		}

		/// <summary>
		/// Some special hotkeys for app manipulation since the UI is minimal
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void formQuick_KeyDown(object sender, KeyEventArgs e)
		{
			//Open the directory that holds the log files in explorer
			//(control + d)
			if (e.Control && e.KeyCode.ToString() == "D")
			{
				if (Directory.Exists(LogPath))
				{
					System.Diagnostics.Process.Start("explorer.exe", LogPath);
				}

				hideWindow();
			}
			//open the specific log file currently in use (control + o)
			else if (e.Control && e.KeyCode.ToString() == "O")
			{
				if (File.Exists(fullLogPath(lblCurrentLog.Text)))
				{
					System.Diagnostics.Process.Start(fullLogPath(lblCurrentLog.Text));
				}

				hideWindow();
			}
			//exit the application (control + x)
			else if (e.Control && e.KeyCode.ToString() == "X")
			{
				this.Close();
			}
		}

		/// <summary>
		/// removes the global hot key registration when the app is closed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!showUI.Unregiser())
				MessageBox.Show("Failed to un-register key");
		}

		/// <summary>
		/// handle the tab key special like
		/// </summary>
		/// <param name="keyData"></param>
		/// <returns></returns>
		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Tab) return false;
			
			return base.ProcessDialogKey(keyData);
		}

		/// <summary>
		/// hide the UI, basically minimizing it right now
		/// </summary>
		private void hideWindow()
		{
			txtLog.Text = "";
			this.WindowState = FormWindowState.Minimized;
			//this.ShowInTaskbar = false;
		}

		//returns a consistant log name for the current week in the format
		//year-month-day.log
		private string logFile(string currentLog)
		{
			if(currentLog!= "todo")
				return DateTime.Today.StartOfWeek(DayOfWeek.Monday).ToString("yyyy-MM-dd") + LogExtension;

			return "todo" + LogExtension;
		}

		/// <summary>
		/// returns the full path + filename for the current log file
		/// </summary>
		/// <param name="currentLog"></param>
		/// <returns></returns>
		private string fullLogPath(string currentLog)
		{
			return LogPath + logFile(currentLog);
		}

		/// <summary>
		/// Handles key presses
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void txtLog_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			//enter key
			if (e.KeyChar == (char)13)
			{
				LogData(lblCurrentLog.Text, txtLog.Text);

				if (lblCurrentLog.Text != "todo" && dualLog)
					LogData(lblCurrentLog.Text, txtLog.Text, LogPath, lblCurrentLog.Text + LogExtension);

				//hide the logger UI
				hideWindow();
			}
			//tab key, used to switch categories or to the 
			//'todo' file
			else if (e.KeyChar == '\t')
			{
				e.Handled = true;
				lblCurrentLog.Text = txtLog.Text;
				txtLog.Text = "";
			}//escape key
			else if (e.KeyChar == (char)27)
			{
				//hide the UI
				hideWindow();
			}
		}

		/// <summary>
		/// Log data to a file
		/// </summary>
		/// <param name="category">category to log</param>
		/// <param name="log">string to log</param>
		private void LogData(string category, string log)
		{
			LogData(category, log, LogPath, logFile(lblCurrentLog.Text));
		}

		/// <summary>
		/// Log data to a csv file
		/// </summary>
		/// <param name="category">category</param>
		/// <param name="log">data to log</param>
		/// <param name="path">path to log file</param>
		/// <param name="file">filename</param>
		private void LogData(string category, string log, string path, string file)
		{
			//if the directory does not exist
			//LogPath is a global config variable, later will be loaded from config file
			if (!Directory.Exists(path))
			{
				//create it
				Directory.CreateDirectory(path);
			}

			//if the file does not exist
			//create it
			if (!File.Exists(path + file))
			{
				FileStream myStream = File.Create(path + file);
				myStream.Close();
			}

			//open the file so we can append to it
			StreamWriter myWriter = File.AppendText(path + file);
			//write our line
			//date, category, text to log
			myWriter.WriteLine(DateTime.Now.ToString() + "," + formatToCSV(category) + "," + formatToCSV(log));

			myWriter.Close();
		}

		/// <summary>
		/// Writes a single row to a CSV file.
		/// </summary>
		/// <param name="row">The row to be written</param>
		public string formatToCSV(string data)
		{
			StringBuilder builder = new StringBuilder();
			// Implement special handling for values that contain comma or quote
			// Enclose in quotes and double up any double quotes
			if (data.IndexOfAny(new char[] { '"', ',' }) != -1)
				builder.AppendFormat("\"{0}\"", data.Replace("\"", "\"\""));
			else
				builder.Append(data);

			return builder.ToString();
		}
	}

	public static class DateTimeExtensions
	{
		public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
		{
			return dt.AddDays(-(dt.DayOfWeek - startOfWeek));
		}
	}
}
