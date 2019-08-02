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
    public partial class SpareRLY : UserControl
    {
        private PWR_STATE PwrState;

        #region Constructor
        public SpareRLY()
        {
            InitializeComponent();
            InitializeProperties();
        }
        #endregion

        #region Event registring
        private void SpareRLY_Load(object sender, EventArgs e)
        {
            RegisterEventMethods();
            InitializeValues();
        }
        #endregion

        #region  UC Action methods
        private void button1_Click(object sender, EventArgs e)  // Start triiger button click
        {
            if (PwrState == PWR_STATE.ON)
            {
                SetPwrState(PWR_STATE.OFF);
            }
            else
            {
                SetPwrState(PWR_STATE.ON);
            }
        }

        #endregion

        #region Event called methods
        public void PowerStateChange_GLB_EventMethod(PWR_STATE state)
        {
            SetPwrState(state);
        }
        public void HamburgBoxPowerStates_UPD_EventMethod(BOX_ADDRESS Addr)
        {
            Color newcolor;
            PwrState = GetPwrState(Addr);

            if (PwrState == PWR_STATE.OFF)
                newcolor = Color.Red;
            else
                newcolor = Color.ForestGreen;
         
            if (this.button1.InvokeRequired)
                this.button1.Invoke(new Action(() => button1.BackColor = newcolor));
            else
                this.button1.BackColor = newcolor;
        
        }
        #endregion
    }
}
