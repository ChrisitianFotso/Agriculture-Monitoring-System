#region Assembly GTM.GHIElectronics.TempHumidSI7021.dll, v4.3.8.1
// C:\Program Files (x86)\GHI Electronics\.NET Gadgeteer SDK\Modules\TempHumidSI70\NETMF 4.3\GTM.GHIElectronics.TempHumidSI7021.dll
#endregion

using Gadgeteer.Modules;
using System;

namespace Gadgeteer.Modules.GHIElectronics
{
    // Summary:
    //     A TempHumidity module for Microsoft .NET Gadgeteer
    public class TempHumidSI7021 : Module
    {
        // Summary:
        //     Constructs a new instance.
        //
        // Parameters:
        //   socketNumber:
        //     The socket that this module is plugged in to.
        public TempHumidSI7021(int socketNumber);

        // Summary:
        //     Obtains a single measurement.
        //
        // Returns:
        //     The measurement.
        public TempHumidSI7021.Measurement TakeMeasurement();

        // Summary:
        //     Result of a measurement.
        public class Measurement
        {
            // Summary:
            //     The measured relative humidity.
            public double RelativeHumidity { get; }
            //
            // Summary:
            //     The measured temperature in degrees Celsius.
            public double Temperature { get; }
            //
            // Summary:
            //     The measured temperature in degrees Fahrenheit.
            //public double TemperatureFahrenheit { get; }

            // Summary:
            //     Provides a string representation of the instance.
            //
            // Returns:
            //     A string describing the values contained in the object.
            public override string ToString();
        }
    }
}
