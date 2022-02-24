using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YAN_Controls;
using static AnimatorNS.AnimationType;
using static System.Drawing.Color;
using static YAN_Scripts.YANConstant;
using static YAN_Scripts.YANMethod;

namespace YAN_Scripts
{
    public class YANEvent
    {
        #region Form Move
        //fields
        private static bool _moveFrm;
        private static Point _lastLocation;

        /// <summary>
        /// Focus the control used move form.
        /// </summary>
        public static void MoveFrm_MouseDown(object sender, MouseEventArgs e)
        {
            //sound
            _sound_Change_.Play();
            //action
            _moveFrm = true;
            _lastLocation = e.Location;
            ((Control)sender).FindForm().Opacity = 0.7;
        }

        /// <summary>
        /// Moving the control.
        /// </summary>
        public static void MoveFrm_MouseMove(object sender, MouseEventArgs e)
        {
            if (_moveFrm)
            {
                var frm = ((Control)sender).FindForm();
                frm.Location = new Point(frm.Location.X - _lastLocation.X + e.X, frm.Location.Y - _lastLocation.Y + e.Y);
                frm.Update();
            }
        }

        /// <summary>
        /// Finish move.
        /// </summary>
        public static void MoveFrm_MouseUp(object sender, MouseEventArgs e)
        {
            _moveFrm = false;
            ((Control)sender).FindForm().Opacity = 1;
        }
        #endregion

        #region Button Icon
        /// <summary>
        /// Active effect when Click icon button.
        /// </summary>
        public static void BtnIc_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Hide();
            ShowAnimator(btn, Scale, _animatorSpeed_ * 2);
        }
        #endregion

        #region Button Image
        /// <summary>
        /// Effect when the image button hover.
        /// </summary>
        public static void BtnImg_MouseEnter(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BorderSize = 1;
            SetHighLightLbl(btn, "button", btn.BorderColor, true);
        }

        /// <summary>
        /// Restore default state for the image button.
        /// </summary>
        public static void BtnImg_MouseLeave(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BorderSize = 0;
            SetHighLightLbl(btn, "button", btn.ForeColor, true);
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
        /// Orange icon style fill.
        /// </summary>
        public static void BtnR235G107B60F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(235, 107, 60) : Gray;
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
        /// Blue icon style fill.
        /// </summary>
        public static void BtnR45G85B205F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(45, 85, 205) : Gray;
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

        /// <summary>
        /// Green icon style fill.
        /// </summary>
        public static void BtnR40G159B93F_EnabledChanged(object sender, EventArgs e)
        {
            var btn = (YANButton)sender;
            btn.BackColor = btn.Enabled ? FromArgb(40, 159, 93) : Gray;
        }
        #endregion

        #region ComboBox Enable Changed Color
        /// <summary>
        /// Green style fill.
        /// </summary>
        public static void CmbR38G126B86F_EnabledChanged(object sender, EventArgs e)
        {
            var cmb = (YANComboBox)sender;
            if (cmb.Enabled)
            {
                cmb.BackColor = FromArgb(38, 126, 86);
                cmb.IconColor = White;
            }
            else
            {
                cmb.BackColor = Gray;
                cmb.IconColor = Gray;
            }
        }

        /// <summary>
        /// Violet style border.
        /// </summary>
        public static void CmbMediumPurpleB_EnabledChanged(object sender, EventArgs e)
        {
            var cmb = (YANComboBox)sender;
            if (cmb.Enabled)
            {
                cmb.BorderColor = MediumPurple;
                cmb.IconColor = Cyan;
            }
            else
            {
                cmb.BorderColor = Gray;
                cmb.IconColor = Gray;
            }
        }
        #endregion

        #region DatePicker Enable Changed Color
        /// <summary>
        /// Blue style fill.
        /// </summary>
        public static void DpR68G114B196F_EnabledChanged(object sender, EventArgs e)
        {
            var dp = (YANDatePicker)sender;
            dp.TextColor = ClInvert(dp.TextColor);
            dp.SkinColor = dp.Enabled ? FromArgb(68, 114, 196) : Gray;
        }

        /// <summary>
        /// Pink style border.
        /// </summary>
        public static void DpWhiteF_EnabledChanged(object sender, EventArgs e)
        {
            var dp = (YANDatePicker)sender;
            dp.SkinColor = dp.Enabled ? White : LightGray;
        }

        /// <summary>
        /// White style border.
        /// </summary>
        public static void DpPaleVioletRedB_EnabledChanged(object sender, EventArgs e)
        {
            var dp = (YANDatePicker)sender;
            dp.TextColor = ClInvert(dp.TextColor);
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
        /// Capitalize each word.
        /// </summary>
        public static void TxtCEW_Leave(object sender, EventArgs e)
        {
            var txt = (YANTextBox)sender;
            var t_txt = txt.Txt;
            if (!string.IsNullOrWhiteSpace(t_txt))
            {
                txt.Txt = ToTitleCaseUpgrade(t_txt);
            }
        }

        /// <summary>
        /// Upcase.
        /// </summary>
        public static void TxtUC_Leave(object sender, EventArgs e)
        {
            var txt = (YANTextBox)sender;
            var t_txt = txt.Txt;
            if (!string.IsNullOrWhiteSpace(t_txt))
            {
                txt.Txt = t_txt.ToUpper();
            }
        }

        /// <summary>
        /// Lowcase.
        /// </summary>
        public static void TxtLC_Leave(object sender, EventArgs e)
        {
            var txt = (YANTextBox)sender;
            var t_txt = txt.Txt;
            if (!string.IsNullOrWhiteSpace(t_txt))
            {
                txt.Txt = t_txt.ToLower();
            }
        }
        #endregion

        #region TextBox Input
        /// <summary>
        /// Allow only number key.
        /// </summary>
        public static void TxtNumeric_Keypress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != 8 && e.KeyChar != 3 && e.KeyChar != 22 && e.KeyChar != 24 && e.KeyChar != 1 && e.KeyChar != 26)
            {
                e.KeyChar = '\0';
            }
        }

        /// <summary>
        /// Denied space.
        /// </summary>
        public static void TxtContinuous_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsWhiteSpace(e.KeyChar))
            {
                e.KeyChar = '\0';
            }
        }
        #endregion

        #region RadioButton Link
        /// <summary>
        /// Change state combobox linked radiobutton check.
        /// </summary>
        public static void Rdo2Cmb_CheckedChanged(object sender, EventArgs e)
        {
            var rdo = (YANRadioButton)sender;
            ((YANComboBox)rdo.FindForm().Controls.Find($"comboBox{rdo.Name.Substring("radioButton".Length)}", true).FirstOrDefault()).Enabled = rdo.Checked;
        }

        /// <summary>
        /// Change state groupbox linked radiobutton check.
        /// </summary>
        public static void Rdo2Grp_CheckedChanged(object sender, EventArgs e)
        {
            var rdo = (YANRadioButton)sender;
            ((GroupBox)rdo.FindForm().Controls.Find($"groupBox{rdo.Name.Substring("radioButton".Length)}", true).FirstOrDefault()).Enabled = rdo.Checked;
        }

        /// <summary>
        /// Change state textbox linked radiobutton check.
        /// </summary>
        public static void Rdo2Txt_CheckedChanged(object sender, EventArgs e)
        {
            var rdo = (YANRadioButton)sender;
            ((YANTextBox)rdo.FindForm().Controls.Find($"textBox{rdo.Name.Substring("radioButton".Length)}", true).FirstOrDefault()).Enabled = rdo.Checked;
        }
        #endregion

        #region ToggleButton Link
        /// <summary>
        /// Change state label and datepicker linked togglebutton check.
        /// </summary>
        public static void Tgbtn2Dp_CheckedChanged(object sender, EventArgs e)
        {
            var tgbtn = (YANToggleButton)sender;
            ((YANDatePicker)tgbtn.FindForm().Controls.Find($"datePicker{tgbtn.Name.Substring("toggleButton".Length)}", true).FirstOrDefault()).Enabled = tgbtn.Checked;
        }

        /// <summary>
        /// Change state combobox linked togglebutton check.
        /// </summary>
        public static void Tgbtn2Cmb_CheckedChanged(object sender, EventArgs e)
        {
            var tgbtn = (YANToggleButton)sender;
            ((YANComboBox)tgbtn.FindForm().Controls.Find($"comboBox{tgbtn.Name.Substring("toggleButton".Length)}", true).FirstOrDefault()).Enabled = tgbtn.Checked;
        }
        #endregion
    }
}
