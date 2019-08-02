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
    public enum HamburgBOX_SWOPAMP_OUT_ID : uint
    {
        Out1 = 0x00,  //as marked on the back of the box
        Out2 = 0x01,
        Out3 = 0x02,
        Out4 = 0x03,
    }
    public enum HamburgBOX_SWOPAMP_SLOT : uint
    {
        Slot1 = 0x00,
    }
    public struct SwOpAmpPSU
    {
        public HamburgBOX_DAC_ID OutxA_DacId;  //Default initial state 
        public HamburgBOX_DAC_ID OutxB_DacId;
        public HamburgBOX_ADC_ID AdcId;
        public double Dacoffset_Default;    // Vout-Vmin(set)           if we have Vout<0 take -(Vsomething1 - Vsomething2)
        public double Dacgain_Default;      //  Vmax(set) - Vout        if we have Vmax(set)<0 take -(Vsomething1 - Vsomething2)
        public double Adcoffset_Default;    //  Vrdb-Vmin(set)              same
        public double Adcgain_Default;      //  Vmax(set)- Vrdb             same
        public double Dacoffset_Sw;    // Vout-Vmin(set)                    same
        public double Dacgain_Sw;      // Vmax(set) - Vout                  same
        public double Adcoffset_Sw;    // Vrdb-Vmin(set)                    same
        public double Adcgain_Sw;      // Vmax(set)- Vrdb                   same

        public SwOpAmpPSU(HamburgBOX_DAC_ID OutxA_DacId, HamburgBOX_DAC_ID OutxB_DacId, HamburgBOX_ADC_ID AdcId, double Dacoffset_Default, double Dacgain_Default, double Adcoffset_Default, double Adcgain_Default,
            double Dacoffset_Sw, double Dacgain_Sw, double Adcoffset_Sw, double Adcgain_Sw)
        {
            this.OutxA_DacId = OutxA_DacId;
            this.OutxB_DacId = OutxB_DacId;
            this.AdcId = AdcId;
            this.Dacoffset_Default = Dacoffset_Default;
            this.Dacgain_Default = Dacgain_Default;
            this.Adcoffset_Default = Adcoffset_Default;
            this.Adcgain_Default = Adcgain_Default;
            this.Dacoffset_Sw = Dacoffset_Sw;
            this.Dacgain_Sw = Dacgain_Sw;
            this.Adcoffset_Sw = Adcoffset_Sw;
            this.Adcgain_Sw = Adcgain_Sw;
        }
    };
    public partial class SwOpAmp
    {
        static SwOpAmpPSU[,] SwOpAmpCard = new SwOpAmpPSU[1, 4]{  //one SwOpAmp card - 4 psus(outputs)
        {  new SwOpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT1A_SLOT1, HamburgBOX_DAC_ID.DAC_ID_OUT1B_SLOT1, HamburgBOX_ADC_ID.ADC_ID_OUT1_SLOT1,  -0.25, 6.18, 0, -1.58, 0, 0, 0, 0),   //SLOT1->SwOpAmpCard(0-300V)
            new SwOpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT2A_SLOT1, HamburgBOX_DAC_ID.DAC_ID_OUT2B_SLOT1, HamburgBOX_ADC_ID.ADC_ID_OUT2_SLOT1, -0.31, 6.18, 0, -2.45, 0, 0, 0, 0),
            new SwOpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT3A_SLOT1, HamburgBOX_DAC_ID.DAC_ID_OUT3B_SLOT1, HamburgBOX_ADC_ID.ADC_ID_OUT3_SLOT1, 0.5, 4.64, 0.5, -2.67, 0.31, 5.2, -0.21, 1),
            new SwOpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT4A_SLOT1, HamburgBOX_DAC_ID.DAC_ID_OUT4B_SLOT1, HamburgBOX_ADC_ID.ADC_ID_OUT4_SLOT1, 0.34, 7.01, 0.37, -2.89, 0.06, 7.1, -0.37, 0.6)},
        };

        private BOX_ADDRESS UC_BOX_ADDRESS;
        private HamburgBOX_SWOPAMP_OUT_ID UC_SWOPAMP_OUT_ID;
        private HamburgBOX_SWOPAMP_SLOT UC_SWOPAMP_SLOT;
        private PSU_RANGE UC_PSU_RANGE;
        private double ACCEPTABLE_V_VARIATION;


        #region Initialize UC properties/values and Register the Event metods of the UC
        private void InitializeProperties()
        {
            UC_BOX_ADDRESS = InstrumentCtrlInterface.HamburgBox_1.getAddress();
            UC_SWOPAMP_OUT_ID = HamburgBOX_SWOPAMP_OUT_ID.Out1;
            UC_SWOPAMP_SLOT = HamburgBOX_SWOPAMP_SLOT.Slot1;
            ACCEPTABLE_V_VARIATION = 10.0;
            UC_PSU_RANGE = PSU_RANGE._300V;
        }
        private void InitializeValues()
        {
            if (UC_PSU_RANGE == PSU_RANGE._300V)
            {
                numericTextBoxWithSign1.MinimumDValue = 0.0;
                numericTextBoxWithSign1.MaximumDValue = 300.0;
                numericTextBoxWithSign2.MinimumDValue = 0.0;
                numericTextBoxWithSign2.MaximumDValue = 300.0;
            }
            else if (UC_PSU_RANGE == PSU_RANGE._Plus_Minus_150V)
            {
                numericTextBoxWithSign1.MinimumDValue = -150.0;
                numericTextBoxWithSign1.MaximumDValue = +150.0;
                numericTextBoxWithSign2.MinimumDValue = -150.0;
                numericTextBoxWithSign2.MaximumDValue = +150.0;
            }
        }
        private void RegisterEventMethods()
        {
            InstrumentCtrlInterface.HamburgBoxAdcValues_UPD_Event += HamburgBoxAdcValues_UPD_EventMethod;
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
        public HamburgBOX_SWOPAMP_OUT_ID SwOpAmp_OutID
        {
            set
            {
                UC_SWOPAMP_OUT_ID = value;
            }
            get
            {
                return (UC_SWOPAMP_OUT_ID);
            }
        }
        public HamburgBOX_SWOPAMP_SLOT SwOpAmp_SlotNum
        {
            set
            {
                UC_SWOPAMP_SLOT = value;
            }
            get
            {
                return (UC_SWOPAMP_SLOT);
            }
        }
        public PSU_RANGE Psu_Range
        {
            set
            {
                UC_PSU_RANGE = value;
            }
            get
            {
                return (UC_PSU_RANGE);
            }
        }

        /**** Acceptable Voltage Variation  *****/
        public double Hamburg_PercentVoltageVariation
        {
            set
            {
                ACCEPTABLE_V_VARIATION = value;
            }
            get
            {
                return (ACCEPTABLE_V_VARIATION);
            }
        }
        #endregion

        #region Low level Methods
        private double GetRdbVoltage(BOX_ADDRESS boxAddress)
        {
            double value = 0;
            double adcvalue = 0;
            if (boxAddress == UC_BOX_ADDRESS)
            {
                try
                {
                    HamburgBoxInterface box;                                                                             //create an empty variable of the particular type
                    box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress) - 1]);                     //find the right box  
                    value = box.getReadback(SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].AdcId);     // get the readback you need    
                    if (UC_PSU_RANGE == PSU_RANGE._300V)
                    {
                        adcvalue = box.getReadback(SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].AdcId);                                // get the readback you need    
                        value = adcvalue * 300 / 4095 - SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Adcoffset_Default;              // remove Voffset
                        value = value + SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Adcgain_Default * value / 300;
                        return value;
                        // return ((value * 300 / 4095));
                    }
                    else
                    {
                        adcvalue = box.getReadback(SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].AdcId);                                // get the readback you need    
                        value = ((value * 150 / 2047) - 150) - SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Adcoffset_Default;              // remove Voffset
                        value = value + SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Adcgain_Default * adcvalue / 4095;
                        return value;
                        //return ((value * 150 / 2047) - 150);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return 0;
                }
            }
            return 0;
        }
        private byte SetDefaultVoltage(double Value)
        {
            double DacValue = 0;
            double tmp = 0;
            if (UC_PSU_RANGE == PSU_RANGE._300V)
            {
                // DacValue = (ushort)((65535.0 * Value) / 300);
                tmp = (Value * 65535) / 300 - (SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Dacoffset_Default * 16383) / 300; // remove Voffset
                tmp = tmp + ((SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Dacgain_Default * tmp) / 300);
                if (tmp < 0)
                {
                    DacValue = 0;
                }
                else
                {
                    DacValue = (ushort)tmp;
                }
                if (tmp >= 65535)
                {
                    DacValue = 65535;
                }
            }
            else if (UC_PSU_RANGE == PSU_RANGE._Plus_Minus_150V)
            {
                //DacValue = (ushort)(65535.0 * (Value + 150.0) / (2.0 * 150.0));
                tmp = 65535.0 * (Value + 150.0) / 300 - (SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Dacoffset_Default * 65535.0) / 300; // remove Voffset
                tmp = tmp + ((SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Dacgain_Default * tmp) / 300);
                if (tmp < 0)
                {
                    DacValue = 0;
                }
                else
                {
                    DacValue = (ushort)tmp;
                }
                if (tmp >= 65535)
                {
                    DacValue = 65535;
                }
            }
            try
            {
                HamburgBoxInterface box;                                                                   //create an empty variable of the particular type
                box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS) - 1]);      //find the right box and cast it into the previous object 
                box.MB_updDacValue(SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].OutxA_DacId, (ushort)DacValue);
                return 1;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }
        private byte SetSwitchVoltage(double Value)
        {
            double DacValue = 0;
            double tmp = 0;
            if (UC_PSU_RANGE == PSU_RANGE._300V)
            {
                // DacValue = (ushort)((65535.0 * Value) / 300);
                tmp = (Value * 65535) / 300 - (SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Dacoffset_Sw * 16383) / 300; // remove Voffset
                tmp = tmp + ((SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Dacgain_Sw * tmp) / 300);
                if (tmp < 0)
                {
                    DacValue = 0;
                }
                else
                {
                    DacValue = (ushort)tmp;
                }
                if (tmp >= 65535)
                {
                    DacValue = 65535;
                }
            }
            else if (UC_PSU_RANGE == PSU_RANGE._Plus_Minus_150V)
            {
                //DacValue = (ushort)(65535.0 * (Value + 150.0) / (2.0 * 150.0));
                tmp = 65535.0 * (Value + 150.0) / 300 - (SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Dacoffset_Sw * 65535.0) / 300; // remove Voffset
                tmp = tmp + ((SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].Dacgain_Sw * tmp) / 300);
                if (tmp < 0)
                {
                    DacValue = 0;
                }
                else
                {
                    DacValue = (ushort)tmp;
                }
                if (tmp >= 65535)
                {
                    DacValue = 65535;
                }
            }
            try
            {
                HamburgBoxInterface box;                                                                   //create an empty variable of the particular type
                box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS) - 1]);      //find the right box and cast it into the previous object 
                box.MB_updDacValue(SwOpAmpCard[(byte)UC_SWOPAMP_SLOT, (byte)UC_SWOPAMP_OUT_ID].OutxB_DacId, (ushort)DacValue);
                return 1;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }
        #endregion
    }
}
