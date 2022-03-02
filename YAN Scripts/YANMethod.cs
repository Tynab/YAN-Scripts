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
using System.Text.RegularExpressions;
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
using static System.Linq.Enumerable;
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
using static System.Windows.Forms.Cursor;
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
using Timer = System.Windows.Forms.Timer;

namespace YAN_Scripts
{
    public static class YANMethod
    {
        #region Function
        //chunked
        private static IEnumerable<string> Chunked(this string text, int chunkSize) => Range(0, text.Length / chunkSize).Select(i => text.Substring(i * chunkSize, chunkSize));

        //zero hunderd
        private static bool ShouldShowZeroHundred(this string[] groups) => groups.Reverse().TakeWhile(it => it == "000").Count() < groups.Count() - 1;

        //deconstruct
        internal static void Deconstruct<T>(this IReadOnlyList<T> items, out T t0, out T t1, out T t2)
        {
            t0 = items.Count > 0 ? items[0] : default;
            t1 = items.Count > 1 ? items[1] : default;
            t2 = items.Count > 2 ? items[2] : default;
        }

        //read pair
        private static string ReadPair(int b, int c)
        {
            return b switch
            {
                0 => c == 0 ? "" : " lẻ " + Digits[c],
                1 => "mười " + c switch
                {
                    0 => "",
                    5 => "lăm",
                    _ => Digits[c]
                },
                _ => Digits[b] + " mươi " + c switch
                {
                    0 => "",
                    1 => "mốt",
                    4 => "tư",
                    5 => "lăm",
                    _ => Digits[c]
                }
            };
        }

        //read triple
        private static string ReadTriple(string triple, bool showZeroHundred)
        {
            var (a, b, c) = triple.Select(ch => int.Parse(ch.ToString())).ToArray();
            return a switch
            {
                0 when b == 0 && c == 0 => "",
                0 when showZeroHundred => "không trăm " + ReadPair(b, c),
                0 when b == 0 => Digits[c],
                0 => ReadPair(b, c),
                _ => Digits[a] + " trăm " + ReadPair(b, c)
            };
        }

        //capitalize
        private static string Capitalize(this string input)
        {
            return input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1).ToLower()
            };
        }

        //reset win question
        private static void ResetWinQuest()
        {
            if (MsgboxQuestAdvancedVN("HỎI", "Lỗi ứng dụng chạy ngầm, khởi động lại hệ thống để khắc phục?") == Yes)
            {
                AlarmWin(Restart, 3);
            }
        }

        //crop image
        private static Image CropImg(Image img, Rectangle rect) => ((Bitmap)img).Clone(rect, img.PixelFormat);

        //time trans
        private static uint SwapEndianness(ulong x) => (uint)(((x & 0x000000ff) << 24) + ((x & 0x0000ff00) << 8) + ((x & 0x00ff0000) >> 8) + ((x & 0xff000000) >> 24));

        //check date time online socket
        private static DateTime GetDtmOnlSocket()
        {
            var ntpData = new byte[48];
            ntpData[0] = 0x1B;
            using (var socket = new Socket(InterNetwork, Dgram, Udp))
            {
                socket.ReceiveTimeout = _timeOut_;
                socket.Connect(new IPEndPoint(GetHostEntry("time.windows.com").AddressList[0], 123));
                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }
            return new DateTime(1900, 1, 1, 0, 0, 0, Utc).AddMilliseconds(SwapEndianness(ToUInt32(ntpData, 40)) * 1000 + SwapEndianness(ToUInt32(ntpData, 44)) * 1000 / 0x100000000L).ToLocalTime();
        }

        //check date time online stream
        private static DateTime GetDtmOnlStream()
        {
            var dtm = Now;
            using (var streamReader = new StreamReader(new TcpClient("time.nist.gov", 13).GetStream()))
            {
                if (streamReader != null)
                {
                    dtm = ParseExact(streamReader.ReadToEnd().Substring(7, 17), "yy-MM-dd HH:mm:ss", InvariantCulture, AssumeUniversal);
                }
            }
            return dtm;
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
        /// <param name="nLRect">Left path.</param>
        /// <param name="nTRect">Top path.</param>
        /// <param name="nRRect">Right path.</param>
        /// <param name="nBRect">Bot path.</param>
        /// <param name="nWEllipse">Width path.</param>
        /// <param name="nHElippse">Height path.</param>
        /// <returns>Platform specific.</returns>
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(int nLRect, int nTRect, int nRRect, int nBRect, int nWEllipse, int nHElippse);
        #endregion

        #region Date Time
        /// <summary>
        /// Get week of year.
        /// </summary>
        /// <param name="dtm">Date count.</param>
        /// <returns>Num of week.</returns>
        public static int GetWoyUpgrade(DateTime dtm) => CurrentInfo.Calendar.GetWeekOfYear(dtm, CurrentInfo.CalendarWeekRule, CurrentInfo.FirstDayOfWeek);

        /// <summary>
        /// Covert string to date time.
        /// </summary>
        /// <param name="dtmText">Date time string format.</param>
        /// <param name="dtmFormat">Format of string date time.</param>
        /// <returns>Done or failed.</returns>
        public static bool DtmTryParseExact(string dtmText, string dtmFormat, out DateTime dtm) => TryParseExact(dtmText, dtmFormat, InvariantCulture, DateTimeStyles.None, out dtm);

        /// <summary>
        /// Convert hour and minute text to date time format.
        /// </summary>
        /// <param name="hm">Hour and minute text.</param>
        /// <param name="dtm">Time format.</param>
        /// <returns>Done or failed.</returns>
        public static bool DtmTryParseFromHm(string hm, out DateTime dtm) => DtmTryParseExact(hm, "HH:mm", out dtm) || DtmTryParseExact(hm, "h:mm", out dtm) || DtmTryParseExact(hm, "HH:m", out dtm) || DtmTryParseExact(hm, "h:m", out dtm);

        /// <summary>
        /// Convert hour and minute text to time hour.
        /// </summary>
        /// <param name="hm">Hour and minute text.</param>
        /// <returns>Time hour.</returns>
        public static double HourParseFromHm(string hm) => DtmTryParseFromHm(hm, out var dtm) ? (dtm - Today).TotalHours : 0;

        /// <summary>
        /// Covert number to hour and minute text.
        /// </summary>
        /// <param name="min">Minutes.</param>
        /// <returns>Hour and minute text.</returns>
        public static string HmParseFromMinute(double min)
        {
            var timeSpan = FromHours(min);
            return timeSpan.Hours.ToString("00") + ":" + timeSpan.Minutes.ToString("00");
        }

        /// <summary>
        /// Get date time online.
        /// </summary>
        /// <returns>International date time.</returns>
        public static DateTime DtmOnlAdvanced()
        {
            var dtm = Now;
            if (CheckInternetConnect())
            {
                try
                {
                    dtm = GetDtmOnlSocket();
                }
                catch
                {
                    try
                    {
                        dtm = GetDtmOnlStream();
                    }
                    catch (Exception ex)
                    {
                        MsgboxErrorAdvanced("ERROR", ex.ToString());
                    }
                }
            }
            return dtm;
        }

        /// <summary>
        /// Total month between 2 date.
        /// </summary>
        /// <param name="dtmStart">Start date.</param>
        /// <param name="dtmEnd">End date.</param>
        /// <returns></returns>
        public static int DtmTotalMonth2(DateTime dtmStart, DateTime dtmEnd) => (dtmEnd.Year - dtmStart.Year) * 12 + dtmEnd.Month - dtmStart.Month;
        #endregion

        #region Math
        /// <summary>
        /// Round up span 0.5.
        /// </summary>
        /// <param name="val">Number.</param>
        /// <returns>Rounded number.</returns>
        public static double RoundUpPoint5(double val) => Ceiling(val * 2) / 2;

        /// <summary>
        /// Round down span 0.5.
        /// </summary>
        /// <param name="val">Number.</param>
        /// <returns>Rounded number.</returns>
        public static double RoundDownPoint5(double val) => Floor(val * 2) / 2;

        /// <summary>
        /// Convert string to double.
        /// </summary>
        /// <param name="num">Number text.</param>
        /// <returns>Number double.</returns>
        public static double DoubleParse(string num)
        {
            double.TryParse(num, out var val);
            return val;
        }

        /// <summary>
        /// Convert string to int.
        /// </summary>
        /// <param name="num">Number text.</param>
        /// <returns>Number int.</returns>
        public static int IntParse(string num)
        {
            int.TryParse(num, out var val);
            return val;
        }

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
                objWebReq.Timeout = _timeOut_;
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
        public static T ValFromDb<T>(object obj) => obj == null || obj == Value ? default : (T)obj;

        /// <summary>
        /// Datatable search row index.
        /// </summary>
        /// <param name="dt">Datatable source.</param>
        /// <param name="dcName">Name of column</param>
        /// <param name="searchText">Value to search.</param>
        /// <returns>Index of row datatable.</returns>
        public static int DtSearchRow(DataTable dt, string dcName, string searchText) => dt.Rows.IndexOf(dt.Select($"{dcName} = '{searchText}'")[0]);

        /// <summary>
        /// Datatable numeric column sort.
        /// </summary>
        /// <param name="dt">Datatable source.</param>
        /// <param name="dcName">Name of column</param>
        /// <returns>Datatable sorted.</returns>
        public static DataTable DtSortNumCol(DataTable dt, string dcName) => dt.AsEnumerable().OrderBy(x => int.Parse(x[dcName].ToString())).Select(x => x).CopyToDataTable();

        /// <summary>
        /// Datatable add new row with default value.
        /// </summary>
        /// <param name="dt">Datatable source.</param>
        public static void DtAddRow(DataTable dt) => dt.Rows.Add(dt.NewRow());

        /// <summary>
        /// Datatable add column at index.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="dt">Datatable source.</param>
        /// <param name="dcName">Name of new column.</param>
        /// <param name="index">Index of new column.</param>
        public static void DtAddColAt<T>(DataTable dt, string dcName, int index) => dt.Columns.Add(dcName, typeof(T)).SetOrdinal(index);

        /// <summary>
        /// Datatable crop column.
        /// </summary>
        /// <param name="dtSrc">Datatable form.</param>
        /// <param name="dt">Datatable current.</param>
        public static void DtSyncCol(DataTable dtSrc, DataTable dt)
        {
            while (dt.Columns.Count > dtSrc.Columns.Count)
            {
                dt.Columns.RemoveAt(dt.Columns.Count - 1);
            }
            while (dt.Columns.Count < dtSrc.Columns.Count)
            {
                DtAddColAt<string>(dt, dtSrc.Columns[dt.Columns.Count].ColumnName, dt.Columns.Count);
            }
        }

        /// <summary>
        /// Datatable transfer data.
        /// </summary>
        /// <param name="dtSrc">Datatable from.</param>
        /// <param name="dt">Datatable to.</param>
        public static void DtTransData(DataTable dtSrc, DataTable dt) => dtSrc.AsEnumerable().Take(dtSrc.Rows.Count).CopyToDataTable(dt, OverwriteChanges);

        /// <summary>
        /// Datatable transfer data and reverse row.
        /// </summary>
        /// <param name="dtSrc">Datatable from.</param>
        /// <param name="dt">Datatable to.</param>
        public static void DtTransReverseData(DataTable dtSrc, DataTable dt) => dtSrc.AsEnumerable().Take(dtSrc.Rows.Count).Reverse().CopyToDataTable(dt, OverwriteChanges);

        /// <summary>
        /// Copy datatable to clipboard.
        /// </summary>
        /// <param name="dt">Datatable source.</param>
        public static void Dt2Clipboard(DataTable dt)
        {
            using (var frm = new Form
            {
                Opacity = 0,
                ShowInTaskbar = false
            })
            {
                var dgv = new DataGridView
                {
                    ClipboardCopyMode = EnableWithoutHeaderText,
                    DataSource = dt
                };
                frm.Controls.Add(dgv);
                frm.Show();
                dgv.SelectAll();
                var ods = dgv.GetClipboardContent().GetText();
                SetText(ods);
            }
        }

        /// <summary>
        /// Datatable merge all.
        /// </summary>
        /// <param name="dts">All datatables in array.</param>
        /// <param name="dt">Datatable to.</param>
        /// <param name="length">Length of array.</param>
        public static void DtAll41(DataTable[] dts, DataTable dt, int length)
        {
            for (var i = 0; i < length; i++)
            {
                if (dts[i] != null)
                {
                    DtTransData(dts[i], dt);
                }
            }
        }

        /// <summary>
        /// Convert datagridview to data table.
        /// </summary>
        /// <param name="dgv">Datagridview.</param>
        /// <returns>Datatable</returns>
        public static DataTable Dgv2Dt(DataGridView dgv)
        {
            var dt = new DataTable();
            foreach (DataGridViewColumn dgvc in dgv.Columns)
            {
                dt.Columns.Add(dgvc.Name);
            }
            foreach (DataGridViewRow dgvr in dgv.Rows)
            {
                var dr = dt.NewRow();
                foreach (DataGridViewCell cell in dgvr.Cells)
                {
                    dr[cell.ColumnIndex] = cell.Value;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// Double Buffered of datagridview.
        /// </summary>
        /// <param name="dgv">DataGridView.</param>
        /// <param name="state">State of setting.</param>
        public static void DgvDubBuffered(DataGridView dgv, bool state)
        {
            if (!TerminalServerSession)
            {
                dgv.GetType().GetProperty("DoubleBuffered", Instance | NonPublic).SetValue(dgv, state, null);
            }
        }

        /// <summary>
        /// Auto size mod datagridview.
        /// </summary>
        /// <param name="dgv">DataGridView.</param>
        /// <param name="state">State of setting.</param>
        public static void DgvAutoSizeMod(DataGridView dgv, bool state) => dgv.AutoSizeColumnsMode = state ? AllCells : DataGridViewAutoSizeColumnsMode.None;

        /// <summary>
        /// Reboot no. datagridview columns lock source data.
        /// </summary>
        /// <param name="dgv">Datagridview.</param>
        public static void DgvSrcDataRebootNo(DataGridView dgv)
        {
            foreach (DataGridViewRow dgvr in dgv.Rows)
            {
                dgvr.Cells[0].Value = dgvr.Index + 1;
            }
        }

        /// <summary>
        /// Reboot no. datagridview columns free data.
        /// </summary>
        /// <param name="dgv">Datagridview.</param>
        public static void DgvFreeDataRebootNo(DataGridView dgv)
        {
            for (var i = 0; i < dgv.RowCount - 1; i++)
            {
                dgv.Rows[i].Cells[0].Value = i + 1;
            }
        }
        #endregion

        #region Window
        /// <summary>
        /// Timer window action.
        /// </summary>
        /// <param name="act">Action on window.</param>
        /// /// <param name="sec">Second of countdown.</param>
        public static void AlarmWin(AlarmAction act, int sec)
        {
            var cmt = act == ShutDown ? "s" : "r";
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
        /// <param name="cl">Current color.</param>
        /// <param name="val">Value.</param>
        /// <returns>New color.</returns>
        public static Color ClUpDown(Color cl, int val) => FromArgb((cl.R + val) % 256, (cl.G + val) % 256, (cl.B + val) % 256);

        /// <summary>
        /// Invert color.
        /// </summary>
        /// <param name="cl">The color.</param>
        /// <returns>New color.</returns>
        public static Color ClInvert(Color cl) => FromArgb(cl.ToArgb() ^ 0xffffff);

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

        /// <summary>
        /// Number to Vietnamese words.
        /// </summary>
        /// <param name="num">Number translate.</param>
        /// <returns>Translate text.</returns>
        public static string ToVietnameseWords(this int num)
        {
            if (num == 0)
            {
                return "Không";
            }
            if (num < 0)
            {
                return "Âm " + (-num).ToVietnameseWords().ToLower();
            }
            var str = num.ToString();
            var groups = (ZeroLeftPadding[str.Length % 3] + str).Chunked(3).ToArray();
            var index = -1;
            var rawResult = groups.Aggregate("", (acc, e) =>
            {
                checked
                {
                    index++;
                }
                var readTriple = ReadTriple(e, groups.ShouldShowZeroHundred() && index > 0);
                var multipleThousand = string.IsNullOrWhiteSpace(readTriple) ? "" : (MultipleThousand.ElementAtOrDefault(groups.Length - 1 - index) ?? "");
                return $"{acc} {readTriple} {multipleThousand} ";
            });
            return Regex.Replace(rawResult, "\\s+", " ").Trim().Capitalize(); //replace white space with a specified character
        }
        #endregion

        #region Process
        /// <summary>
        /// Kill process.
        /// </summary>
        /// <param name="name">Name of the application.</param>
        public static void KillPrc(string name)
        {
            if (GetProcessesByName(name).Count() > 0)
            {
                ForEach(GetProcessesByName(name), prc => prc.Kill());
            }
        }

        /// <summary>
        /// Set label text another thread.
        /// </summary>
        /// <param name="lbl">Label other thread.</param>
        /// <param name="text">Text set.</param>
        public static void InvokeLblText(Label lbl, string text) => lbl.Invoke((MethodInvoker)(() => lbl.Text = text));

        /// <summary>
        /// Set panel width another thread.
        /// </summary>
        /// <param name="pnl">Panel other thread.</param>
        /// <param name="wPnl">Width of panel.</param>
        public static void InvokeWPnl(Panel pnl, int wPnl) => pnl.Invoke((MethodInvoker)(() => pnl.Width = wPnl));
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
        /// <param name="srcDirName">Folder source.</param>
        /// <param name="destDirName">New address copy to.</param>
        /// <param name="copySubDirs">With sub folder or not.</param>
        public static void DirectoryCopy(string srcDirName, string destDirName, bool copySubDirs)
        {
            CreateFolderAdvanced(destDirName);
            ForEach(new DirectoryInfo(srcDirName).GetFiles(), file => file.CopyTo(Combine(destDirName, file.Name), false));
            if (copySubDirs)
            {
                ForEach(new DirectoryInfo(srcDirName).GetDirectories(), subdir => DirectoryCopy(subdir.FullName, Combine(destDirName, subdir.Name), copySubDirs));
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
        /// <param name="frm">The form showing.</param>
        public static void FadeInFrm(Form frm)
        {
            while (frm.Opacity < 1)
            {
                frm.Opacity += 0.05;
                frm.Update();
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Fade out the form when hide.
        /// </summary>
        /// <param name="frm">The form hiding.</param>
        public static void FadeOutFrm(Form frm)
        {
            while (frm.Opacity > 0)
            {
                frm.Opacity -= 0.05;
                frm.Update();
                Thread.Sleep(10);
            }
        }
        #endregion

        #region Object
        /// <summary>
        /// Get all objects of type in control.
        /// </summary>
        /// <param name="ctrl">The control finding objects.</param>
        /// <param name="type">Type of object finding.</param>
        /// <returns>Control list.</returns>
        public static IEnumerable<Control> GetAllObj(Control ctrl, Type type)
        {
            var ctrls = ctrl.Controls.Cast<Control>();
            return ctrls.SelectMany(obj => GetAllObj(obj, type)).Concat(ctrls).Where(c => c.GetType() == type);
        }

        /// <summary>
        /// Find main panel.
        /// </summary>
        /// <param name="ctrl">The control focus.</param>
        /// <param name="namePnl">Name of the panel.</param>
        /// <returns>Main panel.</returns>
        public static Panel FindMainPnl(Control ctrl, string namePnl)
        {
            var pnl = (Panel)ctrl.Parent;
            return pnl.Name != namePnl ? FindMainPnl(pnl, namePnl) : pnl;
        }

        /// <summary>
        /// Fill item list of combo box by all file in folder.
        /// </summary>
        /// <param name="cmb">The combo box fill item list.</param>
        /// <param name="path">Path of folder.</param>
        public static void CombFillByFolder(YANComboBox cmb, string path)
        {
            cmb.Items.Clear();
            foreach (var file in GetFiles(path)) //need sort
            {
                cmb.Items.Add(GetFileNameWithoutExtension(file));
            }
        }
        #endregion

        #region Control
        /// <summary>
        /// Uniform horizontal scale image to picture box.
        /// </summary>
        /// <param name="pic">The picture box used display image.</param>
        public static void PicWZoom(PictureBox pic)
        {
            pic.SizeMode = Zoom;
            pic.Image = CropImg(pic.Image, new Rectangle(0, 0, pic.Image.Width, pic.Image.Width));
        }

        /// <summary>
        /// Uniform vertical scale image to picture box.
        /// </summary>
        /// <param name="pic">The picture box used display image.</param>
        public static void PicHZoom(PictureBox pic)
        {
            pic.SizeMode = Zoom;
            pic.Image = CropImg(pic.Image, new Rectangle(0, 0, pic.Image.Height, pic.Image.Height));
        }

        /// <summary>
        /// Check and start timer.
        /// </summary>
        /// <param name="tmr">Timer checking.</param>
        public static void TmrStartAdvanced(Timer tmr)
        {
            if (!tmr.Enabled)
            {
                tmr.Start();
            }
        }

        /// <summary>
        /// Check and end timer.
        /// </summary>
        /// <param name="tmr">Timer checking.</param>
        public static void TmrEndAdvanced(Timer tmr)
        {
            if (tmr.Enabled)
            {
                tmr.Stop();
            }
        }

        /// <summary>
        /// Highlight state of title text when focus content control.
        /// </summary>
        /// <param name="ctrl">Content control.</param>
        /// <param name="nameCtrl">Common name of the control.</param>
        /// <param name="cl">Color of title text.</param>
        /// <param name="isBold">Format bold or regular.</param>
        public static void SetHighLightLbl(Control ctrl, string nameCtrl, Color cl, bool isBold)
        {
            var lbl = (Label)ctrl.FindForm().Controls.Find($"label{ctrl.Name.Substring(nameCtrl.Length)}", true).FirstOrDefault();
            lbl.ForeColor = cl;
            lbl.Font = isBold ? new Font(lbl.Font, Bold) : new Font(lbl.Font, Regular);
        }
        #endregion

        #region MessageBox
        /// <summary>
        /// Show the message box error catch.
        /// </summary>
        /// <param name="ex">Exception.</param>
        public static void MsgBoxErrorCatch(string ex)
        {
            MessageBox.Show(new Form
            {
                StartPosition = FormStartPosition.Manual,
                Location = new Point(Position.X, Position.Y),
                TopMost = true
            }, ex.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Show the message box none freedom text.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="msg">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxNoneAdvanced(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.None);

        /// <summary>
        /// Show the message box infomation freedom text.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="msg">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxInfoAdvanced(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);

        /// <summary>
        /// Show the message box question freedom text.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="msg">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxQuestAdvanced(string cap, string msg) => Show(msg, cap, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        /// <summary>
        /// Show the message box warning freedom text.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxWarningAdvanced(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

        /// <summary>
        /// Show the message box exclamation freedom text.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxExclamationAdvanced(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        /// <summary>
        /// Show the message box error freedom text.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxErrorAdvanced(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);

        /// <summary>
        /// Show the message box none freedom text Japanese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxNoneAdvancedJP(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.None, JAP);

        /// <summary>
        /// Show the message box infomation freedom text Japanese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxInfoAdvancedJP(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information, JAP);

        /// <summary>
        /// Show the message box question freedom text Japanese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxQuestAdvancedJP(string cap, string msg) => Show(msg, cap, MessageBoxButtons.YesNo, MessageBoxIcon.Question, JAP);

        /// <summary>
        /// Show the message box warning freedom text Japanese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxWarningAdvancedJP(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, JAP);

        /// <summary>
        /// Show the message box exclamation freedom text Japanese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxExclamationAdvancedJP(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, JAP);

        /// <summary>
        /// Show the message box error freedom text Japanese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxErrorAdvancedJP(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error, JAP);

        /// <summary>
        /// Show the message box none freedom text Vietnamese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxNoneAdvancedVN(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.None, VIE);

        /// <summary>
        /// Show the message box infomation freedom text Vietnamese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxInfoAdvancedVN(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information, VIE);

        /// <summary>
        /// Show the message box question freedom text Vietnamese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxQuestAdvancedVN(string cap, string msg) => Show(msg, cap, MessageBoxButtons.YesNo, MessageBoxIcon.Question, VIE);

        /// <summary>
        /// Show the message box warning freedom text Vietnamese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxWarningAdvancedVN(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, VIE);

        /// <summary>
        /// Show the message box exclamation freedom text Vietnamese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxExclamationAdvancedVN(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, VIE);

        /// <summary>
        /// Show the message box error freedom text Vietnamese.
        /// </summary>
        /// <param name="cap">Caption of message.</param>
        /// <param name="mess">Text content.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxErrorAdvancedVN(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error, VIE);
        #endregion

        #region Animator
        /// <summary>
        /// Show the control with sync effect.
        /// </summary>
        /// <param name="ctrl">The showing control.</param>
        /// <param name="animationType">Effect type.</param>
        /// <param name="speed">FPMS.</param>
        public static void ShowAnimatorSync(Control ctrl, AnimationType animationType, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = animationType
            };
            animator.ShowSync(ctrl);
        }

        /// <summary>
        /// Hide the control with sync effect.
        /// </summary>
        /// <param name="ctrl">The hiding control.</param>
        /// <param name="animationType">Effect type.</param>
        /// <param name="speed">FPMS.</param>
        public static void HideAnimatorSync(Control ctrl, AnimationType animationType, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = animationType
            };
            animator.HideSync(ctrl);
        }

        /// <summary>
        /// Show the control with async effect.
        /// </summary>
        /// <param name="ctrl">The showing control.</param>
        /// <param name="animationType">Effect type.</param>
        /// <param name="speed">FPMS.</param>
        public static void ShowAnimator(Control ctrl, AnimationType animationType, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = animationType
            };
            animator.Show(ctrl);
        }

        /// <summary>
        /// Hide the control with async effect.
        /// </summary>
        /// <param name="ctrl">The hiding control.</param>
        /// <param name="animationType">Effect type.</param>
        /// <param name="speed">FPMS.</param>
        public static void HideAnimator(Control ctrl, AnimationType animationType, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = animationType
            };
            animator.Hide(ctrl);
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
