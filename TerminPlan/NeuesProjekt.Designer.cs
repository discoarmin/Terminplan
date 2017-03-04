namespace Terminplan
{
    partial class NeuesProjekt
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NeuesProjekt));
            this.ultraLabel1 = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextEditorPrjName = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabel2 = new Infragistics.Win.Misc.UltraLabel();
            this.ultraDateTimeEditor1 = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.ultraLabel3 = new Infragistics.Win.Misc.UltraLabel();
            this.ultraMaskedEditKommission = new Infragistics.Win.UltraWinMaskedEdit.UltraMaskedEdit();
            this.radioButtonNormalerText = new System.Windows.Forms.RadioButton();
            this.rbKommission = new System.Windows.Forms.RadioButton();
            this.ultraTextEditorNormalerText = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextEditorPrjName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeEditor1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextEditorNormalerText)).BeginInit();
            this.SuspendLayout();
            // 
            // ultraLabel1
            // 
            this.ultraLabel1.Location = new System.Drawing.Point(13, 32);
            this.ultraLabel1.Name = "ultraLabel1";
            this.ultraLabel1.Size = new System.Drawing.Size(77, 16);
            this.ultraLabel1.TabIndex = 0;
            this.ultraLabel1.Text = "Projektname:";
            // 
            // ultraTextEditorPrjName
            // 
            this.ultraTextEditorPrjName.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.ultraTextEditorPrjName.Location = new System.Drawing.Point(97, 32);
            this.ultraTextEditorPrjName.Name = "ultraTextEditorPrjName";
            this.ultraTextEditorPrjName.Size = new System.Drawing.Size(175, 21);
            this.ultraTextEditorPrjName.TabIndex = 1;
            // 
            // ultraLabel2
            // 
            this.ultraLabel2.Location = new System.Drawing.Point(13, 73);
            this.ultraLabel2.Name = "ultraLabel2";
            this.ultraLabel2.Size = new System.Drawing.Size(77, 16);
            this.ultraLabel2.TabIndex = 2;
            this.ultraLabel2.Text = "Stardatum:";
            // 
            // ultraDateTimeEditor1
            // 
            this.ultraDateTimeEditor1.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.ultraDateTimeEditor1.Location = new System.Drawing.Point(97, 67);
            this.ultraDateTimeEditor1.Name = "ultraDateTimeEditor1";
            this.ultraDateTimeEditor1.Size = new System.Drawing.Size(175, 21);
            this.ultraDateTimeEditor1.TabIndex = 3;
            // 
            // btnOk
            // 
            this.btnOk.BackColor = System.Drawing.Color.Silver;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Image = ((System.Drawing.Image)(resources.GetObject("btnOk.Image")));
            this.btnOk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOk.Location = new System.Drawing.Point(12, 144);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(92, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Tag = "OK";
            this.btnOk.Text = "Annehmen";
            this.btnOk.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.OnBtnOkClick);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Silver;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(164, 144);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(92, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Tag = "Cancel";
            this.btnCancel.Text = "Abbruch";
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.OnBtnCancelClick);
            // 
            // ultraLabel3
            // 
            this.ultraLabel3.Location = new System.Drawing.Point(12, 104);
            this.ultraLabel3.Name = "ultraLabel3";
            this.ultraLabel3.Size = new System.Drawing.Size(64, 16);
            this.ultraLabel3.TabIndex = 7;
            this.ultraLabel3.Text = "Schlüssel:";
            // 
            // ultraMaskedEditKommission
            // 
            this.ultraMaskedEditKommission.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.ultraMaskedEditKommission.EditAs = Infragistics.Win.UltraWinMaskedEdit.EditAsType.UseSpecifiedMask;
            this.ultraMaskedEditKommission.InputMask = "####/##";
            this.ultraMaskedEditKommission.Location = new System.Drawing.Point(216, 100);
            this.ultraMaskedEditKommission.Name = "ultraMaskedEditKommission";
            this.ultraMaskedEditKommission.NonAutoSizeHeight = 20;
            this.ultraMaskedEditKommission.Size = new System.Drawing.Size(56, 20);
            this.ultraMaskedEditKommission.TabIndex = 8;
            this.ultraMaskedEditKommission.Text = "/";
            this.ultraMaskedEditKommission.UseFlatMode = Infragistics.Win.DefaultableBoolean.True;
            // 
            // radioButtonNormalerText
            // 
            this.radioButtonNormalerText.AutoSize = true;
            this.radioButtonNormalerText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioButtonNormalerText.Location = new System.Drawing.Point(80, 96);
            this.radioButtonNormalerText.Name = "radioButtonNormalerText";
            this.radioButtonNormalerText.Size = new System.Drawing.Size(90, 17);
            this.radioButtonNormalerText.TabIndex = 9;
            this.radioButtonNormalerText.Tag = "Text";
            this.radioButtonNormalerText.Text = "Normaler Text";
            this.radioButtonNormalerText.UseVisualStyleBackColor = true;
            this.radioButtonNormalerText.CheckedChanged += new System.EventHandler(this.OnRadioButtonChanged);
            // 
            // rbKommission
            // 
            this.rbKommission.AutoSize = true;
            this.rbKommission.Checked = true;
            this.rbKommission.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbKommission.Location = new System.Drawing.Point(80, 112);
            this.rbKommission.Name = "rbKommission";
            this.rbKommission.Size = new System.Drawing.Size(101, 17);
            this.rbKommission.TabIndex = 10;
            this.rbKommission.TabStop = true;
            this.rbKommission.Tag = "Kommission";
            this.rbKommission.Text = "Kommissions-Nr.";
            this.rbKommission.UseVisualStyleBackColor = true;
            this.rbKommission.CheckedChanged += new System.EventHandler(this.OnRadioButtonChanged);
            // 
            // ultraTextEditorNormalerText
            // 
            this.ultraTextEditorNormalerText.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2013;
            this.ultraTextEditorNormalerText.Location = new System.Drawing.Point(192, 100);
            this.ultraTextEditorNormalerText.Name = "ultraTextEditorNormalerText";
            this.ultraTextEditorNormalerText.Size = new System.Drawing.Size(82, 21);
            this.ultraTextEditorNormalerText.TabIndex = 11;
            this.ultraTextEditorNormalerText.Visible = false;
            // 
            // NeuesProjekt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(284, 171);
            this.ControlBox = false;
            this.Controls.Add(this.ultraTextEditorNormalerText);
            this.Controls.Add(this.rbKommission);
            this.Controls.Add(this.radioButtonNormalerText);
            this.Controls.Add(this.ultraMaskedEditKommission);
            this.Controls.Add(this.ultraLabel3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.ultraDateTimeEditor1);
            this.Controls.Add(this.ultraLabel2);
            this.Controls.Add(this.ultraTextEditorPrjName);
            this.Controls.Add(this.ultraLabel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NeuesProjekt";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "NeuesProjekt";
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextEditorPrjName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeEditor1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextEditorNormalerText)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Infragistics.Win.Misc.UltraLabel ultraLabel1;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextEditorPrjName;
        private Infragistics.Win.Misc.UltraLabel ultraLabel2;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeEditor1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private Infragistics.Win.Misc.UltraLabel ultraLabel3;
        private Infragistics.Win.UltraWinMaskedEdit.UltraMaskedEdit ultraMaskedEditKommission;
        private System.Windows.Forms.RadioButton radioButtonNormalerText;
        private System.Windows.Forms.RadioButton rbKommission;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextEditorNormalerText;
    }
 }
