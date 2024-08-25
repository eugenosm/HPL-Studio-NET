using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HPLStudio.Properties;
using Markdig;

namespace HPLStudio
{

    public partial class AboutForm : Form
    {
        public static string AboutHtml;

        private void adjustLabelMaxLen(ref Label label)
        {
            var ms = label.MaximumSize;
            ms.Width = this.ClientRectangle.Width - label.Left - 8;
            label.MaximumSize = ms;
        }
        public AboutForm()
        {
            InitializeComponent();
            adjustLabelMaxLen(ref productLabel);
            adjustLabelMaxLen(ref companyLabel);
            adjustLabelMaxLen(ref descrLabel);
            adjustLabelMaxLen(ref cpRghtLabel);
            productLabel.Text = string.Format(Resources.STR_ProductVer, ProductName, ProductVersion);
            companyLabel.Text = CompanyName;
            var attr = Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyDescriptionAttribute));
            var description = ((AssemblyDescriptionAttribute)attr).Description;
            descrLabel.Text = description;
            var cpRghtAttr = Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyCopyrightAttribute));
            cpRghtLabel.Text = ((AssemblyCopyrightAttribute)cpRghtAttr).Copyright;

        }
    }
}
