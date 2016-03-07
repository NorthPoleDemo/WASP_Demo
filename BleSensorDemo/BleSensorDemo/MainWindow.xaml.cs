using System;
using System.Windows.Interop;
using System.Windows;
using System.Collections.Specialized;
using System.ComponentModel;
using NorthPoleEngineering.WaspClassLibrary;
using System.Reflection;

namespace BleSensorDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WaspCollection _wasps;
        AntPlusDevices _ants;
        KeiserM3Collection _bikes;
        GenericBleCollection _ble;
        PolarH7Collection _polarHrms;
        SuuntoHRMCollection _suuntoHRMs;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Connect to Wasp
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UsbOnly.IsChecked == true)
                {
                    WindowInteropHelper w = new WindowInteropHelper(this);
                    _wasps = new WaspCollection(w.Handle);
                }
                else
                {
                    _wasps = new WaspCollection();
                }

                // Register for notification of a new WASP added or removed
                _wasps.CollectionChanged += WASP_CollectionChangedEvent;
                // Extract the list of ANT devices reported by the WASPs
                _ants = new AntPlusDevices(_wasps);
                // Get the list of KeiserM3i bikes
                _bikes = _ants.KeiserM3Bikes; 
                // Register for notification of a bike added or removed
                _bikes.BluetoothNewDeviceEvent += new EventHandler<KeiserM3EventArgs>(Keiser_DataEvent);

                _polarHrms = _ants.PolarH7HeartRateMonitors;
                _polarHrms.CollectionChanged += PolarHrms_CollectionChanged;

                _suuntoHRMs = _ants.SuuntoHeartRateMonitors;
                _suuntoHRMs.CollectionChanged += Suunto_CollectionChanged;

                _ble = _ants.GenericBleDevices;
                _ble.BluetoothNewDeviceEvent += Ble_BluetoothNewDeviceEvent;


                BtnConnect.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to connect: {0}", ex.ToString()));
            }
        }

        /// <summary>
        /// Creates display for new Suunto Heart Rate Monitors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Suunto_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (SuuntoHRM hrm in e.NewItems)
                { 
                    string line = string.Format("Type: Suunto HRM {0} ID:{1} Rate:{2}", hrm.Service.ToString(), hrm.DeviceId, hrm.FilteredHeartRate);
                    BleDevices.Items.Add(line);
                    hrm.PropertyChanged += SuuntoHrm_PropertyChanged; ;
                }
            }
        }

        /// <summary>
        /// Data updates for Suunto HRMs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SuuntoHrm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SuuntoHRM hrm = sender as SuuntoHRM;
            string line = string.Format("Type: Suunto HRM {0} ID:{1} Rate:{2}", hrm.Service.ToString(), hrm.DeviceId, hrm.FilteredHeartRate);

            int i = 0;
            foreach (string s in BleDevices.Items)
            {
                if (s.Contains(hrm.DeviceId.ToString()))
                {
                    BleDevices.Items[i] = line;
                    break;
                }
                i++;
            }
        }

        /// <summary>
        /// Creates display for new Polar Heart Rate monitors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PolarHrms_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PolarH7 polarHRM in e.NewItems)
                {
                    string line = string.Format("Type: Polar H7 {0} ID:{1} Rate:{2}", polarHRM.Service.ToString(), polarHRM.DeviceId, polarHRM.FirstHeartRate);
                    BleDevices.Items.Add(line);
                    polarHRM.PropertyChanged += PolarHRM_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// Updates Polar HRM devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PolarHRM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PolarH7 polarHRM = sender as PolarH7;
            string line = string.Format("Type: Polar H7 {0} ID:{1} Rate:{2}", polarHRM.Service.ToString(), polarHRM.DeviceId, polarHRM.FirstHeartRate);

            int i = 0;
            foreach (string s in BleDevices.Items)
            {
                if (s.Contains(polarHRM.DeviceId.ToString()))
                {
                    BleDevices.Items[i] = line;
                    break;
                }
                i++;
            }
        }

        private void Ble_BluetoothNewDeviceEvent(object sender, GenericBleEventArgs e)
        {
            GenericBle g = _ble.GetData(e.Address);
            g.PropertyChanged += Ble_PropertyChanged;
            Unknown.Items.Add(BitConverter.ToString(g.RawAdvertisingData, 0));
        }

        private void Ble_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        // Show list of bikes found
        void Keiser_DataEvent(object sender, KeiserM3EventArgs e)
        {
            KeiserM3 b = _bikes.GetData(e.Address);

            string line = string.Format("Type: Keiser {0} Firmware={1} DataType={2} Gear={3} Heartrate={4} KCal={5} Power={6} RPM={7} Time={8} Trip={9} Address={10}",
                b.Name, b.Build, b.DataType, b.Gear, b.HeartRate, b.KCal, b.Power, b.RPM, b.Time, b.Trip, b.Address);
            BleDevices.Items.Add(line);

            // Live update for a particular bike            
            b.PropertyChanged += Keiser_PropertyChangedEvent;
        }


        // Update bike data
        private void Keiser_PropertyChangedEvent(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            uint address = uint.Parse(e.PropertyName);
            
            // Build a  line of data
            KeiserM3 b = _bikes.GetData( address);
            string line = string.Format("Type: Keiser {0} Firmware={1} DataType={2} Gear={3} Heartrate={4} KCal={5} Power={6} RPM={7} Time={8} Trip={9} Address={10}",
                b.Name, b.Build, b.DataType, b.Gear, b.HeartRate, b.KCal, b.Power, b.RPM, b.Time, b.Trip, b.Address);

            string strAddress = address.ToString();
            int j = 0;
            // Find which one to update
            foreach (string s in BleDevices.Items)
            {
                if (s.Contains(strAddress))
                {
                    BleDevices.Items[j] = line;
                    break;
                }
                j++;
            }
        }


        // Show list of WASPs in view
        private void WASP_CollectionChangedEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                WaspList.ItemsSource = e.NewItems;
            }
        }

    }
}
