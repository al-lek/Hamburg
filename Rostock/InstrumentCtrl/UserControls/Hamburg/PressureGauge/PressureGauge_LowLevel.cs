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
    public enum GaugeNum : byte
    {
        Gauge1 = 0x00,
        Gauge2 = 0x01,
        Gauge3 = 0x02,
        Gauge4 = 0x03,
    }
    public enum GaugeType : byte
    {
        AutoDetect = 0x00,
        APG_L = 0x01,
        APG100_XM = 0x02,
        APG100_XLC = 0x03,
        WRG_S = 0x04,
        APG_Mx = 0x05,
        STR_GAUGE = 0x06,
    }
    public struct Gauge
    {
        public HamburgBOX_ADC_ID Id_AdcId;   
        public HamburgBOX_ADC_ID Value_AdcId;

        public Gauge(HamburgBOX_ADC_ID Id_AdcId, HamburgBOX_ADC_ID Value_AdcId)
        {
            this.Id_AdcId = Id_AdcId;
            this.Value_AdcId = Value_AdcId;
        }
    };
    public partial class PressureGauge : UserControl
    {
        static Gauge[] GaugeArray = new Gauge[4]{ 
            new Gauge(HamburgBOX_ADC_ID.ADC_ID_PRG_Id1, HamburgBOX_ADC_ID.ADC_ID_PRG_Val1),
            new Gauge(HamburgBOX_ADC_ID.ADC_ID_PRG_Id2 , HamburgBOX_ADC_ID.ADC_ID_PRG_Val2),
            new Gauge(HamburgBOX_ADC_ID.ADC_ID_PRG_Id3 , HamburgBOX_ADC_ID.ADC_ID_PRG_Val3),
            new Gauge(HamburgBOX_ADC_ID.ADC_ID_PRG_Id4 , HamburgBOX_ADC_ID.ADC_ID_PRG_Val4),
        };

        private GaugeVoltageToPressure Volt2PresConverter;
        private BOX_ADDRESS UC_BOX_ADDRESS;
        private GaugeNum UC_GAUGE_NUM;
        private GaugeType UC_GAUGE_TYPE;

        #region Initialize UC properties/values and Register the Event metods of the UC
        private void InitializeProperties()
        {
            UC_BOX_ADDRESS = BOX_ADDRESS.HamburgBox_1;
            UC_GAUGE_NUM = GaugeNum.Gauge1;
            UC_GAUGE_TYPE = GaugeType.AutoDetect;
            Volt2PresConverter = new GaugeVoltageToPressure();
        }
        private void InitializeValues()
        {
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
        public GaugeNum GaugeNumber
        {
            set{
                UC_GAUGE_NUM = value;
            }
            get{
                return (UC_GAUGE_NUM);
            }
        }
        public GaugeType GaugeType {
            set 
            {
                UC_GAUGE_TYPE = value;
            }
            get {
                return (UC_GAUGE_TYPE);
            }
        }
        #endregion

        #region Low level Methods
        private void Get_Id_Pressure(BOX_ADDRESS boxAddress, ref EdwardsGaugeId G_Id, ref double Pressure)
        {
            ushort GaugeIdAdcValue, GaugeAdcValue;
            double GaugeIdVolts, GaugeSigVolts;

            if (boxAddress == UC_BOX_ADDRESS)
            {
                HamburgBoxInterface box;                                                                             //create an empty variable of the particular type
                box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress)-1]);                     //find the right box  

                GaugeIdAdcValue = box.getReadback(GaugeArray[(byte)UC_GAUGE_NUM].Id_AdcId);
                GaugeAdcValue = box.getReadback(GaugeArray[(byte)UC_GAUGE_NUM].Value_AdcId);
               

                GaugeIdVolts = (GaugeIdAdcValue / 4096.0) * 10.0;
                GaugeSigVolts = (GaugeAdcValue / 4096.0) * 10.0;
                if (UC_GAUGE_TYPE == GaugeType.AutoDetect)
                {
                    G_Id = Volt2PresConverter.GetId(GaugeSigVolts, GaugeIdVolts);
                    Pressure = Volt2PresConverter.GetPressure(G_Id, GasType, GaugeSigVolts);
                }
                else if (UC_GAUGE_TYPE == GaugeType.STR_GAUGE)
                {
                    G_Id = (EdwardsGaugeId)UC_GAUGE_TYPE;
                    Pressure = (1000 * GaugeSigVolts) / 10;
                }
                else
                {
                    G_Id = (EdwardsGaugeId)UC_GAUGE_TYPE;
                    Pressure = Volt2PresConverter.GetPressure(G_Id, GasType, GaugeSigVolts);
                }
            }
        }
        #endregion 
    }
}
