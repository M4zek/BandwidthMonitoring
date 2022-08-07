using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace BandwidthMonitoring
{


    
    public partial class MainWindow : Window
    {

        private MyProperties myProperties;


        public MainWindow()
        {
            InitializeComponent();
            InitWindowButton();
            InitStartButton();
            InitClasses();
            InitComboBoxs();
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
            CloseButton.Click += (s, e) => Close();
        }

        // Init start button to test 
        private void InitStartButton()
        {
            StartButton.Click += (s, e) =>
            {
                Console.WriteLine(myProperties.getParameters());
            };
        }

        // Listening for bandwidth changes in the combo box 
        private void comboBox_Speed_change(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem) comboBox_Speed.SelectedItem;
            string sItem = item.Content.ToString();
            if (sItem.Equals("Do not turn off")) myProperties.downloadSpeed = -1;
            else myProperties.downloadSpeed = double.Parse(sItem.Split(' ')[0]);
        }

        // Listening for time to close changes in the combo box 
        private void comboBox_Time_change(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem) comboBox_TimeToShutDown.SelectedItem;
            string sItem = item.Content.ToString();
            if (sItem.Equals("Immediately")) myProperties.timeToShutDown = -1;
            else myProperties.timeToShutDown = double.Parse(sItem.Split(' ')[0]);
        }

        // Listening for save to file changes in the check box
        private void Save_Checked(object sender, RoutedEventArgs e)
        {
            myProperties.saveToFile = (bool)comboBox_SaveToFile.IsChecked;
        }

        // Listening for changes in the check box
        private void StayAwake(object sender, RoutedEventArgs e)
        {
            // TODO Create a class that implements the process
            // of keeping a computer in a ready state 
        }
    }
}
