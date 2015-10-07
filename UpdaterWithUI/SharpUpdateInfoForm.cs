using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpdaterWithUI
{
    internal partial class SharpUpdateInfoForm : Form
    {
        public SharpUpdateInfoForm(ISharpUpdater applicationInfo, SharpUpdateXML updateInfo)
        {
            InitializeComponent();

            if (applicationInfo.ApplicationIcon != null)
                this.Icon = applicationInfo.ApplicationIcon;

            this.Text = applicationInfo.ApplicationName + " - Update Info";
            this.lbVersions.Text = String.Format("Current Version: {0}\nUpdateVersion: {1}", applicationInfo.ApplicationAssembly.GetName().Version.ToString(), updateInfo.Version.ToString());
            this.LBDescription.Text = updateInfo.Description;
        }

        private void B_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
