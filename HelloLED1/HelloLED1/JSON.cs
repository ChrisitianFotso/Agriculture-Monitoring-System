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
    public static class JSON
    { 
        private static int numbOnline = 0;
        private static int numbOffline = 0;

        public static void printConfiguration(SDCard sdCard, object o)
        {
            string rootDirectory = sdCard.StorageDevice.RootDirectory;
            string json;
            FileStream fs;

            fs = new FileStream(rootDirectory + @"\Configuration.JSON", FileMode.Create, FileAccess.ReadWrite);
            JsonSerializer mySerializer = new Json.NETMF.JsonSerializer(DateTimeFormat.Default);
            json = mySerializer.Serialize(o);

            byte[] data = Encoding.UTF8.GetBytes(json);
            fs.Write(data, 0, data.Length);
            fs.Close();
            //write immediately on the sdCard
            VolumeInfo[] vi = VolumeInfo.GetVolumes();
            for (int i = 0; i < vi.Length; i++)
            {
                vi[i].FlushAll();
            }
        }

        public static void printString(SDCard sdCard, object o, bool connection)
        {          
            string rootDirectory = sdCard.StorageDevice.RootDirectory;
            string json;
            FileStream fs;
            //Debug.Print("I am here");
            //timestamp applied is corretc no need to change it
            if (connection)
            {
                fs = new FileStream(rootDirectory + @"\correct_toSend" + numbOnline + ".JSON", FileMode.Create, FileAccess.ReadWrite);
                increseOnline();
                //SetUp.setLastToSend(numbOnline);
            }
            //timestamp applied is not correct change it, before sending
            else
            {
                fs = new FileStream(rootDirectory + @"\wrong_toSend" + numbOffline + ".JSON", FileMode.Create, FileAccess.ReadWrite);
                increaseOffline();
                //SetUp.setWrong(numbOffline);
            }
            JsonSerializer mySerializer = new Json.NETMF.JsonSerializer(DateTimeFormat.Default);
            json = mySerializer.Serialize(o);

            byte[] data = Encoding.UTF8.GetBytes(json);
            fs.Write(data, 0, data.Length);
            fs.Close();

            //write immediately on the sdCard
            VolumeInfo[] vi = VolumeInfo.GetVolumes();
            for (int i = 0; i < vi.Length; i++)
            {
                vi[i].FlushAll();
            }

            //Debug.Print("pringt data to SD!"+properties.Humidity+properties.Temperature+properties.iso_timestamp);
        }

        public static void resetOnline()
        {
            numbOnline = 0;
        }
        public static void resetOffline()
        {
            numbOffline = 0;
        }
        public static void increseOnline()
        {
            numbOnline++;
        }
        public static void increaseOffline()
        {
            numbOffline++;
        }
        public static int getOffline()
        {
            return numbOffline;
        }
        public static int getOnline()
        {
            return numbOnline;
        }
    }
}

