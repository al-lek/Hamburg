namespace Hamburg_namespace
{
    partial class OpAmp
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
            this.numericTextBoxWithSign1 = new Hamburg_namespace.NumericTextBoxWithSign();
            this.SuspendLayout();
            // 
            // numericTextBoxWithSign1
            // 
            this.numericTextBoxWithSign1.DoubleValue = 0D;
            this.numericTextBoxWithSign1.Format = Hamburg_namespace.FormatType.Double;
            this.numericTextBoxWithSign1.Location = new System.Drawing.Point(0, 0);
            this.numericTextBoxWithSign1.MaximumDValue = 0D;
            this.numericTextBoxWithSign1.MinimumDValue = 0D;
            this.numericTextBoxWithSign1.Name = "numericTextBoxWithSign1";
            this.numericTextBoxWithSign1.Size = new System.Drawing.Size(40, 20);
            this.numericTextBoxWithSign1.TabIndex = 0;
            this.numericTextBoxWithSign1.Text = "+0.0";
            this.numericTextBoxWithSign1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericTextBoxWithSign1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.numericTextBoxWithSign1_KeyPress);
            // 
            // OpAmp
            // 
            this.Controls.Add(this.numericTextBoxWithSign1);
            this.MaximumSize = new System.Drawing.Size(41, 21);
            this.MinimumSize = new System.Drawing.Size(41, 21);
            this.Name = "OpAmp";
            this.Size = new System.Drawing.Size(41, 21);
            this.Load += new System.EventHandler(this.OpAmp_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NumericTextBoxWithSign numericTextBoxWithSign1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private NumericTextBoxWithSign numericTextBoxWithSign2;
    }
}
