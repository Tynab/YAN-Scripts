using System.Threading;
using System.Windows.Forms;
using YAN_Controls;

namespace YAN_Scripts
{
    public class YANCounter
    {
        #region Fields
        public Label _label_Percent_;
        private FormCounter _form_Counter;
        private Thread _thread;
        private int _radius;
        private bool _isTop;
        #endregion

        #region Methods
        //loading process
        private void LoadingProcess(object parent)
        {
            _form_Counter = new FormCounter((Form)parent, _radius, _isTop);
            _label_Percent_ = _form_Counter.labelPercent;
            _form_Counter.ShowDialog();
        }

        /// <summary>
        /// Turn on counter form.
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
        /// Turn off counter form.
        /// </summary>
        public void OffLoader()
        {
            if (_form_Counter != null)
            {
                _form_Counter.BeginInvoke(new ThreadStart(_form_Counter.CloseToken));
                _form_Counter = null;
                _thread = null;
            }
        }
        #endregion
    }
}
