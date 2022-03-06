using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YAN_Controls;
using static AnimatorNS.AnimationType;
using static System.Drawing.Color;
using static YAN_Scripts.YANConstant;
using static YAN_Scripts.YANMethod;
using static System.Windows.Forms.Application;

namespace YAN_Scripts
{
    public class YANEvent
    {
        #region Form Move
        //fields
        private static bool _moveFrm;
        private static Point _lastLoc;

        /// <summary>
        /// Focus control dùng để di chuyển form.
        /// </summary>
        public static void MoveFrm_MouseDown(object sender, MouseEventArgs e)
        {
            //sound
            _sound_Change_.Play();
            //action
            _moveFrm = true;
            _lastLoc = e.Location;
            ((Control)sender).FindForm().Opacity = 0.7;
        }

        /// <summary>
        /// Di chuyển control.
        /// </summary>
        public static void MoveFrm_MouseMove(object sender, MouseEventArgs e)
        {
            if (_moveFrm)
            {
                var frm = ((Control)sender).FindForm();
                frm.Location = new Point(frm.Location.X - _lastLoc.X + e.X, frm.Location.Y - _lastLoc.Y + e.Y);
                frm.Update();
            }
        }

        /// <summary>
        /// Kết thúc di chyển.
        /// </summary>
        public static void MoveFrm_MouseUp(object sender, MouseEventArgs e)
        {
            _moveFrm = false;
            ((Control)sender).FindForm().Opacity = 1;
        }
        #endregion

        #region Common
        /// <summary>
        /// Tắt app.
        /// </summary>
        public static void BtnX_Click(object sender, EventArgs e)
        {
            FadeOutFrm(((Button)sender).FindForm());
            //sound
            _sound_Next_.PlaySync();
            //action
            Exit();
        }
        #endregion

        #region Button Icon
        /// <summary>
        /// Effect khi click button icon.
        /// </summary>
        public static void BtnIc_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Hide();
            btn.ShowAnimatorAsync(Scale, _animatorSpeed_ * 2);
        }
        #endregion

        #region Button Image
        /// <summary>
        /// Trạng thái nổi bật của button image khi hover.
        /// </summary>
        public static void BtnImg_MouseEnter(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BorderSize = 1;
            btn.HighLightLblLinkByCtrl("button", btn.BorderColor, true);
        }

        /// <summary>
        /// Trả về trạng thái bình thường của button image khi leave.
        /// </summary>
        public static void BtnImg_MouseLeave(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BorderSize = 0;
            btn.HighLightLblLinkByCtrl("button", btn.ForeColor, true);
        }
        #endregion

        #region Button Enable Changed Color
        /// <summary>
        /// Green style fill.
        /// </summary>
        public static void BtnR113G175B74F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(113, 175, 74) : Gray;
        }

        /// <summary>
        /// Yellow style fill.
        /// </summary>
        public static void BtnR230G192B59F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(230, 192, 59) : Gray;
        }

        /// <summary>
        /// Violet style fill.
        /// </summary>
        public static void BtnR103G84B218F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(103, 84, 218) : Gray;
        }

        /// <summary>
        /// Red style fill.
        /// </summary>
        public static void BtnR229G51B79F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(229, 51, 79) : Gray;
        }

        /// <summary>
        /// Orange style fill.
        /// </summary>
        public static void BtnR235G92B48F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(235, 92, 48) : Gray;
        }

        /// <summary>
        /// Blue style fill.
        /// </summary>
        public static void BtnR10G124B235F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(10, 124, 235) : Gray;
        }

        /// <summary>
        /// Orange icon style fill.
        /// </summary>
        public static void BtnR235G107B60F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(235, 107, 60) : Gray;
        }

        /// <summary>
        /// Blue icon style fill.
        /// </summary>
        public static void BtnR45G85B205F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(45, 85, 205) : Gray;
        }

        /// <summary>
        /// Green icon style fill.
        /// </summary>
        public static void BtnR40G159B93F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(40, 159, 93) : Gray;
        }

        /// <summary>
        /// Green style border.
        /// </summary>
        public static void BtnMediumSeaGreenB_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BorderColor = btn.Enabled ? MediumSeaGreen : Gray;
        }

        /// <summary>
        /// Pink style border.
        /// </summary>
        public static void BtnHotPinkB_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BorderColor = btn.Enabled ? HotPink : Gray;
        }
        #endregion

        #region ComboBox Enable Changed Color
        /// <summary>
        /// Green style fill.
        /// </summary>
        public static void CmbR38G126B86F_EnabledChanged(object sender, EventArgs e)
        {
            var cmb = (YANComboBox)sender;
            cmb.BackColor = cmb.Enabled ? FromArgb(38, 126, 86) : Gray;
            cmb.IconColor = cmb.Enabled ? White : Gray;
        }

        /// <summary>
        /// Violet style border.
        /// </summary>
        public static void CmbMediumPurpleB_EnabledChanged(object sender, EventArgs e)
        {
            var cmb = (YANComboBox)sender;
            cmb.BorderColor = cmb.Enabled ? MediumPurple : Gray;
            cmb.IconColor = cmb.Enabled ? Cyan : Gray;
        }
        #endregion

        #region DatePicker Enable Changed Color
        /// <summary>
        /// Blue style fill.
        /// </summary>
        public static void DpR68G114B196F_EnabledChanged(object sender, EventArgs e)
        {
            var dp = (YANDatePicker)sender;
            dp.TextColor = dp.TextColor.Invert();
            dp.SkinColor = dp.Enabled ? FromArgb(68, 114, 196) : Gray;
        }

        /// <summary>
        /// White style fill.
        /// </summary>
        public static void DpWhiteF_EnabledChanged(object sender, EventArgs e)
        {
            var dp = (YANDatePicker)sender;
            dp.SkinColor = dp.Enabled ? White : LightGray;
        }

        /// <summary>
        /// Pink style border.
        /// </summary>
        public static void DpPaleVioletRedB_EnabledChanged(object sender, EventArgs e)
        {
            var dp = (YANDatePicker)sender;
            dp.TextColor = dp.TextColor.Invert();
            dp.BorderColor = dp.Enabled ? PaleVioletRed : Gray;
        }
        #endregion

        #region NumBox Enable Changed Color
        /// <summary>
        /// Blue style border.
        /// </summary>
        public static void NbCyanB_EnabledChanged(object sender, EventArgs e)
        {
            var nb = (YANNumBox)sender;
            nb.BorderColor = nb.Enabled ? Cyan : Gray;
        }

        /// <summary>
        /// Violet style border.
        /// </summary>
        public static void NbMediumSlateBlueB_EnabledChanged(object sender, EventArgs e)
        {
            var nb = (YANNumBox)sender;
            nb.BorderColor = nb.Enabled ? MediumSlateBlue : Gray;
        }

        /// <summary>
        /// Pink style border.
        /// </summary>
        public static void NbPaleVioletRedB_EnabledChanged(object sender, EventArgs e)
        {
            var nb = (YANNumBox)sender;
            nb.BorderColor = nb.Enabled ? PaleVioletRed : Gray;
        }

        /// <summary>
        /// Date style border.
        /// </summary>
        public static void NbDodgerBlueB_EnabledChanged(object sender, EventArgs e)
        {
            var nb = (YANNumBox)sender;
            nb.BorderColor = nb.Enabled ? DodgerBlue : Gray;
        }
        #endregion

        #region RadioButton Enable Changed Color
        /// <summary>
        /// Pink style border.
        /// </summary>
        public static void RdoHotPink_EnabledChanged(object sender, EventArgs e)
        {
            var rdo = (YANRadioButton)sender;
            rdo.CheckedColor = rdo.Enabled ? HotPink : Gray;
        }

        /// <summary>
        /// Green style border.
        /// </summary>
        public static void RdoMediumSpringGreen_EnabledChanged(object sender, EventArgs e)
        {
            var rdo = (YANRadioButton)sender;
            rdo.CheckedColor = rdo.Enabled ? MediumSpringGreen : Gray;
        }

        /// <summary>
        /// Orange style border.
        /// </summary>
        public static void RdoCoral_EnabledChanged(object sender, EventArgs e)
        {
            var rdo = (YANRadioButton)sender;
            rdo.CheckedColor = rdo.Enabled ? Coral : Gray;
        }
        #endregion

        #region TextBox Enable Changed Color
        /// <summary>
        /// Blue style border.
        /// </summary>
        public static void TxtCyanB_EnabledChanged(object sender, EventArgs e)
        {
            var txt = (YANTextBox)sender;
            txt.BorderColor = txt.Enabled ? Cyan : Gray;
        }
        #endregion

        #region TextBox Focus
        /// <summary>
        /// Textbox auto viết hoa mỗi chữ đầu trong chuỗi.
        /// </summary>
        public static void TxtCapitalizeEachWord_Leave(object sender, EventArgs e)
        {
            var txt = (YANTextBox)sender;
            txt.Txt = txt.Txt.CapitalizeEachWordAdv();
        }

        /// <summary>
        /// Textbox auto viết hoa cả chuỗi.
        /// </summary>
        public static void TxtUpCase_Leave(object sender, EventArgs e)
        {
            var txt = (YANTextBox)sender;
            txt.Txt = txt.Txt.UpperAdv();
        }

        /// <summary>
        /// Textbox auto viết thường cả chuỗi.
        /// </summary>
        public static void TxtLowCase_Leave(object sender, EventArgs e)
        {
            var txt = (YANTextBox)sender;
            txt.Txt = txt.Txt.LowerAdv();
        }
        #endregion

        #region TextBox Input
        /// <summary>
        /// Textbox chỉ nhập được số.
        /// </summary>
        public static void TxtNumeric_Keypress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != 8 && e.KeyChar != 3 && e.KeyChar != 22 && e.KeyChar != 24 && e.KeyChar != 1 && e.KeyChar != 26)
            {
                e.KeyChar = '\0';
            }
        }

        /// <summary>
        /// Textbox không có khoảng trắng.
        /// </summary>
        public static void TxtContinuity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsWhiteSpace(e.KeyChar))
            {
                e.KeyChar = '\0';
            }
        }
        #endregion

        #region RadioButton Link
        /// <summary>
        /// Link trạng thái từ radiobutton sang combobox.
        /// </summary>
        public static void RdoLinkCmb_CheckedChanged(object sender, EventArgs e)
        {
            var rdo = (YANRadioButton)sender;
            ((YANComboBox)rdo.FindForm().Controls.Find($"comboBox{rdo.Name.Substring("radioButton".Length)}", true).FirstOrDefault()).Enabled = rdo.Checked;
        }

        /// <summary>
        /// Link trạng thái từ radiobutton sang groupbox.
        /// </summary>
        public static void RdoLinkGrp_CheckedChanged(object sender, EventArgs e)
        {
            var rdo = (YANRadioButton)sender;
            ((GroupBox)rdo.FindForm().Controls.Find($"groupBox{rdo.Name.Substring("radioButton".Length)}", true).FirstOrDefault()).Enabled = rdo.Checked;
        }

        /// <summary>
        /// Link trạng thái từ radiobutton sang textbox.
        /// </summary>
        public static void RdoLinkTxt_CheckedChanged(object sender, EventArgs e)
        {
            var rdo = (YANRadioButton)sender;
            ((YANTextBox)rdo.FindForm().Controls.Find($"textBox{rdo.Name.Substring("radioButton".Length)}", true).FirstOrDefault()).Enabled = rdo.Checked;
        }
        #endregion

        #region ToggleButton Link
        /// <summary>
        /// Link trạng thái từ togglebutton sang datepicker.
        /// </summary>
        public static void TgbtnLinkDp_CheckedChanged(object sender, EventArgs e)
        {
            var tgbtn = (YANToggleButton)sender;
            ((YANDatePicker)tgbtn.FindForm().Controls.Find($"datePicker{tgbtn.Name.Substring("toggleButton".Length)}", true).FirstOrDefault()).Enabled = tgbtn.Checked;
        }

        /// <summary>
        /// Link trạng thái từ togglebutton sang combobox.
        /// </summary>
        public static void TgbtnLinkCmb_CheckedChanged(object sender, EventArgs e)
        {
            var tgbtn = (YANToggleButton)sender;
            ((YANComboBox)tgbtn.FindForm().Controls.Find($"comboBox{tgbtn.Name.Substring("toggleButton".Length)}", true).FirstOrDefault()).Enabled = tgbtn.Checked;
        }
        #endregion
    }
}
