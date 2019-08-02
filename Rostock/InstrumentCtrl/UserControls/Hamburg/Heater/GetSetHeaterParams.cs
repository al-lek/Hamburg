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
    public partial class GetSetHeaterParams : UserControl
    {
        private BOX_ADDRESS UC_BOX_ADDRESS;
        private HamburgBOX_HTR_ID UC_HTR_ID;

        #region Constructor & Initialization
        public GetSetHeaterParams()
        {
            InitializeComponent();
            UC_BOX_ADDRESS = InstrumentCtrlInterface.HamburgBox_1.getAddress();
            UC_HTR_ID = HamburgBOX_HTR_ID.HTR1_ID;
        }
        #endregion

        #region Properties set/get
        public BOX_ADDRESS Hamburgbox_Address
        {
            set
            {
                UC_BOX_ADDRESS = value;
            }
            get
            {
                return (UC_BOX_ADDRESS);
            }
        }
        public HamburgBOX_HTR_ID HTR_ID
        {
            set
            {
                UC_HTR_ID = value;
            }
            get
            {
                return (UC_HTR_ID);
            }
        }
        #endregion

        #region  UC Action methods

        #region Set Heater Parameters
        /* Set KP Parameter
         * 
         */
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_updHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Kp, (ushort)intTextBoxWithoutSign1.IntegerValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /* Set KI Parameter
         * 
         */
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_updHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Ki, (ushort)intTextBoxWithoutSign2.IntegerValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /* Set KD Parameter
         * 
         */
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_updHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Kd, (ushort)intTextBoxWithoutSign3.IntegerValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /* Set Ts Parameter
         * 
         */
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_updHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Ts, (ushort)intTextBoxWithoutSign4.IntegerValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /* Set Tz Parameter
         * 
         */
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_updHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Tz, (ushort)intTextBoxWithoutSign5.IntegerValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /* Set All Parameters
         * 
         */
        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_updHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Kp, (ushort)intTextBoxWithoutSign1.IntegerValue);
                InstrumentCtrlInterface.HamburgBox_1.MB_updHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Ki, (ushort)intTextBoxWithoutSign2.IntegerValue);
                InstrumentCtrlInterface.HamburgBox_1.MB_updHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Kd, (ushort)intTextBoxWithoutSign3.IntegerValue);
                InstrumentCtrlInterface.HamburgBox_1.MB_updHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Ts, (ushort)intTextBoxWithoutSign4.IntegerValue);
                InstrumentCtrlInterface.HamburgBox_1.MB_updHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Tz, (ushort)intTextBoxWithoutSign5.IntegerValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Get Heater Parameters
        /*Get Heater KP parameter
         * 
         */
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_getHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Kp);
                textBox1.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Kp).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*Get Heater KI parameter
         * 
         */
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_getHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Ki);
                textBox2.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Ki).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*Get Heater KD parameter
         * 
         */
        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_getHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Kd);
                textBox3.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Kd).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*Get Heater Ts parameter
         * 
         */
        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_getHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Ts);
                textBox4.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Ts).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*Get Heater Tz parameter
         * 
         */
        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_getHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Tz);
                textBox5.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Tz).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*Get Heater T parameter
         * 
         */
        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_getHtrParameter(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_T);
                textBox6.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_T).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*Get Heater All parameters
        * 
        */
        private void button12_Click(object sender, EventArgs e)
        {
            ushort[] temp = new ushort[6];
            try
            {
                InstrumentCtrlInterface.HamburgBox_1.MB_getHtrParams(UC_HTR_ID);
             
                textBox1.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Kp).ToString();
                textBox2.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Ki).ToString();
                textBox3.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Kd).ToString();
                textBox4.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Ts).ToString();
                textBox5.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_Tz).ToString();
                textBox6.Text = InstrumentCtrlInterface.HamburgBox_1.getHtrParam(UC_HTR_ID, HTR_PARAM_ID.Heater_Param_ID_T).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion
        #endregion

        #region Resize Code
        /* Set fixed heigh and adjustable width in designer and runtime
         * 
         */
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, 186, specified);
        }
        #endregion
    }
}
