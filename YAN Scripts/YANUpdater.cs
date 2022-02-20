using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANUpdater
    {
        #region Fields
        public Panel _panel_ProgressBar_;
        public Label _label_Capacity_;
        public Label _label_Percent_;
        private FormUpdater _form_Updater;
        private Thread _thread;
        #endregion

        #region Methods
        //loading process
        private void LoadingProcess(object parent)
        {
            _form_Updater = new FormUpdater();
            _panel_ProgressBar_ = _form_Updater.panelProgressBar;
            _label_Capacity_ = _form_Updater.labelCapacity;
            _label_Percent_ = _form_Updater.labelPercent;
            _form_Updater.ShowDialog();
        }

        /// <summary>
        /// Turn on updater form.
        /// </summary>
        public void OnLoader()
        {
            _thread = new Thread(new ParameterizedThreadStart(LoadingProcess));
            _thread.Start();
        }

        /// <summary>
        /// Turn off updater form.
        /// </summary>
        public void OffLoader()
        {
            if (_form_Updater != null)
            {
                _form_Updater.BeginInvoke(new ThreadStart(_form_Updater.CloseToken));
                _form_Updater = null;
                _thread = null;
            }
        }
        #endregion
    }
}
