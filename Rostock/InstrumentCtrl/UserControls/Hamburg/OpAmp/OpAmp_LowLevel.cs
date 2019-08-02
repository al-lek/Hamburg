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

    public enum HamburgBOX_OPAMP_OUT_ID : uint
    {
        Out1 = 0x00,  //as marked on the back of the box
        Out2 = 0x01,
        Out3 = 0x02,
        Out4 = 0x03,
    }
    public enum HamburgBOX_OPAMP_SLOT : uint
    {
        Slot2 = 0x00,
        Slot3 = 0x01,
        Slot4 = 0x02,
    }
    public struct OpAmpPSU
    {
        public HamburgBOX_DAC_ID DacId;
        public HamburgBOX_ADC_ID AdcId;
        public double Dacoffset;    // Vout-Vmin(set)
        public double Dacgain;      // Vmax(set) - Vout
        public double Adcoffset;    // Vrdb-Vmin(set)
        public double Adcgain;      // Vmax(set)- Vrdb

        public OpAmpPSU(HamburgBOX_DAC_ID DacId, HamburgBOX_ADC_ID AdcId, double Dacoffset, double Dacgain, double Adcoffset, double Adcgain)
        {
            this.DacId = DacId;
            this.AdcId = AdcId;
            this.Dacoffset = Dacoffset;
            this.Dacgain = Dacgain;
            this.Adcoffset = Adcoffset;
            this.Adcgain = Adcgain;
        }
    };

    public partial class OpAmp
    {
        static OpAmpPSU[,] OpAmpCard = new OpAmpPSU[3, 4]{  //three op Amp cards - 4 psus(outputs) each
           { new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT1_SLOT2, HamburgBOX_ADC_ID.ADC_ID_OUT1_SLOT2, 2.43, 2.67, 0.20, 1.45),   //SLOT2->OpAmpCard(+\-150V)
            new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT2_SLOT2, HamburgBOX_ADC_ID.ADC_ID_OUT2_SLOT2,  2.18, 2.4, -0.16, 0.94),
            new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT3_SLOT2, HamburgBOX_ADC_ID.ADC_ID_OUT3_SLOT2,  2.3, 3.1, -0.94, 0.64),
            new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT4_SLOT2, HamburgBOX_ADC_ID.ADC_ID_OUT4_SLOT2,  2.25, 2.59, -0.08, 0.94)},

           { new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT1_SLOT3, HamburgBOX_ADC_ID.ADC_ID_OUT1_SLOT3, 0.1, 4.51, 0, -0.68),   //SLOT3->OpAmpCard(+300V)_Module2
            new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT2_SLOT3, HamburgBOX_ADC_ID.ADC_ID_OUT2_SLOT3,  0.02, 4.91, 0, -2),
            new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT3_SLOT3, HamburgBOX_ADC_ID.ADC_ID_OUT3_SLOT3,  -0.07, 4.2, 0, -1.8),
            new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT4_SLOT3, HamburgBOX_ADC_ID.ADC_ID_OUT4_SLOT3,  -0.22, 4.82, 0, -2.5)},

            { new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT1_SLOT4, HamburgBOX_ADC_ID.ADC_ID_OUT1_SLOT4,   0.11, 4.5, 0.07, -1.85),   //SLOT4->OpAmpCard(+300V)_Module1
            new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT2_SLOT4, HamburgBOX_ADC_ID.ADC_ID_OUT2_SLOT4,     0.11, 4.76, 0.07, -1.24),
            new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT3_SLOT4, HamburgBOX_ADC_ID.ADC_ID_OUT3_SLOT4,     -0.173, 4.76, 0.07, -1.63),
            new OpAmpPSU( HamburgBOX_DAC_ID.DAC_ID_OUT4_SLOT4, HamburgBOX_ADC_ID.ADC_ID_OUT4_SLOT4,     0.07, 4.6, 0.07, 0.05)},
        };

        private BOX_ADDRESS UC_BOX_ADDRESS;
        private HamburgBOX_OPAMP_OUT_ID UC_OPAMP_OUT_ID;
        private HamburgBOX_OPAMP_SLOT UC_OPAMP_SLOT;
        public PSU_RANGE UC_PSU_RANGE;
        private double ACCEPTABLE_V_VARIATION;


        #region Initialize UC properties/values and Register the Event metods of the UC
        private void InitializeProperties()
        {
            UC_BOX_ADDRESS = InstrumentCtrlInterface.HamburgBox_1.getAddress();
            UC_OPAMP_OUT_ID = HamburgBOX_OPAMP_OUT_ID.Out1;
            UC_OPAMP_SLOT = HamburgBOX_OPAMP_SLOT.Slot2;
            ACCEPTABLE_V_VARIATION = 10.0;
            UC_PSU_RANGE = PSU_RANGE._300V;
        }
        private void InitializeValues()
        {
            if (UC_PSU_RANGE == PSU_RANGE._300V)
            {
                numericTextBoxWithSign1.MinimumDValue = 0.0;
                numericTextBoxWithSign1.MaximumDValue = 300.0;
            }
            else if (UC_PSU_RANGE == PSU_RANGE._Plus_Minus_150V)
            {
                numericTextBoxWithSign1.MinimumDValue = -150.0;
                numericTextBoxWithSign1.MaximumDValue = +150.0;
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
        public HamburgBOX_OPAMP_OUT_ID Hamburg_OpAmp_OutID
        {
            set
            {
                UC_OPAMP_OUT_ID = value;
            }
            get
            {
                return (UC_OPAMP_OUT_ID);
            }
        }
        public HamburgBOX_OPAMP_SLOT Hamburg_OpAmp_SlotNum
        {
            set
            {
                UC_OPAMP_SLOT = value;
            }
            get
            {
                return (UC_OPAMP_SLOT);
            }
        }
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
                    box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress) - 1]);               //find the right box  
                    value = box.getReadback(OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].AdcId);                // get the readback you need    
                    if (UC_PSU_RANGE == PSU_RANGE._300V)
                    {
                        //  return ((value * 300 / 4095));
                        adcvalue = box.getReadback(OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].AdcId);                                // get the readback you need    
                        value = adcvalue * 300 / 4095 - OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].Adcoffset;              // remove Voffset
                        value = value + OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].Adcgain * value / 300;
                        return value;
                    }
                    else
                    {
                        // return ((value * 150 / 2047) - 150);
                        adcvalue = box.getReadback(OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].AdcId);                                // get the readback you need    
                        value = ((value * 150 / 2047) - 150) - OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].Adcoffset;              // remove Voffset
                        value = value + OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].Adcgain * adcvalue / 4095;
                        return value;
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
        private byte SetVoltage(double Value)
        {
            double DacValue = 0;
            double tmp = 0;
            if (UC_PSU_RANGE == PSU_RANGE._300V)
            {
                //  DacValue = (ushort)((16383.0 * Value) / 300);
                tmp = (Value * 16383) / 300 - (OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].Dacoffset * 16383) / 300; // remove Voffset
                tmp = tmp + ((OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].Dacgain * tmp) / 300);
                if (tmp < 0)
                {
                    DacValue = 0;
                }
                else
                {
                    DacValue = (ushort)tmp;
                }
                if (tmp >= 16383)
                {
                    DacValue = 16383;
                }
            }
            else if (UC_PSU_RANGE == PSU_RANGE._Plus_Minus_150V)
            {
                // DacValue = (ushort)(16383.0 * (Value + 150.0) / (2.0 * 150.0));
                tmp = 16383.0 * (Value + 150.0) / 300 - (OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].Dacoffset * 16383) / 300; // remove Voffset
                tmp = tmp + (OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].Dacgain * tmp / 300);
                if (tmp < 0)
                {
                    DacValue = 0;
                }
                else
                {
                    DacValue = (ushort)tmp;
                }
                if (tmp >= 16383)
                {
                    DacValue = 16383;
                }
            }
            try
            {
                HamburgBoxInterface box;                                                                        //create an empty variable of the particular type
                box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS) - 1]);      //find the right box 
                box.MB_updDacValue(OpAmpCard[(byte)UC_OPAMP_SLOT, (byte)UC_OPAMP_OUT_ID].DacId, (ushort)DacValue);
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
