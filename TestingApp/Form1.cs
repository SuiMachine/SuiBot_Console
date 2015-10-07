using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UpdaterWithUI;

namespace TestingApp
{
    public partial class Form1 : Form, ISharpUpdater
    {
        public Form1()
        {
            InitializeComponent();

            this.label1.Text = this.ApplicationAssembly.GetName().Version.ToString();
        }

        public Assembly ApplicationAssembly
        {
            get { return Assembly.GetExecutingAssembly(); }
        }

        public Icon ApplicationIcon
        {
            get { return this.Icon; }
        }

        public string ApplicationID
        {
            get { return "TestApp"; }
        }

        public string ApplicationName
        {
            get { return "TestApp"; }
        }

        public Form Contex
        {
            get { return this; }
        }

        public Uri UpdateXMLLocation
        {
            get { return new Uri(""); }
        }
    }
}
