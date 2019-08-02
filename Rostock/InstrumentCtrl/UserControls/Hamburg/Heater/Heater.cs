using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hamburg_namespace
{
    public partial class Heater : UserControl
    {
        private double RdbTemperature;
        private PWR_STATE PwrState;

        private ToolTip readBack_toolTip = new ToolTip() { AutoPopDelay = 1000, InitialDelay = 100, ReshowDelay = 100, ShowAlways = true };
        bool editing_temperature;       // this is used to indicate that numericTxtBox is in edit. It will prevent update Method to change color
   
        #region Constructor
        public Heater()
        {
            InitializeComponent();
            InitializeProperties();
        }
        #endregion

        #region Event registring and values Initialisation
        private void Heater_Load(object sender, EventArgs e)
        {
            RegisterEventMethods();
            InitializeValues();
        }
        #endregion

        #region UC Action methods
        private void numericTextBoxWithoutSign1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (SetHtrTemperature(numericTextBoxWithoutSign1.DoubleValue) == true)
                {
                    numericTextBoxWithoutSign1.BackColor = Color.PowderBlue;
                    editing_temperature = false;
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (PwrState == PWR_STATE.ON)
            {
                SetHtrPwrState(PWR_STATE.OFF);
            }
            else
            {
                SetHtrPwrState(PWR_STATE.ON);
            }

        }
        #endregion

        #region Event called methods     
        public void PowerStateChange_GLB_EventMethod(PWR_STATE state)
        {
            SetHtrPwrState(state);
        }               // for the button
        public void HamburgBoxPowerStates_UPD_EventMethod(BOX_ADDRESS Addr)
        {
            Color newcolor;
            PwrState = GetHtrPwrState(Addr);

            if (PwrState == PWR_STATE.OFF)
                newcolor = Color.Red;
            else
                newcolor = Color.ForestGreen;

            if (this.button1.InvokeRequired)
                this.button1.Invoke(new Action(() => button1.BackColor = newcolor));
            else
                this.button1.BackColor = newcolor;
        }         // for the button
        public void HamburgBoxHtrParams_UPD_EventMethod(BOX_ADDRESS Addr)
        {
            Color newcolor;
            string str;

            RdbTemperature = GetHtrTemp(Addr);
            str = RdbTemperature.ToString("+000.00;-000.00;000.00");

            if (this.InvokeRequired)
                this.Invoke(new Action(() => readBack_toolTip.SetToolTip(numericTextBoxWithoutSign1, str)));
            else
                this.readBack_toolTip.SetToolTip(numericTextBoxWithoutSign1, str);

            if (!editing_temperature)
            {
                double perCent_difference = Math.Abs(1.0 - RdbTemperature / numericTextBoxWithoutSign1.DoubleValue) * 100.0;

                if (perCent_difference > ACCEPTABLE_T_VARIATION && numericTextBoxWithoutSign1.DoubleValue != 0.0)
                    newcolor = Color.OrangeRed;
                else
                    newcolor = Color.PowderBlue;

                if (this.numericTextBoxWithoutSign1.InvokeRequired)
                    this.numericTextBoxWithoutSign1.Invoke(new Action(() => numericTextBoxWithoutSign1.BackColor = newcolor));
                else
                    this.numericTextBoxWithoutSign1.BackColor = newcolor;
            }
        }           // for the numeric box
        #endregion
       
    }
}
