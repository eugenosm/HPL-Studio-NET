
namespace HPLStudio
{
    partial class AcroPDFForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.faTabStrip = new FarsiLibrary.Win.FATabStrip();
            ((System.ComponentModel.ISupportInitialize)(this.faTabStrip)).BeginInit();
            this.SuspendLayout();
            // 
            // faTabStrip
            // 
            this.faTabStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.faTabStrip.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.faTabStrip.Location = new System.Drawing.Point(0, 0);
            this.faTabStrip.Name = "faTabStrip";
            this.faTabStrip.Size = new System.Drawing.Size(800, 450);
            this.faTabStrip.TabIndex = 0;
            this.faTabStrip.Text = "faTabStrip1";
            // 
            // AcroPDFForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.faTabStrip);
            this.Name = "AcroPDFForm";
            this.Text = "AcroPDFForm";
            ((System.ComponentModel.ISupportInitialize)(this.faTabStrip)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private FarsiLibrary.Win.FATabStrip faTabStrip;
    }
}