using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BandwidthMonitoring
{
    public class DownloadMonitoring
    {
        ////////////////////////////////
        /// VARIABLES
        ///////////////////////////////
        public event EventHandler<PerformanceEventArgs> DataUpdate;
        private PerformanceEventArgs performanceArgs;
        public MyProperties myProperties;
        private MyPerformaceCounter myPerformaceCounter;
        private DateTime startMonitoringTime;


        private Thread downloadMonitoring;

        // Constructor
        public DownloadMonitoring(MyProperties myProperties)
        {
            this.myProperties = myProperties;
            performanceArgs = new PerformanceEventArgs();
        }

        /////////////////////////////
        ////// METHODS
        ////////////////////////////

        public void start()
        {
            myPerformaceCounter = new MyPerformaceCounter(myProperties.cardName);
            downloadMonitoring = new Thread(new ThreadStart(selectMonitoringType));
            downloadMonitoring.Start();
        }

        public void stop()
        {
            if(downloadMonitoring != null)
            {
                downloadMonitoring.Abort();
                saveInfoToFile();
            }
        }





        // A method that checks the data set by the user and,
        // based on it, starts the corresponding monitoring method 
        private void selectMonitoringType()
        {
            startMonitoringTime = DateTime.Now;
            if (myProperties.downloadSpeed == -1)
                infinityMonitoring();
            else
            {
                if (myProperties.timeToShutDown == -1) shutDownImmediately();
                else shutDownAfterTime();
            }
        }

        // Method send to gui information about closing computer
        // and next shut down the computer 
        private void shutDownComputer()
        {
            for (int i = 10; i >= 0; i--)
            {
                OnInformationUpdate($"Komputer zostanie wyłączony za {i} sekund");
                Thread.Sleep(1000);
            }

            var process = new ProcessStartInfo("shutdown", $"/s /t {0}");
            process.CreateNoWindow = true;
            process.UseShellExecute = false;
            Process.Start(process);
        }

        private void saveInfoToFile()
        {
            if (myProperties.saveToFile)
            {
                DateTime endMonitoringTime = DateTime.Now;
                TimeSpan time = endMonitoringTime - startMonitoringTime;

                string fileName = $"Saved_Info/Monitoring ({startMonitoringTime.Day}{startMonitoringTime.Month}{startMonitoringTime.Year} {startMonitoringTime.Hour}{startMonitoringTime.Minute}).txt";

                using (StreamWriter outFile = new StreamWriter(fileName))
                {
                    outFile.WriteLine( "---------------------------------------------------------------------------");
                    outFile.WriteLine( "-------------------------------- DATA FILE --------------------------------");
                    outFile.WriteLine( "---------------------------------------------------------------------------");
                    outFile.WriteLine($"| Program Start Date: {startMonitoringTime}");
                    outFile.WriteLine($"| Program End Date: {endMonitoringTime}");
                    outFile.WriteLine($"| Program Runtime: {time.Hours}h {time.Minutes}m {time.Seconds}s");
                    outFile.WriteLine($"| Peak Download: {performanceArgs.getPeakBandwith()}");
                    outFile.WriteLine($"| Average Download: {performanceArgs.getAverageDownload()}");
                    outFile.WriteLine($"| Number of samples: {performanceArgs.numberOfSampels}");
                    outFile.WriteLine($"| Total kbps downloaded: {performanceArgs.totalBytes}");
                    outFile.WriteLine("---------------------------------------------------------------------------");
                    outFile.WriteLine("---------------------------------------------------------------------------");
                }
            }
        }

        ////////////////////////
        /// Monitoring method
        ////////////////////////
        

        // Method monitoring bandwith until click stop button or close application
        private void infinityMonitoring()
        {
            double previousDownloadPeak = 0.0;
            double currentDownload = myPerformaceCounter.getDownloadValue();
            OnTimeToShutDownUpdate("NaN");

            while (true)
            {
                if(currentDownload > previousDownloadPeak)
                {
                    previousDownloadPeak = currentDownload;
                    OnPeakBandwithUpdate(previousDownloadPeak.ToString());
                }

                OnAverageDownloadUpload(currentDownload);
                OnDownloadValueUpdate(currentDownload.ToString());

                currentDownload = myPerformaceCounter.getDownloadValue();
                Thread.Sleep(1000);
            }
        }

        // A method of monitoring bandwidth until the set speed is greater than the current speed
        // then the computer is shut down immediately
        private void shutDownImmediately()
        {
            double previousPeakDownload = 0.0;
            double currentDownload = myPerformaceCounter.getDownloadValue();

            while(myProperties.downloadSpeed < currentDownload)
            {

                if(currentDownload > previousPeakDownload)
                {
                    previousPeakDownload = currentDownload;
                    OnPeakBandwithUpdate(previousPeakDownload.ToString());
                }

                OnAverageDownloadUpload(currentDownload);
                OnDownloadValueUpdate(currentDownload.ToString());
                currentDownload = myPerformaceCounter.getDownloadValue();
                Thread.Sleep(1000);
            }
            shutDownComputer();
        }

        // A method of monitoring the bandwidth until the set time is greater than the time
        // to keep the speed below the target then the computer will shut down
        private void shutDownAfterTime()
        {
            double previousPeakDownload = 0.0;
            double currentDownload = myPerformaceCounter.getDownloadValue();

            int sec = 0;
            while(sec < myProperties.timeToShutDown)
            {
                if(currentDownload > previousPeakDownload)
                {
                    previousPeakDownload = currentDownload;
                    OnPeakBandwithUpdate(previousPeakDownload.ToString());
                }

                OnAverageDownloadUpload(currentDownload);
                OnDownloadValueUpdate(currentDownload.ToString());
                currentDownload = myPerformaceCounter.getDownloadValue();

                if (myProperties.downloadSpeed > currentDownload) sec++;
                else sec = 0;

                OnTimeToShutDownUpdate(sec.ToString());
                Thread.Sleep(1000);
            }
            shutDownComputer();
        }


       ///////////////////////
       // Update data method 
       ///////////////////////

        // Update download value in args class 
        private void OnDownloadValueUpdate(string value)
        {
            var handler = DataUpdate;
            if(handler != null)
            {
                performanceArgs.DownloadValue = value;
                handler(this, performanceArgs);
            }
        }

        // Update peak bandwith value 
        private void OnPeakBandwithUpdate(string value)
        {
            var handler = DataUpdate;
            if(handler != null)
            {
                performanceArgs.PeakBandwidth = value;
                handler(this, performanceArgs);
            } 
        }

        // Update information 
        private void OnInformationUpdate(string value)
        {
            var handler = DataUpdate;
            if(handler != null)
            {
                performanceArgs.Information = value;
                handler(this, performanceArgs);
            }
        }

        // Update time to shut down computer
        private void OnTimeToShutDownUpdate(string value)
        {
            var handler = DataUpdate;
            if (handler != null)
            {
                performanceArgs.TimeToShutDown = value;
                handler(this, performanceArgs);
            }
        }

        private void OnAverageDownloadUpload(double value)
        {
            var handler = DataUpdate;
            if (handler != null)
            {
                performanceArgs.totalBytes += value;
                performanceArgs.numberOfSampels++;
                handler(this, performanceArgs);
            }
        }
    }


    public class PerformanceEventArgs : EventArgs
    {
        public string DownloadValue { get; set; }
        public string PeakBandwidth { get; set; }
        public string Information { get; set; }
        public string TimeToShutDown { get; set; }


        public double AverageDownload { get; set; }
        public double totalBytes { get; set; }
        public ulong numberOfSampels { get; set; }
        


        public PerformanceEventArgs()
        {
            this.DownloadValue = "0.0";
            this.PeakBandwidth = "0.0";
        }


        // A method that returns the current bandwidth value in megabytes or kilobytes
        // as a string so that it can be displayed in the GUI
        public string getDownloadValue()
        {
            double value = 0;
            if (DownloadValue != null)
                value = double.Parse(DownloadValue);

            return convertToMbOrKb(value);
        }

        // A method that returns the peak bandwith value in megabytes or kilobytes
        // as a string so that it can be displayed in the GUI
        public string getPeakBandwith()
        {
            double value = 0;
            if (PeakBandwidth != null)
                value = double.Parse(PeakBandwidth);

            return convertToMbOrKb(value).Replace('\n',' ');
        }

        // Return information
        public string getInformation()
        {
            return Information;
        }


        // A method that returns the time for the speed to remain below the assumed speed 
        public string getTimeToShutDown()
        {
            return $"{TimeToShutDown} sec";
        }


        public String getAverageDownload()
        {
            AverageDownload = totalBytes / numberOfSampels;
            return convertToMbOrKb(AverageDownload).Replace("\n", " ");
        }

        public double getAngle()
        {
            return  230 * (double.Parse(DownloadValue) / double.Parse(PeakBandwidth));
        }


        //////////////////////
        /// METHODS
        ////////////////////// 
        
        // A method that returns a string based on the highest value
        // recorded during monitoring. The value can be in kilobytes or megabytes
        // depending on the value passed as the value parameter
        public string convertToMbOrKb(double value)
        {
            if (value >= 1024)
                return $"{Math.Round(value / 1024, 2)}\nMb/s";
            return $"{Math.Round(value, 2)}\nKb/s";
        }
    }

}
