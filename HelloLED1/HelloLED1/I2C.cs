using System;
using Microsoft.SPOT;
using Gadgeteer.SocketInterfaces;

namespace HelloLED1
{
    class I2C : Gadgeteer.SocketInterfaces.I2CBus
    {
        public ushort address;
        public int clockRateKHz;
        public int timeout;
        public override ushort Address
        {
            get
            {
                return this.address;
            }
            set
            {
                this.address = Address;
            }
        }

        public override int Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                this.timeout = Timeout;
            }
        }
        public override int ClockRateKHz
        {
            get
            {
                return this.clockRateKHz;
            }
            set
            {
                this.clockRateKHz = ClockRateKHz;
            }
        }
        public override void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int numWritten, out int numRead)
        {
            numWritten = this.Write(writeBuffer);
            numRead = this.Read(readBuffer);
        }
    }
}