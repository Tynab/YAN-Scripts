using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANUpdater
    {
        #region Fields
        public Panel _pnl_Prg_;
        public Label _lbl_Capacity_;
        public Label _lbl_Percent_;
        private FormUpdater _frm_Updater;
        private Thread _thread;
        #endregion

        #region Methods
        //loading process
        private void LoadingPrc(object parent)
        {
            _frm_Updater = new FormUpdater();
            _pnl_Prg_ = _frm_Updater.panelProgressBar;
            _lbl_Capacity_ = _frm_Updater.labelCapacity;
            _lbl_Percent_ = _frm_Updater.labelPercent;
            _frm_Updater.ShowDialog();
        }

        /// <summary>
        /// Bật form updater.
        /// </summary>
        public void OnLoader()
        {
            _thread = new Thread(new ParameterizedThreadStart(LoadingPrc));
            _thread.Start();
        }

        /// <summary>
        /// Tắt form updater.
        /// </summary>
        public void OffLoader()
        {
            if (_frm_Updater != null)
            {
                _frm_Updater.BeginInvoke(new ThreadStart(_frm_Updater.FrmCloseToken));
                _frm_Updater = null;
                _thread = null;
            }
        }
        #endregion
    }
}
