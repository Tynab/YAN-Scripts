using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANUpdater
    {
        #region Fields
        public Panel _pnlPrg_;
        public Label _lblCapacity_;
        public Label _lblPercent_;
        private FormUpdater _frmUpdater;
        private Thread _thread;
        #endregion

        #region Methods
        //loading process
        private void LoadingPrc(object parent)
        {
            _frmUpdater = new FormUpdater();
            _pnlPrg_ = _frmUpdater.panelProgressBar;
            _lblCapacity_ = _frmUpdater.labelCapacity;
            _lblPercent_ = _frmUpdater.labelPercent;
            _frmUpdater.ShowDialog();
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
            if (_frmUpdater != null)
            {
                _frmUpdater.BeginInvoke(new ThreadStart(_frmUpdater.FrmCloseToken));
                _frmUpdater = null;
                _thread = null;
            }
        }
        #endregion
    }
}
