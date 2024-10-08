﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;
using FarsiLibrary.Win;
using HPLStudio.Properties;
using Markdig;
using Markdig.Prism;
using KeyValList = KeyValList.KeyValList;
using System.Configuration;

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
        public string processString = "";
        public string hplSubDir = "\\hpl";
        public string defaultHplExtension = ".hpl";
        public string fileExtensions = Resources.STR_FileFilters;
//        public string xmlString = "";

//        const string sectionRegex = @"^\[\w+\].*$";

        public static MarkdownPipeline MarkdownPipeline;
        public IniFile ConfigIniFile;
        public AboutForm AboutDlgForm;
        public BrowserForm Browser;
        public Configuration Config;
        public AcroPDFForm PdfForm;


        private const int SW_MAXIMIZE = 3;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int SetActiveWindow( IntPtr hWnd);

        private readonly RecentFileHandler _recentFileHandler;

        public Form1()
        {
            InitializeComponent();
            _recentFileHandler = new RecentFileHandler();
            this._recentFileHandler.RecentFileToolStripItem = this.recentFilesMenuItem;
            MarkdownPipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseBootstrap()
                .UsePrism()
                .Build();
            AboutDlgForm = new AboutForm();
        }

        private static void textEditor_TextChanged(object sender, TextChangedEventArgs e)
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

        private void OpenFile(string fileName, string encoding = null)
        {
            var enc = Encoding.GetEncoding(encoding ?? "windows-1251");
            if (FindFileTab(fileName) is {} tab)
            {
                tsFiles.SelectedItem = tab;
                TextEditor.Focus();
            }
            else
            {
                CreateTab(fileName);
            }
            TextEditor.Text = System.IO.File.ReadAllText(fileName, enc);
            //FileName = FileName;
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
                OpenFile(openFileDialog1.FileName, SelectedEncodingName);
                _recentFileHandler.AddFile(openFileDialog1.FileName);
            }
        }

        private HotkeysMapping defaultHotkeysMapping = null;

        private FastColoredTextBox TextEditor
        {
            get => (tsFiles.SelectedItem is null || tsFiles.SelectedItem.Controls.Count == 0) 
                ? null : tsFiles.SelectedItem?.Controls[0] as FastColoredTextBox;

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

                var tb = new FastColoredTextBox(){AutoIndentChars = false};
                tb.Font = new Font("Consolas", 9.75f);
                tb.ForeColor = _paletteWindowsTextColor;
                var ext = System.IO.Path.GetExtension(fileName);
                tb.BackColor = ext switch
                {
                    ".hpm" => _paletteHpmWindowColor,
                    ".hpl" => _paletteHplWindowColor,
                    _ => _paletteWindowColor
                };
                // tb.BackColor =  _paletteWindowColor;
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
                if (defaultHotkeysMapping != null) tb.HotkeysMapping = defaultHotkeysMapping;
                tb.CustomAction += new System.EventHandler<FastColoredTextBoxNS.CustomActionEventArgs>(this.fctb_CommentAction);

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
                var enc = Encoding.GetEncoding(SelectedEncodingName);
                System.IO.File.WriteAllText(FileName, TextEditor.Text, enc);
                if (FileName == $"{Application.StartupPath}/config.ini")
                {
                    LoadIniFile();
                }

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
                var enc = Encoding.GetEncoding(SelectedEncodingName);
                System.IO.File.WriteAllText(saveFileDialog1.FileName, TextEditor.Text, enc);
                isFileNew = false;

            }
        }

        private void SelectToolMenu( string name )
        {
            progString = name;
            exeString = ConfigIniFile.GetString(name, "exe", "");
            pathString =  Environment.ExpandEnvironmentVariables(ConfigIniFile.GetString(name, "path", ""));
            helpSting = ConfigIniFile.GetString(name, "help", "");
            processString = ConfigIniFile.GetString(name, "process", "");
            defaultHplExtension = ConfigIniFile.GetString(name, "defaultExt", ".hpl");
            fileExtensions = ConfigIniFile.GetString(name, "fileExts", "");
            if (fileExtensions == "")
                fileExtensions = Resources.STR_FileFilters;
            else
            {
                if (fileExtensions.IndexOf("*.*", StringComparison.Ordinal) < 0)
                    fileExtensions += "|Все файлы|*.*";
                if (fileExtensions.IndexOf("*.hpm", StringComparison.Ordinal) < 0)
                    fileExtensions = "HPM files|*.hpm|" + fileExtensions;

            }
            hplSubDir = ConfigIniFile.GetString(name, "hplSubDir", "hpl");
            

            progDef = pathString + "/" + exeString;
            ToPassToMenuItem.Text = Resources.STR_PassTo + progString;
            selectedToolHelpMenuItem.Text = Resources.STR_HelpFor + progString;

            foreach (ToolStripMenuItem item in toolsMenuItem.DropDownItems)
            {
                item.Checked = item.Text == progString;
            }

            ConfigIniFile.WriteValue("selected", "prog", name); 
        }

        private void toolSelect_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem mi) SelectToolMenu(mi.Text);
        }

        private Color _paletteWindowsTextColor;
        private Color _paletteWindowColor;
        private Color _paletteHplWindowColor;
        private Color _paletteHpmWindowColor;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetSysColor(int nIndex);

        private void LoadIniFile()
        {
            ConfigIniFile = new IniFile(Application.StartupPath + "/config.ini");
            var sections = ConfigIniFile.GetSectionNames();
            toolsMenuItem.DropDownItems.Clear();
            foreach (var section in sections)
            {
                if (section.ToLower().Trim() != "selected")
                {
                    try
                    {
                        var program = ConfigIniFile.GetString(section, "path", "")
                                      + "\\" + ConfigIniFile.GetString(section, "exe", "");
                        program = Environment.ExpandEnvironmentVariables(program);
                        Image icon = Icon.ExtractAssociatedIcon(program)?.ToBitmap();
                        //ToolStripMenuItem item = new ToolStripMenuItem(section, icon, toolSelect_Click);
                        toolsMenuItem.DropDownItems.Add(section, icon, toolSelect_Click);

                    }
                    catch (Exception exception)
                    {
                        statusStrip1.Text = exception.ToString();
                    }
                }
            }
            var def = ConfigIniFile.GetString("selected", "prog", "");
            SelectToolMenu(def);
        }

        private void Create_EncodingsMenuDropDownList()
        {
            encodingToolStripMenuItem.DropDownItems.Clear();
            foreach (var encoding in Encoding.GetEncodings())
            {
                var text = $"{encoding.CodePage:D5} {encoding.Name} - {encoding.DisplayName}";
                var item = encodingToolStripMenuItem.DropDownItems.Add(text);
                item.Name = encoding.Name;
                item.Tag = encoding;
            }
        }

        private void ApplyEncoding(string encoding)
        {
            if (!string.IsNullOrEmpty(encoding))
            {
                var encMenuItem = encodingToolStripMenuItem.DropDownItems.Find(encoding, false);
                if (encMenuItem is { Length: > 0 })
                {
                    var ei = (encMenuItem[0].Tag as EncodingInfo);
                    encodingToolStripMenuItem.Text =
                        string.Format(Resources.STR_encodingMenu, ei?.Name);
                    encodingToolStripMenuItem.Tag = ei;

                }
            }
        }

        public EncodingInfo SelectedEncodingInfo => (encodingToolStripMenuItem.Tag as EncodingInfo);
        public string SelectedEncodingName => SelectedEncodingInfo?.Name ?? "windows-1251";


        private void InitHotKeys()
        {
            defaultHotkeysMapping = new HotkeysMapping();
            defaultHotkeysMapping.InitDefault();
            defaultHotkeysMapping.Add(Keys.Control | Keys.OemSemicolon, FCTBAction.CustomAction1);
            defaultHotkeysMapping.Add(Keys.Control | Keys.OemSemicolon | Keys.Shift, FCTBAction.CustomAction2);
            defaultHotkeysMapping.Add(Keys.Control | Keys.LButton, FCTBAction.CustomAction3);

            LoadHotKeys();
        }

        private void LoadHotKeys()
        {
            string hk = null;
            if (ConfigIniFile.GetSectionNames().Contains("Hotkeys"))
            {
                hk = ConfigIniFile.GetString("Hotkeys", "hotkeys", null);
                if (string.IsNullOrEmpty(hk))
                {
                    hk = string.Join(", ",
                        ConfigIniFile.GetSectionValues("Hotkeys")
                            .Select((x) => $"{x.Key}={x.Value}"));
                }

            }
            else
            {
                hk = string.Join(", ", 
                    Config.AppSettings.Settings.AllKeys.Where((x) => x.StartsWith("HotKeys."))
                        .Select((x) => $"{x.Substring("HotKeys.".Length)}={Config.AppSettings.Settings[x].Value}"));
            }

            if (!string.IsNullOrEmpty(hk))
            {
                defaultHotkeysMapping = HotkeysMapping.Parse(hk);
            }
        }

        private void InitPalette()
        {
            _paletteWindowsTextColor = SystemColors.WindowText;//  Color.FromArgb(GetSysColor((int)KnownColor.WindowText));
            _paletteWindowColor = SystemColors.Window; //Color.FromArgb(GetSysColor((int)KnownColor.Window));
            _paletteHplWindowColor = Color.FromArgb(215, 228, 242);
            _paletteHpmWindowColor = Color.FromArgb(255, 255, 210);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = Application.CompanyName + @" " + Application.ProductName + @" v." + Application.ProductVersion;
            LoadIniFile();

            InitPalette();

            Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER",
                Environment.ExpandEnvironmentVariables($"%APPDATA%\\{Application.ProductName}"));

            Config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            ParsingInfo.AddGeneratedMacro = Config.AppSettings.Settings["ParsingInfo.AddGeneratedMacro"]?.Value.ToLower()  == "true";
            addMacroToolStripMenuItem.Checked = ParsingInfo.AddGeneratedMacro;

            ParsingInfo.AddDefs = Config.AppSettings.Settings["ParsingInfo.AddDefs"]?.Value.ToLower() == "true";
            addDefineToolStripMenuItem.Checked = ParsingInfo.AddDefs;

            Create_EncodingsMenuDropDownList();
            var encoding = Config.AppSettings.Settings["default-coding"]?.Value.ToLower();
            ApplyEncoding(encoding);

            InitHotKeys();

        }

        private System.Diagnostics.Process _progerWindow = null;
        private string _progerWindowSoftware = "";

        private void ActivateApp(Process process)
        {
            ShowWindow(_progerWindow.MainWindowHandle, SW_RESTORE);
            SetForegroundWindow(_progerWindow.MainWindowHandle);
        }

        private void ToPassToMenuItem_Click(object sender, EventArgs e)
        {
            PreprocessorMenuItem_Click(sender, e);
            var workDir = Path.GetDirectoryName(FileName);
            var justFileName = Path.GetFileNameWithoutExtension(FileName);

            if (workDir != null && workDir.IndexOf(pathString, StringComparison.Ordinal) < 0)
            {  // Если редактируемый файл не в папке программатора, копируем его туда
                var resultFileName = pathString + "\\" + hplSubDir + "\\" + justFileName + defaultHplExtension;
                
                var fileTab = FindFileTab($"{workDir}/{justFileName}{defaultHplExtension}")?.Controls[0];
                if (fileTab is FastColoredTextBox fctb)
                {
                    fctb.SaveToFile(resultFileName, Encoding.ASCII);
                }
            }

//            saveAsMenuItem_Click(sender, e);
            var passTo = pathString + '\\' + exeString;
            if (!File.Exists(passTo))
            {
                MessageBox.Show($@"File: '{passTo}' not found!", @"Incorrect Tool Path", MessageBoxButtons.OK);
                return;
            }

            if (_progerWindow is {HasExited: false} && _progerWindow.ProcessName != "" && passTo == _progerWindowSoftware )
            {   // Если ПО программатора уже запущено, активируем его окно
                ActivateApp(_progerWindow);
            }
            else
            {
                var processName = string.IsNullOrEmpty(processString)
                    ? string.IsNullOrEmpty(progString)
                        ? justFileName : progString
                    : processString;
                if (!string.IsNullOrEmpty(processName))
                { 
                    var ps = Process.GetProcessesByName(processName);
                    if (ps.Length > 0)
                    {
                        _progerWindow = ps[0];
                        ActivateApp(_progerWindow);
                        return;
                    }
                }
                _progerWindow = Process.Start(passTo); //* ? 
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

        private FATabStripItem FindFileTab(string filename)
        {
            foreach (FATabStripItem item in tsFiles.Items)
            {
                if (item.Tag is string s && s.Replace("/", "\\") == filename.Replace("/", "\\"))
                {
                    return item;
                }
            }
            return null;
        }

        private void PreprocessorMenuItem_Click(object sender, EventArgs e)
        {
            Preprocessor.Variables ??= new global::KeyValList.KeyValList(); 
            Preprocessor.Variables.Clear();
            
            var source = TextEditor.Lines.ToList();
            var result =  Preprocessor.Compile(ref source, out var dest);
            if (result == null || result.Code == ErrorRec.ErrCodes.EcOk)
            {
                var workDir = Path.GetDirectoryName(FileName);
                var justFileName = Path.GetFileNameWithoutExtension(FileName);
                var resultFileName = workDir + "/" + justFileName + defaultHplExtension;
                var enc = Encoding.GetEncoding(SelectedEncodingName);

                File.WriteAllLines(resultFileName, dest, enc);  //? 
                var fileTab = FindFileTab(resultFileName);
                
                if (fileTab is null)
                    CreateTab(resultFileName);
                else
                    ((FastColoredTextBox)fileTab.Controls[0]).Text = System.IO.File.ReadAllText(resultFileName);
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
            var ext = System.IO.Path.GetExtension(helpSting);
            if (string.IsNullOrEmpty(ext))
            {
                MessageBox.Show(string.Format(Resources.STR_unsupported_help_file, helpSting),
                    Resources.STR_Warning, MessageBoxButtons.OK);
                return;
            }

            switch (ext)
            {
                case ".chm":
                    Process.Start(@"hh.exe", 
                        Environment.ExpandEnvironmentVariables(pathString + "/" + helpSting));
                    break;
                case ".md":
                    MarkDownHelp(Environment.ExpandEnvironmentVariables(pathString + "/" + helpSting));
                    break;
                case ".pdf":
                    AcroPDFForm.ShowFile(Environment.ExpandEnvironmentVariables(pathString + "/" + helpSting));
                    break;
                case ".html":
                    var helpUri = (helpSting.StartsWith("http"))
                        ? helpSting
                        : Environment.ExpandEnvironmentVariables(pathString + "/" + helpSting);
                    HtmlHelp(helpUri);
                    break;
                default:
                    MessageBox.Show(string.Format(Resources.STR_unsupported_help_file, helpSting),
                        Resources.STR_Warning, MessageBoxButtons.OK);
                    break;
            }

        }

        private void recentFilesMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var fmi = (RecentFileHandler.FileMenuItem)e.ClickedItem;
            OpenFile(fmi.FileName, SelectedEncodingName);
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
                    var enc = Encoding.GetEncoding(SelectedEncodingName);
                    System.IO.File.WriteAllText(name, tb.Text, enc);
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

        private static void AddDefsToAutoComplete(string text, List<AutocompleteItem> items)
        {
            var defsRe = new Regex(@"^#def\s*(\w*)\s*=\s*(.*)\s*$", RegexOptions.Multiline);
            foreach (Match def in defsRe.Matches(text))
            {
                try
                {
                    var s = def.Groups[1].Value;
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

        private static void AddMacroToAutoComplete(string text, List<AutocompleteItem> items)
        {
            var macroRe = new Regex(@"^#macro\s*(.*)\s*$", RegexOptions.Multiline);
            foreach (Match macro in macroRe.Matches(text))
            {
                try
                {
                    var s = macro.Groups[1].Value;
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


        private static readonly Regex StructsRe = new Regex(@"#struct\s*(\w+)\s*=\s*@?\w+(.*?)#ends",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private static Mutex _structReMutex = new Mutex();
        private static void AddStructsToAutoComplete(string text, List<AutocompleteItem> items)
        {
            //var defsRe = new Regex(@"(#struct\s*(\w*)\s*=\s*(@?\w*))((\s*(\w+)\s*=?\d*\s)+)(#ends)", RegexOptions.Singleline);

            _structReMutex.WaitOne(100);
            foreach (Match def in StructsRe.Matches(text))
            {
                try
                {
                    var s = def?.Groups[1].Value;
                    if (!items.Exists(x => x.Text == s))
                    {
                        items.Add(new AutocompleteItem(s));
                    }

                    var fields = def?.Groups[4].Value.Trim()
                        .Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var f in fields)
                    {
                        var fn = f.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        var sf = $"{s}.{fn[0].Trim()}";
                        if (!items.Exists(x => x.Text == sf))
                        {
                            items.Add(new AutocompleteItem(sf));
                        }
                    }
                }
                catch
                {
                }
            }
            _structReMutex.ReleaseMutex();
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
                AddDefsToAutoComplete(text, items);
                AddStructsToAutoComplete(text, items);
                AddMacroToAutoComplete(text, items);

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

        private void changeHotKeysButton_Click(object sender, EventArgs e)
        {
            if (defaultHotkeysMapping is null && TextEditor is null) return;
            var mapping = defaultHotkeysMapping ?? TextEditor.HotkeysMapping;

            var form = new HotkeysEditorForm(mapping);
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (FATabStripItem item in tsFiles.Items)
                {
                    if (item?.Controls[0] is FastColoredTextBox tb)
                    {
                        tb.HotkeysMapping = form.GetHotkeys();
                    }
                }
                defaultHotkeysMapping = form.GetHotkeys();
                var writeToIni = ConfigIniFile.GetSectionNames().Contains("Hotkeys");
                foreach (var record in defaultHotkeysMapping.ToString().Split(','))
                {
                    var kv = record.Split('=');
                    if(writeToIni)
                        ConfigIniFile.WriteValue("Hotkeys", kv[0].Trim(), kv[1].Trim());

                    SetConfigValue($"HotKeys.{kv[0].Trim()}", kv[1].Trim());
                }

                Config.Save(ConfigurationSaveMode.Minimal);

                //
                //
                // ConfigIniFile.WriteValue("Hotkeys", "hotkeys", defaultHotkeysMapping.ToString());
            }
        }


        private static Regex uncommentRe = new Regex(@"(\n\s*(;))", RegexOptions.Compiled | RegexOptions.Singleline);

        private void fctb_CommentAction(object sender, CustomActionEventArgs e)
        {
            switch (e.Action)
            {
                case FCTBAction.CustomAction1: /*comment*/
                    if (TextEditor is { } tb)
                    {
                        var store = new Range(tb, tb.Selection.Start, tb.Selection.End);
                        if (tb.SelectedText == "")
                            tb.Selection = new Range(tb, tb.Selection.Start.iLine);
                        var s = $"\n{tb.SelectedText}";
                        s =  s.Replace("\n", $"\n;")
                            .Remove(0, 1);
                        tb.SelectedText = s.Remove(s.Length-1);
                        tb.Selection = store;
                        tb.DoSelectionVisible();
                    }
                    break;
                case FCTBAction.CustomAction2: /*uncomment*/
                    if (TextEditor is { } te)
                    {
                        var store = new Range(te, te.Selection.Start, te.Selection.End);
                        if (te.SelectedText == "")
                            te.Selection = new Range(te, te.Selection.Start.iLine);
                        var s = uncommentRe.Replace($"\n{te.SelectedText}",
                            match => match.Value.Remove(match.Value.Length - 1));
                        te.SelectedText = s.Remove(0,1);
                        te.Selection = store;
                    }
                    break;
                case FCTBAction.CustomAction3: /*Go-to-declaration*/
                    if (TextEditor is { } ed)
                    {
                        var p = ed.PointToPosition(ed.PointToClient(Cursor.Position));
                        // display letter
                        var word = GetWord(ed, p);
                        GoToDeclaration(word);
                    }
                    break;

            }
        }

        private readonly List<FCTBPlace> _positions = new();
        private void PushGoTo(FCTBPlace from, int to)
        {
            _positions.Add(from);
            GoToPosition(from.Editor, to);
        }

        private void PushGoTo(FCTBPlace from, FCTBPlace to)
        {
            _positions.Add(from);
            GoToPosition(to);
        }

        private void PopGoTo()
        {
            if (_positions.Count == 0) return;
            var to = _positions.Last();
            GoToPosition(to);
            _positions.RemoveAt(_positions.Count-1);
        }
        private void GoToPosition(FCTBPlace to)
        {
            foreach (FATabStripItem item in tsFiles.Items)
            {
                if (item?.Controls[0] is FastColoredTextBox tb && tb == to.Editor)
                {
                    tsFiles.SelectedItem = item;
                    tb.Selection.Start = to.Position;
                }
            }
        }

        private void GoToPosition(FastColoredTextBox editor, int to)
        {
            foreach (FATabStripItem item in tsFiles.Items)
            {
                if (item?.Controls[0] is FastColoredTextBox tb && tb == editor)
                {
                    tsFiles.SelectedItem = item;
                    tb.SelectionStart = to;
                }
            }
        }

        private void GoToDeclaration(string name)
        {
            var text = TextEditor.Text;
            var patterns = new string[]
            {
                $@"\[{name}\]", $@"#macro\s{name}\b", $@"#struct\s{name}\b", $@"#def\s{name}\b"
            };

            int pos = -1;
            foreach (var pattern in patterns)
            {
                var m = Regex.Match(TextEditor.Text, $@"#macro\s{name}");
                if (m.Success)
                {
                    pos = m.Index;
                    break;
                }
            }

            if (pos >= 0)
            {

                PushGoTo(new FCTBPlace(){ Editor = TextEditor, Position = TextEditor.Selection.Start},
                    new FCTBPlace(){Editor = TextEditor, Position = new Place()}
                    );
            }
        }

        private string GetWord(FastColoredTextBox ct, int p)
        {
            var sb = new StringBuilder(ct.Text);
            if (sb.Length == 0 || p == sb.Length) return "";
            if (!Regex.IsMatch(sb[p].ToString(), @"^\w$")) return sb[p].ToString();
            var n1 = p;
            while (n1 > 0 && Regex.IsMatch(sb[n1 - 1].ToString(), @"^\w$"))
            {
                n1 -= 1;
            }
            var n2 = p;
            while (n2 < sb.Length && Regex.IsMatch(sb[n2 + 1].ToString(), @"^\w$"))
            {
                n2 += 1;
            }

            return sb.ToString().Substring(n1, n2 - n1 + 1);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDlgForm.ShowDialog();
        }

        internal void HtmlHelp(string pathOrUri)
        {
            if (Browser == null || Browser.IsDisposed)
                Browser = new BrowserForm();

            Browser.Text = Resources.STR_Help;
            if (!pathOrUri.StartsWith("http"))
            {
                pathOrUri = Environment.ExpandEnvironmentVariables(pathOrUri);
                pathOrUri = $"file://{pathOrUri}";
                // Browser.webView.CreationProperties.BrowserExecutableFolder = Application.ExecutablePath;
                // Browser.webView.CreationProperties.UserDataFolder = Application.ExecutablePath + "/Resources";
            }

            // Browser.webBrowser.Url = new Uri(pathOrUri);

            Browser.webView.Source = new Uri( pathOrUri );
            Browser.Show();

        }

        internal void MarkDownHelp(string markDownFileName)
        {
            if (Browser == null || Browser.IsDisposed)
                Browser = new BrowserForm();

            Browser.Text = Resources.STR_Help;

            var helpMd = Encoding.UTF8.GetString(File.ReadAllBytes(markDownFileName));
            var (script, md) = BrowserForm.ExtractJsFromMd(helpMd);
            var helpHtml = Markdown.ToHtml(md, Form1.MarkdownPipeline);
            helpHtml = BrowserForm.PlaceBackExtractedScriptToHtml(helpHtml, script);
            helpHtml = BrowserForm.FixHtmlLocalImgPath(helpHtml);
            helpHtml = BrowserForm.AddStylesToMdHtml(helpHtml);

            var path = $"{Application.StartupPath}/md2html.html";
            System.IO.File.WriteAllText(path, helpHtml);
            HtmlHelp(path);
        }

        private void progHelpMenuItem_Click(object sender, EventArgs e)
        {
            MarkDownHelp($"{Application.StartupPath}/readme.md");
        }

        private void onlineHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HtmlHelp("https://github.com/eugenosm/HPL-Studio-NET/blob/master/HPL%20Studio%20NET/README.md");
        }

        private void addHiddenDefineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addDefineToolStripMenuItem.Checked = !addDefineToolStripMenuItem.Checked;
            ParsingInfo.AddDefs = addDefineToolStripMenuItem.Checked;
            WriteConfigValue("ParsingInfo.AddDefs", addDefineToolStripMenuItem.Checked ? "true" : "false");
        }

        private void addMacroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addMacroToolStripMenuItem.Checked = !addMacroToolStripMenuItem.Checked;
            ParsingInfo.AddGeneratedMacro = addMacroToolStripMenuItem.Checked;
            WriteConfigValue("ParsingInfo.AddGeneratedMacro", addDefineToolStripMenuItem.Checked ? "true" : "false");
        }

        public void SetConfigValue(string key, string value)
        {
            Config.AppSettings.Settings.Remove(key);
            Config.AppSettings.Settings.Add(key, value);
        }

        public void WriteConfigValue(string key, string value)
        {
            Config.AppSettings.Settings.Remove(key);
            Config.AppSettings.Settings.Add(key, value);
            Config.Save(ConfigurationSaveMode.Minimal);
        }

        private void programmerSoftwareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile(Application.StartupPath + "/config.ini");
        }
        
        private void encodingToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            encodingToolStripMenuItem.Tag = e.ClickedItem.Tag;
            var encName = (e.ClickedItem?.Tag as EncodingInfo)?.Name;
            encodingToolStripMenuItem.Text =
                string.Format(Resources.STR_encodingMenu, encName);
            WriteConfigValue("default-coding", encName);
        }
    }


    public struct FCTBPlace
    {
        public FastColoredTextBox Editor;
        public Place Position;
    }


}
