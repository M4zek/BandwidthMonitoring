using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandwidthMonitoring
{
    public class MyPerformaceCounter
    {

        ///////////////////////////////
        /// VARIABLES
        //////////////////////////////
        private PerformanceCounter performanceCounter;
        public double lngOldDownloadValue = 0.0;
        public double lngDownloadValue = 0.0;
        public double lngDownloadSpeed = 0.0;


        // Constructor
        public MyPerformaceCounter(string cardName)
        {
            performanceCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", cardName);
            lngOldDownloadValue = performanceCounter.NextSample().RawValue;
            lngDownloadValue = 0.0;
            lngDownloadSpeed = 0.0;
        }

        // Get download value
        public double getDownloadValue()
        {
            lngDownloadValue = performanceCounter.NextSample().RawValue;
            lngDownloadSpeed = lngDownloadValue - lngOldDownloadValue;
            lngOldDownloadValue = lngDownloadValue;
            return Math.Round(lngDownloadSpeed / 1024,2);
        }
    }
}
