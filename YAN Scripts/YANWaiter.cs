using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANWaiter
    {
        #region Fields
        private FormWaiter _frmWaiter;
        private Thread _thread;
        private int _rad;
        private bool _isTop;
        #endregion

        #region Methods
        //loading process
        private void LoadingPrc(object parent)
        {
            _frmWaiter = new FormWaiter((Form)parent, _rad, _isTop);
            _frmWaiter.ShowDialog();
        }

        /// <summary>
        /// Bật form waiter.
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
        /// Tắt form waiter.
        /// </summary>
        public void OffLoader()
        {
            if (_frmWaiter != null)
            {
                _frmWaiter.BeginInvoke(new ThreadStart(_frmWaiter.FrmCloseToken));
                _frmWaiter = null;
                _thread = null;
            }
        }
        #endregion
    }
}
