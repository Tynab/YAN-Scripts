using AnimatorNS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using YAN_Controls;
using static Microsoft.Win32.Registry;
using static System.BitConverter;
using static System.Data.LoadOption;
using static System.DateTime;
using static System.DateTimeKind;
using static System.DBNull;
using static System.Diagnostics.Process;
using static System.Drawing.Color;
using static System.Drawing.CopyPixelOperation;
using static System.Drawing.FontStyle;
using static System.Drawing.Graphics;
using static System.Drawing.Imaging.ImageFormat;
using static System.Drawing.Imaging.PixelFormat;
using static System.Globalization.CultureInfo;
using static System.Globalization.DateTimeFormatInfo;
using static System.Globalization.DateTimeStyles;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;
using static System.Math;
using static System.Net.Dns;
using static System.Net.Sockets.AddressFamily;
using static System.Net.Sockets.ProtocolType;
using static System.Net.Sockets.SocketType;
using static System.Net.WebRequest;
using static System.Reflection.BindingFlags;
using static System.Threading.Tasks.Parallel;
using static System.TimeSpan;
using static System.Windows.Forms.Clipboard;
using static System.Windows.Forms.DataGridViewAutoSizeColumnsMode;
using static System.Windows.Forms.DataGridViewClipboardCopyMode;
using static System.Windows.Forms.DialogResult;
using static System.Windows.Forms.PictureBoxSizeMode;
using static System.Windows.Forms.Screen;
using static System.Windows.Forms.SystemInformation;
using static YAN_Message_Box.ELang;
using static YAN_Message_Box.YANMessageBox;
using static YAN_Scripts.YANConstant;
using static YAN_Scripts.YANConstant.AlarmAction;

namespace YAN_Scripts
{
    public class YANMethod
    {
        #region Function
        //reset win question
        private static void ResetWinQuest()
        {
            if (MessQuestAdvancedVN("HỎI", "Lỗi ứng dụng chạy ngầm, khởi động lại hệ thống để khắc phục?") == Yes)
            {
                TimerWin(Restart, 3);
            }
        }

        //crop image
        private static Image CropImg(Image image, Rectangle rectangle) => ((Bitmap)image).Clone(rectangle, image.PixelFormat);

        //time trans
        private static uint SwapEndianness(ulong x) => (uint)(((x & 0x000000ff) << 24) + ((x & 0x0000ff00) << 8) + ((x & 0x00ff0000) >> 8) + ((x & 0xff000000) >> 24));

        //check date time online socket
        private static DateTime GetDateTimeOnlSocket()
        {
            var ntpData = new byte[48];
            ntpData[0] = 0x1B;
            using (var socket = new Socket(InterNetwork, Dgram, Udp))
            {
                socket.ReceiveTimeout = _timeout_;
                socket.Connect(new IPEndPoint(GetHostEntry("time.windows.com").AddressList[0], 123));
                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }
            return new DateTime(1900, 1, 1, 0, 0, 0, Utc).AddMilliseconds(SwapEndianness(ToUInt32(ntpData, 40)) * 1000 + SwapEndianness(ToUInt32(ntpData, 44)) * 1000 / 0x100000000L).ToLocalTime();
        }

        //check date time online stream
        private static DateTime GetDateTimeOnlStream()
        {
            var d_now = Now;
            using (var streamReader = new StreamReader(new TcpClient("time.nist.gov", 13).GetStream()))
            {
                if (streamReader != null)
                {
                    d_now = ParseExact(streamReader.ReadToEnd().Substring(7, 17), "yy-MM-dd HH:mm:ss", InvariantCulture, AssumeUniversal);
                }
            }
            return d_now;
        }

        //check app installer in app list
        private static bool CheckAppList(string name, string address)
        {
            var check = false;
            var key = LocalMachine.OpenSubKey(address);
            if (key != null)
            {
                ForEach(key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)), (subkey, state) =>
                {
                    var displayName = (string)subkey.GetValue("DisplayName");
                    if (displayName != null && displayName.Contains(name))
                    {
                        check = true;
                        state.Stop();
                    }
                });
                key.Close();
            }
            return check;
        }
        #endregion

        #region Ellipse Form
        /// <summary>
        /// Mod form round ellipse.
        /// </summary>
        /// <param name="nLeftRect">Left path.</param>
        /// <param name="nTopRect">Top path.</param>
        /// <param name="nRightRect">Right path.</param>
        /// <param name="nBottomRect">Bot path.</param>
        /// <param name="nWidthEllipse">Width path.</param>
        /// <param name="nHeightElippse">Height path.</param>
        /// <returns>Platform specific.</returns>
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightElippse);
        #endregion

        #region Date Time
        /// <summary>
        /// Get week of year.
        /// </summary>
        /// <param name="date">Date count.</param>
        /// <returns>Num of week.</returns>
        public static int GetWeekOfYearUpgrade(DateTime date) => CurrentInfo.Calendar.GetWeekOfYear(date, CurrentInfo.CalendarWeekRule, CurrentInfo.FirstDayOfWeek);

        /// <summary>
        /// Covert string to date time.
        /// </summary>
        /// <param name="d_Text">Date time string format.</param>
        /// <param name="d_Format">Format of string date time.</param>
        /// <returns>Date time.</returns>
        public static bool TryParseExactUpgrade(string d_Text, string d_Format, out DateTime dateTime) => TryParseExact(d_Text, d_Format, InvariantCulture, DateTimeStyles.None, out dateTime);

        /// <summary>
        /// Convert hour and minute text to date time format.
        /// </summary>
        /// <param name="hm">Hour and minute text.</param>
        /// <param name="dateTime">Time format.</param>
        /// <returns>Done or failed.</returns>
        public static bool TryParseTimeFromHM(string hm, out DateTime dateTime) => TryParseExactUpgrade(hm, "HH:mm", out dateTime) || TryParseExactUpgrade(hm, "h:mm", out dateTime) || TryParseExactUpgrade(hm, "HH:m", out dateTime) || TryParseExactUpgrade(hm, "h:m", out dateTime);

        /// <summary>
        /// Convert hour and minute text to time hour.
        /// </summary>
        /// <param name="hm">Hour and minute text.</param>
        /// <returns>Time hour.</returns>
        public static double TimeHFromHM(string hm) => TryParseTimeFromHM(hm, out var tempTime) ? (tempTime - Today).TotalHours : 0;

        /// <summary>
        /// Covert number to hour and minute text.
        /// </summary>
        /// <param name="min">Minutes.</param>
        /// <returns>Hour and minute text.</returns>
        public static string HMFromDouble(double min)
        {
            var timeSpan = FromHours(min);
            return timeSpan.Hours.ToString("00") + ":" + timeSpan.Minutes.ToString("00");
        }

        /// <summary>
        /// Get date time online.
        /// </summary>
        /// <returns>International date time.</returns>
        public static DateTime DateTimeOnlAdvanced()
        {
            var d_now = Now;
            if (CheckInternetConnect())
            {
                try
                {
                    d_now = GetDateTimeOnlSocket();
                }
                catch
                {
                    try
                    {
                        d_now = GetDateTimeOnlStream();
                    }
                    catch (Exception ex)
                    {
                        MessErrorAdvanced("ERROR", ex.ToString());
                    }
                }
            }
            return d_now;
        }
        #endregion

        #region Math
        /// <summary>
        /// Round up span 0.5.
        /// </summary>
        /// <param name="x">Number.</param>
        /// <returns>Rounded number.</returns>
        public static double RoundUpPoint5(double x) => Ceiling(x * 2) / 2;

        /// <summary>
        /// Round down span 0.5.
        /// </summary>
        /// <param name="x">Number</param>
        /// <returns>Rounded number.</returns>
        public static double RoundDownPoint5(double x) => Floor(x * 2) / 2;

        /// <summary>
        /// Find min value.
        /// </summary>
        /// <param name="val">Current value.</param>
        /// <param name="lim">Check value.</param>
        public static void Miner(ref DateTime val, DateTime lim)
        {
            if (val > lim)
            {
                val = lim;
            }
        }
        public static void Miner(ref int val, int lim)
        {
            if (val > lim)
            {
                val = lim;
            }
        }

        /// <summary>
        /// Find max value.
        /// </summary>
        /// <param name="val">Current value.</param>
        /// <param name="lim">Check value.</param>
        public static void Maxer(ref DateTime val, DateTime lim)
        {
            if (val < lim)
            {
                val = lim;
            }
        }
        public static void Maxer(ref int val, int lim)
        {
            if (val < lim)
            {
                val = lim;
            }
        }
        #endregion

        #region Internet
        /// <summary>
        /// Check internet connection.
        /// </summary>
        /// <returns>Connected or failed.</returns>
        public static bool CheckInternetConnect()
        {
            try
            {
                var objWebReq = Create(new Uri("https://www.google.com/"));
                objWebReq.Timeout = _timeout_;
                using (var objResp = objWebReq.GetResponse())
                {
                    objResp.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Data
        /// <summary>
        /// Convert value from database.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="obj">Object value.</param>
        /// <returns>Converted value.</returns>
        public static T ConvertValFromDB<T>(object obj) => obj == null || obj == Value ? default : (T)obj;

        /// <summary>
        /// Datatable search row index.
        /// </summary>
        /// <param name="dt">Datatable source.</param>
        /// <param name="dcHeader">Column header.</param>
        /// <param name="searching">Value to search.</param>
        /// <returns>Index of row datatable.</returns>
        public static int DTSearchRow(DataTable dt, string dcHeader, string searching) => dt.Rows.IndexOf(dt.Select($"{dcHeader} = '{searching}'")[0]);

        /// <summary>
        /// Datatable add new row with default value.
        /// </summary>
        /// <param name="dt">Datatable source.</param>
        public static void DTAddRow(DataTable dt) => dt.Rows.Add(dt.NewRow());

        /// <summary>
        /// Datatable add column at index.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="dt">Datatable source.</param>
        /// <param name="dcName">Name of new column.</param>
        /// <param name="index">Index of new column.</param>
        public static void DTAddColAt<T>(DataTable dt, string dcName, int index) => dt.Columns.Add(dcName, typeof(T)).SetOrdinal(index);

        /// <summary>
        /// Datatable crop column.
        /// </summary>
        /// <param name="dtSource">Datatable form.</param>
        /// <param name="dtCurrent">Datatable crop.</param>
        public static void DTSyncCol(DataTable dtSource, DataTable dtCurrent)
        {
            while (dtCurrent.Columns.Count > dtSource.Columns.Count)
            {
                dtCurrent.Columns.RemoveAt(dtCurrent.Columns.Count - 1);
            }
            while (dtCurrent.Columns.Count < dtSource.Columns.Count)
            {
                DTAddColAt<string>(dtCurrent, dtSource.Columns[dtCurrent.Columns.Count].ColumnName, dtCurrent.Columns.Count);
            }
        }

        /// <summary>
        /// Datatable transfer data.
        /// </summary>
        /// <param name="dtSource">Datatable form.</param>
        /// <param name="dtCurrent">Datatable to.</param>
        public static void DTTransData(DataTable dtSource, DataTable dtCurrent) => dtSource.AsEnumerable().Take(dtSource.Rows.Count).CopyToDataTable(dtCurrent, OverwriteChanges);

        /// <summary>
        /// Datatable transfer data and reverse row.
        /// </summary>
        /// <param name="dtSource">Datatable form.</param>
        /// <param name="dtCurrent">Datatable to.</param>
        public static void DTTransReverseData(DataTable dtSource, DataTable dtCurrent) => dtSource.AsEnumerable().Take(dtSource.Rows.Count).Reverse().CopyToDataTable(dtCurrent, OverwriteChanges);

        /// <summary>
        /// Datatable merge all.
        /// </summary>
        /// <param name="dataTables">All datatables in array.</param>
        /// <param name="dtSource">Datatable to.</param>
        public static void DTAll41(DataTable[] dataTables, DataTable dtSource, int length)
        {
            for (var i = 0; i < length; i++)
            {
                if (dataTables[i] != null)
                {
                    DTTransData(dataTables[i], dtSource);
                }
            }
        }

        /// <summary>
        /// Double Buffered of datagridview.
        /// </summary>
        /// <param name="dgv">DataGridView.</param>
        /// <param name="state">State of setting.</param>
        public static void DoubleBuffered(DataGridView dgv, bool state)
        {
            if (!TerminalServerSession)
            {
                dgv.GetType().GetProperty("DoubleBuffered", Instance | NonPublic).SetValue(dgv, state, null);
            }
        }

        /// <summary>
        /// Copy datatable to clipboard.
        /// </summary>
        /// <param name="dt">Datatable source.</param>
        public static void DT2Clipboard(DataTable dt)
        {
            var form = new Form
            {
                Opacity = 0,
                ShowInTaskbar = false
            };
            var dgv = new DataGridView
            {
                ClipboardCopyMode = EnableWithoutHeaderText,
                DataSource = dt
            };
            form.Controls.Add(dgv);
            form.Show();
            dgv.SelectAll();
            var dobj = dgv.GetClipboardContent().GetText();
            SetText(dobj);
            form.Dispose();
        }

        /// <summary>
        /// Auto size mod datagridview.
        /// </summary>
        /// <param name="dgv">DataGridView.</param>
        /// <param name="state">State of setting.</param>
        public static void DGVAutoSizeMod(DataGridView dgv, bool state) => dgv.AutoSizeColumnsMode = state ? AllCells : DataGridViewAutoSizeColumnsMode.None;
        #endregion

        #region Window
        /// <summary>
        /// Timer window action.
        /// </summary>
        /// <param name="action">Action on window.</param>
        /// /// <param name="sec">Second of countdown.</param>
        public static void TimerWin(AlarmAction action, int sec)
        {
            var cmt = action == ShutDown ? "s" : "r";
            Start("shutdown.exe", $"-{cmt} -t {sec}");
        }

        /// <summary>
        /// Check application installed by name.
        /// </summary>
        /// <param name="name">Name of the application checking.</param>
        /// <returns>Installed or not.</returns>
        public static bool CheckAppInstalled(string name)
        {
            var address = @"Microsoft\Windows\CurrentVersion\Uninstall";
            return CheckAppList(name, $"SOFTWARE\\{address}") || CheckAppList(name, $"SOFTWARE\\Wow6432Node\\{address}");
        }

        /// <summary>
        /// Change color by value.
        /// </summary>
        /// <param name="color">Current color.</param>
        /// <param name="val">Value.</param>
        /// <returns>New color.</returns>
        public static Color ColorUpDown(Color color, int val) => FromArgb((color.R + val) % 256, (color.G + val) % 256, (color.B + val) % 256);

        /// <summary>
        /// Invert color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>New color.</returns>
        public static Color ColorInvert(Color color) => FromArgb(color.ToArgb() ^ 0xffffff);

        /// <summary>
        /// Capitalize each word.
        /// </summary>
        /// <param name="text">Text value.</param>
        /// <returns>Text converted.</returns>
        public static string CapitalizeEachWord(string text) => CurrentCulture.TextInfo.ToTitleCase(text.ToLower());

        /// <summary>
        /// Title case converter.
        /// </summary>
        /// <param name="text">Text value.</param>
        /// <returns>Text converted.</returns>
        public static string ToTitleCaseUpgrade(string text) => CurrentCulture.TextInfo.ToTitleCase(text);
        #endregion

        #region Process
        /// <summary>
        /// Kill process.
        /// </summary>
        /// <param name="name">Name of the application.</param>
        public static void KillProcess(string name)
        {
            if (GetProcessesByName(name).Count() > 0)
            {
                ForEach(GetProcessesByName(name), process => process.Kill());
            }
        }

        /// <summary>
        /// Set label text another thread.
        /// </summary>
        /// <param name="label">Label other thread.</param>
        /// <param name="text">Text set.</param>
        public static void InvokeLabelText(Label label, string text) => label.Invoke((MethodInvoker)(() => label.Text = text));

        /// <summary>
        /// Set panel width another thread.
        /// </summary>
        /// <param name="panel">Panel other thread.</param>
        /// <param name="w">Width of panel.</param>
        public static void InvokeWPanel(Panel panel, int w) => panel.Invoke((MethodInvoker)(() => panel.Width = w));
        #endregion

        #region File
        /// <summary>
        /// Get all file info in folder.
        /// </summary>
        /// <param name="path">Folder path.</param>
        /// <param name="searchText">Searching text.</param>
        /// <returns></returns>
        public static FileInfo[] GetAllFileInFolder(string path, string searchText) => new DirectoryInfo(path).GetFiles(searchText).OrderBy(f => f.Name).ToArray();

        /// <summary>
        /// Delete the file, if the file deleting is open reboot program.
        /// </summary>
        /// <param name="ad">Address of the file.</param>
        public static void DelFileAdvanced(string ad)
        {
            if (File.Exists(ad))
            {
                try
                {
                    File.Delete(ad);
                }
                catch
                {
                    ResetWinQuest();
                }
            }
        }

        /// <summary>
        /// If the file not exists write byte array to the file.
        /// </summary>
        /// <param name="ad">Address of the file.</param>
        /// <param name="bytes">Byte array.</param>
        public static void FileWriteBytesAdvanced(string ad, byte[] bytes)
        {
            if (!File.Exists(ad))
            {
                WriteAllBytes(ad, bytes);
            }
        }

        /// <summary>
        /// Capture current screen and save to jpeg file.
        /// </summary>
        /// <param name="path">Path of folder of jpeg file.</param>
        /// <param name="name">Name of jpeg file.</param>
        /// <returns>Full address of jpeg file.</returns>
        public static string CaptureScreenToFile(string path, string name)
        {
            using (var bmpScreenshot = new Bitmap(PrimaryScreen.Bounds.Width, PrimaryScreen.Bounds.Height, Format32bppArgb))
            {
                using (var gfxScreenshot = FromImage(bmpScreenshot))
                {
                    gfxScreenshot.CopyFromScreen(PrimaryScreen.Bounds.X, PrimaryScreen.Bounds.Y, 0, 0, PrimaryScreen.Bounds.Size, SourceCopy);
                    var ad = $"{path}\\{name}.jpg";
                    bmpScreenshot.Save(ad, Jpeg);
                    return ad;
                }
            }
        }
        #endregion

        #region Directory
        /// <summary>
        /// If the folder not exists create the new folder.
        /// </summary>
        /// <param name="path">Path of the folder.</param>
        public static void CreateFolderAdvanced(string path)
        {
            if (!Directory.Exists(path))
            {
                CreateDirectory(path);
            }
        }

        /// <summary>
        /// If the folder exists delete the folder.
        /// </summary>
        /// <param name="path">Path of the folder.</param>
        public static void DelFolderAdvanced(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Delete(path, true);
                }
                catch
                {
                    ResetWinQuest();
                }
            }
        }

        /// <summary>
        /// If the folder exists delete the folder and create the new folder else create the new folder.
        /// </summary>
        /// <param name="path">Path of the folder.</param>
        public static void ReCreateFolder(string path)
        {
            DelFolderAdvanced(path);
            CreateDirectory(path);
        }

        /// <summary>
        /// Delete all folder with header name.
        /// </summary>
        /// <param name="path">Path of the folder.</param>
        /// <param name="name">Header name of folders.</param>
        public static void DelAllFolderByName(string path, string name) => ForEach(GetDirectories(path, $"{name}*"), folder => DelFolderAdvanced(folder));

        /// <summary>
        /// Copy folder.
        /// </summary>
        /// <param name="sourceDirName">Folder source.</param>
        /// <param name="destDirName">New address copy to.</param>
        /// <param name="copySubDirs">With sub folder or not.</param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            CreateFolderAdvanced(destDirName);
            ForEach(new DirectoryInfo(sourceDirName).GetFiles(), file => file.CopyTo(Combine(destDirName, file.Name), false));
            if (copySubDirs)
            {
                ForEach(new DirectoryInfo(sourceDirName).GetDirectories(), subdir => DirectoryCopy(subdir.FullName, Combine(destDirName, subdir.Name), copySubDirs));
            }
        }
        #endregion

        #region Console
        //fields
        private const byte SW_HIDE = 0;
        private const byte SW_SHOW = 5;

        //get window
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        //show window
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Hide the console window.
        /// </summary>
        public static void HideConsole() => ShowWindow(GetConsoleWindow(), SW_HIDE);

        /// <summary>
        /// Show the console window.
        /// </summary>
        public static void ShowConsole() => ShowWindow(GetConsoleWindow(), SW_SHOW);
        #endregion

        #region Form
        /// <summary>
        /// Fade in the form when show.
        /// </summary>
        /// <param name="form">The form showing.</param>
        public static void FadeInForm(Form form)
        {
            while (form.Opacity < 1)
            {
                form.Opacity += 0.05;
                form.Update();
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Fade out the form when hide.
        /// </summary>
        /// <param name="form">The form hiding.</param>
        public static void FadeOutForm(Form form)
        {
            while (form.Opacity > 0)
            {
                form.Opacity -= 0.05;
                form.Update();
                Thread.Sleep(10);
            }
        }
        #endregion

        #region Object
        /// <summary>
        /// Get all objects of type in control.
        /// </summary>
        /// <param name="control">The control finding objects.</param>
        /// <param name="type">Type of object finding.</param>
        /// <returns>Control list.</returns>
        public static IEnumerable<Control> GetAllObject(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();
            return controls.SelectMany(ctrl => GetAllObject(ctrl, type)).Concat(controls).Where(c => c.GetType() == type);
        }

        /// <summary>
        /// Find main panel.
        /// </summary>
        /// <param name="control">The control focus.</param>
        /// <param name="namePanel">Name of the panel.</param>
        /// <returns>Main panel.</returns>
        public static Panel FindMainPanel(Control control, string namePanel)
        {
            var panel = (Panel)control.Parent;
            return panel.Name != namePanel ? FindMainPanel(panel, namePanel) : panel;
        }

        /// <summary>
        /// Fill item list of combo box by all file in folder.
        /// </summary>
        /// <param name="comboBox">The combo box fill item list.</param>
        /// <param name="path">Path of folder.</param>
        public static void ComboBoxFillByFolder(YANComboBox comboBox, string path)
        {
            comboBox.Items.Clear();
            foreach (var file in GetFiles(path)) //need sort
            {
                comboBox.Items.Add(GetFileNameWithoutExtension(file));
            }
        }
        #endregion

        #region Control
        /// <summary>
        /// Uniform horizontal scale image to picture box.
        /// </summary>
        /// <param name="pictureBox">The picture box used display image.</param>
        public static void PictureBoxZoomWidth(PictureBox pictureBox)
        {
            pictureBox.SizeMode = Zoom;
            pictureBox.Image = CropImg(pictureBox.Image, new Rectangle(0, 0, pictureBox.Image.Width, pictureBox.Image.Width));
        }

        /// <summary>
        /// Uniform vertical scale image to picture box.
        /// </summary>
        /// <param name="pictureBox">The picture box used display image.</param>
        public static void PictureBoxZoomHeight(PictureBox pictureBox)
        {
            pictureBox.SizeMode = Zoom;
            pictureBox.Image = CropImg(pictureBox.Image, new Rectangle(0, 0, pictureBox.Image.Height, pictureBox.Image.Height));
        }

        /// <summary>
        /// Check and start timer.
        /// </summary>
        /// <param name="timer">Timer checking.</param>
        public static void TimerStartAdvanced(System.Windows.Forms.Timer timer)
        {
            if (!timer.Enabled)
            {
                timer.Start();
            }
        }

        /// <summary>
        /// Check and end timer.
        /// </summary>
        /// <param name="timer">Timer checking.</param>
        public static void TimerEndAdvanced(System.Windows.Forms.Timer timer)
        {
            if (timer.Enabled)
            {
                timer.Stop();
            }
        }

        /// <summary>
        /// Highlight state of title text when focus content control.
        /// </summary>
        /// <param name="control">Content control.</param>
        /// <param name="nameControl">Common name of the control.</param>
        /// <param name="color">Color of title text.</param>
        /// <param name="isBold">Format bold or regular.</param>
        public static void SetHighLightLabel(Control control, string nameControl, Color color, bool isBold)
        {
            var label = (Label)control.FindForm().Controls.Find($"label{control.Name.Substring(nameControl.Length)}", true).FirstOrDefault();
            label.ForeColor = color;
            label.Font = isBold ? new Font(label.Font, Bold) : new Font(label.Font, Regular);
        }
        #endregion

        #region Message Box
        /// <summary>
        /// Show the message box none freedom text.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessNoneAdvanced(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OK, MessageBoxIcon.None);

        /// <summary>
        /// Show the message box infomation freedom text.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessInfoAdvanced(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);

        /// <summary>
        /// Show the message box question freedom text.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessQuestAdvanced(string cap, string mess) => Show(mess, cap, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        /// <summary>
        /// Show the message box warning freedom text.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessWarningAdvanced(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

        /// <summary>
        /// Show the message box error freedom text.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessErrorAdvanced(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);

        /// <summary>
        /// Show the message box none freedom text Japanese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessNoneAdvancedJP(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OK, MessageBoxIcon.None, JAP);

        /// <summary>
        /// Show the message box infomation freedom text Japanese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessInfoAdvancedJP(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OK, MessageBoxIcon.Information, JAP);

        /// <summary>
        /// Show the message box question freedom text Japanese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessQuestAdvancedJP(string cap, string mess) => Show(mess, cap, MessageBoxButtons.YesNo, MessageBoxIcon.Question, JAP);

        /// <summary>
        /// Show the message box warning freedom text Japanese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessWarningAdvancedJP(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, JAP);

        /// <summary>
        /// Show the message box error freedom text Japanese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessErrorAdvancedJP(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OK, MessageBoxIcon.Error, JAP);

        /// <summary>
        /// Show the message box none freedom text Vietnamese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessNoneAdvancedVN(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OK, MessageBoxIcon.None, VIE);

        /// <summary>
        /// Show the message box infomation freedom text Vietnamese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessInfoAdvancedVN(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OK, MessageBoxIcon.Information, VIE);

        /// <summary>
        /// Show the message box question freedom text Vietnamese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessQuestAdvancedVN(string cap, string mess) => Show(mess, cap, MessageBoxButtons.YesNo, MessageBoxIcon.Question, VIE);

        /// <summary>
        /// Show the message box warning freedom text Vietnamese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessWarningAdvancedVN(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, VIE);

        /// <summary>
        /// Show the message box error freedom text Vietnamese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MessErrorAdvancedVN(string cap, string mess) => Show(mess, cap, MessageBoxButtons.OK, MessageBoxIcon.Error, VIE);
        #endregion

        #region Animator
        /// <summary>
        /// Show the control with sync effect.
        /// </summary>
        /// <param name="control">The showing control.</param>
        /// <param name="animationType">Effect type.</param>
        /// <param name="speed">FPMS.</param>
        public static void ShowAnimatorSync(Control control, AnimationType animationType, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = animationType
            };
            animator.ShowSync(control);
        }

        /// <summary>
        /// Hide the control with sync effect.
        /// </summary>
        /// <param name="control">The hiding control.</param>
        /// <param name="animationType">Effect type.</param>
        /// <param name="speed">FPMS.</param>
        public static void HideAnimatorSync(Control control, AnimationType animationType, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = animationType
            };
            animator.HideSync(control);
        }

        /// <summary>
        /// Show the control with async effect.
        /// </summary>
        /// <param name="control">The showing control.</param>
        /// <param name="animationType">Effect type.</param>
        /// <param name="speed">FPMS.</param>
        public static void ShowAnimator(Control control, AnimationType animationType, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = animationType
            };
            animator.Show(control);
        }

        /// <summary>
        /// Hide the control with async effect.
        /// </summary>
        /// <param name="control">The hiding control.</param>
        /// <param name="animationType">Effect type.</param>
        /// <param name="speed">FPMS.</param>
        public static void HideAnimator(Control control, AnimationType animationType, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = animationType
            };
            animator.Hide(control);
        }
        #endregion

        #region Animate
        /// <summary>
        /// Low animation sync.
        /// </summary>
        /// <param name="hwand">Control.</param>
        /// <param name="dwTime">Timer.</param>
        /// <param name="dwFlags">Animation type.</param>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern void AnimateWindow(IntPtr hwand, int dwTime, AnimateWindowFlags dwFlags);
        #endregion
    }
}
