using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANWaiter
    {
        #region Fields
        private FormWaiter _form_Waiter;
        private Thread _thread;
        private int _radius;
        private bool _isTop;
        #endregion

        #region Methods
        //loading process
        private void LoadingProcess(object parent)
        {
            _form_Waiter = new FormWaiter((Form)parent, _radius, _isTop);
            _form_Waiter.ShowDialog();
        }

        /// <summary>
        /// Turn on waiter form.
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
        /// Turn off waiter form.
        /// </summary>
        public void OffLoader()
        {
            if (_form_Waiter != null)
            {
                _form_Waiter.BeginInvoke(new ThreadStart(_form_Waiter.CloseToken));
                _form_Waiter = null;
                _thread = null;
            }
        }
        #endregion
    }
}
