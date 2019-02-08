
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

    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset. 
        public string rootDirectory;
        temphumid7021 mytempsensor = new temphumid7021();
        uvsensor myuvsensor = new uvsensor();
        read_back myread = new read_back();
        Measurement lastMeasureTemperature = new Measurement();
        Measurement lastMeasureHumidity = new Measurement();
        Measurement lastMeasureUVindex = new Measurement();
        static int BufferSize = 90;
        Measurement[] MeasurementBuffer = new Measurement[BufferSize];
        int BufferCounter = 0;
        static int lastSent = 0;
        Configuration config = new Configuration();
        static bool isInternet = false;
        static bool timeSyncfinished = false;
        static bool waiting = true;
        static string host = "18.188.127.57";
        static int port = 1883;
        static string topic = "FEZ27";
        Font font = Resources.GetFont(Resources.FontResources.NinaB);
        private static MqttClient _mqttclient = new MqttClient(host, port, false, null, null, MqttSslProtocols.None);
        static string jsonString;
        void ProgramStarted()
        {
            if (sdCard.IsCardMounted)
                rootDirectory = sdCard.StorageDevice.RootDirectory;
            else
                Mainboard.SetDebugLED(true);
            Byte[] buffer = new Byte[10];
            TimeService.SystemTimeChanged += TimeService_SystemTimeChanged;
            TimeService.TimeSyncFailed += TimeService_TimeSyncFailed;
            ethernetJ11D.NetworkDown += ethernetJ11D_NetworkDown;
            ethernetJ11D.NetworkUp += ethernetJ11D_NetworkUp;
            button.ButtonPressed += button_ButtonPressed;
            sdCard.Unmounted += sdCard_Unmounted;
            sdCard.Mounted += sdCard_Mounted;
            _mqttclient.MqttMsgPublished += _mqttclient_MqttMsgPublished;
            Thread syncTimerThread, measureThread, systemTimerThread, publisheverythingThread;
            if (sdCard.IsCardMounted)
                JSON.printConfiguration(sdCard, config);
            //Debug.Print("Hello PC");
            Bitmap my_bitmap = new Bitmap((Int32)displayTE35.Width, (Int32)displayTE35.Height);
            string mystring = "waiting for measure";
            my_bitmap.DrawText(mystring, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 10);
            displayTE35.SimpleGraphics.DisplayImage(my_bitmap, 0, 0);


            ethernetJ11D.NetworkInterface.Open();
            ethernetJ11D.NetworkSettings.EnableDhcp();
            ethernetJ11D.NetworkSettings.EnableDynamicDns();
            //ethernetJ11D.UseDHCP();
            //initializeNetwork();

            if (!sdCard.IsCardMounted)
                sdCard.Mount();

            try
            {
                if (sdCard.IsCardMounted)
                {
                    myread.delete_wrong_file(sdCard);
                    FileStream fs = new FileStream(rootDirectory + @"\setup.txt", FileMode.Open);
                    int byteRead = fs.Read(buffer, 0, 0);
                    if (byteRead != 0)
                    {
                        SetUp.Wrong = 0;
                        SetUp.lastToSend = 0;
                        SetUp.writeFile(sdCard);
                    }
                    Debug.Print(buffer[0].ToString());
                    fs.Close();
                }
            }
            catch (System.IO.IOException e)
            {

            }

            mytempsensor.Setup();
            myuvsensor.Setup();

            //myread.readSetup(sdCard);

            //initializeNetworkTimerThread = new Thread(this.initializeNetworkThread);
            //initializeNetworkTimerThread.Start();
            //Creation of threads starting from there
            //Timer systemTimeDisplayTimer = new Timer(new TimerCallback(systemTimeDisplay), null, 0, 1000);
            systemTimerThread = new Thread(this.systemTimeDisplay);
            systemTimerThread.Start();
            syncTimerThread = new Thread(this.syncTimer);
            syncTimerThread.Start();
            //Thread.Sleep(1000);
            measureThread = new Thread(this.Measure);
            measureThread.Start();
            publisheverythingThread = new Thread(this.PublishEverything);
            publisheverythingThread.Start();
            //publishthread = new thread(this.publishmqtt);
            //publishthread.start();

        }
        void SaveToBuffer(Measurement a)
        {
            MeasurementBuffer[BufferCounter] = a.DeepCopy();
            BufferCounter++;
            if (BufferCounter >= BufferSize)
                BufferCounter = 0;
        }

        void _mqttclient_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            Debug.Print("MqttMsgPublished\n");
            setInternet(true);
        }

        void sdCard_Mounted(SDCard sender, GT.StorageDevice device)
        {
            Mainboard.SetDebugLED(false);
            rootDirectory = sdCard.StorageDevice.RootDirectory;
        }

        void sdCard_Unmounted(SDCard sender, EventArgs e)
        {
            Mainboard.SetDebugLED(true);
        }

        private void publishMqtt(Measurement mymeasure)
        {
            try
            {
                if (!_mqttclient.IsConnected)
                {
                    byte b = _mqttclient.Connect(Guid.NewGuid().ToString());
                    Debug.Print(b.ToString());
                }

                JsonSerializer mySerializer = new Json.NETMF.JsonSerializer(DateTimeFormat.ISO8601);
                jsonString = mySerializer.Serialize(mymeasure);
                _mqttclient.Publish(topic, Encoding.UTF8.GetBytes(jsonString), 0, false);
                Debug.Print("publishing\n");
                //_mqttclient.Disconnect();
                Debug.Print(jsonString);

            }
            catch (Exception ex)
            {
                //Debug.Print(ex.ToString());
            }
        }

        private void PublishEverything()
        {
            Thread.Sleep(30000);
            while (true)
            {
                if (!_mqttclient.IsConnected)
                {
                    try
                    {
                        byte b = _mqttclient.Connect(Guid.NewGuid().ToString());
                        Debug.Print(b.ToString());
                    }
                    catch (Exception ex)
                    {
                        //Debug.Print(ex.ToString());
                    }
                }
                Debug.Print("Publishing everything");
                Debug.Print("LastToSend:" + SetUp.getLastToSend().ToString());

                //send everything in buffer and clear buffer
                try
                {
                    for (int i = 0; i < BufferCounter; i++)
                    {
                        publishMqtt(MeasurementBuffer[i]);
                    }
                    BufferCounter = 0;
                }
                catch
                { }
                if (sdCard.IsCardMounted)
                {

                    //send everything correct in sd card and delete
                    for (int i = 0; i <= SetUp.getLastToSend(); i++)
                    {
                        try
                        {
                            FileStream FileHandle;
                            //string rootDirectory = sdCard.StorageDevice.RootDirectory;
                            //send and delete if file exists
                            if (File.Exists(rootDirectory + @"\correct_toSend" + i + ".JSON"))
                            {
                                FileHandle = new FileStream(rootDirectory + @"\correct_toSend" + i + ".JSON", FileMode.Open, FileAccess.ReadWrite);
                                byte[] data = new byte[200];
                                int read_count = FileHandle.Read(data, 0, data.Length);
                                _mqttclient.Publish(topic, data, 0, false);
                                Debug.Print(new string(Encoding.UTF8.GetChars(data), 0, read_count));
                                FileHandle.Close();
                                File.Delete(rootDirectory + @"\correct_toSend" + i + ".JSON");
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                Thread.Sleep(60000);
            }    
        }

        private void systemTimeDisplay()
        {
            displayTE35.BacklightEnabled = true;
            //displayTE35.DebugPrintEnabled = true;
            //bottom half of screen for system time display
            Bitmap my_bitmap = new Bitmap((Int32)displayTE35.Width, (Int32)displayTE35.Height / 2);
            Thread.Sleep(10000);
            while (true)
            {
                my_bitmap.Clear();
                my_bitmap.DrawText("System Time:" + DateTime.Now.ToString(), font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 0);
                my_bitmap.DrawText("IP address:" + ethernetJ11D.NetworkInterface.IPAddress, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 15);
                my_bitmap.DrawText("Router IP:" + ethernetJ11D.NetworkInterface.GatewayAddress, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 30);
                my_bitmap.DrawText("MAC: " + ByteExtensions.ToHexString(ethernetJ11D.NetworkSettings.PhysicalAddress), font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 45);
                displayTE35.SimpleGraphics.DisplayImage(my_bitmap, 0, (Int32)displayTE35.Height / 2);
                Thread.Sleep(1000);
            }
        }

        private void button_ButtonPressed(GTM.GHIElectronics.Button sender, GTM.GHIElectronics.Button.ButtonState state)
        {
            ImidiateMeasure();
            //Bitmap my_bitmap = new Bitmap((Int32)displayTE35.Width / 2, (Int32)displayTE35.Height / 2);
            //Measurement mymeasure = new Measurement();
        }

        private void ethernetJ11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            //initializeNetwork();
            while (ethernetJ11D.NetworkSettings.IPAddress == "0.0.0.0")
            {
                //Debug.Print("Waiting for DHCP");
                waiting = true;
                //Thread.Sleep(25);
            }
            waiting = false;
            Debug.Print("network just got up!\n");
            setInternet(true);
        }

        private void ethernetJ11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Program.setInternet(false);
            //initializeNetwork();
            //ethernetJ11D.NetworkInterface.Close();
            Debug.Print("network just got down!\n");
        }

        void ImidiateMeasure()
        {

            displayTE35.BacklightEnabled = true;
            //displayTE35.DebugPrintEnabled = true;
            //half top of screen to show data
            Bitmap my_bitmap = new Bitmap((Int32)displayTE35.Width, (Int32)displayTE35.Height / 2);
            Measurement temp = new Measurement();
            Measurement humid = new Measurement();
            Measurement UXindex = new Measurement();
            if (!mytempsensor.Measure(temp, humid))
            {
                temp.status = "FAIL";
                humid.status = "FAIL";
                string timestring = "Measurement Time:" + DateTime.Now.ToString();
                my_bitmap.DrawText(timestring, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 10);
                my_bitmap.DrawText("Measure fails!", font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 25);
                displayTE35.SimpleGraphics.DisplayImage(my_bitmap, 0, 0);
            }
            else
            {
                my_bitmap.Clear();
                string timestring = "Measurement Time:" + DateTime.Now.ToString();
                string tempstring = "Temperature:" + temp.value.ToString("F") + "°C";
                string humdstring = "Humidity:" + humid.value.ToString("F") + "%";
                string internetstring = "Ineternet is" + (isInternet ? " " : " not ") + "available";
                my_bitmap.DrawText("This is a requested measurement.", font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 10);
                my_bitmap.DrawText(timestring, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 25);
                my_bitmap.DrawText(tempstring, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 40);
                my_bitmap.DrawText(humdstring, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 55);
                my_bitmap.DrawText(internetstring, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 70);
                displayTE35.SimpleGraphics.DisplayImage(my_bitmap, 0, 0);
            }
        }
        void Measure()
        {
            Thread.Sleep(20000);
            displayTE35.BacklightEnabled = true;
            //displayTE35.DebugPrintEnabled = true;
            Bitmap my_bitmap = new Bitmap((Int32)displayTE35.Width, (Int32)displayTE35.Height / 2);
            int counter = 0;
            Measurement temp = new Measurement();
            temp.sensor = 1;
            temp.device_id = "FEZ27_1";
            Measurement humid = new Measurement();
            humid.sensor = 2;
            humid.device_id = "FEZ27_2";
            Measurement UVindex = new Measurement();
            UVindex.sensor = 3;
            UVindex.device_id = "FEZ27_3";
            //string rootDirectory = sdCard.StorageDevice.RootDirectory;
            while (true)
            {
                if (!mytempsensor.Measure(temp, humid))
                {
                    Debug.Print("Mesure fails!");
                    string timestring = "Measurement Time:" + DateTime.Now.ToString();
                    my_bitmap.DrawText(timestring, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 10);
                    my_bitmap.DrawText("Measure fails!", font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 25);
                    displayTE35.SimpleGraphics.DisplayImage(my_bitmap, 0, 0);
                }
                else
                {
                    counter++;
                    myuvsensor.Measure(UVindex);

                    if (counter >= 10)//mandatory publish every 10 measurements
                    {
                        if (sdCard.IsCardMounted)
                        {
                            //there is internet or cardmounted ,dont need buffer
                            counter = 0;
                            //publishMqtt(temp);
                            //publishMqtt(humid);
                            //publishMqtt(UVindex);
                            JSON.printString(sdCard, temp, isInternet);
                            try
                            {
                                if (timeSyncfinished)
                                {
                                    publishMqtt(temp);
                                    File.Delete(rootDirectory + @"\correct_toSend" + (JSON.getOnline() - 1) + ".JSON");
                                }
                            }

                            catch
                            {

                            }
                            JSON.printString(sdCard, humid, isInternet);
                            try
                            {
                                if (timeSyncfinished)
                                {
                                    publishMqtt(humid);
                                    File.Delete(rootDirectory + @"\correct_toSend" + (JSON.getOnline() - 1) + ".JSON");
                                }
                            }
                            catch
                            {

                            }
                            JSON.printString(sdCard, UVindex, isInternet);
                            try
                            {
                                if (timeSyncfinished)
                                {
                                    publishMqtt(UVindex);
                                    File.Delete(rootDirectory + @"\correct_toSend" + (JSON.getOnline() - 1) + ".JSON");
                                }
                            }
                            catch
                            {

                            }
                            Debug.Print("I wrote on sd card(mandatory).\n");
                            lastMeasureTemperature = temp.DeepCopy();
                            lastMeasureHumidity = humid.DeepCopy();
                            lastMeasureUVindex = UVindex.DeepCopy();
                            SetUp.setWrong(JSON.getOnline());
                            SetUp.setLastToSend(JSON.getOffline());
                            SetUp.writeFile(sdCard);
                        }
                        else//sd card unmounted
                        {
                            if (ethernetJ11D.IsNetworkUp&&timeSyncfinished)
                            {
                                publishMqtt(humid);
                                publishMqtt(temp);
                                publishMqtt(UVindex);
                            }
                            else
                            {
                                //save to buffer
                                SaveToBuffer(humid);
                                SaveToBuffer(temp);
                                SaveToBuffer(UVindex);
                            }
                        }
                    }
                    else
                    {
                        //increse index anyway


                        if (Measurement.isSimilar(lastMeasureHumidity, humid, humid.sensor))
                        //add the online case
                        {
                            if (!isInternet)
                                JSON.increaseOffline();//when should we increse??
                            else
                                JSON.increseOnline();
                        }
                        else
                        {
                            if (sdCard.IsCardMounted)
                            {
                                //publishMqtt(humid);
                                //Debug.Print(DateTime.UtcNow.ToString());
                                JSON.printString(sdCard, humid, isInternet);
                                Debug.Print("I wrote on sd card.\n");
                                lastMeasureHumidity = humid.DeepCopy();
                                try
                                {
                                    publishMqtt(humid);
                                    File.Delete(rootDirectory + @"\correct_toSend" + (JSON.getOnline() - 1) + ".JSON");
                                }
                                catch
                                {

                                }
                                SetUp.setWrong(JSON.getOnline());
                                SetUp.setLastToSend(JSON.getOffline());
                                SetUp.writeFile(sdCard);
                            }
                            else
                            {
                                if (timeSyncfinished)
                                {
                                    if (ethernetJ11D.IsNetworkUp)
                                        publishMqtt(humid);
                                    else
                                        SaveToBuffer(humid);
                                }
                            }
                        }
                        if (Measurement.isSimilar(lastMeasureTemperature, temp, temp.sensor))
                        //add the online case
                        {
                            if (!isInternet)
                                JSON.increaseOffline();//when should we increse??
                            else
                                JSON.increseOnline();
                        }
                        else
                        {
                            //publishMqtt(temp);
                            if (sdCard.IsCardMounted)
                            {
                                JSON.printString(sdCard, temp, isInternet);
                                Debug.Print("I wrote on sd card.\n");
                                lastMeasureTemperature = temp.DeepCopy();
                                try
                                {
                                    publishMqtt(temp);
                                    File.Delete(rootDirectory + @"\correct_toSend" + (JSON.getOnline() - 1) + ".JSON");
                                }
                                catch
                                {

                                }
                                SetUp.setWrong(JSON.getOnline());
                                SetUp.setLastToSend(JSON.getOffline());
                                SetUp.writeFile(sdCard);
                            }
                            else
                            {
                                if (timeSyncfinished)
                                {
                                    if (ethernetJ11D.IsNetworkUp)
                                        publishMqtt(temp);
                                    else
                                        SaveToBuffer(temp);
                                }
                            }
                        }
                        if (Measurement.isSimilar(lastMeasureUVindex, UVindex, UVindex.sensor))
                        //add the online case
                        {
                            if (!isInternet)
                                JSON.increaseOffline();//when should we increse??
                            else
                                JSON.increseOnline();
                        }
                        else
                        {
                            if (sdCard.IsCardMounted)
                            {
                                //publishMqtt(UVindex);
                                JSON.printString(sdCard, UVindex, isInternet);
                                Debug.Print("I wrote on sd card.\n");
                                lastMeasureUVindex = UVindex.DeepCopy();
                                try
                                {
                                    publishMqtt(UVindex);
                                    File.Delete(rootDirectory + @"\correct_toSend" + (JSON.getOnline() - 1) + ".JSON");
                                }
                                catch
                                {

                                }
                                SetUp.setWrong(JSON.getOnline());
                                SetUp.setLastToSend(JSON.getOffline());
                                SetUp.writeFile(sdCard);
                            }
                            else
                            {
                                if (timeSyncfinished)
                                {
                                    if (ethernetJ11D.IsNetworkUp)
                                        publishMqtt(UVindex);
                                    else
                                        SaveToBuffer(UVindex);
                                }
                            }
                        }
                        //}
                    }

                    // measure_count++;
                    my_bitmap.Clear();
                    string timestring1 = "Measurement Time:" + DateTime.Now.ToString();
                    string tempstring = "Temperature:" + temp.value.ToString("F") + "°C";
                    string humdstring = "Humidity:" + humid.value.ToString("F") + "%";
                    string internetstring = "Ineternet is" + (isInternet ? " " : " not ") + "available";
                    my_bitmap.DrawText("This is a scheduled measurement.", font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 10);
                    my_bitmap.DrawText(timestring1, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 25);
                    my_bitmap.DrawText(tempstring, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 40);
                    my_bitmap.DrawText(humdstring, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 55);
                    my_bitmap.DrawText(internetstring, font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 85);
                    my_bitmap.DrawText("UVINDEX:" + UVindex.value.ToString(), font, Microsoft.SPOT.Presentation.Media.Color.White, 5, 70);
                    displayTE35.SimpleGraphics.DisplayImage(my_bitmap, 0, 0);
                    Debug.Print("Current Real-time Clock " + DateTime.Now.ToString());
                    Debug.Print("Humidity:" + humid.value.ToString("F") + "%");
                    Debug.Print("Temperature:" + temp.value.ToString("F") + "°C");
                    //Debug.Print("back from json!");
                }
                Thread.Sleep(10000);
            }
        }

        void initializeNetwork()
        {
            string[] googleDNS = new string[] { "8.8.8.8" };
            try
            {
                //ethernetJ11D.NetworkInterface.Open();
                //ethernetJ11D.NetworkSettings.EnableDhcp();
                //ethernetJ11D.NetworkSettings.EnableDynamicDns();
                //ethernetJ11D.UseStaticIP("192.168.1.222", "255.255.254.0", "192.168.1.1");
                ethernetJ11D.UseDHCP();
                ethernetJ11D.NetworkSettings.EnableStaticDns(googleDNS);
                Microsoft.SPOT.Net.NetworkInformation.NetworkInterface settings = ethernetJ11D.NetworkSettings;

                while (ethernetJ11D.NetworkSettings.IPAddress == "0.0.0.0")
                {
                    Debug.Print("Waiting for DHCP");
                    Thread.Sleep(2500);
                }

                ethernetJ11D.UseDHCP();
                setInternet(true);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

            if (ethernetJ11D.IsNetworkUp)
            {
                Debug.Print("network is up");
            }
            else
            {
                Debug.Print("network down");
            }

        }

        void syncTimer()
        {

            Microsoft.SPOT.Time.TimeServiceSettings timeSettings = new Microsoft.SPOT.Time.TimeServiceSettings()
            {
                ForceSyncAtWakeUp = true,
                Tolerance = 1000
            };

            Thread.Sleep(8000);
            //TimeService.Start();
            while (true)
            {
                try
                {
                    if (ethernetJ11D.IsNetworkUp && !waiting)
                    {
                        IPAddress[] address = System.Net.Dns.GetHostEntry("time.google.com").AddressList;
                        timeSettings.PrimaryServer = address[0].GetAddressBytes();
                        TimeService.Settings = timeSettings;
                        TimeService.SetTimeZoneOffset(120);
                        //TimeService.UpdateNow(1000);
                        Debug.Print("About to start time sync\n");
                        TimeService.UpdateNow(address[0].GetAddressBytes(), 1000);
                        Debug.Print("Time sync finished.\n");
                        timeSyncfinished = true;
                    }

                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Debug.Print("Exception catched.\n");
                }

                //if (TimeService.LastSyncStatus.Flags.Equals(TimeServiceStatus.TimeServiceStatusFlags.SyncSucceeded))
                //{
                //    isInternet = true;
                //}
                //else
                //{
                //    isInternet = false;
                //}
                Debug.Print("Sync thread finished there.\n");
                if (ethernetJ11D.IsNetworkUp && !waiting && isInternet)
                    Thread.Sleep(3600000);//sync every 6 hours if there is internet
                else
                    Thread.Sleep(1000);//try again after 1s if no internet
            }
        }

        static public void setInternet(bool internet)
        {
            isInternet = internet;
        }

        private void TimeService_TimeSyncFailed(object sender, TimeSyncFailedEventArgs e)
        {
            Program.setInternet(false);
            Debug.Print("I set internet to false.\n");
        }

        private void TimeService_SystemTimeChanged(object sender, SystemTimeChangedEventArgs e)
        {
            Program.setInternet(true);
            Debug.Print("Time is changed.\n");
        }

        //void initializeNetworkThread()
        //{
        //    while (true)
        //    {
        //        if (!isInternet)
        //        {
        //            initializeNetwork();
        //            Debug.Print("backing to internet\n");
        //        }
        //        Thread.Sleep(20000);
        //    }
        //}
    }
}