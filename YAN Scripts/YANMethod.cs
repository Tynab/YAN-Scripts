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
using static YAN_Scripts.YANConstant.CountdownAction;
using Timer = System.Windows.Forms.Timer;

namespace YAN_Scripts
{
    public static class YANMethod
    {
        #region Function
        //xac nhận reset máy
        private static void ResetVerify()
        {
            if (MsgboxQuestionAdvVn("HỎI", "Lỗi ứng dụng chạy ngầm, khởi động lại hệ thống để khắc phục?") == Yes)
            {
                CountdownWin(Restart, 3);
            }
        }

        //căt ảnh
        private static Image Crop(this Image img, Rectangle rect) => ((Bitmap)img).Clone(rect, img.PixelFormat);

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
        private static bool CheckAppInList(string name, string path)
        {
            var check = false;
            var key = LocalMachine.OpenSubKey(path);
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

        #region Console
        //fields
        private const byte SW_HIDE = 0;
        private const byte SW_SHOW = 5;

        //get window
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        //hiện window
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Ẩn window console.
        /// </summary>
        public static void HideConsole() => ShowWindow(GetConsoleWindow(), SW_HIDE);

        /// <summary>
        /// Hiện window console.
        /// </summary>
        public static void ShowConsole() => ShowWindow(GetConsoleWindow(), SW_SHOW);
        #endregion

        #region Vietnamese Format
        //fân khuc
        private static IEnumerable<string> Chunked(this string str, int chunkSize) => Range(0, str.Length / chunkSize).Select(i => str.Substring(i * chunkSize, chunkSize));

        //fần ngìn
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
            var i = -1;
            var rawResult = groups.Aggregate("", (acc, e) =>
            {
                checked
                {
                    i++;
                }
                var readTriple = ReadTriple(e, groups.ShouldShowZeroHundred() && i > 0);
                var multipleThousand = string.IsNullOrWhiteSpace(readTriple) ? "" : (MultipleThousand.ElementAtOrDefault(groups.Length - 1 - i) ?? "");
                return $"{acc} {readTriple} {multipleThousand} ";
            });
            return Regex.Replace(rawResult, "\\s+", " ").Trim().CapitalizeAdvEx(); //replace white space with a specified character
        }

        /// <summary>
        /// Chuyển ngày sang chuỗi định dạng Việt (dd/mm/yyyy).
        /// </summary>
        /// <param name="dtm">Ngày cần chuyển.</param>
        /// <returns>Chuỗi định dạng ngày Việt.</returns>
        public static string ToDateStringVn(this DateTime dtm) => dtm.ToString("dd/MM/yyyy");

        /// <summary>
        /// Chuyển ngày sang chuỗi định dạng Việt dùng để đặt tên file (dd-mm-yyyy).
        /// </summary>
        /// <param name="dtm">Ngày cần chuyển.</param>
        /// <returns>Chuỗi định dạng ngày Việt để đặt tên file.</returns>
        public static string ToDateStringNameVn(this DateTime dtm) => dtm.ToString("dd-MM-yyyy");

        /// <summary>
        /// Chuyển ngày sang chuỗi định dạng Việt theo kiểu từ ngày đến ngày (dd.mm.yyyy).
        /// </summary>
        /// <param name="dtm">Ngày cần chuyển.</param>
        /// <returns>Chuỗi định dạng ngày Việt theo kiểu từ ngày đến ngày.</returns>
        public static string ToDateStringMultiVn(this DateTime dtm) => dtm.ToString("dd.MM.yyyy");

        /// <summary>
        /// Chuyển chuỗi sang số ngày Việt.
        /// </summary>
        /// <param name="str">Chuỗi cần chuyển có định dạng ngày Việt (dd/mm/yyyy).</param>
        /// <returns>Ngày giờ.</returns>
        public static DateTime ParseDateVn(this string str)
        {
            DtmTryParseExactEx(str, "dd/MM/yyyy", out var dtm);
            return dtm;
        }
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
        /// <param name="format">Định dạng ngày của chuỗi.</param>
        /// <returns>Thành công hoặc thất bại.</returns>
        public static bool DtmTryParseExactEx(string str, string format, out DateTime dtm) => TryParseExact(str, format, InvariantCulture, DateTimeStyles.None, out dtm);

        /// <summary>
        /// Chuyển chuỗi giờ phút sang giờ.
        /// </summary>
        /// <param name="hhmm">Chuỗi cần chuyển.</param>
        /// <param name="dtm">Giờ kết quả</param>
        /// <returns>Thành công hoặc thất bại.</returns>
        public static bool DtmTryParseFromHhmm(string hhmm, out DateTime dtm) => DtmTryParseExactEx(hhmm, "HH:mm", out dtm) || DtmTryParseExactEx(hhmm, "h:mm", out dtm) || DtmTryParseExactEx(hhmm, "HH:m", out dtm) || DtmTryParseExactEx(hhmm, "h:m", out dtm);

        /// <summary>
        /// Chuyển chuỗi giờ phút sang số giờ.
        /// </summary>
        /// <param name="hm">Hour and minute text.</param>
        /// <returns>Time hour.</returns>
        public static double ParseFromHhmm(this string hm) => DtmTryParseFromHhmm(hm, out var dtm) ? (dtm - Today).TotalHours : 0;

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
        /// Chuyển số phút sang chuỗi giờ phút.
        /// </summary>
        /// <param name="mm">Số phút cần chuyển.</param>
        /// <returns>Chuỗi giờ phút</returns>
        public static string ToHhmmFromMm(this int mm)
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
                        MsgboxErrorAdv("ERROR", ex.ToString());
                    }
                }
            }
            return dtm;
        }

        /// <summary>
        /// Tính số tháng giữa 2 ngày.
        /// </summary>
        /// <param name="dtmSt">Ngày thứ nhất.</param>
        /// <param name="dtmNd">Ngày thứ 2.</param>
        /// <returns>Số tháng được tính.</returns>
        public static int DtmTotalMonth(DateTime dtmSt, DateTime dtmNd) => dtmNd > dtmSt ? (dtmNd.Year - dtmSt.Year) * 12 + dtmNd.Month - dtmSt.Month : (dtmSt.Year - dtmNd.Year) * 12 + dtmSt.Month - dtmNd.Month;
        #endregion

        #region Math
        /// <summary>
        /// Làm tròn lên 0.5.
        /// </summary>
        /// <param name="num">Số cần làm tròn.</param>
        /// <returns>Số đã được làm tròn.</returns>
        public static double RoundUpHalf(this double num) => Ceiling(num * 2) / 2;

        /// <summary>
        /// Làm tròn xuống 0.5.
        /// </summary>
        /// <param name="num">Số cần làm tròn.</param>
        /// <returns>Số đã được làm tròn.</returns>
        public static double RoundDownHalf(this double num) => Floor(num * 2) / 2;

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
        public static T ToVal<T>(this object obj) => obj == null || obj == Value ? default : (T)obj;

        /// <summary>
        /// Tìm vị trí dòng trong datatable chứa ô có chuỗi cần tìm.
        /// </summary>
        /// <param name="dt">Datatable tìm kiếm.</param>
        /// <param name="dcName">Tên cột tìm kiếm.</param>
        /// <param name="str">Chuỗi cần tìm.</param>
        /// <returns>Vị trí dòng trong datatable.</returns>
        public static int SearchRowIndexWithText(this DataTable dt, string dcName, string str) => dt.Rows.IndexOf(dt.Select($"{dcName} = '{str}'")[0]);

        /// <summary>
        /// Sort datatable theo cột số từ nhỏ đến lớn.
        /// </summary>
        /// <param name="dt">Datatable sort.</param>
        /// <param name="dcName">Tên cột sort.</param>
        /// <returns>Datatable mới đã sort.</returns>
        public static DataTable SortByNumCol(this DataTable dt, string dcName) => dt.AsEnumerable().OrderBy(x => int.Parse(x[dcName].ToString())).Select(x => x).CopyToDataTable();

        /// <summary>
        /// Filter datatable theo giá trị.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu.</typeparam>
        /// <param name="dt">Datatable filter.</param>
        /// <param name="dcName">Tên cột filter.</param>
        /// <param name="val">Giá trị filter.</param>
        /// <returns>Datatable mới đã filter.</returns>
        public static DataTable Filter<T>(this DataTable dt, string dcName, T val)
        {
            dynamic x = val;
            return dt.AsEnumerable().Where(row => row.Field<T>(dcName) == x).CopyToDataTable();
        }

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
        /// <param name="dtDest">Datatable cần đồng bộ.</param>
        /// <param name="dtSrc">Datatable mẫu.</param>
        public static void SyncColTo(this DataTable dtDest, DataTable dtSrc)
        {
            while (dtDest.Columns.Count > dtSrc.Columns.Count)
            {
                dtDest.Columns.RemoveAt(dtDest.Columns.Count - 1);
            }
            while (dtDest.Columns.Count < dtSrc.Columns.Count)
            {
                dtDest.AddColAt<string>(dtSrc.Columns[dtDest.Columns.Count].ColumnName, dtDest.Columns.Count);
            }
        }

        /// <summary>
        /// Chép dữ liệu từ datatable này sang datatable khác.
        /// </summary>
        /// <param name="dtSrc">Datatable cần chép.</param>
        /// <param name="dtDest">Datatable nhận.</param>
        public static void CopTo(this DataTable dtSrc, DataTable dtDest) => dtSrc.AsEnumerable().Take(dtSrc.Rows.Count).CopyToDataTable(dtDest, OverwriteChanges);

        /// <summary>
        /// Chép dữ liệu từ datatable này đảo nghịch sang datatable khác.
        /// </summary>
        /// <param name="dtSrc">Datatable cần chép.</param>
        /// <param name="dtDest">Datatable nhận.</param>
        public static void CopReverseTo(this DataTable dtSrc, DataTable dtDest) => dtSrc.AsEnumerable().Take(dtSrc.Rows.Count).Reverse().CopyToDataTable(dtDest, OverwriteChanges);

        /// <summary>
        /// Chép datatable lên clipboard.
        /// </summary>
        /// <param name="dt">Datatable cần chép.</param>
        public static void ToClipboard(this DataTable dt)
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
        /// <param name="dtDest">Datatable gộp.</param>
        /// <param name="dtsSrc">Chuỗi datatable.</param>
        public static void MergeEx(this DataTable dtDest, params DataTable[] dtsSrc)
        {
            foreach (var item in dtsSrc)
            {
                if (item != null)
                {
                    item.CopTo(dtDest);
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
        public static void AutoNoColLock(this DataGridView dgv)
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
        public static void AutoNoColFree(this DataGridView dgv)
        {
            for (var i = 0; i < dgv.RowCount - 1; i++)
            {
                dgv.Rows[i].Cells[0].Value = i + 1;
            }
        }
        #endregion

        #region Window
        /// <summary>
        /// Đồng hồ hẹn giờ cho windows.
        /// </summary>
        /// <param name="act">Hành động áp dụng.</param>
        /// /// <param name="ss">Số giây đếm ngược.</param>
        public static void CountdownWin(CountdownAction act, int ss)
        {
            var cmt = act == ShutDown ? "s" : "r";
            Start("shutdown.exe", $"-{cmt} -t {ss}");
        }

        /// <summary>
        /// Kiểm tra app đã cài đặt bằng tên.
        /// </summary>
        /// <param name="name">Tên app cần tìm.</param>
        /// <returns>Đã cài hoặc chưa,</returns>
        public static bool CheckAppInstalled(string name)
        {
            var path = @"Microsoft\Windows\CurrentVersion\Uninstall";
            return CheckAppInList(name, $"SOFTWARE\\{path}") || CheckAppInList(name, $"SOFTWARE\\Wow6432Node\\{path}");
        }

        /// <summary>
        /// Tăng giảm hệ màu theo vector giá trị.
        /// </summary>
        /// <param name="cl">Màu gốc.</param>
        /// <param name="val">Giá trị thêm.</param>
        /// <returns>Màu mới.</returns>
        public static Color UpDown(this Color cl, int val) => FromArgb((cl.R + val) % 256, (cl.G + val) % 256, (cl.B + val) % 256);

        /// <summary>
        /// Đảo nghịch màu.
        /// </summary>
        /// <param name="cl">Màu gốc.</param>
        /// <returns>Màu mới.</returns>
        public static Color Invert(this Color cl) => FromArgb(cl.ToArgb() ^ 0xffffff);

        /// <summary>
        /// Viết thường cả chuỗi.
        /// </summary>
        /// <param name="str">Chuỗi cần chuyển.</param>
        /// <returns>Chuỗi đã được chuyển.</returns>
        public static string LowerAdv(this string str) => !string.IsNullOrWhiteSpace(str) ? str.ToLower() : str;

        /// <summary>
        /// Viết hoa cả chuỗi.
        /// </summary>
        /// <param name="str">Chuỗi cần chuyển.</param>
        /// <returns>Chuỗi đã được chuyển.</returns>
        public static string UpperAdv(this string str) => !string.IsNullOrWhiteSpace(str) ? str.ToUpper() : str;

        /// <summary>
        /// Viết hoa chữ đầu chuỗi.
        /// </summary>
        /// <param name="str">Chuỗi cần chuyển.</param>
        /// <returns>Chuỗi đã được chuyển.</returns>
        public static string CapitalizeAdv(this string str) => !string.IsNullOrWhiteSpace(str) ? str.First().ToString().ToUpper() + str.Substring(1) : str;

        /// <summary>
        /// Viết hoa chữ đầu chuỗi còn lại viết thường.
        /// </summary>
        /// <param name="str">Chuỗi cần chuyển.</param>
        /// <returns>Chuỗi đã được chuyển.</returns>
        public static string CapitalizeAdvEx(this string str) => !string.IsNullOrWhiteSpace(str) ? str.First().ToString().ToUpper() + str.Substring(1).ToLower() : str;

        /// <summary>
        /// Viết hoa mỗi chữ đầu trong chuỗi.
        /// </summary>
        /// <param name="str">Chuỗi cần chuyển.</param>
        /// <returns>Chuỗi đã được chuyển.</returns>
        public static string CapitalizeEachWordAdv(this string str) => !string.IsNullOrWhiteSpace(str) ? CurrentCulture.TextInfo.ToTitleCase(str) : str;

        /// <summary>
        /// Viết hoa mỗi chữ đầu trong chuỗi còn lại viết thường.
        /// </summary>
        /// <param name="str">Chuỗi cần chuyển.</param>
        /// <returns>Chuỗi đã được chuyển.</returns>
        public static string CapitalizeEachWordAdvEx(this string str) => !string.IsNullOrWhiteSpace(str) ? CurrentCulture.TextInfo.ToTitleCase(str.ToLower()) : str;
        #endregion

        #region Process
        /// <summary>
        /// Tắt tất cả process theo tên.
        /// </summary>
        /// <param name="name">Tên app cần tắt.</param>
        public static void KillPrc(this string name)
        {
            if (GetProcessesByName(name).Count() > 0)
            {
                ForEach(GetProcessesByName(name), prc => prc.Kill());
            }
        }

        /// <summary>
        /// Invoke text tới label khác thread.
        /// </summary>
        /// <param name="lbl">Label cần invoke.</param>
        /// <param name="text">Text cần invoke.</param>
        public static void InvokeText(this Label lbl, string text) => lbl.Invoke((MethodInvoker)(() => lbl.Text = text));

        /// <summary>
        /// Invoke độ rộng tới panel khác thread.
        /// </summary>
        /// <param name="pnl">Panel cần invoke.</param>
        /// <param name="w">Độ rộng cần invoke.</param>
        public static void InvokeW(this Panel pnl, int w) => pnl.Invoke((MethodInvoker)(() => pnl.Width = w));
        #endregion

        #region File
        /// <summary>
        /// Get tất cả file trong folder.
        /// </summary>
        /// <param name="path">Folder path.</param>
        /// <param name="str">Tên hoặc đuôi của file cần search.</param>
        /// <returns></returns>
        public static FileInfo[] GetAllFilesInFolder(string path, string str) => new DirectoryInfo(path).GetFiles(str).OrderBy(f => f.Name).ToArray();

        /// <summary>
        /// Xóa file nâng cấp.
        /// </summary>
        /// <param name="ad">File address.</param>
        public static void DelFileAdv(string ad)
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
        /// Ghi file bằng mảng byte nâng cấp.
        /// </summary>
        /// <param name="ad">File address.</param>
        /// <param name="bytes">Mảng byte.</param>
        public static void WriteAllBytesToFileAdv(string ad, byte[] bytes)
        {
            if (!File.Exists(ad))
            {
                WriteAllBytes(ad, bytes);
            }
        }

        /// <summary>
        /// Lưu trữ ảnh chụp màn hình.
        /// </summary>
        /// <param name="path">Folder path.</param>
        /// <param name="name">Tên file.</param>
        public static void CapScreenShotToFile(string path, string name, out string ad)
        {
            using (var bmpScreenshot = new Bitmap(PrimaryScreen.Bounds.Width, PrimaryScreen.Bounds.Height, Format32bppArgb))
            {
                using (var gfxScreenshot = FromImage(bmpScreenshot))
                {
                    gfxScreenshot.CopyFromScreen(PrimaryScreen.Bounds.X, PrimaryScreen.Bounds.Y, 0, 0, PrimaryScreen.Bounds.Size, SourceCopy);
                    ad = $"{path}\\{name}.jpg";
                    bmpScreenshot.Save(ad, Jpeg);
                }
            }
        }
        #endregion

        #region Directory
        /// <summary>
        /// Tạo folder nâng cấp.
        /// </summary>
        /// <param name="path">Folder path.</param>
        public static void CreateFolderAdv(string path)
        {
            if (!Directory.Exists(path))
            {
                CreateDirectory(path);
            }
        }

        /// <summary>
        /// Xóa folder nâng cấp.
        /// </summary>
        /// <param name="path">Folder path.</param>
        public static void DelFolderAdv(string path)
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
        /// Khởi tạo lại folder mới.
        /// </summary>
        /// <param name="path">Folder path.</param>
        public static void ReCreateFolder(string path)
        {
            DelFolderAdv(path);
            CreateDirectory(path);
        }

        /// <summary>
        /// Xóa tất cả các folder có chuỗi mặc định trong tên.
        /// </summary>
        /// <param name="path">Folder path.</param>
        /// <param name="str">Chuỗi mục tiêu.</param>
        public static void DelAllFoldersByText(string path, string str) => ForEach(GetDirectories(path, $"{str}*"), folder => DelFolderAdv(folder));

        /// <summary>
        /// Copy folder đến folder khác.
        /// </summary>
        /// <param name="srcDirName">Folder gốc.</param>
        /// <param name="destDirName">Folder cần copy đến.</param>
        /// <param name="copSubDirs">Copy tất cả folder con hoặc không.</param>
        public static void DirectoryCop(string srcDirName, string destDirName, bool copSubDirs)
        {
            CreateFolderAdv(destDirName);
            ForEach(new DirectoryInfo(srcDirName).GetFiles(), file => file.CopyTo(Combine(destDirName, file.Name), false));
            if (copSubDirs)
            {
                ForEach(new DirectoryInfo(srcDirName).GetDirectories(), subdir => DirectoryCop(subdir.FullName, Combine(destDirName, subdir.Name), copSubDirs));
            }
        }
        #endregion

        #region Form
        /// <summary>
        /// Fade in form.
        /// </summary>
        /// <param name="frm">Form áp dụng.</param>
        public static void FadeInFrm(this Form frm)
        {
            while (frm.Opacity < 1)
            {
                frm.Opacity += 0.05;
                frm.Update();
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Fade out form.
        /// </summary>
        /// <param name="frm">Form áp dụng.</param>
        public static void FadeOutFrm(this Form frm)
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
        /// Get tất cả control con theo loại.
        /// </summary>
        /// <param name="ctrl">Parent control.</param>
        /// <param name="type">Loại control cần get.</param>
        /// <returns>Control list.</returns>
        public static IEnumerable<Control> GetAllObjs(this Control ctrl, Type type)
        {
            var ctrls = ctrl.Controls.Cast<Control>();
            return ctrls.SelectMany(obj => obj.GetAllObjs(type)).Concat(ctrls).Where(c => c.GetType() == type);
        }

        /// <summary>
        /// Tạo list item cho combobox từ các file trong folder.
        /// </summary>
        /// <param name="cmb">Combobox cần tạo list.</param>
        /// <param name="path">Folder path.</param>
        public static void GetItemListFromFilesInFolder(this YANComboBox cmb, string path)
        {
            cmb.Items.Clear();
            foreach (var file in GetFiles(path))
            {
                cmb.Items.Add(GetFileNameWithoutExtension(file));
            }
        }
        #endregion

        #region Control
        /// <summary>
        /// Scale picturebox theo chiều ngang.
        /// </summary>
        /// <param name="pic">Picturebox mục tiêu.</param>
        public static void PicHorizontalScale(this PictureBox pic)
        {
            pic.SizeMode = Zoom;
            pic.Image = pic.Image.Crop(new Rectangle(0, 0, pic.Image.Width, pic.Image.Width));
        }

        /// <summary>
        /// Scale picturebox theo chiều dọc.
        /// </summary>
        /// <param name="pic">Picturebox mục tiêu.</param>
        public static void PicVerticalScale(this PictureBox pic)
        {
            pic.SizeMode = Zoom;
            pic.Image = pic.Image.Crop(new Rectangle(0, 0, pic.Image.Height, pic.Image.Height));
        }

        /// <summary>
        /// Chạy timer nâng cấp.
        /// </summary>
        /// <param name="tmr">Timer mục tiêu.</param>
        public static void TmrStartAdv(this Timer tmr)
        {
            if (!tmr.Enabled)
            {
                tmr.Start();
            }
        }

        /// <summary>
        /// Dừng timer nâng cấp.
        /// </summary>
        /// <param name="tmr">Timer mục tiêu.</param>
        public static void TmrEndAdv(this Timer tmr)
        {
            if (tmr.Enabled)
            {
                tmr.Stop();
            }
        }

        /// <summary>
        /// Highlight label link bằng tên control.
        /// </summary>
        /// <param name="ctrl">Control mục tiêu.</param>
        /// <param name="typeName">Loại control.</param>
        /// <param name="cl">Màu highlight.</param>
        /// <param name="isBold">In đậm hoặc không.</param>
        public static void HighLightLblLinkByCtrl(this Control ctrl, string typeName, Color cl, bool isBold)
        {
            var lbl = (Label)ctrl.FindForm().Controls.Find($"label{ctrl.Name.Substring(typeName.Length)}", true).FirstOrDefault();
            lbl.ForeColor = cl;
            lbl.Font = isBold ? new Font(lbl.Font, Bold) : new Font(lbl.Font, Regular);
        }
        #endregion

        #region MessageBox
        /// <summary>
        /// Hiện messagebox bắt lỗi.
        /// </summary>
        /// <param name="ex">Exception.</param>
        public static void MsgBoxCatchError(string ex)
        {
            MessageBox.Show(new Form
            {
                StartPosition = FormStartPosition.Manual,
                Location = new Point(Position.X, Position.Y),
                TopMost = true
            }, ex.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Hiện mẫu messagebox none nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxNoneAdv(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.None);

        /// <summary>
        /// Hiện mẫu messagebox infomation nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxInfomationAdv(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);

        /// <summary>
        /// Hiện mẫu messagebox question nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxQuestionAdv(string cap, string msg) => Show(msg, cap, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        /// <summary>
        /// Hiện mẫu messagebox warning nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxWarningAdv(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

        /// <summary>
        /// Hiện mẫu messagebox exclamation nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxExclamationAdv(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        /// <summary>
        /// Hiện mẫu messagebox error nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxErrorAdv(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);

        /// <summary>
        /// Hiện mẫu messagebox none tiếng Nhật nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxNoneAdvJp(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.None, JAP);

        /// <summary>
        /// Hiện mẫu messagebox infomation tiếng Nhật nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxInfomationAdvJp(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information, JAP);

        /// <summary>
        /// Hiện mẫu messagebox question tiếng Nhật nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxQuestionAdvJp(string cap, string msg) => Show(msg, cap, MessageBoxButtons.YesNo, MessageBoxIcon.Question, JAP);

        /// <summary>
        /// Hiện mẫu messagebox warning tiếng Nhật nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxWarningAdvJp(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, JAP);

        /// <summary>
        /// Hiện mẫu messagebox exclamation tiếng Nhật nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxExclamationAdvJp(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, JAP);

        /// <summary>
        /// Hiện mẫu messagebox error tiếng Nhật nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxErrorAdvJp(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error, JAP);

        /// <summary>
        /// Hiện mẫu messagebox none tiếng Việt nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxNoneAdvVn(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.None, VIE);

        /// <summary>
        /// Hiện mẫu messagebox infomation tiếng Việt nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxInfomationAdvVn(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information, VIE);

        /// <summary>
        /// Hiện mẫu messagebox question tiếng Việt nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxQuestionAdvVn(string cap, string msg) => Show(msg, cap, MessageBoxButtons.YesNo, MessageBoxIcon.Question, VIE);

        /// <summary>
        /// Hiện mẫu messagebox warning tiếng Việt nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxWarningAdvVn(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, VIE);

        /// <summary>
        /// Hiện mẫu messagebox exclamation tiếng Việt nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxExclamationAdvVn(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, VIE);

        /// <summary>
        /// Hiện mẫu messagebox error tiếng Việt nâng cấp.
        /// </summary>
        /// <param name="cap">Tiêu đề.</param>
        /// <param name="msg">Nội dung.</param>
        /// <returns>Dialog result.</returns>
        public static DialogResult MsgboxErrorAdvVn(string cap, string msg) => Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error, VIE);
        #endregion

        #region Animator
        /// <summary>
        /// Hiện control với animation đồng bộ.
        /// </summary>
        /// <param name="ctrl">Control mục tiêu.</param>
        /// <param name="type">Loại hiệu ứng.</param>
        /// <param name="speed">Frame per milisecond.</param>
        public static void ShowAnimator(this Control ctrl, AnimationType type, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = type
            };
            animator.ShowSync(ctrl);
        }

        /// <summary>
        /// Ẩn control với animation đồng bộ.
        /// </summary>
        /// <param name="ctrl">Control mục tiêu.</param>
        /// <param name="type">Loại hiệu ứng.</param>
        /// <param name="speed">Frame per milisecond.</param>
        public static void HideAnimator(this Control ctrl, AnimationType type, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = type
            };
            animator.HideSync(ctrl);
        }

        /// <summary>
        /// Hiện control với animation bất đồng bộ.
        /// </summary>
        /// <param name="ctrl">Control mục tiêu.</param>
        /// <param name="type">Loại hiệu ứng.</param>
        /// <param name="speed">Frame per milisecond.</param>
        public static void ShowAnimatorAsync(this Control ctrl, AnimationType type, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = type
            };
            animator.Show(ctrl);
        }

        /// <summary>
        /// Ẩn control với animation bất đồng bộ.
        /// </summary>
        /// <param name="ctrl">Control mục tiêu.</param>
        /// <param name="type">Loại hiệu ứng.</param>
        /// <param name="speed">Frame per milisecond.</param>
        public static void HideAnimatorAsync(this Control ctrl, AnimationType type, float speed)
        {
            var animator = new Animator
            {
                TimeStep = speed,
                AnimationType = type
            };
            animator.Hide(ctrl);
        }
        #endregion

        #region Animate
        /// <summary>
        /// Điều khiển object với animation đồng bộ.
        /// </summary>
        /// <param name="hwand">Object.</param>
        /// <param name="dwTime">Thời gian tính bằng milisecond.</param>
        /// <param name="dwFlags">Flag animate.</param>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern void AnimateWindow(IntPtr hwand, int dwTime, AnimateWindowFlags dwFlags);
        #endregion
    }
}
