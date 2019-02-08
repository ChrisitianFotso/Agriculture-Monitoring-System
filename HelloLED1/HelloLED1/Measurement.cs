using System;
using Microsoft.SPOT;

namespace HelloLED1
{

    public class Measurement
    {
        /* This is the class that represents all the data retrieved by the sensor.
         * Both temperature and humidity are reprsented by a 16 bits unsigned variable that thanks to an appropriate function will be later on converted
         * in the proper measure.
         * This class has a variable "counter" that will be used to understand how old the measurement is. This value is stored in each object within 
         * the variable "orderNumber" (now it's on 16 bits, if the memory on chip cannot store more than 255 measures it is possible to reduce it to a 
         * variable of 8 bits). Saving the order should be useful whenever the connection to internet is lost.
         * */

        public Double value { get; set; }                           //sensor value
        public DateTime iso_timestamp { get; set; }
        public String device_id { get; set; }
        public int sensor { get; set; }
        public String _id { get; set; }
        public int timestamp { get; set; }
        public String status { get; set; }
        public Measurement DeepCopy()
        {
            Measurement other = (Measurement)this.MemberwiseClone();
            return other;
        }

        public static bool isSimilar(Measurement a, Measurement b,int sensorid)
        {
            if (a.sensor == b.sensor)
            {
                if (sensorid == 1)//temprature
                {
                    if (System.Math.Abs(a.value - b.value) < 0.5)
                        return true;
                    else
                        return false;
                }
                else if (sensorid == 2)//humidity
                {
                    if (System.Math.Abs(a.value - b.value) < 1)
                        return true;
                    else
                        return false;
                }
                else if (sensorid == 3)//UV index
                {
                    if (a.value == b.value)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

    }
}
