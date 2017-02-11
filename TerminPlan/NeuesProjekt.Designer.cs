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
            this.ultraLabel1 = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextEditor1 = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabel2 = new Infragistics.Win.Misc.UltraLabel();
            this.ultraDateTimeEditor1 = new Infragistics.Win.UltraWinEditors.UltraDateTimeEditor();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextEditor1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeEditor1)).BeginInit();
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
            // ultraTextEditor1
            // 
            this.ultraTextEditor1.Location = new System.Drawing.Point(97, 32);
            this.ultraTextEditor1.Name = "ultraTextEditor1";
            this.ultraTextEditor1.Size = new System.Drawing.Size(175, 21);
            this.ultraTextEditor1.TabIndex = 1;
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
            this.ultraDateTimeEditor1.Location = new System.Drawing.Point(97, 67);
            this.ultraDateTimeEditor1.Name = "ultraDateTimeEditor1";
            this.ultraDateTimeEditor1.Size = new System.Drawing.Size(175, 21);
            this.ultraDateTimeEditor1.TabIndex = 3;
            // 
            // btnOk
            // 
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Location = new System.Drawing.Point(13, 106);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Tag = "OK";
            this.btnOk.Text = "Annehmen";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.OnBtnOkClick);
            // 
            // btnCancel
            // 
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Location = new System.Drawing.Point(197, 106);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Tag = "Cancel";
            this.btnCancel.Text = "Abbruch";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.OnBtnCancelClick);
            // 
            // NeuesProjekt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(284, 135);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.ultraDateTimeEditor1);
            this.Controls.Add(this.ultraLabel2);
            this.Controls.Add(this.ultraTextEditor1);
            this.Controls.Add(this.ultraLabel1);
            this.Name = "NeuesProjekt";
            this.Text = "NeuesProjekt";
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextEditor1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraDateTimeEditor1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Infragistics.Win.Misc.UltraLabel ultraLabel1;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextEditor1;
        private Infragistics.Win.Misc.UltraLabel ultraLabel2;
        private Infragistics.Win.UltraWinEditors.UltraDateTimeEditor ultraDateTimeEditor1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
 }
