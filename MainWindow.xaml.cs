using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BandwidthMonitoring
{
    public partial class MainWindow : Window
    {

        private MyProperties myProperties;
        private DownloadMonitoring downloadMonitoring;
        public MainWindow()
        {
            InitializeComponent();
            InitFolders();
            InitWindowButton();
            InitStartButton();
            InitClasses();
            InitComboBoxs();
        }

        private void InitFolders()
        {
            if (!System.IO.Directory.Exists("Saved_Info"))
            {
                System.IO.Directory.CreateDirectory("Saved_Info");
            }
        }

        private void InitClasses()
        {
            myProperties = new MyProperties();
        }


        // Initialize combo box with values from network interfece
        // and set boxes to default values
        private void InitComboBoxs()
        {
            PerformanceCounterCategory pcNetworkInterface = new PerformanceCounterCategory("Network Interface");
            String[] cardName = pcNetworkInterface.GetInstanceNames();
            if (cardName.Length > 0)
            {
                foreach (string card in cardName)
                {
                    comboBox_NetworkCard.Items.Add(card);
                }
                comboBox_NetworkCard.SelectedIndex = 0;
            }
            comboBox_Speed.SelectedIndex = 0;
            comboBox_TimeToShutDown.SelectedIndex = 0;
        }


        // Initializa window button to minimaized window and exit application
        private void InitWindowButton()
        {
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximizeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => { stopMonitoring(); Close(); };
        }

        // Init start button to test 
        private void InitStartButton()
        {
            StartButton.Click += (s, e) =>
            {
                if (text_ButtonStart.Text == "Start")
                    startMonitoring();
                else
                    stopMonitoring();
            };
        }


        private void buttonTestCard(object sender, EventArgs e)
        {
            MyPerformaceCounter myPerformace = new MyPerformaceCounter((string)comboBox_NetworkCard.SelectedItem);

            MessageBox.Show($"Testing {(string)comboBox_NetworkCard.SelectedItem} has begun. Wait about 10 seconds for the result!", "Information");

            Thread thread = new Thread(()=>
            {
                double downloadCount = 0.0;
                
                for(int i = 0; i < 10; i++)
                {
                    Action action = new Action(() => downloadCount += myPerformace.getDownloadValue());
                    Dispatcher.BeginInvoke(action);
                    Thread.Sleep(1000);
                }

                String msg = "";

                if (downloadCount > 10.0) msg = "The test was successful. It looks like the card is being used!";
                else msg = "The test went negative. The card is not used! Try using another one from the list.";

                Action ness = new Action(()=> MessageBox.Show(msg,"Result"));
                Dispatcher.BeginInvoke(ness);
            });

            thread.Start();
        }


        // The method that realizes the stop of network
        // monitoring and sets the GUI.
        private void stopMonitoring()
        {
            if (downloadMonitoring != null)
            {
                downloadMonitoring.stop();
                text_ButtonStart.Text = "Start";
                text_Information.Text = "";
                text_CurrentDownload.Text = "0.0\nKb/s";
                text_PeakSpeed.Text = "0.0 Kb/s";
                text_TimeToShutDown.Text = "0 s";
                text_AverageSpeed.Text = "0.0 Kb/s";
                text_PeakSpeedUnderProgressBar.Text = "0.0\nKb/s";
                changeAngle(0.0);
            }
        }

        // The method that realizes the start of network
        // monitoring and sets the GUI.
        private void startMonitoring()
        {
            downloadMonitoring = new DownloadMonitoring(myProperties);
            downloadMonitoring.DataUpdate += HandleDataUpdate;
            downloadMonitoring.start();
            text_ButtonStart.Text = "Stop";
        }

        // Listening for bandwidth changes in the combo box 
        private void comboBox_Speed_change(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)comboBox_Speed.SelectedItem;
            string sItem = item.Content.ToString();
            if (sItem.Equals("Do not turn off"))
            {
                myProperties.downloadSpeed = -1;
                comboBox_TimeToShutDown.IsEnabled = false;
            }
            else
            {
                comboBox_TimeToShutDown.IsEnabled = true;
                myProperties.downloadSpeed = double.Parse(sItem.Split(' ')[0]);
            }
        }

        // Listening for time to close changes in the combo box 
        private void comboBox_Time_change(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)comboBox_TimeToShutDown.SelectedItem;
            string sItem = item.Content.ToString();
            if (sItem.Equals("Immediately")) myProperties.timeToShutDown = -1;
            else myProperties.timeToShutDown = double.Parse(sItem.Split(' ')[0]);
        }

        private void comboBox_Card_change(object sender, SelectionChangedEventArgs e)
        {
            myProperties.cardName = (string)comboBox_NetworkCard.SelectedItem;
        }

        // Listening for save to file changes in the check box
        private void Save_Checked(object sender, RoutedEventArgs e)
        {
            myProperties.saveToFile = (bool)checkBox_SaveToFile.IsChecked;
        }

        // Set the computer on keep awake mode
        private void checkBox_DoNotAllowSleep(object sender, RoutedEventArgs e)
        {
            KeepAwake.start();
        }

        // Disable keep awake mode
        private void checkBox_AllowSleep(object sender, RoutedEventArgs e)
        {
            KeepAwake.stop();
        }

        // Animation and change angle of progress bar 
        private void changeAngle(double newAngle)
        {
            double oldAngle = progressBar.EndAngle;

            double difference = Math.Abs(oldAngle - newAngle); 
            if (difference < 1) return;

            int time = (int)(1 * (230 / difference));  
            if (time < 0) return;


            Task task = new Task(() =>
            {
                if (oldAngle > newAngle)
                {
                    for (double i = oldAngle; i > newAngle; i--)
                    {
                        Action action = new Action(() => progressBar.EndAngle = i);
                        Dispatcher.BeginInvoke(action);
                        Thread.Sleep(time);
                    }
                } else
                {
                    for (double i = oldAngle; i < newAngle; i++)
                    {
                        Action action = new Action(() => progressBar.EndAngle = i);
                        Dispatcher.BeginInvoke(action);
                        Thread.Sleep(time);
                    }
                }
            });
            task.Start();
        }
        

        // Handler to update GUI
        private void HandleDataUpdate(object sender, PerformanceEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                text_CurrentDownload.Text = e.getDownloadValue();
                text_PeakSpeed.Text = e.getPeakBandwith();
                text_Information.Text = e.getInformation();
                text_TimeToShutDown.Text = e.getTimeToShutDown();
                text_AverageSpeed.Text = e.getAverageDownload();
                changeAngle(e.getAngle());
                text_PeakSpeedUnderProgressBar.Text = e.getPeakBandwith().Replace(" ", "\n");
            });
        }
    }
}
