using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANLoader
    {
        #region Fields
        private FormLoader _form_Loader;
        private Thread _thread;
        private int _radius;
        private bool _isTop;
        #endregion

        #region Methods
        private void LoadingProcess(object parent)
        {
            _form_Loader = new FormLoader((Form)parent, _radius, _isTop);
            _form_Loader.ShowDialog();
        }

        /// <summary>
        /// Turn on loader form.
        /// </summary>
        /// <param name="form">Parent form.</param>
        /// <param name="corner">Radius border.</param>
        /// <param name="onTop">Enable top most.</param>
        public void OnLoader(Form form, int corner, bool onTop)
        {
            _thread = new Thread(new ParameterizedThreadStart(LoadingProcess));
            _thread.Start(form);
            _radius = corner;
            _isTop = onTop;
        }

        /// <summary>
        /// Turn off loader form.
        /// </summary>
        public void OffLoader()
        {
            if (_form_Loader != null)
            {
                _form_Loader.BeginInvoke(new ThreadStart(_form_Loader.CloseToken));
                _form_Loader = null;
                _thread = null;
            }
        }
        #endregion
    }
}
