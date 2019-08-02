using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;


namespace Hamburg_namespace
{

    /*Format Enumeration
     */
    public enum FormatType : byte
    {
        Double = 0x00,
        Scientific = 0x01,
        Double_ThreePoints = 0x02,
    }

    /*Sign Enumeration
     */
    public enum SignType : byte {
        Possitive = 0x00,
        Negative = 0x01,    
    }

    public class NumericTextBoxWithFixedSign : TextBox{
        private double DValue;
        private SignType CurrentSign;

        #region Constructor
        /* Constructor
         * 
         */
        public NumericTextBoxWithFixedSign()
        {
            DoubleValue = 0.0;
            Format = FormatType.Double;
            Sign = SignType.Possitive;
            AbsMinimumDValue = 0.0;
            AbsMaximumDValue = 0.0;
            this.Text = DValue.ToString("+0.0;-0.0;+0.0", CultureInfo.CreateSpecificCulture("en-US"));
            this.TextAlign = HorizontalAlignment.Center;
        }
        #endregion

        #region Extra properties
        public double DoubleValue{
            set{
                DValue = value;
                if (Format == FormatType.Double)
                {
                    this.Text = DValue.ToString("+0.0;-0.0;+0.0", CultureInfo.CreateSpecificCulture("en-US"));
                }
                else if (Format == FormatType.Scientific)
                {
                    this.Text = DValue.ToString("+0.#e0;-0.#e0;+0.0", CultureInfo.CreateSpecificCulture("en-US"));
                };
            }
            get{
                return (DValue);
            }
        }

        public FormatType Format{ get; set;}

        public SignType Sign {
            set{
                CurrentSign = value;
                if (this.Text.Length >= 1){
                    string temp = this.Text.Substring(1);
                    if (CurrentSign == SignType.Possitive){
                        this.Text = ("+" + temp);
                    }
                    else{
                        this.Text = ("-" + temp);
                    }
                }
            }
            get{
                return (CurrentSign);
            }
        }

        public double AbsMinimumDValue { get; set; }
        public double AbsMaximumDValue { get; set; }
        #endregion

        #region Events
        /* TextBox Preview Key
        * Use this event to triger KeyDown Event for Up or Left Key
        */
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Left:
                    e.IsInputKey = true;
                    break;
            }
        }

        /* Key Down Event. Use this event to evaluate left and up keypressed
            * 
            */
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up)
            {
                if (this.SelectionStart == 1)
                {
                    e.Handled = true;
                }
            }
        }

        /* if this is an invalid character set e.handled to true 
            * 
            */
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);            
            // if this is a valid character proceed
            if (IsOKForDecimalTextBox(e.KeyChar, this) == true)
            {
                if (e.KeyChar != (char)Keys.Enter)
                {
                    this.BackColor = Color.MistyRose;
                }
            }
            // else set set e.handled to true so that the framework will not update the textbox with this value
            else
            {
                e.Handled = true;
            }
        }

        /* Set Selection to the end of the text when enter to texbox using tab
            * 
            */
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            this.SelectionStart = this.Text.Length;
        }

        /*Allow movement of caret to all possitions except possition0
            *In case caret on possition 0 set caret to possition 1
            *
            */
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (this.SelectionStart == 0)
            {
                this.SelectionStart = 1;
            }
        }

        /* If we select text using nouse dont take in to acount sign
            * 
            */
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                if (this.SelectedText.Length > 0)
                {
                    if (this.SelectionStart == 0)
                    {
                        this.SelectionStart = 1;
                    }
                }
            }
        }

        /* If we select text using Key like Shift andHome dont take in to acount sign
            * 
            */
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (this.SelectedText.Length > 0)
            {
                if (this.SelectionStart == 0)
                {
                    this.SelectionStart = 1;
                }
            }
        }

        /* When text changes update double value
            * 
            */
        protected override void OnTextChanged(EventArgs e){
            double temp;
            int possition;

            if (double.TryParse(this.Text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CreateSpecificCulture("en-US"), out temp) == true)
            {
                if (IsInAcceptableRange(temp) == true)
                {
                    DValue = temp;
                }
                else
                {
                    possition = this.SelectionStart;
                    this.Text = DValue.ToString("+0;-0;+0", CultureInfo.CreateSpecificCulture("en-US"));
                    if (possition > 0)
                    {
                        this.SelectionStart = possition - 1;
                    }
                    MessageBox.Show("Value Out of Range!! \n" + AbsMinimumDValue + " < Absolute Value < " + AbsMaximumDValue);
                }
            }
            base.OnTextChanged(e);
        }
        #endregion

        #region Test char
        /* Evaluate charachter and text
            * return true if character is ok
            */
        private bool IsOKForDecimalTextBox(char theCharacter, TextBox theTextBox){
            //We should never have the caret at the first possition but if we do for unexpected reazon
            //set the sign according to sign value
            if (theTextBox.SelectionStart == 0)
            {
                if (Sign == SignType.Possitive)
                {
                    theTextBox.Text = "+";
                }
                else
                {
                    theTextBox.Text = "-";
                }
                return (false);
            }
            else
            {
                //Only allow  control characters, digits and '.'
                if (!char.IsControl(theCharacter) && !char.IsDigit(theCharacter) && (theCharacter != '.'))
                {
                    // Then it is NOT a character we want allowed in the text box.
                    return false;
                }

                // Only allow one decimal point.
                if (theCharacter == '.' && theTextBox.Text.IndexOf('.') > -1)
                {
                    // Then there is already a decimal point in the text box.
                    return false;
                }

                // Omly allow BackSpace Until Possition 1 of the textbox
                if ((theCharacter == (char)Keys.Back) && theTextBox.SelectedText.Length == 0 && theTextBox.SelectionStart == 1)
                {
                    return false;
                }

                // Only Allow two digits after Decimal Point
                if (theCharacter != (char)Keys.Back && theCharacter != (char)Keys.Enter)
                {
                    string word = theTextBox.Text.Trim();
                    string[] wordArr = word.Split('.');
                    if (wordArr.Length > 1)
                    {
                        string afterDot = wordArr[1];
                        if (afterDot.Length >= 2 && (theTextBox.SelectionStart >= word.Length - 2))
                        {
                            return false;
                        }
                    }
                }

                // Otherwise the character is perfectly fine for a decimal value and the character
                // may indeed be placed at the current insertion position.
                return true;
            }
        }


        /* Evaluate DValue
         * return true if MinimumDValue < DValue < MaximumDValue
         */
        private bool IsInAcceptableRange(double Value)
        {
            if ((AbsMinimumDValue <= Math.Abs(Value)) && (Math.Abs(Value) <= AbsMaximumDValue))
            {
                return (true);
            }
            return (false);
        }
        #endregion
    }
}



