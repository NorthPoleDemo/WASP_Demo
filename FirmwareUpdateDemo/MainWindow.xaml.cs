using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using NorthPoleEngineering.WaspClassLibrary.Programming;
using NorthPoleEngineering.WaspClassLibrary;

namespace FirmwareUpdateDemo
{
    /// <summary>
    /// This example shows how to update the firmware on a WASP-PoE over Ethernet or a WASP-N over WiFi.
    /// 
    /// The devices used in this example are given names of PoE-Test and WASP-N_Test221. You may substitute the
    /// names of your own devices in the  _deviceNames array below.
    /// 
    /// The firmware versions included are PoE 2.4.11 and N 5.4.22. 
    /// </summary>
    public partial class MainWindow : Window
    {
        WaspCollection _wasps;
        Wasp _waspUT;
        Wasp _waspUsb;
        EventWaitHandle _waspReady;
        string _poeBundle;
        ProgrammingStatusCode _status;

        AutoResetEvent _programCycleComplete;

        /// <summary>
        /// Name of the WASP to be used for this test
        /// </summary>
        string[] _deviceNames = { "PoE-Test", "WASP-N_Test" };

        public MainWindow()
        {
            InitializeComponent();
            buttonPoE.IsEnabled = false;
            buttonOta.IsEnabled = false;
            buttonUsb.IsEnabled = false;
            Loaded += MainWindow_Loaded;
            _waspReady = new EventWaitHandle(false, EventResetMode.AutoReset);
            label.Content = "";
            _poeBundle = "WASP-PoE 2.4.11_1.6-1.11_Bootloader-Update-R1.zip";
        }

        /// <summary>
        /// Called after the window has loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper w = new WindowInteropHelper(this);
            _wasps = new WaspCollection(new IPAddress(new byte[] { 239, 78, 80, 1 }), 51113, w.Handle);
            _wasps.CollectionVerbosity = Wasp.WaspLogLevel.None;
            _wasps.CollectionChanged += Wasps_CollectionChanged;
        }


        /// <summary>
        /// Example of updating WASP-PoE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPoE_Click(object sender, RoutedEventArgs e)
        {
            Progress.Background = Brushes.Yellow;
            Progress.Value = 0;
            var progress = new Progress<ProgrammingProgress>(TesterProgrammingProgress);
            label.Content = "Programming...";
            _waspUT.OverrideFirmwareVersion = true;
            _waspUT.UpdateFirmware(Bundle.GetBundle(_poeBundle), progress, new CompletionCallback(ProgrammingComplete), AccessMethod.Ethernet);
        }

        private void Wasp_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RadioVersion")
            {
                Progress.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { label2.Content = string.Format("Radio 1 F/W: {0}",_waspUT.RadioVersion[1]); }));                
            }
        }

        /// <summary>
        /// Called when a WASP is added or removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wasps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                int count = e.NewItems.Count;
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    if (((Wasp)e.NewItems[i]).Name == _deviceNames[0])
                    {
                        _waspUT = (Wasp)e.NewItems[i];
                        _waspReady.Set();
                        buttonPoE.IsEnabled = true;
                        buttonRadio.IsEnabled = true;
                        Progress.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { label1.Content = string.Format("{0} F/W version {1}.{2}.{3}", 
                        _waspUT.Name, _waspUT.VersionMajor, _waspUT.VersionMinor, _waspUT.ProductVersion); }));
                        break;
                    }
                    else if (((Wasp)e.NewItems[i]).Name == _deviceNames[1])
                    {
                        _waspUT = (Wasp)e.NewItems[i];
                        _waspUT.PropertyChanged += Wasp_PropertyChanged;
                        WaspCollection.SendExtendedWaspQuery(_waspUT.WaspIPAddress);
                        Progress.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                            label1.Content = string.Format("{0} F/W version {1}.{2}.{3}", _waspUT.Name, _waspUT.VersionMajor, _waspUT.VersionMinor, _waspUT.ProductVersion);
                        }));

                        _waspReady.Set();
                        buttonOta.IsEnabled = true;
                        break;
                    }
                    else if (((Wasp)e.NewItems[i]).BasestationMode)
                    {
                        _waspUsb = (Wasp)e.NewItems[i];
                        buttonUsb.IsEnabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Called on programming update
        /// </summary>
        /// <param name="obj"></param>
        private void TesterProgrammingProgress(ProgrammingProgress obj)
        {
            // Need to call the dispatcher since this update is coming in on a different thread
            Progress.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { Progress.Value = obj.Progress; }));
            Progress.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { label.Content = obj.Action; }));
            System.Diagnostics.Trace.WriteLine(obj.Progress);
        }

        /// <summary>
        /// Called upon completion of programming
        /// </summary>
        /// <param name="status">Status update</param>
        private void ProgrammingComplete(ProgrammingStatusCode status)
        {
            if (status == ProgrammingStatusCode.SUCCESS)
            {
                WaspCollection.SendExtendedWaspQuery(_waspUT.WaspIPAddress);
                Progress.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { Progress.Background = Brushes.Green; label.Content = "Programming Complete"; }));
            }
            else
            {
                string statusCode = string.Format("Programming Failed: 0x{0:X}", status);
                Progress.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { Progress.Background = Brushes.OrangeRed; label.Content = statusCode; }));
            }
        }

        /// <summary>
        /// Example of updating a WASP-N over WiFi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOta_Click(object sender, RoutedEventArgs e)
        {
            Progress.Background = Brushes.Yellow;
            Progress.Value = 0;
            var progress = new Progress<ProgrammingProgress>(TesterProgrammingProgress);
            label.Content = "Programming...";
            _waspUT.UpdateFirmwareWithBundle(ManufacturingPackage.GetPackage("WASP-N 5.4.22-Firmware-R1.zip"), progress, new CompletionCallback(ProgrammingComplete), false, AccessMethod.WiFi);
        }

        /// <summary>
        /// Example of updating a WASP-N over USB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonUsb_Click(object sender, RoutedEventArgs e)
        {
            Progress.Background = Brushes.Yellow;
            Progress.Value = 0;
            var progress = new Progress<ProgrammingProgress>(TesterProgrammingProgress);
            label.Content = "Programming...";
            if (_waspUsb != null)
            {
                _waspUsb.GetUsbStream().CancelIo();
                _waspUsb.UpdateFirmwareWithBundle(ManufacturingPackage.GetPackage("WASP-N 5.4.17-Firmware-R1.zip"), progress, new CompletionCallback(ProgrammingComplete), false, AccessMethod.USB);
            }
        }

        /// <summary>
        /// Updates radio 4 to the selected type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRadio_Click(object sender, RoutedEventArgs e)
        {
            //Make sure the radio exists
            if (_waspUT.Radio4Type == RadioTypes.NotPopulated)
            {
                return;
            }
            Progress.Background = Brushes.Yellow;
            Progress.Value = 0;
            var progress = new Progress<ProgrammingProgress>(TesterProgrammingProgress);

            RadioFirmwareLoader rfl = new RadioFirmwareLoader(_waspUT);

            Bundle bundle = Bundle.GetBundle(_poeBundle);
            RadioFirmware bleFirmware = bundle.RadioFirmwares.FirstOrDefault(x => x.FirmwareType == FirmwareTypes.BLE_ACTIVE_SCAN);
            RadioFirmware antFirmware = bundle.RadioFirmwares.FirstOrDefault(x => x.FirmwareType == FirmwareTypes.ANT);
            RadioFirmware bcnFirmware = bundle.RadioFirmwares.FirstOrDefault(x => x.FirmwareType == FirmwareTypes.BEACON);
            RadioFirmware programmingFirmware = null;

            if (radioButtonAnt.IsChecked == true)
            {
                programmingFirmware = antFirmware;
            }
            else if (radioButtonBle.IsChecked == true)
            {
                programmingFirmware = bleFirmware;
            }
            else
            {
                programmingFirmware = bcnFirmware;
            }

            if (programmingFirmware == null)
            {
                MessageBox.Show("No firmware of that type in Bundle");
                return;
            }
            rfl.Program(1, programmingFirmware, progress, new CompletionCallback(ProgrammingComplete));
        }
    }
}
