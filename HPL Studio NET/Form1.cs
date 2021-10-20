using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;
using FarsiLibrary.Win;
using HPLStudio.Properties;
using Microsoft.Win32;

namespace HPLStudio
{
    public partial class Form1 : Form
    {
        //TODO: Добавить PIN, BUS к defined names, и в автоподстановку

        public string FileName => tsFiles.SelectedItem.Tag as string;
        public bool isFileNew = true;
        public string progDef = "";
        public string progString = "";
        public string pathString = "";
        public string exeString = "";
        public string helpSting = "";
        public string hplSubDir = "/hpl";
        public string defaultHplExtension = ".hpl";
        public string fileExtensions = Resources.STR_FileFilters;
//        public string xmlString = "";

//        const string sectionRegex = @"^\[\w+\].*$";

        public IniFile iniFile;


        private const int SW_MAXIMIZE = 3;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private readonly RecentFileHandler _recentFileHandler;
        public Form1()
        {
            InitializeComponent();
            _recentFileHandler = new RecentFileHandler();
            this._recentFileHandler.RecentFileToolStripItem = this.recentFilesMenuItem;
        }

        private void textEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            //clear folding markers
            e.ChangedRange.ClearFoldingMarkers();
            e.ChangedRange.SetFoldingMarkers("{", "}");
            e.ChangedRange.SetFoldingMarkers("#struct", "#ends");
            e.ChangedRange.SetFoldingMarkers("#macro", "#endm");
            //
            foreach (var r in e.ChangedRange.GetRangesByLines(Preprocessor.SectionRegex, RegexOptions.None))
            {
                if (r.Start.iLine > 0) r.tb[r.Start.iLine - 1].FoldingEndMarker = "section";
                r.tb[r.Start.iLine].FoldingStartMarker = "section";
            }
        }

        private void OpenFile(string fileName)
        {
            if (tsFiles.Items.DrawnCount == 0)
            {
                CreateTab(fileName);
            }
            TextEditor.Text = System.IO.File.ReadAllText(fileName);
            //FileName = fileName;
            isFileNew = false;
        }

        private void openFileMenuItem_Click(object sender, EventArgs e)
        {
            // 
            openFileDialog1.Title = Resources.STR_OpenDocument;
            openFileDialog1.InitialDirectory = Application.StartupPath;
            openFileDialog1.Filter = fileExtensions;
                //"HPM files|*.hpm|HPL files|*.hpl|Orange 4, HPL files|*.hp4|Orange 5, HPL files|*.hp5|HPF files|*.hpf|Все файлы|*.*";
            openFileDialog1.DefaultExt = "hpm";
            openFileDialog1.FilterIndex = 1;
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                OpenFile(openFileDialog1.FileName);
                _recentFileHandler.AddFile(openFileDialog1.FileName);
            }
        }

        private FastColoredTextBox TextEditor
        {
            get => tsFiles.SelectedItem?.Controls[0] as FastColoredTextBox;

            set
            {
                tsFiles.SelectedItem = (value.Parent as FATabStripItem);
                value.Focus();
            }
        }

        private void CreateTab(string fileName)
        {
            try
            {
                var tb = new FastColoredTextBox();
                tb.Font = new Font("Consolas", 9.75f);
                tb.ForeColor = paletteWindowsTextColor;
                tb.BackColor =  paletteWindowColor;
//                tb.ContextMenuStrip = cmMain;
                tb.Dock = DockStyle.Fill;
                tb.BorderStyle = BorderStyle.Fixed3D;
                //tb.VirtualSpace = true;
                tb.LeftPadding = 17;
                tb.Language = Language.Custom;
                tb.DescriptionFile = Application.StartupPath + "/hpm.xml";
                tb.TextChanged += textEditor_TextChanged;
                //                tb.AddStyle(sameWordsStyle);//same words style
                var tab = new FATabStripItem(fileName != null ? System.IO.Path.GetFileName(fileName) : "[new]", tb);
                tab.Tag = fileName;
                if (fileName != null)
                    tb.OpenFile(fileName);
                tb.Tag = new TbInfo();
                tsFiles.AddTab(tab);
                tsFiles.SelectedItem = tab;
                tb.Focus();
                tb.DelayedTextChangedInterval = 1000;
                tb.DelayedEventsInterval = 500;
                tb.TextChangedDelayed += tb_TextChangedDelayed;
                tb.KeyDown += FCTB_KeyDown;

                AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                popupMenu.Opening += popupMenu_Opening;
                BuildAutocompleteMenu(popupMenu);
                ((TbInfo)tb.Tag).popupMenu = popupMenu;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    CreateTab(fileName);
            }
        }

       private void newFileMenuItem_Click(object sender, EventArgs e)
        {
            CreateTab(null);
            TextEditor.Text = Resources.NewFileTemplate;
            // FileName = "";
            isFileNew = true;
        }

       private void saveMenuItem_Click(object sender, EventArgs e)
        {
            if (isFileNew)
            {
                saveAsMenuItem_Click(sender, e);

            }
            else
            {
                System.IO.File.WriteAllText(FileName, TextEditor.Text);
            }
            isFileNew = false;
        }

        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = Resources.STR_SaveAs;
            saveFileDialog1.Filter = Resources.STR_FileFilters;
            saveFileDialog1.DefaultExt = "hpm";
            saveFileDialog1.FilterIndex = 1;
            string fname = System.IO.Path.GetFileName(FileName);
            string path;
            try
            {
                path = System.IO.Path.GetDirectoryName(FileName);
            }
            catch
            {
                path = "";
            }
            saveFileDialog1.InitialDirectory = (path == "") ? Application.StartupPath : path;
            saveFileDialog1.FileName = fname;
            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                System.IO.File.WriteAllText(saveFileDialog1.FileName, TextEditor.Text);
                isFileNew = false;

            }
        }

        private void SelectToolMenu( string name )
        {
            progString = name;
            exeString = iniFile.GetString(name, "exe", "");
            pathString = iniFile.GetString(name, "path", "");
            helpSting = iniFile.GetString(name, "help", "");
            defaultHplExtension = iniFile.GetString(name, "defaultExt", ".hpl");
            fileExtensions = iniFile.GetString(name, "fileExts", "");
            if (fileExtensions == "")
                fileExtensions = Resources.STR_FileFilters;
            else
            {
                if (fileExtensions.IndexOf("*.*", StringComparison.Ordinal) < 0)
                    fileExtensions += "|Все файлы|*.*";
                if (fileExtensions.IndexOf("*.hpm", StringComparison.Ordinal) < 0)
                    fileExtensions = "HPM files|*.hpm|" + fileExtensions;

            }
            hplSubDir = iniFile.GetString(name, "hplSubDir", "hpl");
            

            progDef = pathString + "/" + exeString;
            ToPassToMenuItem.Text = Resources.STR_PassTo + progString;
            selectedToolHelpMenuItem.Text = Resources.STR_HelpFor + progString;

            foreach (ToolStripMenuItem item in toolsMenuItem.DropDownItems)
            {
                item.Checked = item.Text == progString;
            }

            iniFile.WriteValue("selected", "prog", name); 
        }

        private void toolSelect_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem mi) SelectToolMenu(mi.Text);
        }

        private Color paletteWindowsTextColor;
        private Color paletteWindowColor;

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = Application.CompanyName + @" " + Application.ProductName + @" v." + Application.ProductVersion;
            iniFile = new IniFile(Application.StartupPath + "/config.ini");
            var sections = iniFile.GetSectionNames();
            foreach (var section in sections)
            {
                if (section.ToLower().Trim() != "selected")
                {
                    try
                    {
                        var program = iniFile.GetString(section, "path", "") + "/" + iniFile.GetString(section, "exe", "");
                        Image icon = Icon.ExtractAssociatedIcon(program)?.ToBitmap();
                        //ToolStripMenuItem item = new ToolStripMenuItem(section, icon, toolSelect_Click);
                        toolsMenuItem.DropDownItems.Add(section, icon, toolSelect_Click);
                        
                    }
                    catch (Exception)
                    {
                    }
                }               
            }
            var def = iniFile.GetString("selected", "prog", "");
            SelectToolMenu( def );

            paletteWindowsTextColor = SystemColors.WindowText;
            paletteWindowColor = SystemColors.Window;

        }

        private System.Diagnostics.Process progerWindow = null;
        private string _progerWindowSoftware = "";

        private void ToPassToMenuItem_Click(object sender, EventArgs e)
        {
            PreprocessorMenuItem_Click(sender, e);
            var workDir = System.IO.Path.GetDirectoryName(FileName);
            var justFileName = System.IO.Path.GetFileNameWithoutExtension(FileName);
            if (workDir != null && workDir.IndexOf(pathString, StringComparison.Ordinal) < 0)
            {
                var resultFileName = pathString + "/" + hplSubDir + "/" + justFileName + defaultHplExtension;
                TextEditor.SaveToFile(resultFileName, Encoding.ASCII);
            }

//            saveAsMenuItem_Click(sender, e);
            var passTo = pathString + '/' + exeString;
            if (progerWindow != null && !progerWindow.HasExited && progerWindow.ProcessName != ""  && passTo == _progerWindowSoftware )
            {
                SetForegroundWindow(progerWindow.Handle);//  ShowWindow(progerWindow.Handle, SW_SHOW);
            }
            else
            {
                progerWindow = Process.Start(passTo); //? 
                _progerWindowSoftware = passTo;
            }
        }
    

        private void printMenuItem_Click(object sender, EventArgs e)
        {
            var pds = new PrintDialogSettings
            {
                ShowPageSetupDialog = true,
                ShowPrintDialog = true,
                ShowPrintPreviewDialog = true
            };
            TextEditor.Print(pds);            
        }

        private void PreprocessorMenuItem_Click(object sender, EventArgs e)
        {
            var vars = new KeyValList.KeyValList();
            var source = TextEditor.Lines.ToList();
            var result =  Preprocessor.Compile(ref source, out var dest, ref vars);
            if (result == null || result.errorCode == Preprocessor.ErrorRec.ErrCodes.EcOk)
            {
                var workDir = System.IO.Path.GetDirectoryName(FileName);
                var justFileName = System.IO.Path.GetFileNameWithoutExtension(FileName);
                var resultFileName = workDir + "/" + justFileName + defaultHplExtension;
                System.IO.File.WriteAllLines(resultFileName, dest);  //? 
                FATabStripItem found = null;
                foreach (FATabStripItem item in tsFiles.Items)
                {
                    if (item.Tag is string s && s == resultFileName)
                    {
                        found = item;
                        break;
                    }
                }
                if (found is null)
                    CreateTab(resultFileName);
                else
                    ((FastColoredTextBox)found.Controls[0]).Text = System.IO.File.ReadAllText(resultFileName);
//                textEditor. = fname + ".hpl"; //?
                // isFileNew = true;
                MessageBox.Show(Resources.STR_CompletedSuccessfully);
            }
            else
            {
                MessageBox.Show(@"error:" + result);
            }
        }

        private void toolsMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void selectedToolHelpMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"hh.exe", pathString + "/" +  helpSting); 

        }

        private void recentFilesMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var fmi = (RecentFileHandler.FileMenuItem)e.ClickedItem;
            OpenFile(fmi.FileName);
        }

        public class TbInfo
        {
            public AutocompleteMenu popupMenu;
        }

        private void tsFiles_TabStripItemClosing(TabStripItemClosingEventArgs e)
        {
            if (e?.Item?.Controls == null || e.Item.Controls.Count < 1) return;
            if (((FastColoredTextBox)e.Item.Controls[0]).IsChanged)
            {
                switch(MessageBox.Show(Resources.STR_DoYouWannaSave + e.Item.Title + @" ?", @"Save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information))
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        if (!Save(e.Item))
                            e.Cancel = true;
                        break;
                    case DialogResult.Cancel:
                         e.Cancel = true;
                        break;
                    default:
                        return;
                }
            }
        }

        private bool Save(FATabStripItem tab)
        {
            var tb = TextEditor; // (tab.Controls[0] as FastColoredTextBox);
            if (tab.Tag == null)
            {
                saveFileDialog1.Filter = Resources.STR_FileFilters;
                if (saveFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return false;
                tab.Title = System.IO.Path.GetFileName(saveFileDialog1.FileName);
                tab.Tag = saveFileDialog1.FileName;
            }

            try
            {
                if (tab.Tag is string name)
                {
                    System.IO.File.WriteAllText(name, tb.Text);
                    tb.IsChanged = false;
                }
            }
            catch (Exception ex)
            {
                return MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry && Save(tab);
            }

            tb.Invalidate();

            return true;
        }

// AVTO PODSTANOVKA??        AutocompleteMenu popupMenu;

        private static void BuildAutocompleteMenu(AutocompleteMenu popupMenu)
        {
            var items = Preprocessor.keywords.Select(item => new AutocompleteItem(item)).ToList();
            items.AddRange(Preprocessor.registers.Select(item => new AutocompleteItem(item)));
            items.AddRange(Preprocessor.addons.Select(item => new AutocompleteItem(item)));

            //set as autocomplete source
            popupMenu.SearchPattern = @"[\w\.]|[\$\w\.]|[\#\w\.]|[@\w\.]";
            popupMenu.Items.SetAutocompleteItems(items);
        }

        private static void FCTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is FastColoredTextBox tb)
            {
                if (e.KeyData == (Keys.Space & Keys.Control))
                {
                    //forced show (MinFragmentLength will be ignored)
                    if(tb.Tag is TbInfo ti) ti.popupMenu.Show(true);
                    e.Handled = true;
                }
            }
        }

        private void popupMenu_Opening(object sender, CancelEventArgs e)
        {
            //---block autocomplete menu for comments
            //get index of green style (used for comments)
            var iGreenStyle = TextEditor.GetStyleIndex(TextEditor.SyntaxHighlighter.GreenStyle);
            if (iGreenStyle >= 0)
                if (TextEditor.Selection.Start.iChar > 0)
                {
                    //current char (before caret)
                    var c = TextEditor[TextEditor.Selection.Start.iLine][TextEditor.Selection.Start.iChar - 1];
                    //green Style
                    var greenStyleIndex = Range.ToStyleIndex(iGreenStyle);
                    //if char contains green style then block popup menu
                    if ((c.style & greenStyleIndex) != 0)
                        e.Cancel = true;
                }
        }

        private static void tb_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            if (sender is FastColoredTextBox fctb)
            {
                var text = fctb.Text;
                ThreadPool.QueueUserWorkItem(
                    (o) => ReBuildAutoCompleteList(text, fctb)
                );
            }

        }

        private static void AddSectionsToAutoComplete(string text, List<AutocompleteItem> items)
        {
            var regex = new Regex(@"^\[((?<range>\w+)?|[\#\!\~]+(?<range>\w+)?)\]", RegexOptions.Multiline);
            foreach (Match r in regex.Matches(text))
            {
                try
                {
                    var s = r.Value;
                    if (s[0] == '[' && s[s.Length - 1] == ']')
                    {
                        var temp = s.Split(new char[] { '[', ']', '#', '!', '~' }, StringSplitOptions.RemoveEmptyEntries);
                        s = temp[0];
                    }
                    if (!items.Exists(x => x.Text == s))
                    {
                        items.Add(new AutocompleteItem(s));
                    }
                }
                catch
                {

                }
            }
        }

        private static void AddPinsToAutoComplete(string text, List<AutocompleteItem> items)
        {
            var pinsRe = new Regex(@"((BUS.?|PIN.?|R\d+)\s?=\s?(\w+),(\w+)(,(\w))?)", RegexOptions.Multiline);
            foreach (Match pinRec in pinsRe.Matches(text))
            {
                try
                {
                    var s = pinRec.Groups[3].Value;
                    if (!items.Exists(x => x.Text == s))
                    {
                        items.Add(new AutocompleteItem(s));
                    }
                }
                catch
                {

                }
            }
        }

        private static void AddArraysToAutoComplete(string text, List<AutocompleteItem> items)
        {
            var pinsRe = new Regex(@"^(@\w+)\s*=\s*((0x[0-9A-Za-z]+|[0-9A-Za-z]+H|\d+)|(\{(\w+)(,\s*\w+)*\}))", RegexOptions.Multiline);
            foreach (Match pinRec in pinsRe.Matches(text))
            {
                try
                {
                    var s = pinRec.Groups[1].Value;
                    if (!items.Exists(x => x.Text == s))
                    {
                        items.Add(new AutocompleteItem(s));
                    }
                }
                catch
                {

                }
            }
        }

        private static void ReBuildAutoCompleteList(string text, FastColoredTextBox tb)
        {
            try
            {
                var items = Preprocessor.keywords.Select(item => new AutocompleteItem(item)).ToList();
                items.AddRange(Preprocessor.registers.Select(item => new AutocompleteItem(item)));
                items.AddRange(Preprocessor.addons.Select(item => new AutocompleteItem(item)));

                AddSectionsToAutoComplete(text, items);
                AddPinsToAutoComplete(text, items);
                AddArraysToAutoComplete(text, items);

                tb.Invoke( new Action<TbInfo>( tbi => tbi.popupMenu.Items.SetAutocompleteItems(items)), (tb.Tag as TbInfo));
                //set as autocomplete source                
//                AutocompleteMenu pm = new AutocompleteMenu();
//                AutocompleteMenu old = (tb.Tag as TbInfo).popupMenu;
               
 //               pm.Items.SetAutocompleteItems(items);
//                pm.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
             
//                tb.Invoke(new Action((tb.Tag as TbInfo).popupMenu.Dispose));
//                tb.Invoke(new Action<AutocompleteMenu>(m => (tb.Tag as TbInfo).popupMenu = m), pm);
//                (tb.Tag as TbInfo).popupMenu = pm;

            }
            catch
            {

            }
        }


    }
}
