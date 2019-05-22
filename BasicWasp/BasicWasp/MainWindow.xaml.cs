using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

using NorthPoleEngineering.WaspClassLibrary;
using NorthPoleEngineering.WaspClassLibrary.FitnessEquipmentControls;

namespace BasicWasp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Create a class to hold displayable WASP data
        /// </summary>
        public class WaspData
        {
            public string MAC { get; set; }
            public string FW { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string ChargingState { get; set; }
            public string Voltage { get; set; }
            public string FuelGauge { get; set; }

            public WaspData()
            {

            }
        }

        /// <summary>
        /// Build a collection of WASPs we get data from
        /// </summary>
        public ObservableCollection<WaspData> _waspData;

        WaspCollection _wasps;
        AntPlusDevices _ants;
        public MainWindow()
        {
            InitializeComponent();
            //OEMCryptoClass.AddKeys(PrivateKeychain.Keychain.Keys);
            _waspData = new ObservableCollection<WaspData>();
            DataGridWasps.DataContext = _waspData;
        }

        /// <summary>
        /// Creates a WASP collection  which will then accept data from BLE and ANT devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a collection of wasps on all available networks
            if (checkBoxUsb.IsChecked == true)
            {
                WindowInteropHelper w = new WindowInteropHelper(this);
                _wasps = new WaspCollection(WaspCollection.DefaultMulticastPort, w.Handle);
            }
            else
            {
                _wasps = new WaspCollection();
            }

            _wasps.CollectionVerbosity = Wasp.WaspLogLevel.All;
            // Get Query response message
            _wasps.CollectionChanged += wasps_CollectionChanged;
            // Extract the list of ANT devices reported by the WASPs
            _ants = new AntPlusDevices(_wasps);
            _ants.AntPlusDeviceEvent += _ants_AntPlusDeviceEvent;
            _ants.KeiserM3Bikes.BluetoothNewDeviceEvent += KeiserM3Bikes_BluetoothNewDeviceEvent; 
            StartButton.IsEnabled = false;
        }

        /// <summary>
        /// Display WASP information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wasps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Wasp w = e.NewItems[0] as Wasp;
                try
                {
                    WaspData wd = new WaspData
                    {
                        ChargingState = w.IsCharging.ToString(),
                        FW = string.Format("{0}",w.FirmwareVersion),
                        MAC = BitConverter.ToString(w.MAC),
                        Name = w.Name,
                        Type = TypeDescriptor.GetConverter(w.ProductType).ConvertToString(w.ProductType),
                        Voltage = string.Format("{0:0.00}", w.BatteryLevel),
                        FuelGauge = string.Format("{0:0.0}%", w.FuelGauge*100)
                    };
                    _waspData.Add(wd);
                }
                catch (ArgumentNullException)
                {
                    // USB mode does not have MAC address at this time
                }
            }
        }

        /// <summary>
        /// Handler for Keiser M3 bluetooth bike
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeiserM3Bikes_BluetoothNewDeviceEvent(object sender, KeiserM3EventArgs e)
        {
        }

        /// <summary>
        /// This method is called when an ANT device is added or removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ants_AntPlusDeviceEvent(object sender, AntPlusDeviceEventArgs e)
        {
            // Format data and display
            GroupBox g = CreateDeviceBox(e.Device, e);                   
            if (g != null)
            {
                MainPanel.Children.Add(g);

                // Register heart rate monitor for data updates
                if (e.DeviceType == AntDeviceType.HeartRateMonitor)
                {
                    ((HeartRateMonitor)(e.Device)).HeartRateMessageEvent += MainWindow_AntMessageEvent;
                }
                else if (e.DeviceType == AntDeviceType.BikeSpeedSensor)
                {
                    ((BikeSpeedSensor)(e.Device)).BikeSpeedMessageEvent += MainWindow_AntMessageEvent;
                }
                else if (e.DeviceType == AntDeviceType.BikePowerSensor)
                {
                    ((BikePowerSensor)(e.Device)).BikePowerOnlyEvent += MainWindow_AntMessageEvent;
                }
                else if (e.DeviceType == AntDeviceType.MuscleOxygenMonitor)
                {
                    ((MuscleOxygenMonitor)(e.Device)).PropertyChanged += MainWindow_AntMessageEvent;
                }
                else if (e.DeviceType == AntDeviceType.FitnessEquipment)
                {
                    ((FitnessEquipment)(e.Device)).FitnessEquipmentEvent += MainWindow_AntMessageEvent;
                }
                else if (e.DeviceType == AntDeviceType.UnknownSensor)
                {
                    ((UnknownDevice)(e.Device)).PropertyChanged += MainWindow_AntMessageEvent;
                }
            }
        }


        /// <summary>
        /// Generic handler for ANT message updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_AntMessageEvent(object sender, EventArgs e)
        {
            // Find the listview it corresponds to and refresh            
            string targetName = "ID: " + ((AntDevice)sender).ExtendedDeviceNumber.ToString();
            try
            {
                GroupBox gb = MainPanel.Children.OfType<GroupBox>().First<GroupBox>(x => x.Header.ToString() == targetName);
                ListView lv = gb.Content as ListView;

                List<string> newData = DeviceFormatter.FormatDevice((AntDevice)sender, e);
                //lv.ItemsSource = null;
                if (newData?.Count > 0)
                {
                    lv.ItemsSource = newData;
                    lv.Items.Refresh();
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Format a group box for the device
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="args">Arguments, if any</param>
        /// <returns></returns>
        GroupBox CreateDeviceBox(AntDevice dev, EventArgs args)
        {
            GroupBox gb = null;
            List<string> d = DeviceFormatter.FormatDevice(dev, args);
            if (d.Count > 0)
            {
                ListView lv = new ListView();
                lv.ItemsSource = d;
                lv.Margin = new Thickness(5);
                lv.Padding = new Thickness(5);
                gb = new GroupBox()
                {
                    Width = 200,
                    // First item in the list is the ID
                    Header = d[0],
                    Content = lv
                };
            }
            return gb;
        }
    }
}
