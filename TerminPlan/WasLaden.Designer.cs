namespace Terminplan
{
    partial class WasLaden
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WasLaden));
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            this.btnOk = new System.Windows.Forms.Button();
            this.ultraLabel1 = new Infragistics.Win.Misc.UltraLabel();
            this.rbKommission = new System.Windows.Forms.RadioButton();
            this.radioButtonNormalerText = new System.Windows.Forms.RadioButton();
            this.ultraLabel3 = new Infragistics.Win.Misc.UltraLabel();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.BackColor = System.Drawing.Color.Silver;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Image = ((System.Drawing.Image)(resources.GetObject("btnOk.Image")));
            this.btnOk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOk.Location = new System.Drawing.Point(96, 108);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(68, 23);
            this.btnOk.TabIndex = 6;
            this.btnOk.Tag = "OK";
            this.btnOk.Text = "OK";
            this.btnOk.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.OnBtnOkClickClick);
            // 
            // ultraLabel1
            // 
            appearance3.FontData.BoldAsString = "True";
            this.ultraLabel1.Appearance = appearance3;
            this.ultraLabel1.Location = new System.Drawing.Point(16, 20);
            this.ultraLabel1.Name = "ultraLabel1";
            this.ultraLabel1.Size = new System.Drawing.Size(256, 23);
            this.ultraLabel1.TabIndex = 7;
            this.ultraLabel1.Text = "Bitte wählen Sie aus, was geladen werden soll:";
            // 
            // rbKommission
            // 
            this.rbKommission.AutoSize = true;
            this.rbKommission.Checked = true;
            this.rbKommission.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbKommission.Location = new System.Drawing.Point(124, 76);
            this.rbKommission.Name = "rbKommission";
            this.rbKommission.Size = new System.Drawing.Size(108, 17);
            this.rbKommission.TabIndex = 13;
            this.rbKommission.TabStop = true;
            this.rbKommission.Tag = "Neu";
            this.rbKommission.Text = "Neuer Terminplan";
            this.rbKommission.UseVisualStyleBackColor = true;
            this.rbKommission.CheckedChanged += new System.EventHandler(this.OnRadioButtonChanged);
            // 
            // radioButtonNormalerText
            // 
            this.radioButtonNormalerText.AutoSize = true;
            this.radioButtonNormalerText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioButtonNormalerText.Location = new System.Drawing.Point(124, 56);
            this.radioButtonNormalerText.Name = "radioButtonNormalerText";
            this.radioButtonNormalerText.Size = new System.Drawing.Size(140, 17);
            this.radioButtonNormalerText.TabIndex = 12;
            this.radioButtonNormalerText.Tag = "Vorhanden";
            this.radioButtonNormalerText.Text = "Vorhandener Terminplan";
            this.radioButtonNormalerText.UseVisualStyleBackColor = true;
            this.radioButtonNormalerText.CheckedChanged += new System.EventHandler(this.OnRadioButtonChanged);
            // 
            // ultraLabel3
            // 
            this.ultraLabel3.Location = new System.Drawing.Point(20, 64);
            this.ultraLabel3.Name = "ultraLabel3";
            this.ultraLabel3.Size = new System.Drawing.Size(88, 16);
            this.ultraLabel3.TabIndex = 11;
            this.ultraLabel3.Text = "Datenauswahl:";
            // 
            // WasLaden
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(284, 133);
            this.ControlBox = false;
            this.Controls.Add(this.rbKommission);
            this.Controls.Add(this.radioButtonNormalerText);
            this.Controls.Add(this.ultraLabel3);
            this.Controls.Add(this.ultraLabel1);
            this.Controls.Add(this.btnOk);
            this.MaximizeBox = false;
            this.Name = "WasLaden";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WasLaden";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnOk;
        private Infragistics.Win.Misc.UltraLabel ultraLabel1;
        private System.Windows.Forms.RadioButton rbKommission;
        private System.Windows.Forms.RadioButton radioButtonNormalerText;
        private Infragistics.Win.Misc.UltraLabel ultraLabel3;
    }
}