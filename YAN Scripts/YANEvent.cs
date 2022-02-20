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
        private static bool _moveForm;
        private static Point _lastLocation;

        /// <summary>
        /// Focus the control used move form.
        /// </summary>
        public static void MoveForm_MouseDown(object sender, MouseEventArgs e)
        {
            //sound
            _sound_Change_.Play();
            //action
            _moveForm = true;
            _lastLocation = e.Location;
            ((Control)sender).FindForm().Opacity = 0.7;
        }

        /// <summary>
        /// Moving the control.
        /// </summary>
        public static void MoveForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (_moveForm)
            {
                var form = ((Control)sender).FindForm();
                form.Location = new Point(form.Location.X - _lastLocation.X + e.X, form.Location.Y - _lastLocation.Y + e.Y);
                form.Update();
            }
        }

        /// <summary>
        /// Finish move.
        /// </summary>
        public static void MoveForm_MouseUp(object sender, MouseEventArgs e)
        {
            _moveForm = false;
            ((Control)sender).FindForm().Opacity = 1;
        }
        #endregion

        #region Common
        /// <summary>
        /// Active effect when Click icon button.
        /// </summary>
        public static void ButtonICO_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            button.Hide();
            ShowAnimator(button, Scale, _animatorSpeed_ * 2);
        }
        #endregion

        #region Button IMG
        /// <summary>
        /// Effect when the image button hover.
        /// </summary>
        public static void ButtonIMG_MouseEnter(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BorderSize = 1;
            SetHighLightLabel(button, "button", button.BorderColor, true);
        }

        /// <summary>
        /// Restore default state for the image button.
        /// </summary>
        public static void ButtonIMG_MouseLeave(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BorderSize = 0;
            SetHighLightLabel(button, "button", button.ForeColor, true);
        }
        #endregion

        #region Button Enable Changed Color
        /// <summary>
        /// Green style fill.
        /// </summary>
        public static void ButtonR113G175B74F_EnabledChanged(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BackColor = button.Enabled ? FromArgb(113, 175, 74) : Gray;
        }

        /// <summary>
        /// Yellow style fill.
        /// </summary>
        public static void ButtonR230G192B59F_EnabledChanged(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BackColor = button.Enabled ? FromArgb(230, 192, 59) : Gray;
        }

        /// <summary>
        /// Violet style fill.
        /// </summary>
        public static void ButtonR103G84B218F_EnabledChanged(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BackColor = button.Enabled ? FromArgb(103, 84, 218) : Gray;
        }

        /// <summary>
        /// Red style fill.
        /// </summary>
        public static void ButtonR229G51B79F_EnabledChanged(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BackColor = button.Enabled ? FromArgb(229, 51, 79) : Gray;
        }

        /// <summary>
        /// Orange style fill.
        /// </summary>
        public static void ButtonR235G92B48F_EnabledChanged(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BackColor = button.Enabled ? FromArgb(235, 92, 48) : Gray;
        }

        /// <summary>
        /// Orange icon style fill.
        /// </summary>
        public static void ButtonR235G107B60F_EnabledChanged(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BackColor = button.Enabled ? FromArgb(235, 107, 60) : Gray;
        }

        /// <summary>
        /// Blue style fill.
        /// </summary>
        public static void ButtonR10G124B235F_EnabledChanged(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BackColor = button.Enabled ? FromArgb(10, 124, 235) : Gray;
        }

        /// <summary>
        /// Blue icon style fill.
        /// </summary>
        public static void ButtonR45G85B205F_EnabledChanged(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BackColor = button.Enabled ? FromArgb(45, 85, 205) : Gray;
        }

        /// <summary>
        /// Green style border.
        /// </summary>
        public static void ButtonMediumSeaGreenB_EnabledChanged(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BorderColor = button.Enabled ? MediumSeaGreen : Gray;
        }

        /// <summary>
        /// Green icon style fill.
        /// </summary>
        public static void ButtonR40G159B93F_EnabledChanged(object sender, EventArgs e)
        {
            var button = (YANButton)sender;
            button.BackColor = button.Enabled ? FromArgb(40, 159, 93) : Gray;
        }
        #endregion

        #region ComboBox Enable Changed Color
        /// <summary>
        /// Green style fill.
        /// </summary>
        public static void ComboBoxR38G126B86F_EnabledChanged(object sender, EventArgs e)
        {
            var comboBox = (YANComboBox)sender;
            if (comboBox.Enabled)
            {
                comboBox.BackColor = FromArgb(38, 126, 86);
                comboBox.IconColor = White;
            }
            else
            {
                comboBox.BackColor = Gray;
                comboBox.IconColor = Gray;
            }
        }

        /// <summary>
        /// Violet style border.
        /// </summary>
        public static void ComboBoxMediumPurpleB_EnabledChanged(object sender, EventArgs e)
        {
            var comboBox = (YANComboBox)sender;
            if (comboBox.Enabled)
            {
                comboBox.BorderColor = MediumPurple;
                comboBox.IconColor = Cyan;
            }
            else
            {
                comboBox.BorderColor = Gray;
                comboBox.IconColor = Gray;
            }
        }
        #endregion

        #region DatePicker Enable Changed Color
        /// <summary>
        /// Blue style fill.
        /// </summary>
        public static void DatePickerR68G114B196F_EnabledChanged(object sender, EventArgs e)
        {
            var datePicker = (YANDatePicker)sender;
            datePicker.TextColor = ColorInvert(datePicker.TextColor);
            datePicker.SkinColor = datePicker.Enabled ? FromArgb(68, 114, 196) : Gray;
        }

        /// <summary>
        /// Pink style border.
        /// </summary>
        public static void DatePickerWhiteF_EnabledChanged(object sender, EventArgs e)
        {
            var datePicker = (YANDatePicker)sender;
            datePicker.SkinColor = datePicker.Enabled ? White : LightGray;
        }

        /// <summary>
        /// White style border.
        /// </summary>
        public static void DatePickerPaleVioletRedB_EnabledChanged(object sender, EventArgs e)
        {
            var datePicker = (YANDatePicker)sender;
            datePicker.TextColor = ColorInvert(datePicker.TextColor);
            datePicker.BorderColor = datePicker.Enabled ? PaleVioletRed : Gray;
        }
        #endregion

        #region NumBox Enable Changed Color
        /// <summary>
        /// Blue style border.
        /// </summary>
        public static void NumBoxCyanB_EnabledChanged(object sender, EventArgs e)
        {
            var numBox = (YANNumBox)sender;
            numBox.BorderColor = numBox.Enabled ? Cyan : Gray;
        }

        /// <summary>
        /// Violet style border.
        /// </summary>
        public static void NumBoxMediumSlateBlueB_EnabledChanged(object sender, EventArgs e)
        {
            var numBox = (YANNumBox)sender;
            numBox.BorderColor = numBox.Enabled ? MediumSlateBlue : Gray;
        }

        /// <summary>
        /// Pink style border.
        /// </summary>
        public static void NumBoxPaleVioletRedB_EnabledChanged(object sender, EventArgs e)
        {
            var numBox = (YANNumBox)sender;
            numBox.BorderColor = numBox.Enabled ? PaleVioletRed : Gray;
        }

        /// <summary>
        /// Date style border.
        /// </summary>
        public static void NumBoxDodgerBlueB_EnabledChanged(object sender, EventArgs e)
        {
            var numBox = (YANNumBox)sender;
            numBox.BorderColor = numBox.Enabled ? DodgerBlue : Gray;
        }
        #endregion

        #region TextBox Enable Changed Color
        /// <summary>
        /// Blue style border.
        /// </summary>
        public static void TextBoxCyanB_EnabledChanged(object sender, EventArgs e)
        {
            var textBox = (YANTextBox)sender;
            textBox.BorderColor = textBox.Enabled ? Cyan : Gray;
        }
        #endregion

        #region TextBox Focus
        /// <summary>
        /// Capitalize each word.
        /// </summary>
        public static void TextBoxCEW_Leave(object sender, EventArgs e)
        {
            var textBox = (YANTextBox)sender;
            var strConvert = textBox.Txt;
            if (!string.IsNullOrWhiteSpace(strConvert))
            {
                textBox.Txt = ToTitleCaseUpgrade(strConvert);
            }
        }

        /// <summary>
        /// Upcase.
        /// </summary>
        public static void TextBoxUC_Leave(object sender, EventArgs e)
        {
            var textBox = (YANTextBox)sender;
            var strConvert = textBox.Txt;
            if (!string.IsNullOrWhiteSpace(strConvert))
            {
                textBox.Txt = strConvert.ToUpper();
            }
        }

        /// <summary>
        /// Lowcase.
        /// </summary>
        public static void TextBoxLC_Leave(object sender, EventArgs e)
        {
            var textBox = (YANTextBox)sender;
            var strConvert = textBox.Txt;
            if (!string.IsNullOrWhiteSpace(strConvert))
            {
                textBox.Txt = strConvert.ToLower();
            }
        }
        #endregion

        #region TextBox Input
        /// <summary>
        /// Allow only number key.
        /// </summary>
        public static void TextBoxNumeric_Keypress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != 8 && e.KeyChar != 3 && e.KeyChar != 22 && e.KeyChar != 24 && e.KeyChar != 1 && e.KeyChar != 26)
            {
                e.KeyChar = '\0';
            }
        }

        /// <summary>
        /// Denied space.
        /// </summary>
        public static void TextBoxContinuous_KeyPress(object sender, KeyPressEventArgs e)
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
        public static void R2C_CheckedChanged(object sender, EventArgs e)
        {
            var radioButton = (YANRadioButton)sender;
            ((YANComboBox)radioButton.FindForm().Controls.Find($"comboBox{radioButton.Name.Substring("radioButton".Length)}", true).FirstOrDefault()).Enabled = radioButton.Checked;
        }

        /// <summary>
        /// Change state groupbox linked radiobutton check.
        /// </summary>
        public static void R2G_CheckedChanged(object sender, EventArgs e)
        {
            var radioButton = (YANRadioButton)sender;
            ((GroupBox)radioButton.FindForm().Controls.Find($"groupBox{radioButton.Name.Substring("radioButton".Length)}", true).FirstOrDefault()).Enabled = radioButton.Checked;
        }

        /// <summary>
        /// Change state groupbox linked radiobutton check.
        /// </summary>
        public static void R2T_CheckedChanged(object sender, EventArgs e)
        {
            var radioButton = (YANRadioButton)sender;
            ((YANTextBox)radioButton.FindForm().Controls.Find($"textBox{radioButton.Name.Substring("radioButton".Length)}", true).FirstOrDefault()).Enabled = radioButton.Checked;
        }
        #endregion

        #region ToggleButton Link
        /// <summary>
        /// Change state label and datepicker linked togglebutton check.
        /// </summary>
        public static void B2D_CheckedChanged(object sender, EventArgs e)
        {
            var button = (YANToggleButton)sender;
            ((YANDatePicker)button.FindForm().Controls.Find($"datePicker{button.Name.Substring("toggleButton".Length)}", true).FirstOrDefault()).Enabled = button.Checked;
        }

        /// <summary>
        /// Change state combobox linked togglebutton check.
        /// </summary>
        public static void B2C_CheckedChanged(object sender, EventArgs e)
        {
            var button = (YANToggleButton)sender;
            ((YANComboBox)button.FindForm().Controls.Find($"comboBox{button.Name.Substring("toggleButton".Length)}", true).FirstOrDefault()).Enabled = button.Checked;
        }
        #endregion
    }
}
