using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HPLStudio
{
    public partial class BrowserForm : Form
    {
        public BrowserForm()
        {
            InitializeComponent();
        }

        /*
           var about_md = Encoding.UTF8.GetString(Resources.About);
           var (script, md) = ExtractJsFromMd(about_md);

           AboutHtml = Markdown.ToHtml(about_md, Form1.MarkdownPipeline);
           AboutHtml = PlaceBackExtractedScriptToHtml(AboutHtml, script);
           AboutHtml = FixHtmlLocalImgPath(AboutHtml);
           webBrowser1.DocumentText = AboutHtml;

         */

        public static string FixHtmlLocalImgPath(string html)
        {
            var path = $"file://{Application.StartupPath.Replace("\\", "/")}";
            var imgInjection = $"<img src=\"{path}/images";
            return html.Replace("<img src=\"/images", imgInjection);
        }

        public static (string, string) ExtractJsFromMd(string md)
        {
            var scr_start = md.IndexOf("<script", StringComparison.Ordinal);
            var scr_end = md.IndexOf("</script>", StringComparison.Ordinal) + 9;
            var script = "";
            if (scr_start > 0 && scr_end > 0)
            {
                script = md.Substring(scr_start, (scr_end - scr_start));
                md = md.Replace(script, "<script/>");
            }

            return (script, md);
        }

        public static string PlaceBackExtractedScriptToHtml(string html, string script)
        {
            return html.Replace("<p>\ufeff<script/></p>", script);
        }

    }


}
