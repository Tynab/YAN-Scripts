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
        //xac nhận reset máy
        private static void ResetVerify()
        {
            if (MsgboxQuestAdvancedVN("HỎI", "Lỗi ứng dụng chạy ngầm, khởi động lại hệ thống để khắc phục?") == Yes)
            {
                AlarmWin(Restart, 3);
            }
        }

        //căt ảnh
        private static Image CropImg(Image img, Rectangle rect) => ((Bitmap)img).Clone(rect, img.PixelFormat);

        //hoán đổi
        private static uint SwapEndianness(ulong x) => (uint)(((x & 0x000000ff) << 24) + ((x & 0x0000ff00) << 8) + ((x & 0x00ff0000) >> 8) + ((x & 0xff000000) >> 24));

        //lấy datetime online socket
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

        //lấy datetime online stream
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

        //check app installer trong app list
        private static bool CheckAppInList(string name, string address)
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

        #region Vietnamese Format
        //fân khuc
        private static IEnumerable<string> Chunked(this string text, int chunkSize) => Range(0, text.Length / chunkSize).Select(i => text.Substring(i * chunkSize, chunkSize));

        //không trăm
        private static bool ShouldShowZeroHundred(this string[] groups) => groups.Reverse().TakeWhile(it => it == "000").Count() < groups.Count() - 1;

        //zải cấu truc (private của function number to vietnamese words)
        internal static void Deconstruct<T>(this IReadOnlyList<T> items, out T t0, out T t1, out T t2)
        {
            t0 = items.Count > 0 ? items[0] : default;
            t1 = items.Count > 1 ? items[1] : default;
            t2 = items.Count > 2 ? items[2] : default;
        }

        //băt cặp
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

        //bộ 3
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

        //viêt hoa
        private static string Capitalize(this string input)
        {
            return input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1).ToLower()
            };
        }

        /// <summary>
        /// Chuyển số sang chữ Việt.
        /// </summary>
        /// <param name="num">Số cần chuyển</param>
        /// <returns>Chuỗi số bằng chữ tiếng Việt.</returns>
        public static string ToWordsVn(this int num)
        {
            if (num == 0)
            {
                return "Không";
            }
            if (num < 0)
            {
                return "Âm " + (-num).ToWordsVn().ToLower();
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

        /// <summary>
        /// Chuyển ngày sang chuỗi định dạng Việt.
        /// </summary>
        /// <param name="dtm">Ngày cần chuyển.</param>
        /// <returns>Chuỗi định dạng ngày Việt.</returns>
        public static string ToDateStringVn(this DateTime dtm) => dtm.ToString("dd/MM/yyyy");

        /// <summary>
        /// Chuyển ngày sang chuỗi định dạng Việt dùng để đặt tên file.
        /// </summary>
        /// <param name="dtm">Ngày cần chuyển.</param>
        /// <returns>Chuỗi định dạng ngày Việt để đặt tên file.</returns>
        public static string ToDateStringNameVn(this DateTime dtm) => dtm.ToString("dd-MM-yyyy");

        /// <summary>
        /// Chuyển ngày sang chuỗi định dạng Việt theo kiểu từ ngày đến ngày.
        /// </summary>
        /// <param name="dtm">Ngày cần chuyển.</param>
        /// <returns>Chuỗi định dạng ngày Việt theo kiểu từ ngày đến ngày.</returns>
        public static string ToDateStringMultiVn(this DateTime dtm) => dtm.ToString("dd.MM.yyyy");
        #endregion

        #region Ellipse Form
        /// <summary>
        /// Tạo khung ellipse cho form.
        /// </summary>
        /// <param name="nLRect">Tọa độ trái.</param>
        /// <param name="nTRect">Tọa độ trên.</param>
        /// <param name="nRRect">Tọa độ phải.</param>
        /// <param name="nBRect">Tọa độ dưới.</param>
        /// <param name="nWEllipse">Độ rộng.</param>
        /// <param name="nHElippse">Độ cao.</param>
        /// <returns>Platform specific.</returns>
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(int nLRect, int nTRect, int nRRect, int nBRect, int nWEllipse, int nHElippse);
        #endregion

        #region Date Time
        /// <summary>
        /// Lấy số tuần của năm dựa trên ngày.
        /// </summary>
        /// <param name="dtm">Ngày mục tiêu.</param>
        /// <returns>Số thứ tự tuần chứa ngày mục tiêu trong năm.</returns>
        public static int WeekOfYear(this DateTime dtm) => CurrentInfo.Calendar.GetWeekOfYear(dtm, CurrentInfo.CalendarWeekRule, CurrentInfo.FirstDayOfWeek);

        /// <summary>
        /// Chuyển chuỗi sang ngày giờ mở rộng.
        /// </summary>
        /// <param name="str">Chuỗi cần chuyển.</param>
        /// <param name="dtmFormat">Định dạng ngày của chuỗi.</param>
        /// <returns>Thành công hoặc thất bại.</returns>
        public static bool TryParseExactEx(this string str, string dtmFormat, out DateTime dtm) => TryParseExact(str, dtmFormat, InvariantCulture, DateTimeStyles.None, out dtm);

        /// <summary>
        /// Chuyển chuỗi giờ phút sang giờ.
        /// </summary>
        /// <param name="hhmm">Chuỗi cần chuyển.</param>
        /// <param name="dtm">Giờ kết quả</param>
        /// <returns>Thành công hoặc thất bại.</returns>
        public static bool TryParseFromHhmm(this string hhmm, out DateTime dtm) => hhmm.TryParseExactEx("HH:mm", out dtm) || hhmm.TryParseExactEx("h:mm", out dtm) || hhmm.TryParseExactEx("HH:m", out dtm) || hhmm.TryParseExactEx("h:m", out dtm);

        /// <summary>
        /// Chuyển chuỗi giờ phút sang số giờ.
        /// </summary>
        /// <param name="hm">Hour and minute text.</param>
        /// <returns>Time hour.</returns>
        public static double ParseFromHhmm(this string hm) => hm.TryParseFromHhmm(out var dtm) ? (dtm - Today).TotalHours : 0;

        /// <summary>
        /// Chuyển số phút sang chuỗi giờ phút.
        /// </summary>
        /// <param name="mm">Số phút cần chuyển.</param>
        /// <returns>Chuỗi giờ phút</returns>
        public static string ToHhmmFromMm(this double mm)
        {
            var timeSpan = FromHours(mm);
            return timeSpan.Hours.ToString("00") + ":" + timeSpan.Minutes.ToString("00");
        }

        /// <summary>
        /// Lấy ngày giờ online nâng cấp.
        /// </summary>
        /// <returns>Ngày giờ quốc tế.</returns>
        public static DateTime DtmOnlAdv()
        {
            var dtm = Now;
            if (CheckInternet())
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
        /// Tính số tháng giữa 2 ngày.
        /// </summary>
        /// <param name="dtmFront">Ngày trước.</param>
        /// <param name="dtmBack">Ngày sau.</param>
        /// <returns>Số tháng được tính.</returns>
        public static int DtmTotalMonth(DateTime dtmFront, DateTime dtmBack) => (dtmBack.Year - dtmFront.Year) * 12 + dtmBack.Month - dtmFront.Month;
        #endregion

        #region Math
        /// <summary>
        /// Làm tròn lên 0.5.
        /// </summary>
        /// <param name="num">Số cần làm tròn.</param>
        /// <returns>Số đã được làm tròn.</returns>
        public static double RoundUpPoint5(double num) => Ceiling(num * 2) / 2;

        /// <summary>
        /// Làm tròn xuống 0.5.
        /// </summary>
        /// <param name="num">Số cần làm tròn.</param>
        /// <returns>Số đã được làm tròn.</returns>
        public static double RoundDownPoint5(double num) => Floor(num * 2) / 2;

        /// <summary>
        /// Chuyển chuỗi sang số double.
        /// </summary>
        /// <param name="str">Chuỗi cần chuyển.</param>
        /// <returns>Số kiểu double.</returns>
        public static double ParseDouble(this string str)
        {
            double.TryParse(str, out var num);
            return num;
        }

        /// <summary>
        /// Chuyển chuỗi sang số int.
        /// </summary>
        /// <param name="str">Chuỗi cần chuyển.</param>
        /// <returns>Số kiểu int.</returns>
        public static int ParseInt(this string str)
        {
            int.TryParse(str, out var num);
            return num;
        }

        /// <summary>
        /// Tìm giá trị nhỏ nhất.
        /// </summary>
        /// <param name="list">Chuỗi dữ liệu so sánh.</param>
        /// <returns>Giá trị nhỏ nhất.</returns>
        public static T Miner<T>(params T[] list)
        {
            dynamic res = list[0];
            foreach (var item in list)
            {
                if (item < res)
                {
                    res = item;
                }
            }
            return res;
        }

        /// <summary>
        /// Tìm giá trị lớn nhất.
        /// </summary>
        /// <param name="list">Chuỗi dữ liệu so sánh.</param>
        /// <returns>Giá trị lớn nhất.</returns>
        public static T Maxer<T>(params T[] list)
        {
            dynamic res = list[0];
            foreach (var item in list)
            {
                if (item > res)
                {
                    res = item;
                }
            }
            return res;
        }
        #endregion

        #region Internet
        /// <summary>
        /// Kiểm tra kết nối internet.
        /// </summary>
        /// <returns>Kết nổi hoặc không.</returns>
        public static bool CheckInternet()
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
        /// Chuyển dữ liệu từ database.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu.</typeparam>
        /// <param name="obj">Dữ liệu mục tiêu.</param>
        /// <returns>Giá trị được chuyển.</returns>
        public static T ValFromDb<T>(object obj) => obj == null || obj == Value ? default : (T)obj;

        /// <summary>
        /// Tìm vị trí dòng trong datatable chứa ô có chuỗi cần tìm.
        /// </summary>
        /// <param name="dt">Datatable tìm kiếm.</param>
        /// <param name="dcName">Tên cột tìm kiếm.</param>
        /// <param name="searchText">Chuỗi cần tìm.</param>
        /// <returns>Vị trí dòng trong datatable.</returns>
        public static int SearchRowIndexWithText(this DataTable dt, string dcName, string searchText) => dt.Rows.IndexOf(dt.Select($"{dcName} = '{searchText}'")[0]);

        /// <summary>
        /// Sort datatable theo cột số từ nhỏ đến lớn.
        /// </summary>
        /// <param name="dt">Datatable sort.</param>
        /// <param name="dcName">Tên cột sort.</param>
        /// <returns>Datatable mới đã sort.</returns>
        public static DataTable DtSortNumCol(DataTable dt, string dcName) => dt.AsEnumerable().OrderBy(x => int.Parse(x[dcName].ToString())).Select(x => x).CopyToDataTable();

        /// <summary>
        /// Datatable thêm dòng mới.
        /// </summary>
        /// <param name="dt">Datatable cần thêm dòng.</param>
        public static void AddNewRow(this DataTable dt) => dt.Rows.Add(dt.NewRow());

        /// <summary>
        /// Datatable thêm cột tại vị trí.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu.</typeparam>
        /// <param name="dt">Datatable cần thêm cột.</param>
        /// <param name="dcName">Tên cột cần thêm.</param>
        /// <param name="i">Vị trí cột cần thêm.</param>
        public static void AddColAt<T>(this DataTable dt, string dcName, int i) => dt.Columns.Add(dcName, typeof(T)).SetOrdinal(i);

        /// <summary>
        /// Datatable cắt cột theo mẫu.
        /// </summary>
        /// <param name="dt">Datatable cần đồng bộ.</param>
        /// <param name="dtSrc">Datatable mẫu.</param>
        public static void SyncCol(this DataTable dt, DataTable dtSrc)
        {
            while (dt.Columns.Count > dtSrc.Columns.Count)
            {
                dt.Columns.RemoveAt(dt.Columns.Count - 1);
            }
            while (dt.Columns.Count < dtSrc.Columns.Count)
            {
                dt.AddColAt<string>(dtSrc.Columns[dt.Columns.Count].ColumnName, dt.Columns.Count);
            }
        }

        /// <summary>
        /// Chép dữ liệu từ datatable này sang datatable khác.
        /// </summary>
        /// <param name="dtSrc">Datatable cần chép.</param>
        /// <param name="dt">Datatable nhận.</param>
        public static void CopTo(this DataTable dtSrc, DataTable dt) => dtSrc.AsEnumerable().Take(dtSrc.Rows.Count).CopyToDataTable(dt, OverwriteChanges);

        /// <summary>
        /// Chép dữ liệu từ datatable này đảo nghịch sang datatable khác.
        /// </summary>
        /// <param name="dtSrc">Datatable cần chép.</param>
        /// <param name="dt">Datatable nhận.</param>
        public static void CopReverseTo(this DataTable dtSrc, DataTable dt) => dtSrc.AsEnumerable().Take(dtSrc.Rows.Count).Reverse().CopyToDataTable(dt, OverwriteChanges);

        /// <summary>
        /// Chép datatable lên clipboard.
        /// </summary>
        /// <param name="dt">Datatable cần chép.</param>
        public static void DtToClipboard(DataTable dt)
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
        /// Gộp datatable.
        /// </summary>
        /// <param name="dt">Datatable gộp.</param>
        /// <param name="dts">Chuỗi datatable.</param>
        public static void DtMerge(DataTable dt, params DataTable[] dts)
        {
            foreach (var item in dts)
            {
                if (item != null)
                {
                    item.CopTo(dt);
                }
            }
        }

        /// <summary>
        /// Chuyển datagridview sang datatable.
        /// </summary>
        /// <param name="dgv">Datagridview cần chuyển.</param>
        /// <returns>Datatable đã được chuyển.</returns>
        public static DataTable ToDt(this DataGridView dgv)
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
        /// Thay đổi trạng thái double buffered của datagridview.
        /// </summary>
        /// <param name="dgv">Datagridview thay đổi.</param>
        /// <param name="state">Trạng thái thay đổi.</param>
        public static void DubBuffered(this DataGridView dgv, bool state)
        {
            if (!TerminalServerSession)
            {
                dgv.GetType().GetProperty("DoubleBuffered", Instance | NonPublic).SetValue(dgv, state, null);
            }
        }

        /// <summary>
        /// Thay đổi trạng thái auto size của datagridview.
        /// </summary>
        /// <param name="dgv">Datagridview thay đổi.</param>
        /// <param name="state">Trạng thái thay đổi.</param>
        public static void AutoSizeMod(this DataGridView dgv, bool state) => dgv.AutoSizeColumnsMode = state ? AllCells : DataGridViewAutoSizeColumnsMode.None;

        /// <summary>
        /// Tự động khởi tạo lại cột số thứ tự cho datagridview khóa dòng.
        /// </summary>
        /// <param name="dgv">Datagridview nguồn.</param>
        public static void RebootNoLock(this DataGridView dgv)
        {
            foreach (DataGridViewRow dgvr in dgv.Rows)
            {
                dgvr.Cells[0].Value = dgvr.Index + 1;
            }
        }

        /// <summary>
        /// Tự động khởi tạo lại cột số thứ tự cho datagridview dòng tự do.
        /// </summary>
        /// <param name="dgv">Datagridview nguồn.</param>
        public static void RebootNoFree(this DataGridView dgv)
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
            return CheckAppInList(name, $"SOFTWARE\\{address}") || CheckAppInList(name, $"SOFTWARE\\Wow6432Node\\{address}");
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
                    ResetVerify();
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
                    ResetVerify();
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
