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
    public enum CARD_PSU_ID : byte
    {
        PSU_PN_150V = 0x00,
        PSU_P_300V = 0x01,
    }

    public struct CardPsu
    {
        public HamburgBOX_RLY_ID RelayId;
        public CardPsu(HamburgBOX_RLY_ID RelayId)
        {
            this.RelayId = RelayId;
        }
    };

    public partial class Card_Psu 
    {
        private BOX_ADDRESS UC_BOX_ADDRESS;
        private CARD_PSU_ID UC_HV_PSU_ID;
        static CardPsu[] CardPsusArray = new CardPsu[2]
        {
             new CardPsu(HamburgBOX_RLY_ID.RLY_ID_M30D200V),
             new CardPsu(HamburgBOX_RLY_ID.RLY_ID_M30S300V),
        };


        #region Initialize UC properties/values and Register the Event metods of the UC
        private void InitializeProperties()
        {
            UC_BOX_ADDRESS = InstrumentCtrlInterface.HamburgBox_1.getAddress();
            UC_HV_PSU_ID = CARD_PSU_ID.PSU_P_300V;
        }
        private void InitializeValues()
        {

        }
        private void RegisterEventMethods()
        {
            InstrumentCtrlInterface.PowerStateChange_GLB_Event += PowerStateChange_GLB_EventMethod;
            InstrumentCtrlInterface.HamburgBoxPowerStates_UPD_Event += HamburgBoxPowerStates_UPD_EventMethod;
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
        public CARD_PSU_ID HV_Psu_Id
        {
            set
            {
                UC_HV_PSU_ID = value;
            }
            get
            {
                return (UC_HV_PSU_ID);
            }
        }
        #endregion

        #region Low level Methods
        private PWR_STATE GetPwrState(BOX_ADDRESS boxAddress)
        {
            PWR_STATE state = PWR_STATE.OFF;
            if (boxAddress == UC_BOX_ADDRESS)
            {
                try
                {
                    HamburgBoxInterface box;                                                                             //create an empty variable of the particular type
                    box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress)-1]);                     //find the right box  
                    state = box.getPowerState(CardPsusArray[(byte)UC_HV_PSU_ID].RelayId);                                       // get the readback you need    
                    return state;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return state;
                }
            }
            return state;
        }
        private void SetPwrState(PWR_STATE state)
        {
            try
            {
                HamburgBoxInterface box;                                                                   //create an empty variable of the particular type
                box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS)-1]);      //find the right box and cast it into the previous object 
                box.setPowerState(CardPsusArray[(byte)UC_HV_PSU_ID].RelayId, state);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

    }
}
