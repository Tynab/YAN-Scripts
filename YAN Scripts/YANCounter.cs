using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANCounter
    {
        #region Fields
        public Label _lblPercent_;
        private FormCounter _frmCounter;
        private Thread _thread;
        private int _rad;
        private bool _isTop;
        #endregion

        #region Methods
        //loading process
        private void LoadingPrc(object parent)
        {
            _frmCounter = new FormCounter((Form)parent, _rad, _isTop);
            _lblPercent_ = _frmCounter.labelPercent;
            _frmCounter.ShowDialog();
        }

        /// <summary>
        /// Bật form counter.
        /// </summary>
        /// <param name="frm">Parent form.</param>
        /// <param name="cor">Góc bo của form.</param>
        /// <param name="onTop">Hiển thị đè hoặc không.</param>
        public void OnLoader(Form frm, int cor, bool onTop)
        {
            _thread = new Thread(new ParameterizedThreadStart(LoadingPrc));
            _thread.Start(frm);
            _rad = cor;
            _isTop = onTop;
        }

        /// <summary>
        /// Tắt form counter.
        /// </summary>
        public void OffLoader()
        {
            if (_frmCounter != null)
            {
                _frmCounter.BeginInvoke(new ThreadStart(_frmCounter.FrmCloseToken));
                _frmCounter = null;
                _thread = null;
            }
        }
        #endregion
    }
}
