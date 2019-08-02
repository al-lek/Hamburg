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
        public partial class DMCU
        {
            private BOX_ADDRESS UC_BOX_ADDRESS;
            private HamburgBOX_DMCU UC_DMCU;

            #region Initialize UC properties/values and Register the Event metods of the UC
            private void InitializeProperties()
            {
                UC_BOX_ADDRESS = InstrumentCtrlInterface.HamburgBox_1.getAddress();
                UC_DMCU = HamburgBOX_DMCU.DMCU1;
            }
            private void InitializeValues()
            {
                uintTextBox1.MinimumUintValue = 0;  // the real minimum value is 1, but for warning purposes we set this to zero.
                uintTextBox1.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox2.MinimumUintValue = 0;
                uintTextBox2.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox3.MinimumUintValue = 0;
                uintTextBox3.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox4.MinimumUintValue = 0;
                uintTextBox4.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox5.MinimumUintValue = 0;
                uintTextBox5.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox6.MinimumUintValue = 0;
                uintTextBox6.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox7.MinimumUintValue = 0;
                uintTextBox7.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox8.MinimumUintValue = 0;
                uintTextBox8.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox9.MinimumUintValue = 0;
                uintTextBox9.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox10.MinimumUintValue = 0;
                uintTextBox10.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox11.MinimumUintValue = 0;
                uintTextBox11.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox12.MinimumUintValue = 0;
                uintTextBox12.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox13.MinimumUintValue = 0;
                uintTextBox13.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox14.MinimumUintValue = 0;
                uintTextBox14.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox15.MinimumUintValue = 0;
                uintTextBox15.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox16.MinimumUintValue = 0;
                uintTextBox16.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox17.MinimumUintValue = 0;
                uintTextBox17.MaximumUintValue = Math.Pow(2, 32) - 1;
                uintTextBox18.MinimumUintValue = 0;
                uintTextBox18.MaximumUintValue = Math.Pow(2, 32) - 1;

                uintTextBox1.UintegerValue = 1;
                uintTextBox2.UintegerValue = 1;
                uintTextBox3.UintegerValue = 1;
                uintTextBox4.UintegerValue = 1;
                uintTextBox5.UintegerValue = 1;
                uintTextBox6.UintegerValue = 1;
                uintTextBox7.UintegerValue = 1;
                uintTextBox8.UintegerValue = 1;
                uintTextBox9.UintegerValue = 1;
                uintTextBox10.UintegerValue = 1;
                uintTextBox11.UintegerValue = 1;
                uintTextBox12.UintegerValue = 1;
                uintTextBox13.UintegerValue = 1;
                uintTextBox14.UintegerValue = 1;
                uintTextBox15.UintegerValue = 1;
                uintTextBox16.UintegerValue = 1;
                uintTextBox17.UintegerValue = 1;
                uintTextBox18.UintegerValue = 1;
            }
            private void RegisterEventMethods()
            {
             
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
            public HamburgBOX_DMCU Hamburg_DMCU
            {
                set
                {
                    UC_DMCU = value;
                }
                get
                {
                    return (UC_DMCU);
                }
            }
            #endregion

            #region Low level Methods
            private void SetDMCUParameters(BOX_ADDRESS boxAddress, ref uint[] Parameters)
            {
                if (boxAddress == UC_BOX_ADDRESS)
                {
                    try
                    {
                        HamburgBoxInterface box;                                                                             //create an empty variable of the particular type
                        box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(boxAddress) - 1]);               //find the right box  
                        box.setDmcuParameters(UC_DMCU, ref Parameters);                  
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            private void GetDMCUParameters(BOX_ADDRESS boxAddress, ref uint[] Parameters)
            {
                try
                {
                    HamburgBoxInterface box;                                                                        //create an empty variable of the particular type
                     box = (HamburgBoxInterface)(InstrumentCtrlInterface.objArray[(ushort)(UC_BOX_ADDRESS) - 1]);      //find the right box 
                    box.getDmcuParameters(UC_DMCU, ref Parameters);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            #endregion
        }

}
