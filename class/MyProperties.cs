using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandwidthMonitoring
{
    public class MyProperties
    {
        public String cardName { get; set; }
        public bool saveToFile { get; set; }
        public double downloadSpeed { get; set; }
        public double timeToShutDown { get; set; }

        public MyProperties()
        {
            this.downloadSpeed = 0;
            this.timeToShutDown = 0;
            this.saveToFile = false;
            this.cardName = "";
        }

        public String getParameters()
        {
            return $"MyProperties [ \nTMPNAME: {cardName}\nSave? {saveToFile}\nDownload {downloadSpeed}\nTime {timeToShutDown}]";
        }
    }
}
