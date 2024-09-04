using FarsiLibrary.Win;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfiumViewer;

namespace HPLStudio
{
    public partial class AcroPDFForm : Form
    {
        public AcroPDFForm()
        {
            InitializeComponent();
        }

        public static AcroPDFForm instance = null;

        public static void ShowFile(string fileName)
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new AcroPDFForm();
            }

            if (instance.FindFileTab(fileName) is { } tab)
            {
                if (tab.Controls.Count == 0 || tab.Controls[0].IsDisposed)
                {
                    tab.Controls.Clear();
                    tab.Controls.Add(OpenPfdDoc(fileName));
                    tab.Controls.SetChildIndex(tab.Controls[0], 0);

                }
                instance.faTabStrip.SelectedItem = tab;
            }
            else
            {
                instance.CreateTab(fileName);
            }
            instance.Show();
        }

        private FATabStripItem FindFileTab(string filename)
        {
            foreach (FATabStripItem item in faTabStrip.Items)
            {
                if (item.Tag is string s && s.Replace("/", "\\") == filename.Replace("/", "\\"))
                {
                    return item;
                }
            }
            return null;
        }

        public static PdfViewer OpenPfdDoc(string path)
        {
            var viewer = new PdfViewer();
            viewer.Dock = DockStyle.Fill;
            viewer.ShowBookmarks = true;
            viewer.ShowToolbar = true;
            viewer.ZoomMode = PdfViewerZoomMode.FitWidth;
            viewer.Renderer.Load(PdfDocument.Load(path));
            return viewer;
        }

        private void CreateTab(string fileName)
        {
            try
            {
                var fname = System.IO.Path.GetFileName(fileName);
                var viewer = OpenPfdDoc(fileName);
                var tab = new FATabStripItem(fname, viewer);
                faTabStrip.AddTab(tab);
                faTabStrip.SelectedItem = tab;
                tab.Tag = fileName;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    CreateTab(fileName);
            }
        }

    }
}
