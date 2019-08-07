using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Reflection;
using System.Globalization;

using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Hamburg_namespace
{
    public partial class Form1 : Form
    {
        #region Variables

        double freq_min = 4, freq_max = 230000;
        string default_trig_mode = "internal";
        bool display_pulser = false, display_clock = false;

        #endregion

        public Form1()
        {
            //ListEmbeddedResourceNames();
            //HookResolver hk = new HookResolver();

            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            //AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            //{
            //    // string resourceName = new AssemblyName(args.Name).Name + ".dll";
            //    //string resource = Array.Find(this.GetType().Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

            //    //String resourceName = "AssemblyLoadingAndReflection." + new AssemblyName(args.Name).Name + ".dll";

            //    //String thisExe = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            //    //System.Reflection.AssemblyName embeddedAssembly = new System.Reflection.AssemblyName(args.Name);
            //    //String resourceName = thisExe + "." + embeddedAssembly.Name + ".dll";

            //    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(dlls[dll_idx]))
            //    {
            //        dll_idx++;
            //        Byte[] assemblyData = new Byte[stream.Length];
            //        stream.Read(assemblyData, 0, assemblyData.Length);
            //        return Assembly.Load(assemblyData);

            //    }
            //};

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            InstrumentCtrlInterface.objArray[(ushort)(InstrumentCtrlInterface.HamburgBox_1.getAddress()) - 1] = InstrumentCtrlInterface.HamburgBox_1; // box addresses start from 1 not 0 !

            InitializeComponent();
            initialize_gui();
            initialize_volt_chart();
            

            this.Shown += (s, e) => { post_initialization(); };
        }

        #region Initialization

        private void initialize_gui()
        {
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            sw1volt_lbl.ForeColor = sw1time_lbl.ForeColor = Color.RoyalBlue;
            sw2volt_lbl.ForeColor = sw2time_lbl.ForeColor = Color.SeaGreen;
            control_timings_display();

            freq_txtBox.Visible = freq_lbl.Visible = false;
            freq_txtBox.TextChanged += (s, e) => { if (dParser(freq_txtBox) != 0) rebuild_time_chart(); };
            
            // events for triggering controls
            foreach (TextBox txtBox in GetControls(timing_grpBox).OfType<TextBox>())
            {
                if (txtBox == freq_txtBox) continue;

                txtBox.TextChanged += (s, e) => { txtBox.BackColor = Color.MistyRose; if (txtBox_valueCheck(txtBox)) update_charting_timing(); };
                txtBox.KeyPress += (s, e) => 
                { 
					if (!(char.IsDigit(e.KeyChar) || e.KeyChar == '.' || e.KeyChar == (char)Keys.Back)) e.Handled = true;
                    if (e.KeyChar == '.' && txtBox.Text.Contains(".")) e.Handled = true;
                    if (e.KeyChar == (char)Keys.Enter) 
                        if (txtBox_rangeCheck(txtBox))
                            if (transmit_triggerMode() && transmit_timings())
                                validate_frequency_timings_color();
                };
                txtBox.Leave += (s, e) => { txtBox_rangeCheck(txtBox); };
            }

            // tof pulses diplay
            display_ToF_pulses(display_pulser);

            // multi-pulses control
            sw3delay2_txtBox.Visible = sw3delay3_txtBox.Visible = sw3delay4_txtBox.Visible = sw3delay5_txtBox.Visible = sw3delay6_txtBox.Visible = false;

            pulses_cmbBox.Items.AddRange(new object[4] { 1, 2, 3, 6 });
            pulses_cmbBox.SelectedIndex = 0;
            pulses_cmbBox.SelectedIndexChanged += (s, e) => { pulses_number_changed(); };

            toolStripProgressBar1.Visible = false;
            enableUI(false);

            //monitorTimer.Tick += (s, e) => { check_press(); };
        }

        private void enableUI(bool enable)
        {
            voltages_grpBox.Enabled = press_grpBox.Enabled = timing_grpBox.Enabled = PWon_btn.Enabled = PWoff_btn.Enabled = enable;

            if (enable)
            {
                Button btn1 = GetControls(card_Psu1).OfType<Button>().FirstOrDefault();
                Button btn2 = GetControls(card_Psu2).OfType<Button>().FirstOrDefault();
                if (btn1.BackColor == Color.ForestGreen && btn2.BackColor == Color.ForestGreen) enable_powerBtns("On");
                else enable_powerBtns("Off");
                //msVisionHpa_EmcoPSU_OnOff1.Invalidate(true);
            }
        }

        private void enable_powerBtns(string state)
        {
            if (state == "On") { PWon_btn.Enabled = false; PWoff_btn.Enabled = true; }
            else { PWon_btn.Enabled = true; PWoff_btn.Enabled = false; }
        }

        private void post_initialization()
        {
            // UI refernced controls
            initialize_time_chart();

            // will set triger mode and timings to a default 
            if (default_trig_mode == "internal") async_rdBtn.Checked = true;
            else sync_rdBtn.Checked = true;
        }

        public void display_ToF_pulses(bool enable)
        {
            // will display or not the pulses controls
            if (enable) pulsesGate_grpBox.Size = new Size(pulsesGate_grpBox.Width, 135);
            else pulsesGate_grpBox.Size = new Size(pulsesGate_grpBox.Width, 83);

            sw3width_txtBox.Visible = pulses_cmbBox.Visible = pulsesNum_lbl.Visible = enable;
        }

        #endregion
        
        #region Menu Items

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            initialize_settings();
        }

        private void initialize_settings(bool voltages = true, bool timings = true)
        {
            // reset all voltage controls to 0
            if (voltages)
            {
                IEnumerable<Control> voltCtrls = GetControls(voltages_grpBox);
                foreach (var ctrl in voltCtrls.Where(c => c is NumericTextBoxWithFixedSign || c is NumericTextBoxWithoutSign || c is NumericTextBoxWithSign))
                {
                    if (ctrl.Text.Contains("+")) ctrl.Text = "+0";
                    else if (ctrl.Text.Contains("-")) ctrl.Text = "-0";
                    else ctrl.Text = "0";
                }
            }

            if (timings)
            {
                IEnumerable<Control> timeCtrls = GetControls(timing_grpBox);
                foreach (var ctrl in timeCtrls.Where(c => c is TextBox))
                {
                    if (ctrl.TabIndex < 10) ctrl.Text = "10";
                    else ctrl.Text = "1";
                }
            }

            setFileNameLabel("");
            update_charting_timing();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            open_settings();
        }

        private void open_settings()
        {
            // auxiliary text lists to hold parameters
            List<string> settings = new List<string>();

            // Set the path, and create if not existing 
            string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\RF transfer line\\Settings";

            OpenFileDialog loadSettings = new OpenFileDialog() { RestoreDirectory = true, InitialDirectory = directoryPath, Filter = "Interface settings (*.set)|*.set" };

            if (loadSettings.ShowDialog() == DialogResult.OK)
            {
                // a. open file and store all data at settings List
                System.IO.StreamReader objReader;
                objReader = new System.IO.StreamReader(loadSettings.FileName);

                do { settings.Add(objReader.ReadLine()); }
                while (objReader.Peek() != -1);
                objReader.Close();

                Cursor.Current = Cursors.WaitCursor;
                //toolStripProgressBar1.Value = 0; toolStripProgressBar1.Visible = true;

                // line by line resolve and assign list contents
                // keep track of the line number to pass it to error handling
                IEnumerable<Control> voltCtrls = GetControls(voltages_grpBox);
                IEnumerable<Control> timeCtrls = GetControls(timing_grpBox);
                int line_no = 0;
                try
                {
                    for (int j = 0; j != (settings.Count); j++)
                    {
                        line_no = j + 1;                                                            // keep current line number
                        if (settings[j] != "" && settings[j].StartsWith("--") == false)             // ignore empty lines and ccommnets
                        {
                            string[] tmp = settings[j].Split('\t');
                            Label lbl_t = timeCtrls.OfType<Label>().FirstOrDefault(l => l.Text == tmp[0]);
                            Label lbl_v = voltCtrls.OfType<Label>().FirstOrDefault(l => l.Text == tmp[0]);

                            if (lbl_t != null && lbl_v != null)
                                if (tmp[1].StartsWith("+")) lbl_t = null;

                            // 1. check to see if it is switch time (2 values) or frequency (1 value) or switch volt (2 values) or plain volt (1 value)

                            // trigger mode frequency
                            if (tmp.Length == 1) timeCtrls.OfType<RadioButton>().FirstOrDefault(r => r.Text == tmp[0]).Checked = true;
                            else if (tmp.Length == 2 && tmp[0].EndsWith("[KHz]")) timeCtrls.OfType<TextBox>().FirstOrDefault(t => t.TabIndex == lbl_t.TabIndex).Text = tmp[1];

                            // time variables                                                        
                            else if (lbl_t != null)
                            {
                                // switches
                                if (tmp.Length == 3)
                                {
                                    int lbl_tab = lbl_t.TabIndex;
                                    timeCtrls.OfType<TextBox>().FirstOrDefault(t => t.TabIndex == lbl_tab && t.Name[3] == 'd').Text = tmp[1];
                                    timeCtrls.OfType<TextBox>().FirstOrDefault(t => t.TabIndex == lbl_tab && t.Name[3] == 'w').Text = tmp[2];
                                }
                                else { pulses_cmbBox.SelectedIndex = Convert.ToInt16(tmp[1]); sw3delay2_txtBox.Text = tmp[2]; sw3delay3_txtBox.Text = tmp[3]; sw3delay4_txtBox.Text = tmp[4]; sw3delay5_txtBox.Text = tmp[5]; sw3delay6_txtBox.Text = tmp[6]; }
                            }
                            
                            // combined code for both plain and switch volt
                            else
                            {
                                // find label and respective userControl
                                lbl_v = voltCtrls.OfType<Label>().FirstOrDefault(l => l.Text == tmp[0]);
                                UserControl usrCtrl = voltCtrls.OfType<UserControl>().FirstOrDefault(u => u.TabIndex == lbl_v.TabIndex);

                                GetControls(usrCtrl).First(x => (x is NumericTextBoxWithSign || x is NumericTextBoxWithoutSign || x is NumericTextBoxWithFixedSign) && x.TabIndex == 0).Text = tmp[1];

                                // this is for the switches that have 2 values
                                if (tmp.Length == 3) GetControls(usrCtrl).First(x => (x is NumericTextBoxWithSign) && x.TabIndex == 1).Text = tmp[2];
                            }
                        }
                        //toolStripProgressBar1.Value = 100 * j / 8;
                    }
                }
                catch { MessageBox.Show("Corrupt voltage profile file!!!\r\nError at line: " + line_no.ToString()); }
            }

            setFileNameLabel(loadSettings.SafeFileName);
            //toolStripProgressBar1.Visible = false;
            Cursor.Current = Cursors.Default;
            update_charting_timing();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            save_settings();
        }

        private void save_settings()
        {
			// auxiliary text lists to hold parameters
            List<string> settings = new List<string>();

            // Set the path, and create if not existing 
            string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\RF transfer line\\Settings";
            DirectoryInfo di = new DirectoryInfo(directoryPath);
            if (di.Exists != true) di.Create();

            // define saveFile dialogue properties 
            SaveFileDialog saveSettings = new SaveFileDialog() { AddExtension = true, RestoreDirectory = true, OverwritePrompt = true, InitialDirectory = directoryPath, Filter = "Interface settings (*.set)|*.set" };

            if (saveSettings.ShowDialog() == DialogResult.OK)
            {
                IEnumerable<Control> voltCtrls = GetControls(voltages_grpBox);
                foreach (UserControl usrCtrl in voltCtrls.OfType<UserControl>())
                {
                    string str_to_add;
                    string name = voltCtrls.OfType<Label>().FirstOrDefault(n => n.TabIndex == usrCtrl.TabIndex).Text;
                    string value = GetControls(usrCtrl).Where(u => u is NumericTextBoxWithSign || u is NumericTextBoxWithoutSign || u is NumericTextBoxWithFixedSign).FirstOrDefault(ntb => ntb.TabIndex == 0).Text;
                    str_to_add = name + "\t" + value;

                    // switches volt have 2 settings
                    if (usrCtrl is SwOpAmp)
                        str_to_add += "\t" + GetControls(usrCtrl).Where(u => u is NumericTextBoxWithSign).FirstOrDefault(ntb => ntb.TabIndex == 1).Text;

                    settings.Add(str_to_add);
                }

                IEnumerable<Control> timeCtrls = GetControls(timing_grpBox);

                // save mode of trigger, and frequency if extrnal
                settings.Add(timeCtrls.OfType<RadioButton>().FirstOrDefault(c => c.Checked).Text);
                if (sync_rdBtn.Checked) 
                    settings.Add(timeCtrls.OfType<Label>().FirstOrDefault(n => n.TabIndex == freq_txtBox.TabIndex).Text + "\t" + freq_txtBox.Text);

                // record timings
                foreach (Label lbl in timeCtrls.OfType<Label>().Where(l => l.Tag == "save"))
                {
                    string str_to_add;
                    string name = lbl.Text;
                    string delay = timeCtrls.OfType<TextBox>().FirstOrDefault(t => t.TabIndex == lbl.TabIndex && t.Name[3] == 'd').Text;
                    string width = timeCtrls.OfType<TextBox>().FirstOrDefault(t => t.TabIndex == lbl.TabIndex && t.Name[3] == 'w').Text;
                    str_to_add = name + "\t" + delay + "\t" + width;
                    settings.Add(str_to_add);
                }
                
                // brute non generic - add all pulses
                settings.Add(pulsesNum_lbl.Text + "\t" + pulses_cmbBox.SelectedIndex + "\t" + sw3delay2_txtBox.Text + "\t" + sw3delay3_txtBox.Text + "\t" + sw3delay4_txtBox.Text + "\t" + sw3delay5_txtBox.Text + "\t" + sw3delay6_txtBox.Text);

                // Open streamWriter for voltage profiles
                System.IO.StreamWriter volt_profile = new System.IO.StreamWriter(saveSettings.OpenFile());
                for (int i = 0; i < settings.Count; i++)
                    volt_profile.WriteLine(settings[i]);

                volt_profile.Dispose(); volt_profile.Close();

                setFileNameLabel(Path.GetFileName(saveSettings.FileName));
            }
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            String message = this.Text.Remove(this.Text.Length - 4, 4) + "\n";
            message += "Version " + this.Text.Substring(this.Text.Length - 3) + " - April 2018\n\n";
            message += "Fasmatech S.A." + "   \u00A9";
            MessageBox.Show(message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void optionsToolStripButton_Click(object sender, EventArgs e)
        {
            options();
        }

        private void options()
        {
            Form optionsForm = new Form()
            {
                Text = "Program settings",
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.Manual,
                Location = Control.MousePosition,
                Size = new Size(150, 110),
                AutoSize = false
            };

            CheckBox tof_pulser = new CheckBox() { Text = "Display ToF pulses.", Checked = display_pulser, AutoSize = true, Location = new Point(5, 5) };
            tof_pulser.CheckedChanged += (sk, ek) => { display_pulser = tof_pulser.Checked; display_ToF_pulses(display_pulser); resize_time_chart(); rebuild_time_chart(); };
            tof_pulser.PreviewKeyDown += (sk, ek) => { if (ek.KeyCode == Keys.Escape) optionsForm.Close(); };

            CheckBox clock = new CheckBox() { Text = "Display trigger clock.", Checked = display_clock, AutoSize = true, Location = new Point(5, 30) };
            clock.CheckedChanged += (sk, ek) => { display_clock = clock.Checked; rebuild_time_chart(); };
            clock.PreviewKeyDown += (sk, ek) => { if (ek.KeyCode == Keys.Escape) optionsForm.Close(); };

            optionsForm.FormClosing += (sf, ef) => { display_pulser = tof_pulser.Checked; display_clock = clock.Checked; };

            optionsForm.Controls.AddRange(new Control[] { tof_pulser, clock });

            optionsForm.Show();
        }

        #endregion
        
        #region Global Enable / Power / Set

        public void connection_change(bool connected)
        {
            // is called from userControl connection
            if (connected) enableUI(true);
            else enableUI(false);
        }

        private void HVon_btn_Click(object sender, EventArgs e)
        {
            // set all DACs at safe default state BEFORE turning On
            if (initialize_interface_state())
            {
                try { InstrumentCtrlInterface.GlobalPowerOn(); enable_powerBtns("On"); }
                catch { }
            }
            else MessageBox.Show("Error: Could not set interface to default state before powering On/Off!", "Error!");
        }

        private void HVoff_btn_Click(object sender, EventArgs e)
        {
            // set all DACs at safe default state BEFORE turning Off
            if (initialize_interface_state())
            {
                try { InstrumentCtrlInterface.GlobalPowerOff(); enable_powerBtns("Off"); }
                catch { }
            }
            else MessageBox.Show("Error: Could not set interface to default state before powering On/Off!", "Error!");
        }

        private void setAll_btn_Click(object sender, EventArgs e)
        {
            if (!isConnected()) return;

            DialogResult result = MessageBox.Show("Apply All Voltages?", "Confirm", MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK) set_all();
        }

        private void set_all()
        {
            BlockMouse bl = new BlockMouse();
            bl.DisableMouse();

            IEnumerable<Control> voltCtrls = GetControls(voltages_grpBox).Where(n => n.Enabled && (n is NumericTextBoxWithFixedSign || n is NumericTextBoxWithoutSign || n is NumericTextBoxWithSign));

            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Maximum = voltCtrls.Count();

            foreach (var numTxt in voltCtrls)
            {
                if (numTxt.Enabled)
                {
                    this.ActiveControl = numTxt;
                    SendKeys.SendWait("{ENTER}");
                    Thread.Sleep(100); Application.DoEvents();
                    toolStripProgressBar1.Value++;
                }
            }

            // set timings
            this.ActiveControl = sw1delay1_txtBox;
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(100); Application.DoEvents();

            toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

            bl.EnableMouse();

            // generate a timer to make ProgressBar diappear after 3 seconds
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer() { Interval = 2000 };
            timer.Tick += (st, et) => { toolStripProgressBar1.Visible = false; toolStripProgressBar1.Value = 0; timer.Stop(); timer.Dispose(); };
            timer.Start();
        }

        private bool initialize_interface_state()
        {
            // stop triggering, set default values, and apply
            bool ok = false;

            start_StopTrigger1.StopTrigger();
            initialize_settings();
            set_all();
            ok = true;

            return ok;
        }

        #endregion
        
        #region Charting
        public void update_charting_timing(int debug = 0)   //UserControl usrCtrl = null, 
        {
            // is also called from all voltage userControls after a succesful set.
            setFileNameLabel(" *");
            rebuild_volt_chart();
            rebuild_time_chart();
            control_timings_display();
            //calculate_pulses();
        }

        private void initialize_volt_chart()
        {
            // define chart properties
            OxyPlot.WindowsForms.PlotView oxyPlotV = new OxyPlot.WindowsForms.PlotView() { Name = "Voltage chart", Location = new Point(panel1.Location.X, panel1.Location.Y - 160), Size = new Size(panel1.Size.Width, 150), BackColor = Color.WhiteSmoke };
            SuspendLayout();
            this.Controls.Add(oxyPlotV);

            PlotModel oxyModelV = new PlotModel { PlotType = PlotType.XY, TitleFontSize = 11 };

            var linearAxis1 = new OxyPlot.Axes.LinearAxis() { MajorGridlineStyle = LineStyle.Solid, Title = "Voltage [V]", MaximumPadding = 0.25, IsPanEnabled = false, Minimum = -5.0 };        //, MinorGridlineStyle = LineStyle.Solid
            oxyModelV.Axes.Add(linearAxis1);

            var linearAxis2 = new OxyPlot.Axes.LinearAxis() { MajorGridlineStyle = LineStyle.Solid, Title = "axial dimension [mm]", Position = OxyPlot.Axes.AxisPosition.Bottom, Minimum = -5.0, Maximum = 430.0 };
            oxyModelV.Axes.Add(linearAxis2);

            var myController = new PlotController();
            oxyPlotV.Controller = myController;
            myController.BindMouseDown(OxyMouseButton.Left, PlotCommands.ZoomRectangle);
            myController.BindMouseDown(OxyMouseButton.Right, PlotCommands.ResetAt);

            oxyPlotV.Model = oxyModelV;
        }

        private void rebuild_volt_chart()
        {
            PlotView plot = (PlotView)GetControls(this).FirstOrDefault(p => p is PlotView && p.Name == "Voltage chart");
            plot.SuspendLayout();
            plot.Model.Series.Clear();

            double[] insrt_dims = new double[20] { 0.0, 63.5, 65.5, 65.5, 164, 164, 166, 170, 176.6, 176.6, 192.8, 192.8, 193.6, 194.2, 196.2, 196.2, 414.2, 414.2, 416.2, 416.8 };
            UserControl[] ctrl = new UserControl[10] { opAmp1, opAmp2, opAmp3, opAmp4, opAmp5, opAmp6, swOpAmp1, opAmp8, opAmp9, swOpAmp2 };

            LineSeries firstState = new LineSeries() { StrokeThickness = 2, Color = OxyColors.DarkGray, LabelFormatString = "{1}", LabelMargin = 2 };
            LineSeries hexGate = new LineSeries() { StrokeThickness = 2, Color = OxyColors.RoyalBlue, LabelFormatString = "{1}", LabelMargin = 2, TextColor = OxyColors.RoyalBlue };

            for (int i = 0; i < ctrl.Length; i++)
            {
                firstState.Points.Add(new OxyPlot.DataPoint(insrt_dims[2*i], GetVoltage(ctrl[i])));
                firstState.Points.Add(new OxyPlot.DataPoint(insrt_dims[2*i + 1], GetVoltage(ctrl[i])));
            }

            hexGate.Points.Add(new OxyPlot.DataPoint(insrt_dims[17], GetVoltage(opAmp9)));
            hexGate.Points.Add(new OxyPlot.DataPoint(insrt_dims[18], GetVoltage(swOpAmp2, 1)));
            hexGate.Points.Add(new OxyPlot.DataPoint(insrt_dims[19], GetVoltage(swOpAmp2, 1)));
            //hexGate.Points.Add(new OxyPlot.DataPoint(insrt_dims[20], GetVoltage(opAmp10)));
            
            plot.Model.Series.Add(firstState);
            if (isSwitching(swOpAmp2)) plot.Model.Series.Add(hexGate);
            plot.Model.InvalidatePlot(true);

            plot.Model.DefaultYAxis.MajorStep = (plot.Model.DefaultYAxis.Maximum - plot.Model.DefaultYAxis.Minimum) / 5.0;
        }

        private void initialize_time_chart()
        {
            // define chart properties
            OxyPlot.WindowsForms.PlotView oxyPlotT = new OxyPlot.WindowsForms.PlotView() { Name = "Timings chart", BackColor = Color.WhiteSmoke };
            SuspendLayout();
            timing_grpBox.Controls.Add(oxyPlotT);
            resize_time_chart();

            PlotModel oxyModelT = new PlotModel { PlotType = PlotType.XY, TitleFontSize = 11 };

            var linearAxis1T = new OxyPlot.Axes.LinearAxis() { MajorGridlineStyle = LineStyle.Solid, Title = "", MaximumPadding = 0.25, IsPanEnabled = false, IsAxisVisible = false };        //, MinorGridlineStyle = LineStyle.Solid
            oxyModelT.Axes.Add(linearAxis1T);

            var linearAxis2T = new OxyPlot.Axes.LinearAxis() { MajorGridlineStyle = LineStyle.Solid, Title = "Time [ms]", Position = OxyPlot.Axes.AxisPosition.Bottom }; //Minimum = -1.0, Maximum = 10.0
            oxyModelT.Axes.Add(linearAxis2T);

            var myControllerT = new PlotController();
            oxyPlotT.Controller = myControllerT;
            myControllerT.BindMouseDown(OxyMouseButton.Left, PlotCommands.ZoomRectangle);
            myControllerT.BindMouseDown(OxyMouseButton.Right, PlotCommands.ResetAt);

            oxyPlotT.Model = oxyModelT;
        }

        public void resize_time_chart()
        {
            // will decide the height of the time chart
            PlotView plot = (PlotView)GetControls(timing_grpBox).FirstOrDefault(p => p is PlotView && p.Name == "Timings chart");
            plot.Location = new Point(pulsesGate_grpBox.Location.X, pulsesGate_grpBox.Location.Y + pulsesGate_grpBox.Height + 10);
            plot.Size = new Size(pulsesGate_grpBox.Width, timing_grpBox.Height - triggering_grpBox.Height - pulsesGate_grpBox.Height - 40);
        }

        private void rebuild_time_chart()
        {
            PlotView plot = (PlotView)GetControls(timing_grpBox).FirstOrDefault(p => p is PlotView && p.Name == "Timings chart");
            plot.Model.Series.Clear();

            // get the times
            double funnelDelay = 0.001, funnelWidth = 0.001, gateDelay = 0.001, gateWidth = 0.001, pulseDelay = 0.001, pulseWidth = 0.001;
            // break if txtbox is empty
            foreach (TextBox txtBox in GetControls(timing_grpBox).OfType<TextBox>()) if (txtBox.Text == "") return;

            if (GetVoltage(swOpAmp1, 0) != GetVoltage(swOpAmp1, 1)) { funnelDelay = Convert.ToDouble(sw1delay1_txtBox.Text); funnelWidth = Convert.ToDouble(sw1width_txtBox.Text); }
            if (GetVoltage(swOpAmp2, 0) != GetVoltage(swOpAmp2, 1)) { gateDelay = Convert.ToDouble(sw2delay1_txtBox.Text); gateWidth = Convert.ToDouble(sw2width_txtBox.Text); }
            pulseDelay = Convert.ToDouble(sw3delay1_txtBox.Text); pulseWidth = Convert.ToDouble(sw3width_txtBox.Text);

            // define lineseries
            LineSeries frequency = new LineSeries() { StrokeThickness = 2, Color = OxyColors.DarkSlateGray };
            LineSeries funnelT = new LineSeries() { StrokeThickness = 2, Color = OxyColors.RoyalBlue };
            LineSeries gateT = new LineSeries() { StrokeThickness = 2, Color = OxyColors.SeaGreen };
            LineSeries pulseT = new LineSeries() { StrokeThickness = 2, Color = OxyColors.Black };

            // define one run total time
            double single_run_duration = (new double[3] { funnelDelay + funnelWidth, gateDelay + gateWidth, pulseDelay + pulseWidth }).Max();

            // get the reference frequency if declared and build freq lineserie
            try
            {
                double freq = dParser(freq_txtBox);

                if (freq != 0.0)
                {
                    //double period = 1.0 / dParser(freq_txtBox); //millisecond
                    List<double[]> freq_points = build_frequency_pulses(1.0 / freq, single_run_duration);
                    for (int i = 0; i < freq_points.Count; i++)
                        frequency.Points.Add(new OxyPlot.DataPoint(freq_points[i][0], 6 + freq_points[i][1]));
                }
            }
            catch { }

            // build the rest lineseries
            int init = 0, mid = 0, fin = 0;

            if (GetVoltage(swOpAmp1, 0) == GetVoltage(swOpAmp1, 1)) { init = 4; mid = 4; fin = 4; }
            else if (GetVoltage(swOpAmp1, 0) < GetVoltage(swOpAmp1, 1)) { init = 4; mid = 5; fin = 4; }
            else { init = 5; mid = 4; fin = 5; }

            funnelT.Points.Add(new OxyPlot.DataPoint(0, init));
            funnelT.Points.Add(new OxyPlot.DataPoint(funnelDelay, init));
            funnelT.Points.Add(new OxyPlot.DataPoint(funnelDelay, mid));
            funnelT.Points.Add(new OxyPlot.DataPoint(funnelDelay + funnelWidth, mid));
            funnelT.Points.Add(new OxyPlot.DataPoint(funnelDelay + funnelWidth, fin));
            funnelT.Points.Add(new OxyPlot.DataPoint(single_run_duration, fin));

            if (GetVoltage(swOpAmp2, 0) == GetVoltage(swOpAmp2, 1)) { init = 2; mid = 2; fin = 2; }
            else if (GetVoltage(swOpAmp2, 0) < GetVoltage(swOpAmp2, 1)) { init = 2; mid = 3; fin = 2; }
            else { init = 3; mid = 2; fin = 3; }

            gateT.Points.Add(new OxyPlot.DataPoint(0, init));
            gateT.Points.Add(new OxyPlot.DataPoint(gateDelay, init));
            gateT.Points.Add(new OxyPlot.DataPoint(gateDelay, mid));
            gateT.Points.Add(new OxyPlot.DataPoint(gateDelay + gateWidth, mid));
            gateT.Points.Add(new OxyPlot.DataPoint(gateDelay + gateWidth, fin));
            gateT.Points.Add(new OxyPlot.DataPoint(single_run_duration, fin));
                        
            init = 0; mid = 1; fin = 0;

            pulseT.Points.Add(new OxyPlot.DataPoint(0, init));
            pulseT.Points.Add(new OxyPlot.DataPoint(pulseDelay, init));
            pulseT.Points.Add(new OxyPlot.DataPoint(pulseDelay, mid));
            pulseT.Points.Add(new OxyPlot.DataPoint(pulseDelay + pulseWidth, mid));
            pulseT.Points.Add(new OxyPlot.DataPoint(pulseDelay + pulseWidth, fin));
            pulseT.Points.Add(new OxyPlot.DataPoint(single_run_duration, fin));

            plot.Model.Series.Add(funnelT);
            plot.Model.Series.Add(gateT);
            if (display_clock) plot.Model.Series.Add(frequency);
            if (display_pulser) plot.Model.Series.Add(pulseT);

            plot.Model.InvalidatePlot(true);
        }

        private List<double[]> build_frequency_pulses(double period, double duration)
        {
            // returns an array of time and height |`|_|`|_|`|_|`|_|`|_|`|_|`|_|`|_|`|_|`|_
            List<double[]> data = new List<double[]>();

            int total_periods = Convert.ToInt32(duration / period);

            for (int i = 0; i <= total_periods; )
            {
                data.Add(new double[2] { i * period, 0 }); data.Add(new double[2] { i * period, 1 });
                data.Add(new double[2] { i * period + 0.1 * period, 1 }); data.Add(new double[2] { i * period + 0.1 * period, 0 });

                i = i + 1 + total_periods / 1000;   // downSample chart for performance
            }

            return data;
        }

        private List<double[]> build_frequency(double period, double duration)
        {
            // returns an array of time and height |`|_|`|_|`|_|`|_|`|_|`|_|`|_|`|_|`|_|`|_
            List<double[]> data = new List<double[]>();

            double half_period = 0.5 * period;
            int total_half_periods = Convert.ToInt32(duration / half_period);

            for (int i = 0; i <= total_half_periods; i++)
            {
                if (i % 2 == 0) { data.Add(new double[2] { i * half_period, 0 }); data.Add(new double[2] { i * half_period, 1 }); }
                else { data.Add(new double[2] { i * half_period, 1 }); data.Add(new double[2] { i * half_period, 0 }); }
            }

            return data;
        }

        #endregion
               
        #region Timing manipulation

        private void control_timings_display()
        {
            // if switching is off disable timing controls
            bool sw1 = isSwitching(swOpAmp1);
            sw1time_lbl.Enabled = sw1width_txtBox.Enabled = sw1delay1_txtBox.Enabled = sw1;

            bool sw2 = isSwitching(swOpAmp2);
            sw2time_lbl.Enabled = sw2width_txtBox.Enabled = sw2delay1_txtBox.Enabled = sw2;        
        }

        private void pulses_number_changed()
        {
            // display the appropriate number of pulses textboxes
            int tot_pulses = (int)pulses_cmbBox.SelectedItem;

            foreach (TextBox txtBox in GetControls(timing_grpBox).OfType<TextBox>().Where(t => t.Name.StartsWith("sw3delay")))
                txtBox.Visible = (Convert.ToInt32(txtBox.Name[8].ToString()) <= tot_pulses);
        }

        private void rdBtn_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdBtn = sender as RadioButton;

            // suppress the twin event of the unchecking rdBtn
            if (!rdBtn.Checked) return;

            // en/dis frequency text with mode
            freq_txtBox.Visible = freq_lbl.Visible = sync_rdBtn.Checked;
            //start_StopTrigger1.Enabled = async_rdBtn.Checked; //freq_txtBox.Enabled = 

            // check if frequency is too low. In external mode any value is acceptable. When switching to internal, minimum is 4Hz.
            //if (async_rdBtn.Checked)
            //    if (Convert.ToDouble(freq_txtBox.Text) < freq_min) freq_txtBox.Text = freq_min.ToString();

            // set mode 
            if (isConnected()) { transmit_triggerMode(); }
        }

        private bool transmit_triggerMode()
        {
            // decide and set trigger mode
            RadioButton rdBtn = GetControls(timing_grpBox).OfType<RadioButton>().FirstOrDefault(r => r.Checked);
            Hamburg_TRIGGER_MODE mode = Hamburg_TRIGGER_MODE.External;
            if (rdBtn.Text == "Internal") mode = Hamburg_TRIGGER_MODE.Internal;

            try { InstrumentCtrlInterface.HamburgBox_1.setTriggerMode(mode, (uint)20000); return true; }
            catch { MessageBox.Show("Error in setting mode of operation!", "Error!"); return false; }
        }

        private bool transmit_timings()
        {
            uint[,] times = new uint[3, 18];
            int tot_pulses = (int)pulses_cmbBox.SelectedItem;

            IEnumerable<TextBox> pulses = GetControls(timing_grpBox).OfType<TextBox>().Where(t => t.Name.StartsWith("sw3delay"));
            uint funnelDelay = 1, funnelWidth = 1, gateDelay = 1, gateWidth = 1, pulserDelay = 1, pulserWidth = 1;

            funnelDelay = Convert.ToUInt32(dParser(sw1delay1_txtBox) * 1000);
            funnelWidth = Convert.ToUInt32(dParser(sw1width_txtBox) * 1000);
            gateDelay = Convert.ToUInt32(dParser(sw2delay1_txtBox) * 1000);
            gateWidth = Convert.ToUInt32(dParser(sw2width_txtBox) * 1000);

            for (int p = 0; p < 6; p++)
            {
                string temp = "sw3delay" + (p + 1).ToString() + "_txtBox";
                temp += ""; ;
                TextBox curr_pulse_delay =  pulses.FirstOrDefault(n => n.Name == "sw3delay" + (p + 1).ToString() + "_txtBox");

                // get the times
                if (curr_pulse_delay.Visible) pulserDelay = Convert.ToUInt32(dParser(curr_pulse_delay) * 1000);
                else pulserDelay = repetition_patern(tot_pulses, p + 1); 
                pulserWidth = Convert.ToUInt32(dParser(sw3width_txtBox) * 1000);

                // define one run total time
                uint single_run_duration = (new uint[3] { funnelDelay + funnelWidth, gateDelay + gateWidth, pulserDelay + pulserWidth }).Max();

                // [0]delay, [1]width, [2]dead time
                // All switches must stop at the same time. Which is single_run_duration + 1 μs.
                times[0, 3 * p] = funnelDelay; times[0, 3 * p + 1] = funnelWidth; times[0, 3 * p + 2] = single_run_duration - funnelDelay - funnelWidth + 1;                                                                            // spare
                times[1, 3 * p] = pulserDelay; times[1, 3 * p + 1] = pulserWidth; times[1, 3 * p + 2] = single_run_duration - pulserDelay - pulserWidth + 1;       // pulser timmings
                times[2, 3 * p] = gateDelay; times[2, 3 * p + 1] = gateWidth; times[2, 3 * p + 2] = single_run_duration - gateDelay - gateWidth + 1;                 // gate timmings
            }

            // transmit values
            try { InstrumentCtrlInterface.HamburgBox_1.setALLDmcuParameters(ref times); Thread.Sleep(500); check_timings_transmit(times); return true; }
            catch { MessageBox.Show("Error in downloading time parameters!", "Error!"); return false; }
        }

        private uint repetition_patern(int pulses, int curr_pulse)
        {
            // will construct the repetition patern
            // 1 pulse -> copy to all, 
            // 2 pulse -> copy (1,3,5) (2,4,6) 
            // 3 pulse -> (1,4) (2,5) (3,6)
            // 6 pulses -> no copy (method will not be called anyway...)
            uint pulserDelay = 1;

            if (pulses == 1) pulserDelay = Convert.ToUInt32(dParser(sw3delay1_txtBox) * 1000);
            else if (pulses == 2 && curr_pulse % 2 == 1) pulserDelay = Convert.ToUInt32(dParser(sw3delay1_txtBox) * 1000);
            else if (pulses == 2 && curr_pulse % 2 == 0) pulserDelay = Convert.ToUInt32(dParser(sw3delay2_txtBox) * 1000);
            else if (pulses == 3 && curr_pulse == 4) pulserDelay = Convert.ToUInt32(dParser(sw3delay1_txtBox) * 1000);
            else if (pulses == 3 && curr_pulse == 5) pulserDelay = Convert.ToUInt32(dParser(sw3delay2_txtBox) * 1000);
            else if (pulses == 3 && curr_pulse == 6) pulserDelay = Convert.ToUInt32(dParser(sw3delay3_txtBox) * 1000);

            return pulserDelay;
        }

        private void check_timings_transmit(uint[,] times)
        {
            uint[,] readBack = new uint[3, 18];
            InstrumentCtrlInterface.HamburgBox_1.getALLDmcuParameters(ref readBack);
            //readBack.sequenceEquals
            bool ok = true;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 18; j++)
                    if (readBack[i, j] != times[i, j])
                        ok = false;

            if (!ok) MessageBox.Show("Timing values not transsmited correctly!", "Error!");
        }

        private bool txtBox_valueCheck(TextBox txtBox)
        {
            // check validity of text
            double value;
            bool error = false;
            string error_type = "";

            // numeric value check
            if (txtBox.Text == "") return !error; // allow empty text. It will be corrected on textBox.leave.
            if (!double.TryParse(txtBox.Text, out value)) { error_type = "Input not a number!"; error = true; }

            if (error)
            {
                MessageBox.Show(error_type, "Error!");
                txtBox.Undo();
                this.ActiveControl = txtBox;
            }
            return !error;
        }

        private bool txtBox_rangeCheck(TextBox txtBox)
        {
            // check validity of text
            double value;
            bool error = false;
            string error_type = "";
            double.TryParse(txtBox.Text, out value);

            // range check for timings
            if (!error && txtBox != freq_txtBox)
                if (value < 0.001 || value > 1e6) { error_type = "Wrong value! Valid time values are:\r\n0.001ms < time < 1e6ms"; error = true; }
            // range check for frequency
            if (!error && txtBox == freq_txtBox && async_rdBtn.Checked)
                if (value < freq_min || value > freq_max) { error_type = "Wrong value! Valid frequency values are:\r\n" + freq_min.ToString() + " < frequency < " + freq_max.ToString(); error = true; }

            if (error)
            {
                MessageBox.Show(error_type, "Error!");
                txtBox.Undo();
                this.ActiveControl = txtBox;
            }
            return !error;
        }

        #endregion

        #region Helpers
        private bool isConnected()
        {
            if (GetControls(connection1).OfType<Button>().FirstOrDefault(b => b.Text == "Disconnect").Enabled) return true;
            else return false;
        }

        private bool isSwitching(SwOpAmp swOpAmp)
        {
            return GetVoltage(swOpAmp, 0) != GetVoltage(swOpAmp, 1);
        }

        private double GetVoltage(UserControl usrCtrl, int tabIdx = 0)
        {
            double voltage = 0.0;

            string text = GetControls(usrCtrl).FirstOrDefault(t => (t is NumericTextBoxWithFixedSign || t is NumericTextBoxWithSign) && t.TabIndex == tabIdx).Text;
            double.TryParse(text, out voltage);

            return voltage;
        }

        public IEnumerable<Control> GetControls(Control c)
        {
            return new[] { c }.Concat(c.Controls.OfType<Control>().SelectMany(x => GetControls(x)));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                saveToolStripButton.PerformClick();
                return true;
            }
            if (keyData == (Keys.Control | Keys.O))
            {
                openToolStripButton.PerformClick();
                return true;
            }
            if (keyData == (Keys.Control | Keys.N))
            {
                newToolStripButton.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void setFileNameLabel(string fileName)
        {
            if (fileName != " *") fileNameLabel.Text = "Voltage profile: " + fileName;
            else if (!fileNameLabel.Text.EndsWith("*") && !fileNameLabel.Text.EndsWith(" ")) fileNameLabel.Text += " *";
        }

        private double dParser(TextBox t)
        {
            double d;
            if (double.TryParse(t.Text, out d)) return d;

            return 0.0;
        }

        private void validate_frequency_timings_color()
        {
            foreach (TextBox txtBox in GetControls(timing_grpBox).OfType<TextBox>())
            {
                if (txtBox == freq_txtBox) continue;
                txtBox.BackColor = Color.PowderBlue;
            }
        }

        public class BlockMouse : IMessageFilter
        {
            public void EnableMouse()
            {
                Application.UseWaitCursor = false;
                Application.RemoveMessageFilter(this);
            }

            public void DisableMouse()
            {
                Application.UseWaitCursor = true;
                Application.AddMessageFilter(this);
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == 0x201 || m.Msg == 0x202 || m.Msg == 0x203) return true;
                if (m.Msg == 0x204 || m.Msg == 0x205 || m.Msg == 0x206) return true;
                return false;
            }
        }


        //public class CustomSettings : ApplicationSettingsBase
        //{
        //    [UserScopedSetting()]
        //    [DefaultSettingValue("10")]
        //    public double GateWidth
        //    {
        //        get
        //        {
        //            return ((double)this["GateWidth"]);
        //        }
        //        set
        //        {
        //            this["GateWidth"] = (double)value;
        //        }
        //    }

        //    [UserScopedSetting()]
        //    [DefaultSettingValue("10")]
        //    public double PulserWidth
        //    {
        //        get
        //        {
        //            return ((double)this["PulserWidth"]);
        //        }
        //        set
        //        {
        //            this["PulserWidth"] = (double)value;
        //        }
        //    }

        //}
        static void ListEmbeddedResourceNames()
        {
            Trace.WriteLine("Listing Embedded Resource Names");

            foreach (var resource in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                Trace.WriteLine("Resource: " + resource);
        }

        public class HookResolver
        {
            Dictionary<string, Assembly> _loaded;

            public HookResolver()
            {
                _loaded = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            }

            System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
            {
                string name = args.Name.Split(',')[0];
                Assembly asm;
                lock (_loaded)
                {
                    if (!_loaded.TryGetValue(name, out asm))
                    {
                        using (Stream io = this.GetType().Assembly.GetManifestResourceStream(name))
                        {
                            byte[] bytes = new BinaryReader(io).ReadBytes((int)io.Length);
                            asm = Assembly.Load(bytes);
                            _loaded.Add(name, asm);
                        }
                    }
                }
                return asm;
            }
        }

        #endregion




    }
}
