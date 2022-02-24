﻿using System.Drawing;
using System.Media;
using static System.Drawing.Color;
using static YAN_Scripts.Properties.Resources;

namespace YAN_Scripts
{
    public class YANConstant
    {
        #region Variable
        public static Font _fntTitVie_ = new Font("Tahoma", 10);
        public static Font _fntTitJap_ = new Font("Yu Gothic", 11);
        public static Font _fntTextVie_ = new Font("Segoe UI Light", 10);
        public static Font _fntTextJap_ = new Font("Meiryo", 10);
        public static Font _fntCapVie_ = new Font("Verdana", 9);
        public static readonly Color _clTitXl_ = FromArgb(0, 122, 204);
        public const string _trueVie_ = "Có";
        public const string _falseVie_ = "Không";
        public const string _namePrcXl_ = "Microsoft Excel";
        public const string _nameCo_ = "Nephilim";
        public const string _numFormat_ = "N0";
        public const string _dateFormat_ = "dd/MM/yyyy";
        public const string _dateFormatName_ = "dd-MM-yyyy";
        public const string _dateFormatPara_ = "dd.MM.yyyy";
        public const string _timezoneVN_ = "07:00:00";
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
        public enum AlarmAction
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
    }
}
