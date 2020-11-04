using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Corex.Log;
using Corex.Log.ANLog.FileLog;

namespace WI5000Controller
{
    
    public partial class Form1 : Form
    {
        private void initLog()
        {
            // Logger
            FileProxy fileProxy = new FileProxy(@".\logs\WI-5000.log", 50000, 100);

            if (Corex.Env.DEBUG)
            {
                Logger.Init(true, fileProxy);
            }
            else
            {
                Logger.Init(false, fileProxy);
            }
            Logger.EnableCollapsing(15, 1.0);

        }

        public Form1()
        {
            initLog();
            InitializeComponent();
        }
    }
}
