using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANLoader
    {
        #region Fields
        private FormLoader _frm_Loader;
        private Thread _thread;
        private int _rad;
        private bool _isTop;
        #endregion

        #region Methods
        private void LoadingPrc(object parent)
        {
            _frm_Loader = new FormLoader((Form)parent, _rad, _isTop);
            _frm_Loader.ShowDialog();
        }

        /// <summary>
        /// Turn on loader form.
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
        /// Turn off loader form.
        /// </summary>
        public void OffLoader()
        {
            if (_frm_Loader != null)
            {
                _frm_Loader.BeginInvoke(new ThreadStart(_frm_Loader.FrmCloseToken));
                _frm_Loader = null;
                _thread = null;
            }
        }
        #endregion
    }
}
