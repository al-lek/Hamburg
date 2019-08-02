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
    public partial class PressureGauge : UserControl
    {
        private EdwardsGaugeId GaugeID;
        private double Pressure;
        private GaugeGasType GasType;

        #region Constructor
        public PressureGauge()
        {
            InitializeComponent();
            InitializeProperties();
            GasType = GaugeGasType.Nitrogen;
            button1.Text = "N\u2082";
            button1.BackColor = Color.LightSkyBlue;
            label1.Text = EdwardsGaugeId.NoId.ToString();
        }
        #endregion

        #region Event registring and values Initialisation
        private void PressureGauge_Load(object sender, EventArgs e)
        {
            RegisterEventMethods();
            InitializeValues();
        }
        #endregion

        #region Get Parameters
        /* Return Edwards Gauge Id
         * 
         */
        public EdwardsGaugeId GetGaugeId()
        {
            return (GaugeID);
        }


        /* Return Pressure
         * 
         */
        public double GetPressure()
        {
            return (Pressure);
        }


        /* Return GasType
         * 
         */
        public GaugeGasType GetGasType()
        {
            return (GasType);
        }
        #endregion

        #region UC Action methods
        private void button1_Click(object sender, EventArgs e) {
            if (GasType == GaugeGasType.Nitrogen) {
                GasType = GaugeGasType.Helium;
                button1.Text = "He";
                button1.BackColor = Color.OrangeRed;
            }
            else {
                GasType = GaugeGasType.Nitrogen;
                button1.Text = "N\u2082";
                button1.BackColor = Color.LightSkyBlue;
            }
        }
        #endregion

        #region Event called methods
        public void HamburgBoxAdcValues_UPD_EventMethod(BOX_ADDRESS Addr)
        {
            Get_Id_Pressure(Addr, ref GaugeID, ref Pressure);
            if (this.label1.InvokeRequired)
                this.label1.Invoke(new Action(() => label1.Text = GaugeID.ToString()));
            else
                this.label1.Text = GaugeID.ToString();

            if (this.textBox1.InvokeRequired)
                this.textBox1.Invoke(new Action(() => textBox1.Text = Pressure.ToString("0.#e0;0.#e0;0.00")));
            else
                this.textBox1.Text = Pressure.ToString("0.#e0;0.#e0;0.00");
        }
        #endregion

        #region Resize Code
        /* Set fixed heigh and adjustable width in designer and runtime
         * 
         */
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, 26, specified);
        }
        #endregion
  
    }
}
