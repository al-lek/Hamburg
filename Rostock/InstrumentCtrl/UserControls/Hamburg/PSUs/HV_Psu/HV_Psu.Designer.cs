namespace Hamburg_namespace
{
    partial class HV_Psu
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.numericTextBoxWithoutSign1 = new Hamburg_namespace.NumericTextBoxWithoutSign();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Red;
            this.button1.Location = new System.Drawing.Point(46, -1);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(20, 20);
            this.button1.TabIndex = 1;
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // numericTextBoxWithoutSign1
            // 
            this.numericTextBoxWithoutSign1.DoubleValue = 0D;
            this.numericTextBoxWithoutSign1.Format = Hamburg_namespace.FormatType.Double;
            this.numericTextBoxWithoutSign1.Location = new System.Drawing.Point(0, 0);
            this.numericTextBoxWithoutSign1.MaximumDValue = 0D;
            this.numericTextBoxWithoutSign1.MaximumSize = new System.Drawing.Size(41, 21);
            this.numericTextBoxWithoutSign1.MinimumDValue = 0D;
            this.numericTextBoxWithoutSign1.Name = "numericTextBoxWithoutSign1";
            this.numericTextBoxWithoutSign1.Size = new System.Drawing.Size(40, 20);
            this.numericTextBoxWithoutSign1.TabIndex = 0;
            this.numericTextBoxWithoutSign1.Text = "0.0";
            this.numericTextBoxWithoutSign1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericTextBoxWithoutSign1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.numericTextBoxWithoutSign1_KeyPress);
            // 
            // HV_Psu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.numericTextBoxWithoutSign1);
            this.Name = "HV_Psu";
            this.Size = new System.Drawing.Size(65, 20);
            this.Load += new System.EventHandler(this.RF_Psu_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NumericTextBoxWithoutSign numericTextBoxWithoutSign1;
        private System.Windows.Forms.Button button1;
    }
}
