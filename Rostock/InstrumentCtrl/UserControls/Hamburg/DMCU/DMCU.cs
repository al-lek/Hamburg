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
    
    public partial class DMCU : UserControl
    {

        #region Constructor
        public DMCU()
        {
            InitializeComponent();
            InitializeProperties();
        }
        #endregion

        #region Event registring and values Initialisation
        private void DMCU_Load(object sender, EventArgs e)
        {
            RegisterEventMethods();
            InitializeValues();
        }
        #endregion

        #region UC Action methods
        private void Setbutton_Click(object sender, EventArgs e)
        {
            uint[] Parameters = new uint[18];
            Parameters[0] = uintTextBox1.UintegerValue;
            Parameters[1] = uintTextBox2.UintegerValue;
            Parameters[2] = uintTextBox3.UintegerValue;
            Parameters[3] = uintTextBox4.UintegerValue;
            Parameters[4] = uintTextBox5.UintegerValue;
            Parameters[5] = uintTextBox6.UintegerValue;
            Parameters[6] = uintTextBox7.UintegerValue;
            Parameters[7] = uintTextBox8.UintegerValue;
            Parameters[8] = uintTextBox9.UintegerValue;
            Parameters[9] = uintTextBox10.UintegerValue;
            Parameters[10] = uintTextBox11.UintegerValue;
            Parameters[11] = uintTextBox12.UintegerValue;
            Parameters[12] = uintTextBox13.UintegerValue;
            Parameters[13] = uintTextBox14.UintegerValue;
            Parameters[14] = uintTextBox15.UintegerValue;
            Parameters[15] = uintTextBox16.UintegerValue;
            Parameters[16] = uintTextBox17.UintegerValue;
            Parameters[17] = uintTextBox18.UintegerValue;
            SetDMCUParameters(UC_BOX_ADDRESS, ref Parameters);  
        }

        private void Getbutton_Click(object sender, EventArgs e)
        {
            uint[] temp = new uint[18];
            GetDMCUParameters(UC_BOX_ADDRESS, ref temp);

            uintTextBox1.UintegerValue = temp[0];
            uintTextBox2.UintegerValue = temp[1];
            uintTextBox3.UintegerValue = temp[2];
            uintTextBox4.UintegerValue = temp[3];
            uintTextBox5.UintegerValue = temp[4];
            uintTextBox6.UintegerValue = temp[5];
            uintTextBox7.UintegerValue = temp[6];
            uintTextBox8.UintegerValue = temp[7];
            uintTextBox9.UintegerValue = temp[8];
            uintTextBox10.UintegerValue = temp[9];
            uintTextBox11.UintegerValue = temp[10];
            uintTextBox12.UintegerValue = temp[11];
            uintTextBox13.UintegerValue = temp[12];
            uintTextBox14.UintegerValue = temp[13];
            uintTextBox15.UintegerValue = temp[14];
            uintTextBox16.UintegerValue = temp[15];
            uintTextBox17.UintegerValue = temp[16];
            uintTextBox18.UintegerValue = temp[17];
        }
     
        #endregion

        #region Event called methods

        #endregion

        #region Resize Code
      //  protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
   //     {
        //    base.SetBoundsCore(x, y, width, 26, specified);
   //     }
        #endregion

    }
}
