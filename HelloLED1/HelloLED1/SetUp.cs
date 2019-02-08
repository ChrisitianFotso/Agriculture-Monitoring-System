using System;
using System.Text;
using Json.NETMF;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Input;
using System.IO;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using GTI = Gadgeteer.SocketInterfaces;

namespace HelloLED1
{
    public class SetUp
    {
        public static int Wrong { get; set; }
        public static int lastToSend { get; set; }

        public static void writeFile(SDCard sdCard)
        {
            string rootDirectory = sdCard.StorageDevice.RootDirectory; ;
            FileStream fs;

            try
            {
                fs = new FileStream(rootDirectory + @"\SetUp.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite,FileShare.ReadWrite);
                byte[] data = Encoding.UTF8.GetBytes(Wrong.ToString() + " " + lastToSend.ToString());
                fs.Write(data, 0, data.Length);
                fs.Close();
            }
            catch(System.IO.IOException e){
                Debug.Print(e.ToString() + " " + e.Message);
            }
           
            //write immediately on the sdCard
            VolumeInfo[] vi = VolumeInfo.GetVolumes();
            for (int i = 0; i < vi.Length; i++)
            {
                vi[i].FlushAll();
            }
        }

        public static void setWrong(int number)
        {
            Wrong = number - 1;
        }
        public static void setLastToSend(int number)
        {
            lastToSend = number - 1;
        }
        public static int getLastSent()
        {
            return Wrong;
        }
        public static int getLastToSend()
        {
            return lastToSend;
        }
    }
}
