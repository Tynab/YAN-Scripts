using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANCounter
    {
        #region Fields
        public Label _lbl_Percent_;
        private FormCounter _frm_Counter;
        private Thread _thread;
        private int _rad;
        private bool _isTop;
        #endregion

        #region Methods
        //loading process
        private void LoadingPrc(object parent)
        {
            _frm_Counter = new FormCounter((Form)parent, _rad, _isTop);
            _lbl_Percent_ = _frm_Counter.labelPercent;
            _frm_Counter.ShowDialog();
        }

        /// <summary>
        /// Turn on counter form.
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
        /// Turn off counter form.
        /// </summary>
        public void OffLoader()
        {
            if (_frm_Counter != null)
            {
                _frm_Counter.BeginInvoke(new ThreadStart(_frm_Counter.FrmCloseToken));
                _frm_Counter = null;
                _thread = null;
            }
        }
        #endregion
    }
}
