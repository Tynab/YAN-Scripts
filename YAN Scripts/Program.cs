﻿using System;
using System.Windows.Forms;

namespace YAN_Scripts
{
    internal static class Program
    {
        //main
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
