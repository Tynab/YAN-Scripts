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
        private bool _isTop;
        #endregion

        #region Methods
        //loading process
        private void LoadingPrc(object parent)
        {
            _frm_Waiter = new FormWaiter((Form)parent, _rad, _isTop);
            _frm_Waiter.ShowDialog();
        }

        /// <summary>
        /// Turn on waiter form.
        /// </summary>
        /// <param name="frm">Parent form.</param>
        /// <param name="cor">Radius border.</param>
        /// <param name="onTop">Enable top most.</param>
        public void OnLoader(Form frm, int cor, bool onTop)
        {
            _thread = new Thread(new ParameterizedThreadStart(LoadingPrc));
            _thread.Start(frm);
            _rad = cor;
            _isTop = onTop;
        }

        /// <summary>
        /// Turn off waiter form.
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
