using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANWaiter
    {
        #region Fields
        private FormWaiter _frm_Waiter;
        private Thread _thread;
        private int _rad;
        private bool _is_Top;
        #endregion

        #region Methods
        //loading process
        private void LoadingPrc(object parent)
        {
            _frm_Waiter = new FormWaiter((Form)parent, _rad, _is_Top);
            _frm_Waiter.ShowDialog();
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
            _is_Top = onTop;
        }

        /// <summary>
        /// Tắt form waiter.
        /// </summary>
        public void OffLoader()
        {
            if (_frm_Waiter != null)
            {
                _frm_Waiter.BeginInvoke(new ThreadStart(_frm_Waiter.FrmCloseToken));
                _frm_Waiter = null;
                _thread = null;
            }
        }
        #endregion
    }
}
