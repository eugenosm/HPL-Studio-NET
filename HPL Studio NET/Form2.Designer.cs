
namespace HPLStudio
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.button1 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.productLabel = new System.Windows.Forms.Label();
            this.companyLabel = new System.Windows.Forms.Label();
            this.descrLabel = new System.Windows.Forms.Label();
            this.cpRghtLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(144, 284);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(105, 37);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::HPLStudio.Properties.Resources.omega_min;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(12, 62);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(149, 148);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // productLabel
            // 
            this.productLabel.AutoSize = true;
            this.productLabel.Font = new System.Drawing.Font("Courier New", 14F);
            this.productLabel.Location = new System.Drawing.Point(8, 35);
            this.productLabel.Name = "productLabel";
            this.productLabel.Size = new System.Drawing.Size(142, 21);
            this.productLabel.TabIndex = 2;
            this.productLabel.Text = "productLabel";
            // 
            // companyLabel
            // 
            this.companyLabel.AutoSize = true;
            this.companyLabel.Font = new System.Drawing.Font("Courier New", 14F);
            this.companyLabel.Location = new System.Drawing.Point(167, 85);
            this.companyLabel.Name = "companyLabel";
            this.companyLabel.Size = new System.Drawing.Size(142, 21);
            this.companyLabel.TabIndex = 3;
            this.companyLabel.Text = "companyLabel";
            // 
            // descrLabel
            // 
            this.descrLabel.AutoSize = true;
            this.descrLabel.Font = new System.Drawing.Font("Courier New", 14F);
            this.descrLabel.Location = new System.Drawing.Point(8, 226);
            this.descrLabel.Name = "descrLabel";
            this.descrLabel.Size = new System.Drawing.Size(120, 21);
            this.descrLabel.TabIndex = 4;
            this.descrLabel.Text = "descrLabel";
            // 
            // cpRghtLabel
            // 
            this.cpRghtLabel.AutoSize = true;
            this.cpRghtLabel.Font = new System.Drawing.Font("Courier New", 14F);
            this.cpRghtLabel.Location = new System.Drawing.Point(167, 156);
            this.cpRghtLabel.Name = "cpRghtLabel";
            this.cpRghtLabel.Size = new System.Drawing.Size(131, 21);
            this.cpRghtLabel.TabIndex = 5;
            this.cpRghtLabel.Text = "cpRghtLabel";
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 338);
            this.Controls.Add(this.cpRghtLabel);
            this.Controls.Add(this.descrLabel);
            this.Controls.Add(this.companyLabel);
            this.Controls.Add(this.productLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.Text = "программе";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label productLabel;
        private System.Windows.Forms.Label companyLabel;
        private System.Windows.Forms.Label descrLabel;
        private System.Windows.Forms.Label cpRghtLabel;
    }
}