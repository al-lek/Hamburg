using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace Hamburg_namespace
{
    public partial class OpAmp : UserControl
    {
        private double ReadbackVoltage;

        private ToolTip readBack_toolTip = new ToolTip() { AutoPopDelay = 1000, InitialDelay = 100, ReshowDelay = 100, ShowAlways = true };
        bool editing_voltage;       // this is used to indicate that numericTxtBox is in edit. It will prevent update Method to change color
        double acceptable_V_variation_on_zero;

        #region Constructor
        public OpAmp()
        {
            InitializeComponent();
            InitializeProperties();
        }
        #endregion

        #region Event registring and values Initialisation
        private void OpAmp_Load(object sender, EventArgs e)
        {
            RegisterEventMethods();
            InitializeValues();
            acceptable_V_variation_on_zero = 0.002 * (Math.Abs(numericTextBoxWithSign1.MaximumDValue) + Math.Abs(numericTextBoxWithSign1.MinimumDValue));
        }
        #endregion

        #region UC Action methods
      
        private void numericTextBoxWithSign1_KeyPress(object sender, KeyPressEventArgs e)
        {
            editing_voltage = true;
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (SetVoltage(numericTextBoxWithSign1.DoubleValue) == 1)
                {
                    numericTextBoxWithSign1.BackColor = Color.PowderBlue;
                    signal_mainForm(numericTextBoxWithSign1.Parent);
                    editing_voltage = false;
                }
            }
        }
        #endregion

        #region Event called methods
        public void HamburgBoxAdcValues_UPD_EventMethod(BOX_ADDRESS Addr)
        {
            Color newcolor;
            string str;
            
            ReadbackVoltage = GetRdbVoltage(Addr);
            str = ReadbackVoltage.ToString("+0.00;-0.00;0.00") + "V";

            if (this.InvokeRequired)
                this.Invoke(new Action(() => readBack_toolTip.SetToolTip(numericTextBoxWithSign1, str)));
            else
                this.readBack_toolTip.SetToolTip(numericTextBoxWithSign1, str);

            if (!editing_voltage)
            {
                double perCent_difference = Math.Abs(1.0 - ReadbackVoltage / numericTextBoxWithSign1.DoubleValue) * 100.0;

                if ((perCent_difference > ACCEPTABLE_V_VARIATION && numericTextBoxWithSign1.DoubleValue != 0.0) || (ReadbackVoltage > acceptable_V_variation_on_zero && numericTextBoxWithSign1.DoubleValue == 0.0))
                    newcolor = Color.OrangeRed;
                else
                    newcolor = Color.PowderBlue;

                if (this.numericTextBoxWithSign1.InvokeRequired)
                    this.numericTextBoxWithSign1.Invoke(new Action(() => numericTextBoxWithSign1.BackColor = newcolor));
                else
                    this.numericTextBoxWithSign1.BackColor = newcolor;
            }
        }
        #endregion

        #region Resize Code
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, 20, specified);
        }
        #endregion

        #region Raise event on new voltage set
        private void signal_mainForm(Control usrCtrl)
        {
            // this will signal mainform that the voltage of a specific opAmpPsu has changed.
            // mainForm will deceide if it is included in charts, and will update.
            try
            {
                Type type = typeof(Form1);
                Form form1 = Application.OpenForms["Form1"];
                MethodInfo pass_changes_to_mainForm_method = type.GetMethod("update_charting_timing");
                pass_changes_to_mainForm_method.Invoke(form1, new object[] { null });
            }
            catch { }
        }
        
        #endregion
    }
}