namespace MilSymbolPicker
{
    partial class FormExtras
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
            this.tbManuallyEnterCode = new System.Windows.Forms.TextBox();
            this.cbManuallyEnterCode = new System.Windows.Forms.CheckBox();
            this.butOK = new System.Windows.Forms.Button();
            this.butCancel = new System.Windows.Forms.Button();
            this.cbShowSymbolCenterReference = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // tbManuallyEnterCode
            // 
            this.tbManuallyEnterCode.Location = new System.Drawing.Point(13, 80);
            this.tbManuallyEnterCode.Name = "tbManuallyEnterCode";
            this.tbManuallyEnterCode.Size = new System.Drawing.Size(169, 20);
            this.tbManuallyEnterCode.TabIndex = 0;
            this.tbManuallyEnterCode.Visible = false;
            this.tbManuallyEnterCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbManuallyEnterCode_KeyDown);
            // 
            // cbManuallyEnterCode
            // 
            this.cbManuallyEnterCode.AutoSize = true;
            this.cbManuallyEnterCode.Location = new System.Drawing.Point(13, 57);
            this.cbManuallyEnterCode.Name = "cbManuallyEnterCode";
            this.cbManuallyEnterCode.Size = new System.Drawing.Size(302, 17);
            this.cbManuallyEnterCode.TabIndex = 1;
            this.cbManuallyEnterCode.Text = "Manually Set Code (8 | 20 Chars=2525D or 10 | 15=2525C)";
            this.cbManuallyEnterCode.UseVisualStyleBackColor = true;
            this.cbManuallyEnterCode.CheckedChanged += new System.EventHandler(this.cbManuallyEnterCode_CheckedChanged);
            // 
            // butOK
            // 
            this.butOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.butOK.Location = new System.Drawing.Point(95, 220);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(75, 23);
            this.butOK.TabIndex = 2;
            this.butOK.Text = "OK";
            this.butOK.UseVisualStyleBackColor = true;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(204, 220);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(75, 23);
            this.butCancel.TabIndex = 3;
            this.butCancel.Text = "Cancel";
            this.butCancel.UseVisualStyleBackColor = true;
            // 
            // cbShowSymbolCenterReference
            // 
            this.cbShowSymbolCenterReference.AutoSize = true;
            this.cbShowSymbolCenterReference.Location = new System.Drawing.Point(13, 23);
            this.cbShowSymbolCenterReference.Name = "cbShowSymbolCenterReference";
            this.cbShowSymbolCenterReference.Size = new System.Drawing.Size(216, 17);
            this.cbShowSymbolCenterReference.TabIndex = 4;
            this.cbShowSymbolCenterReference.Text = "Show Symbol Center Reference Overlay";
            this.cbShowSymbolCenterReference.UseVisualStyleBackColor = true;
            // 
            // FormExtras
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 255);
            this.Controls.Add(this.cbShowSymbolCenterReference);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.cbManuallyEnterCode);
            this.Controls.Add(this.tbManuallyEnterCode);
            this.Name = "FormExtras";
            this.Text = "Extras";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button butOK;
        private System.Windows.Forms.Button butCancel;
        public System.Windows.Forms.TextBox tbManuallyEnterCode;
        public System.Windows.Forms.CheckBox cbManuallyEnterCode;
        public System.Windows.Forms.CheckBox cbShowSymbolCenterReference;
    }
}