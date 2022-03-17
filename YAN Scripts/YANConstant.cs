using System.Drawing;
using System.Media;
using static System.Drawing.Color;
using static YAN_Scripts.Properties.Resources;

namespace YAN_Scripts
{
    public class YANConstant
    {
        #region Variable
        public static Font _fntTitVn_ = new Font("Tahoma", 10);
        public static Font _fntTitJp_ = new Font("Yu Gothic", 11);
        public static Font _fntCapVn_ = new Font("Verdana", 9);
        public static Font _fntCapJp_ = new Font("Meiryo", 11);
        public static Font _fntTextVn_ = new Font("Segoe UI Light", 10);
        public static Font _fntTextJp_ = new Font("Meiryo", 10);
        public static readonly Color _clHeaderXl_ = FromArgb(0, 122, 204);
        public const string _trueVn_ = "Có";
        public const string _falseVn_ = "Không";
        public const string _prcXl_ = "excel";
        public const string _nameCo_ = "Nephilim";
        public const string _numFormat_ = "N0";
        public const string _timezoneVn_ = "07:00:00";
        public const string _formatDateVn_ = "dd/MM/yyyy";
        public const string _formatTimeVn_ = "HH:mm:ss";
        public const string _xlFormatText_ = "@";
        public const string _xlFormatNum_ = "#,##0";
        public const string _codeFormatText = "Unicode Text";
        public const string _fontDoc_ = "Times New Roman";
        public const float _animatorSpeed_ = 0.02f;
        public const int _timeAnimate_ = 500;
        public const int _timeOut_ = 7000;
        public const int _wUpdater_ = 360;
        #endregion

        #region Sound
        public static readonly SoundPlayer _sound_Back_ = new SoundPlayer(sBack);
        public static readonly SoundPlayer _sound_Change_ = new SoundPlayer(sChange);
        public static readonly SoundPlayer _sound_Next_ = new SoundPlayer(sNext);
        public static readonly SoundPlayer _sound_Hover_ = new SoundPlayer(sHover);
        public static readonly SoundPlayer _sound_Press_ = new SoundPlayer(sPress);
        #endregion

        #region Type
        public enum CountdownAction
        {
            ShutDown,
            Restart
        }

        public enum AnimateWindowFlags
        {
            AW_HOR_POSITIVE = 0x00000001,
            AW_HOR_NEGATIVE = 0x00000002,
            AW_VER_POSITIVE = 0x00000004,
            AW_VER_NEGATIVE = 0x00000008,
            AW_CENTER = 0x00000010,
            AW_HIDE = 0x00010000,
            AW_ACTIVATE = 0x00020000,
            AW_SLIDE = 0x00040000,
            AW_BLEND = 0x00080000
        }
        #endregion

        #region Range
        public static readonly string[] ZeroLeftPadding =
        {
            "",
            "00",
            "0"
        };
        public static readonly string[] Digits =
        {
            "không",
            "một",
            "hai",
            "ba",
            "bốn",
            "năm",
            "sáu",
            "bảy",
            "tám",
            "chín"
        };
        public static readonly string[] MultipleThousand =
        {
            "",
            "nghìn",
            "triệu",
            "tỷ",
            "nghìn tỷ",
            "triệu tỷ",
            "tỷ tỷ"
        };
        #endregion
    }
}
