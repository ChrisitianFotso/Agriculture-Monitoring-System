using System;
using System.Text;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Input;
using System.IO;
using System.Net;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using GTI = Gadgeteer.SocketInterfaces;
using Microsoft.SPOT.Time;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Json.NETMF;

namespace HelloLED1
{
    class uvsensor
    {
        int UVindex;
        double voltage;
        AnalogInput analogsensor;
        public void Setup()
        {
            analogsensor = new AnalogInput(Cpu.AnalogChannel.ANALOG_2);
            
        }
        public void Measure(Measurement mymeasure)
        {
            voltage=analogsensor.Read();
            if (voltage <= 0.05)
                UVindex = 0;
            else
                if (voltage <= 0.227)
                    UVindex = 1;
                else
                    if (voltage <= 0.318)
                        UVindex = 2;
                    else
                        if (voltage <= 0.408)
                            UVindex = 3;
                        else
                            if (voltage <= 0.503)
                                UVindex = 4;
                            else
                                if (voltage <= 0.606)
                                    UVindex = 5;
                                else
                                    if (voltage <= 0.696)
                                        UVindex = 6;
                                    else
                                        if (voltage <= 0.795)
                                            UVindex = 7;
                                        else
                                            if (voltage <= 0.881)
                                                UVindex = 8;
                                            else
                                                if (voltage <= 0.976)
                                                    UVindex = 9;
                                                else
                                                    if (voltage <= 1.079)
                                                        UVindex = 10;
                                                    else
                                                        UVindex = 11;
            mymeasure.value = UVindex;
            mymeasure.iso_timestamp = DateTime.UtcNow;
            mymeasure.timestamp = (Int32)((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).Ticks / TimeSpan.TicksPerSecond);
            mymeasure._id = Guid.NewGuid().ToString();
            if (voltage >= 0 && voltage <= 1.2)
                mymeasure.status = "OK";
            else
                mymeasure.status = "OUTOFRANGE";
        }
    }
}
