﻿namespace HPLStudio
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.printMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.recentFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.PreprocessorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToPassToMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.progHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedToolHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewFileStrip1 = new System.Windows.Forms.ToolStrip();
            this.NewButton1 = new System.Windows.Forms.ToolStripButton();
            this.OpenButton1 = new System.Windows.Forms.ToolStripButton();
            this.SaveButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.printButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.CompileButton1 = new System.Windows.Forms.ToolStripButton();
            this.toPassInButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.tsFiles = new FarsiLibrary.Win.FATabStrip();
            this.faTabStripItem1 = new FarsiLibrary.Win.FATabStripItem();
            this.menuStrip1.SuspendLayout();
            this.NewFileStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tsFiles)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 525);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(746, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolsMenuItem,
            this.helpMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(746, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newFileMenuItem,
            this.openFileMenuItem,
            this.saveMenuItem,
            this.saveAsMenuItem,
            this.toolStripSeparator4,
            this.printMenuItem,
            this.toolStripSeparator5,
            this.recentFilesMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // newFileMenuItem
            // 
            this.newFileMenuItem.Name = "newFileMenuItem";
            this.newFileMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newFileMenuItem.Size = new System.Drawing.Size(173, 22);
            this.newFileMenuItem.Text = "Создать";
            this.newFileMenuItem.Click += new System.EventHandler(this.newFileMenuItem_Click);
            // 
            // openFileMenuItem
            // 
            this.openFileMenuItem.Name = "openFileMenuItem";
            this.openFileMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openFileMenuItem.Size = new System.Drawing.Size(173, 22);
            this.openFileMenuItem.Text = "Открыть";
            this.openFileMenuItem.Click += new System.EventHandler(this.openFileMenuItem_Click);
            // 
            // saveMenuItem
            // 
            this.saveMenuItem.Name = "saveMenuItem";
            this.saveMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveMenuItem.Size = new System.Drawing.Size(173, 22);
            this.saveMenuItem.Text = "Сохранить";
            this.saveMenuItem.Click += new System.EventHandler(this.saveMenuItem_Click);
            // 
            // saveAsMenuItem
            // 
            this.saveAsMenuItem.Name = "saveAsMenuItem";
            this.saveAsMenuItem.Size = new System.Drawing.Size(173, 22);
            this.saveAsMenuItem.Text = "Сохранить как";
            this.saveAsMenuItem.Click += new System.EventHandler(this.saveAsMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(170, 6);
            // 
            // printMenuItem
            // 
            this.printMenuItem.Name = "printMenuItem";
            this.printMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.printMenuItem.Size = new System.Drawing.Size(173, 22);
            this.printMenuItem.Text = "Печать";
            this.printMenuItem.Click += new System.EventHandler(this.printMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(170, 6);
            // 
            // recentFilesMenuItem
            // 
            this.recentFilesMenuItem.Name = "recentFilesMenuItem";
            this.recentFilesMenuItem.Size = new System.Drawing.Size(173, 22);
            this.recentFilesMenuItem.Text = "Недавние файлы";
            this.recentFilesMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.recentFilesMenuItem_DropDownItemClicked);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PreprocessorMenuItem,
            this.ToPassToMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(88, 20);
            this.toolStripMenuItem1.Text = "Компилятор";
            // 
            // PreprocessorMenuItem
            // 
            this.PreprocessorMenuItem.Name = "PreprocessorMenuItem";
            this.PreprocessorMenuItem.Size = new System.Drawing.Size(180, 22);
            this.PreprocessorMenuItem.Text = "Собрать";
            this.PreprocessorMenuItem.Click += new System.EventHandler(this.PreprocessorMenuItem_Click);
            // 
            // ToPassToMenuItem
            // 
            this.ToPassToMenuItem.Name = "ToPassToMenuItem";
            this.ToPassToMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ToPassToMenuItem.Text = "Передать в ,,,";
            this.ToPassToMenuItem.Click += new System.EventHandler(this.ToPassToMenuItem_Click);
            // 
            // toolsMenuItem
            // 
            this.toolsMenuItem.Name = "toolsMenuItem";
            this.toolsMenuItem.Size = new System.Drawing.Size(95, 20);
            this.toolsMenuItem.Text = "Инструменты";
            this.toolsMenuItem.Click += new System.EventHandler(this.toolsMenuItem_Click);
            // 
            // helpMenuItem
            // 
            this.helpMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progHelpMenuItem,
            this.selectedToolHelpMenuItem,
            this.toolStripMenuItem2,
            this.оПрограммеToolStripMenuItem});
            this.helpMenuItem.Name = "helpMenuItem";
            this.helpMenuItem.Size = new System.Drawing.Size(65, 20);
            this.helpMenuItem.Text = "Справка";
            // 
            // progHelpMenuItem
            // 
            this.progHelpMenuItem.Name = "progHelpMenuItem";
            this.progHelpMenuItem.Size = new System.Drawing.Size(206, 22);
            this.progHelpMenuItem.Text = "Помощь по программе";
            // 
            // selectedToolHelpMenuItem
            // 
            this.selectedToolHelpMenuItem.Name = "selectedToolHelpMenuItem";
            this.selectedToolHelpMenuItem.Size = new System.Drawing.Size(206, 22);
            this.selectedToolHelpMenuItem.Text = "Помощь по ...";
            this.selectedToolHelpMenuItem.Click += new System.EventHandler(this.selectedToolHelpMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(203, 6);
            // 
            // оПрограммеToolStripMenuItem
            // 
            this.оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.оПрограммеToolStripMenuItem.Text = "О программе...";
            // 
            // NewFileStrip1
            // 
            this.NewFileStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.NewFileStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewButton1,
            this.OpenButton1,
            this.SaveButton1,
            this.toolStripSeparator1,
            this.printButton1,
            this.toolStripSeparator2,
            this.CompileButton1,
            this.toPassInButton1,
            this.toolStripSeparator3});
            this.NewFileStrip1.Location = new System.Drawing.Point(0, 24);
            this.NewFileStrip1.Name = "NewFileStrip1";
            this.NewFileStrip1.Size = new System.Drawing.Size(746, 25);
            this.NewFileStrip1.TabIndex = 4;
            this.NewFileStrip1.Text = "toolStrip1";
            // 
            // NewButton1
            // 
            this.NewButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.NewButton1.Image = ((System.Drawing.Image)(resources.GetObject("NewButton1.Image")));
            this.NewButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.NewButton1.Name = "NewButton1";
            this.NewButton1.Size = new System.Drawing.Size(23, 22);
            this.NewButton1.Text = "Новый";
            this.NewButton1.Click += new System.EventHandler(this.newFileMenuItem_Click);
            // 
            // OpenButton1
            // 
            this.OpenButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.OpenButton1.Image = ((System.Drawing.Image)(resources.GetObject("OpenButton1.Image")));
            this.OpenButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenButton1.Name = "OpenButton1";
            this.OpenButton1.Size = new System.Drawing.Size(23, 22);
            this.OpenButton1.Text = "Открыть";
            this.OpenButton1.Click += new System.EventHandler(this.openFileMenuItem_Click);
            // 
            // SaveButton1
            // 
            this.SaveButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SaveButton1.Image = ((System.Drawing.Image)(resources.GetObject("SaveButton1.Image")));
            this.SaveButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveButton1.Name = "SaveButton1";
            this.SaveButton1.Size = new System.Drawing.Size(23, 22);
            this.SaveButton1.Text = "Сохранить";
            this.SaveButton1.Click += new System.EventHandler(this.saveMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // printButton1
            // 
            this.printButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.printButton1.Image = ((System.Drawing.Image)(resources.GetObject("printButton1.Image")));
            this.printButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.printButton1.Name = "printButton1";
            this.printButton1.Size = new System.Drawing.Size(23, 22);
            this.printButton1.Text = "Печать";
            this.printButton1.Click += new System.EventHandler(this.printMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // CompileButton1
            // 
            this.CompileButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.CompileButton1.Image = global::HPLStudio.Properties.Resources.wrench_orange;
            this.CompileButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CompileButton1.Name = "CompileButton1";
            this.CompileButton1.Size = new System.Drawing.Size(23, 22);
            this.CompileButton1.Text = "Собрать";
            this.CompileButton1.Click += new System.EventHandler(this.PreprocessorMenuItem_Click);
            // 
            // toPassInButton1
            // 
            this.toPassInButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toPassInButton1.Image = global::HPLStudio.Properties.Resources.cog_go;
            this.toPassInButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toPassInButton1.Name = "toPassInButton1";
            this.toPassInButton1.Size = new System.Drawing.Size(23, 22);
            this.toPassInButton1.Text = "Передать в";
            this.toPassInButton1.Click += new System.EventHandler(this.ToPassToMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
            // 
            // tsFiles
            // 
            this.tsFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tsFiles.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.tsFiles.Location = new System.Drawing.Point(0, 49);
            this.tsFiles.Name = "tsFiles";
            this.tsFiles.SelectedItem = this.faTabStripItem1;
            this.tsFiles.Size = new System.Drawing.Size(746, 476);
            this.tsFiles.TabIndex = 5;
            this.tsFiles.Text = "faTabStrip1";
            this.tsFiles.TabStripItemClosing += new FarsiLibrary.Win.TabStripItemClosingHandler(this.tsFiles_TabStripItemClosing);
            // 
            // faTabStripItem1
            // 
            this.faTabStripItem1.IsDrawn = true;
            this.faTabStripItem1.Name = "faTabStripItem1";
            this.faTabStripItem1.Selected = true;
            this.faTabStripItem1.Size = new System.Drawing.Size(744, 455);
            this.faTabStripItem1.TabIndex = 0;
            this.faTabStripItem1.Title = "TabStrip Page 1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(746, 547);
            this.Controls.Add(this.tsFiles);
            this.Controls.Add(this.NewFileStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.NewFileStrip1.ResumeLayout(false);
            this.NewFileStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tsFiles)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMenuItem;
        private System.Windows.Forms.ToolStrip NewFileStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveAsMenuItem;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.ToolStripButton NewButton1;
        private System.Windows.Forms.ToolStripButton OpenButton1;
        private System.Windows.Forms.ToolStripButton SaveButton1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem PreprocessorMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToPassToMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printMenuItem;
        private System.Windows.Forms.ToolStripButton printButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem helpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem progHelpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectedToolHelpMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton CompileButton1;
        private System.Windows.Forms.ToolStripButton toPassInButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem recentFilesMenuItem;
        private FarsiLibrary.Win.FATabStrip tsFiles;
        private FarsiLibrary.Win.FATabStripItem faTabStripItem1;
    }
}

