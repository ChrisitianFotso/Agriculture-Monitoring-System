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
    class read_back
    {
        private int lastSent = 0;
        private int lastTosend = 0;
        public void delete_wrong_file(SDCard sdCard)
        {
            //string json;
            // Measurement mymeasure = new Measurement();
            //string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;
            string rootDirectory = sdCard.StorageDevice.RootDirectory;

            FileStream FileHandle = new FileStream(rootDirectory + @"\SetUp.txt", FileMode.Open, FileAccess.Read);
            byte[] data = new byte[100];
            int read_count = FileHandle.Read(data, 0, data.Length);
            string str = new string(Encoding.UTF8.GetChars(data), 0, read_count);
            Debug.Print(str);
            string[] parts = str.Split(' ');
            lastSent = Int32.Parse(parts[0]);

            Debug.Print(lastSent.ToString() + " this is the last sent");
            lastTosend = Int32.Parse(parts[1]);
            Debug.Print(lastTosend.ToString() + " this is the last to send");
            FileHandle.Close();
            FileStream Fs = new FileStream(rootDirectory + @"\SetUp.txt", FileMode.Open, FileAccess.Read);
            int j;
            if (lastSent == 0)
                j = lastSent;
            else
                j = lastSent + 1;
            for (int i = j; i <= lastTosend; i++)
            {
                try
                {
                    File.Delete(rootDirectory + @"\wrong_toSend" + i + ".JSON");
                }
                catch (System.IO.IOException)
                {
                    Debug.Print("File " + i + "not found.\n");
                }
            }
            string[] list = sdCard.StorageDevice.ListRootDirectoryFiles();
            int ll = list.Length;
            Debug.Print("files present in the sd card after deleting the wrong files");
            for (int k = 0; k < ll; k++)
            {
                Debug.Print(list[k]);
            }
            Fs.Close();
        }
        public void correct_time_stamp(SDCard sdCard)
        {

            Measurement mymeasure = new Measurement();
            string rootDirectory = sdCard.StorageDevice.RootDirectory;
            FileStream FileHandle = new FileStream(rootDirectory + @"\SetUp.txt", FileMode.Open, FileAccess.Read);
            byte[] data = new byte[100];
            int read_count = FileHandle.Read(data, 0, data.Length);
            string str = new string(Encoding.UTF8.GetChars(data), 0, read_count);
            Debug.Print(str);
            string[] parts = str.Split(' ');
            lastSent = Int32.Parse(parts[0]);
            lastTosend = Int32.Parse(parts[1]);
            FileHandle.Close();
            //FileStream 
            //public static TimeSpan operator -(DateTime d1, DateTime d2);

            int j;
            if (lastSent == 0)
                j = lastSent;
            else
                j = lastSent + 1;
            for (int i = j; i <= lastTosend; i++)
            {
                FileStream fs = new FileStream(rootDirectory + @"\wrong_toSend" + i + ".JSON", FileMode.Open, FileAccess.Read);
                try
                {
                    byte[] data1 = new byte[100];
                    int read_count1 = fs.Read(data1, 0, data1.Length);
                    string str1 = new string(Encoding.UTF8.GetChars(data), 0, read_count1);
                    JsonSerializer myserializer = new Json.NETMF.JsonSerializer(DateTimeFormat.Default);
                    mymeasure = (Measurement)myserializer.Deserialize(str1);
                    Debug.Print(i + " " + mymeasure.iso_timestamp.ToString());
                    DateTime d2 = mymeasure.iso_timestamp;
                    DateTime d1 = DateTime.Now; //starting local time
                    TimeSpan T = d2 - d1;
                    DateTime d3 = DateTime.Now - T;
                    mymeasure.iso_timestamp = d3;
                    JSON.printString(sdCard, mymeasure, true);
                    // d1 =


                    //File.Delete(rootDirectory + @"\wrong_tosend" + i + ".JSON");

                    fs.Close();
                }
                catch (System.IO.IOException)
                {
                    Debug.Print("File " + i + "not found.\n");
                }
            }
        }



    }
}
