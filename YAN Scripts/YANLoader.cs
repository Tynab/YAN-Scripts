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
        private bool _is_Top;
        #endregion

        #region Methods
        private void LoadingPrc(object parent)
        {
            _frm_Loader = new FormLoader((Form)parent, _rad, _is_Top);
            _frm_Loader.ShowDialog();
        }

        /// <summary>
        /// Bật form loader.
        /// </summary>
        /// <param name="frm">Parent form.</param>
        /// <param name="cor">Góc bo của form.</param>
        /// <param name="is_Top">Hiển thị đè hoặc không.</param>
        public void OnLoader(Form frm, int cor, bool is_Top)
        {
            _thread = new Thread(new ParameterizedThreadStart(LoadingPrc));
            _thread.Start(frm);
            _rad = cor;
            _is_Top = is_Top;
        }

        /// <summary>
        /// Tắt form loader.
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
