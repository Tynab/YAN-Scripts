using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANLoader
    {
        #region Fields
        private FormLoader _frmLoader;
        private Thread _thread;
        private int _rad;
        private bool _isTop;
        #endregion

        #region Methods
        private void LoadingPrc(object parent)
        {
            _frmLoader = new FormLoader((Form)parent, _rad, _isTop);
            _frmLoader.ShowDialog();
        }

        /// <summary>
        /// Bật form loader.
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
        /// Tắt form loader.
        /// </summary>
        public void OffLoader()
        {
            if (_frmLoader != null)
            {
                _frmLoader.BeginInvoke(new ThreadStart(_frmLoader.FrmCloseToken));
                _frmLoader = null;
                _thread = null;
            }
        }
        #endregion
    }
}
