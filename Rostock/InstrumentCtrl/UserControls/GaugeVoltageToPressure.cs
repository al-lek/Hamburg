using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hamburg_namespace
{
    /* Gauge Gas Type
 */
    public enum GaugeGasType : byte
    {
        Nitrogen = 0x00,
        Helium = 0x01,
    }

    /* EdwardsGauge ID
     */
    public enum EdwardsGaugeId : byte
    {
        NoId = 0x00,
        APG_L = 0x01,
        APG100_XM = 0x02,
        APG100_XLC = 0x03,
        WRG_S = 0x04,
        APG_Mx = 0x05,
        STR_GAUGE = 0x06,
    }

    class GaugeVoltageToPressure
    {
        #region lookuptables
        #region Helium lookup tables
        private static double[] APGM_Helium = new double[45]{//2.2Volt, 2,4Volt,...,11Volt
            1.70E-03, 4.20E-03, 6.50E-03, 9.50E-03, 1.40E-02, 1.90E-02, 2.50E-02, 3.40E-02, 5.10E-02, 7.00E-02, 
            9.20E-02, 1.20E-01, 1.40E-01, 1.70E-01, 2.00E-01, 2.40E-01, 3.50E-01, 4.80E-01, 6.30E-01, 7.90E-01,
            9.80E-01, 1.20E+00, 1.40E+00, 1.70E+00, 2.00E+00, 2.30E+00, 2.60E+00, 2.90E+00, 3.30E+00, 3.70E+00,
            4.10E+00, 4.60E+00, 5.10E+00, 5.60E+00, 6.20E+00, 6.70E+00, 7.40E+00, 8.10E+00, 8.80E+00, 9.50E+00, 
            1.00E+01, 1.10E+01, 1.20E+01, 1.30E+01, 1.40E+01
        };

        private static double[] APGL_Helium = new double[46]{//2Volt, 2,2Volt,...,11Volt
            2.50E-04, 6.90E-04, 1.20E-03, 1.80E-03, 2.70E-03, 4.00E-03, 5.70E-03, 7.50E-03, 1.10E-02, 1.60E-02,
            2.30E-02, 3.00E-02, 3.70E-02, 4.60E-02, 5.50E-02, 6.50E-02, 8.00E-02, 1.10E-02, 1.50E-01, 2.00E-01,
            2.60E-01, 3.20E-01, 3.90E-01, 4.60E-01, 5.50E-01, 6.40E-01, 7.40E-01, 8.40E-01, 9.60E-01, 1.10E+00,
            1.20E+00, 1.40E+00, 1.50E+00, 1.70E+00, 1.90E+00, 2.10E+00, 2.30E+00, 2.50E+00, 2.70E+00, 3.00E+00,
            3.20E+00, 3.50E+00, 3.80E+00, 4.10E+00, 4.50E+00, 4.80E+00
        };

        private static double[] WRG_Helium = new double[61]{//4Volt, 4.1Volt,...,10Volt
            5.10E-06, 7.30E-06, 1.15E-05, 1.70E-05, 2.40E-05, 3.40E-05, 5.18E-05, 7.20E-05, 1.14E-04, 1.55E-04,
            2.20E-04, 3.00E-04, 4.12E-04, 5.40E-04, 7.00E-04, 8.90E-04, 1.15E-03, 1.32E-03, 1.62E-03, 1.82E-03,
            2.18E-03, 2.30E-03, 2.80E-03, 3.40E-03, 4.60E-03, 6.34E-03, 9.12E-03, 1.32E-02, 1.86E-02, 2.60E-02,
            3.60E-02, 5.00E-02, 7.00E-02, 9.00E-02, 1.32E-01, 1.80E-01, 2.40E-01, 3.25E-01, 4.40E-01, 5.90E-01,
            7.50E-01, 9.30E-01, 1.18E+00, 1.40E+00, 1.55E+00, 1.70E+00, 1.80E+00, 1.90E+00, 2.00E+00, 2.05E+00,
            2.10E+00, 2.15E+00, 2.20E+00, 2.25E+00, 2.30E+00, 2.35E+00, 2.40E+00, 2.45E+00, 2.50E+00, 2.55E+00,
            2.60E+00
        };
        #endregion

        #region Nitrogen Lookup tables
        //(2.00V->  1x10-5), (2.05V-> 2.31x10-4), (2.1V-> 6.21x10-4), (2.2Volt, 2.4Volt,...,10Volt -> lookup) 
        //(9.5V -> 2.88E+1), (9.6V ->   3.53E+1), (9.7V ->  4.48E+1), (9.8V -> 6.65E+1), (9.9V -> 1.41E+2), (10.0V -> 1.00E+3)
        private static double[] APGM_Nitrogen = new double[37]{
            1.36E-3, 2.97E-3, 4.61E-3, 6.51E-3, 1.02E-2, 1.47E-2, 1.91E-2, 2.95E-2, 4.16E-2, 5.61E-2,
            7.20E-2, 8.94E-2, 1.13E-1, 1.45E-1, 1.76E-1, 2.22E-1, 3.16E-1, 4.13E-1, 5.40E-1, 6.82E-1,
            8.41E-1,    1.06,    1.33,    1.60,    1.87,    2.26,    2.75,    3.24,    3.73,    4.39,
               5.29,    6.27,    7.63,    9.39, 1.27E+1, 1.67E+1, 2.24E+1
        };

        //(2.00V->    1E-6), (2.05V-> 8.26E-5), (2.1V-> 2.27E-4), (2.2Volt, 2.4Volt,...,9.5Volt -> lookup)
        //(9.5V -> 1.29E+1), (9.6V -> 2.07E+1), (9.7V -> 3.39E+1), (9.8V -> 6.32E+1), (9.9V -> 1.44E+2), (10.0V -> 1.00E+3)
        private static double[] APGL_Nitrogen = new double[37]{//2.2Volt, 2.4Volt,...,10Volt
            5.00E-4, 1.08E-3, 1.68E-3, 2.60E-3, 3.84E-3, 5.15E-3, 6.87E-3, 1.05E-2, 1.56E-2, 2.10E-2, 
            2.77E-2, 3.45E-2, 4.16E-2, 5.04E-2, 5.92E-2, 8.74E-2, 1.27E-1, 1.71E-1, 2.23E-1, 2.90E-1, 
            3.57E-1, 4.35E-1, 5.33E-1, 6.40E-1, 7.67E-1, 9.23E-1,    1.14,    1.40,    1.66,    1.92,
               2.38,    2.95,    3.51,    4.17,    5.40,    7.06,    9.69
        };
        #endregion
        #endregion

        #region Constructor
        /* Constructor
         * 
         */
        public GaugeVoltageToPressure()
        {
        }
        #endregion

        #region Methods
        /* Return Pressue Value
         * double GetPressure(GaugeId Gauge_Type, GaugeGasType GasType, double VoltageValue)
         */
        public double GetPressure(EdwardsGaugeId Gauge_Type, GaugeGasType GasType, double SignalVoltageValue) {
            int i;

            if (GasType == GaugeGasType.Nitrogen) {
                if (Gauge_Type == EdwardsGaugeId.APG100_XM) {
                    if (SignalVoltageValue < 3) {
                        return (1E-3);
                    }
                    else {
                        return (Math.Pow(10, (SignalVoltageValue - 6.0)));
                    }
                }
                else if (Gauge_Type == EdwardsGaugeId.APG100_XLC) {
                    if (SignalVoltageValue < 2) {
                        return (1E-4);
                    }
                    else {
                        return (Math.Pow(10, (SignalVoltageValue - 6.0)));
                    }
                }
                else if (Gauge_Type == EdwardsGaugeId.APG_L) {
                    if (SignalVoltageValue < 2.05)
                        return(1E-6);
                    else if (SignalVoltageValue >= 2.05 && SignalVoltageValue < 2.1)
                        return(8.26E-5);
                    else{
                        SignalVoltageValue = Math.Round(SignalVoltageValue, 1);
                        if (SignalVoltageValue == 2.1)
                            return (2.27E-4);
                        else if (SignalVoltageValue == 9.5)
                            return (1.29E+1);
                        else if (SignalVoltageValue == 9.6)
                            return (2.07E+1);
                        else if (SignalVoltageValue == 9.7)
                            return (3.39E+1);
                        else if (SignalVoltageValue == 9.8)
                            return (6.32E+1);
                        else if (SignalVoltageValue == 9.9)
                            return (1.44E+2);
                        else if (SignalVoltageValue > 10.0)
                            return (1.00E+3);
                        else {
                            for (i = 0; i < 37; i++) {
                                if (SignalVoltageValue < (2.2 + (0.2 * i))) {
                                    break;
                                }
                            }
                            return(APGL_Nitrogen[i]);
                        }
                    }
                }
                else if (Gauge_Type == EdwardsGaugeId.APG_Mx) {
                    if (SignalVoltageValue < 2.05)
                        return (1E-5);
                    else if (SignalVoltageValue >= 2.05 && SignalVoltageValue < 2.1)
                        return(2.31E-4);
                    else{
                        SignalVoltageValue = Math.Round(SignalVoltageValue, 1);
                        if (SignalVoltageValue == 2.1)
                            return (6.21E-4);
                        else if (SignalVoltageValue == 9.5)
                            return (2.88E+1);
                        else if (SignalVoltageValue == 9.6)
                            return (3.53E+1);
                        else if (SignalVoltageValue == 9.7)
                            return (4.48E+1);
                        else if (SignalVoltageValue == 9.8)
                            return (6.65E+1);
                        else if (SignalVoltageValue == 9.9)
                            return (1.41E+2);
                        else if (SignalVoltageValue >= 10)
                            return (1E+3);
                        else {
                            for (i = 0; i <37 ; i++) {
                                if (SignalVoltageValue < (2.2 + (0.2 * i))) {
                                    break;
                                }
                            }
                            return (APGM_Nitrogen[i]);
                        }
                    }
                }
                else if (Gauge_Type == EdwardsGaugeId.WRG_S) {
                    return (Math.Pow(10, (1.5 * SignalVoltageValue - 12.0)));
                }
                else {
                    return (0);
                }
            }
            else if (GasType == GaugeGasType.Helium) {
                if ((Gauge_Type == EdwardsGaugeId.APG100_XM) || (Gauge_Type == EdwardsGaugeId.APG_Mx)) {
                    if (SignalVoltageValue <= 2.2)
                        return (APGM_Helium[0]);
                    else if (SignalVoltageValue >= 11.0)
                        return (APGM_Helium[44]);
                    else {
                        SignalVoltageValue = Math.Round(SignalVoltageValue, 1);
                        for (i = 0; i < 45; i++) {
                            if (SignalVoltageValue < (2.2 + (0.2 * i))) {
                                break;
                            }
                        }
                        return (APGM_Helium[i]);
                    }
                }
                else if (Gauge_Type == EdwardsGaugeId.APG_L) {
                    if (SignalVoltageValue <= 2.0)
                        return (APGL_Helium[0]);
                    else if (SignalVoltageValue >= 11.0)
                        return (APGL_Helium[45]);
                    else {
                        SignalVoltageValue = Math.Round(SignalVoltageValue, 1);
                        for (i = 0; i < 46; i++) {
                            if (SignalVoltageValue < (2.0 + (0.2 * i))) {
                                break;
                            }
                        }
                        return (APGL_Helium[i]);
                    }
                }
                else if (Gauge_Type == EdwardsGaugeId.APG100_XLC) {
                    return (0);
                }
                else if (Gauge_Type == EdwardsGaugeId.WRG_S) {
                    if (SignalVoltageValue <= 4.0)
                        return (5.10E-06);
                    else if (SignalVoltageValue >= 10.0)
                        return (2.60E+00);
                    else {
                        SignalVoltageValue = Math.Round(SignalVoltageValue, 1);
                        for (i = 0; i < 61; i++) {
                            if (SignalVoltageValue < (4.0 + (0.1 * i))) {
                                break;
                            }
                        }
                        return (WRG_Helium[i]);
                    }
                }
                else {
                    return (0);
                }
            }
            else {
                return (0);
            }
        }

        /* Return Gauge Id
         * GaugeId GetId(double SignalVoltageValue, double IdVoltageValue)
         */
        public EdwardsGaugeId GetId(double SignalVoltageValue, double IdVoltageValue) {
            double Rx;

            if(Math.Abs(SignalVoltageValue - IdVoltageValue)> 0.1){
                Rx = (100000 * IdVoltageValue) / (SignalVoltageValue - IdVoltageValue);

                if(Rx>25000 && Rx<29000 ){ //Rx=27K
                    return (EdwardsGaugeId.APG_Mx);
                }
                else if(Rx>31000 && Rx<34000 ){ //Rx=33K
                    return (EdwardsGaugeId.APG_L);
                }
                if(Rx>34000 && Rx<38000){      //Rx=36K
                    return (EdwardsGaugeId.APG100_XM);
                }
                else if (Rx > 41000 && Rx < 45000) {//Rx=43K
                    return (EdwardsGaugeId.APG100_XLC);
                }
                else if(Rx>70000 && Rx<80000 ){ //Rx=75K
                    return (EdwardsGaugeId.WRG_S);
                }
                else{
                    return (EdwardsGaugeId.NoId);
                }
            }
            else{
                return (EdwardsGaugeId.NoId);
            }
        }

        /* Return Pressue Value and Gauge Id
         * void GetPressure(double SignalVoltageValue, double IdVoltageValue, ref GaugeId Gauge_Type, ref double Pressure)
         */
        public void GetPressure(double SignalVoltageValue, double IdVoltageValue, GaugeGasType GasType, ref EdwardsGaugeId Gauge_Type, ref double Pressure) {

            Gauge_Type = GetId(SignalVoltageValue, IdVoltageValue);

            if (Gauge_Type != EdwardsGaugeId.NoId) { 
                Pressure = GetPressure(Gauge_Type, GasType, SignalVoltageValue);
            }
        }

        /* Return Voltage from Pressure
         * void GetPressure(double SignalVoltageValue, double IdVoltageValue, ref GaugeId Gauge_Type, ref double Pressure)
         */
        public double GetVoltage(GaugeGasType GasType, EdwardsGaugeId Gauge_Type, double Pressure) {
            int i;

            if (GasType == GaugeGasType.Nitrogen) {
                if ((Gauge_Type == EdwardsGaugeId.APG100_XM) || (Gauge_Type == EdwardsGaugeId.APG100_XLC)) {
                    return (Math.Log10(Pressure) + 6);
                }
                else if (Gauge_Type == EdwardsGaugeId.WRG_S) {
                    return ((Math.Log10(Pressure) + 12) / 1.5);
                }
                else if (Gauge_Type == EdwardsGaugeId.APG_Mx) {
                    if (Pressure <= 1E-5) {
                        return (2.0);
                    }
                    else if (Pressure >= 1E-5 && Pressure < 2.31E-4) {
                        return (2.05);
                    }
                    else if (Pressure >= 2.31E-4 && Pressure < 6.21E-4) {
                        return (2.1);
                    }
                    else if (Pressure >= 2.88E+1 && Pressure < 3.53E+1) {
                        return (9.5);
                    }
                    else if (Pressure >= 3.53E+1 && Pressure < 4.48E+1) {
                        return (9.6);
                    }
                    else if (Pressure >= 4.48E+1 && Pressure < 6.65E+1) {
                        return (9.7);
                    }
                    else if (Pressure >= 6.65E+1 && Pressure < 1.41E+2) {
                        return (9.8);
                    }
                    else if (Pressure >= 1.41E+2 && Pressure < 1.00E+3) {
                        return (9.9);
                    }
                    else if (Pressure >= 1.00E+3) {
                        return (10.0);
                    }
                    else {
                        for (i = 0; i < 37; i++) {
                            if (Pressure > APGM_Nitrogen[i]) {
                                break;
                            }
                        }
                        return (2.2 + (0.2 * i));
                    }
                }
                else if (Gauge_Type == EdwardsGaugeId.APG_L) {
                    if (Pressure <= 1E-6) {
                        return (2.0);
                    }
                    else if (Pressure >= 1E-6 && Pressure < 8.26E-5) {
                        return (2.05);
                    }
                    else if (Pressure >= 8.26E-5 && Pressure < 2.27E-4) {
                        return (2.1);
                    }
                    else if (Pressure >= 1.29E+1 && Pressure < 2.07E+1) {
                        return (9.5);
                    }
                    else if (Pressure >= 2.07E+1 && Pressure < 3.39E+1) {
                        return (9.6);
                    }
                    else if (Pressure >= 3.39E+1 && Pressure < 6.32E+1) {
                        return (9.7);
                    }
                    else if (Pressure >= 6.32E+1 && Pressure < 1.44E+2) {
                        return (9.8);
                    }
                    else if (Pressure >= 1.44E+2 && Pressure < 1.00E+3) {
                        return (9.9);
                    }
                    else if (Pressure >= 1.00E+3) {
                        return (10.0);
                    }
                    else {
                        for (i = 0; i < 37; i++) {
                            if (Pressure > APGL_Nitrogen[i]) {
                                break;
                            }
                        }
                        return (2.2 + (0.2 * i));
                    }
                }
                else {
                    return (0);
                }
            }
            // If Gas is Helium
            else if (GasType == GaugeGasType.Helium) {
                if ((Gauge_Type == EdwardsGaugeId.APG100_XM) || (Gauge_Type == EdwardsGaugeId.APG_Mx)) {
                    for (i = 0; i < 45; i++) {
                        if (Pressure > APGM_Helium[i]) {
                            break;
                        }
                    }
                    return (2.2 + (0.2 * i));
                }
                else if (Gauge_Type == EdwardsGaugeId.WRG_S) {
                    for (i = 0; i < 61; i++) {
                        if (Pressure > WRG_Helium[i]) {
                            break;
                        }
                    }
                    return (4.0 + (0.1 * i));
                }
                else if (Gauge_Type == EdwardsGaugeId.APG100_XLC) {
                    return (0);
                }
                else if (Gauge_Type == EdwardsGaugeId.APG_L) {
                    for (i = 0; i < 46; i++) {
                        if (Pressure > APGL_Helium[i]) {
                            break;
                        }
                    }
                    return (2.0 + (0.2 * i));
                }
                else {
                    return (0);
                }
            }
            else {
                return (0);
            }
        }
        #endregion
    }
}
