using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GT = Gadgeteer;
using System.Threading;

namespace HelloLED1
{
    public class temphumid7021
    {
        private I2CDevice myi2c;
        //GT.StorageDevice sdStorageDevice = null;

        public void Setup()
        {
            I2CDevice.Configuration mycof = new I2CDevice.Configuration(0x40, 200);
            myi2c = new I2CDevice(mycof);
        }

        public bool Measure(Measurement temp,Measurement humid)
        {
            I2CDevice.I2CTransaction[] action_write = new I2CDevice.I2CTransaction[1];
            I2CDevice.I2CTransaction[] action_read = new I2CDevice.I2CTransaction[1];
            byte[] Writebuffer_hum = new byte[1] { 0xF5 };
            byte[] Writebuffer_temp = new byte[1] { 0xE0 };
            action_write[0] = I2CDevice.CreateWriteTransaction(Writebuffer_hum);
            byte[] Readbuffer_hum = new byte[2];
            byte[] Readbuffer_temp = new byte[2];
            action_read[0] = I2CDevice.CreateReadTransaction(Readbuffer_hum);
            //double realvalue_hum = new double();
            //double realvalue_temp = new double();
            if (myi2c.Execute(action_write, 50) == 0)
                return false;
            else
            {
                Thread.Sleep(25);//wait for data to be ready
                if (myi2c.Execute(action_read, 50) == 0)
                    return false;
                //swap msb and lsb
                byte tem = Readbuffer_hum[0];
                Readbuffer_hum[0] = Readbuffer_hum[1];
                Readbuffer_hum[1] = tem;
                Readbuffer_hum[1] &= 0xFC;
                humid.value = BitConverter.ToUInt16(Readbuffer_hum, 0) * 125.0 / 65536 - 6;
                //Debug.Print("Humidity:" + Humidity.ToString("F") + "%");


                //read temp
                action_write[0] = I2CDevice.CreateWriteTransaction(Writebuffer_temp);
                action_read[0] = I2CDevice.CreateReadTransaction(Readbuffer_temp);
                if (myi2c.Execute(action_write, 50) == 0)
                    return false;
                else
                {
                    Thread.Sleep(25);
                    if (myi2c.Execute(action_read, 50) == 0)
                        return false;
                    //swap msb and lsb
                    tem = Readbuffer_temp[0];
                    Readbuffer_temp[0] = Readbuffer_temp[1];
                    Readbuffer_temp[1] = tem;
                    Readbuffer_temp[1] &= 0xFC;
                    temp.value = BitConverter.ToUInt16(Readbuffer_temp, 0) * 175.72 / 65536 - 46.85;
                    //Debug.Print("Temperature:" + Temperature.ToString("F") + "¡ãC");
                }
            }
            if (humid.value >= 0 && humid.value <= 100.0)
                humid.status = "OK";
            else
                humid.status = "OUTOFRANGE";
            if (temp.value >= -40.0 && temp.value <= 125.0)
                temp.status = "OK";
            else
                temp.status = "OUTOFRANGE";
            humid.iso_timestamp = DateTime.UtcNow;
            temp.iso_timestamp = DateTime.UtcNow;
            humid.timestamp = (Int32)((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).Ticks/ TimeSpan.TicksPerSecond);
            temp.timestamp = (Int32)((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).Ticks / TimeSpan.TicksPerSecond);
            humid._id = Guid.NewGuid().ToString();
            temp._id = Guid.NewGuid().ToString();
            return true;
        }


    }
}
