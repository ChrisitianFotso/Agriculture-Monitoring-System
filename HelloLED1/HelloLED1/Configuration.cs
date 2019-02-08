using System;
using Microsoft.SPOT;

namespace HelloLED1
{
    public class Configuration
    {
        public String version = "1"; 
        public String id = "FEZ_27";
        public String name = "Temperature/humidity sensor";
        public String group = "FEZ_27";
        public String type = "temperature";
        public String[] sensors = new String[]{"temperature", "humidity"};
        public String location = "Classromm 5i";
        public Double latitude = 45.116177;
        public Double longitude = 7.742615;
        public bool @internal = true;

    }
}
