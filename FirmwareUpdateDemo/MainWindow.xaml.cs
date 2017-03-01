using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NorthPoleEngineering.WaspClassLibrary.Programming;
using NorthPoleEngineering.WaspClassLibrary;

namespace FirmwareUpdateDemo
{
    /// <summary>
    /// This example shows how to update the firmware on a WASP-PoE over Ethernet or a WASP-N over WiFi.
    /// 
    /// The devices used in this example are given names of WASP-PoE_Test221 and WASP-N_Test221. You may substitute the
    /// names of your own devices in the  _deviceNames array below.
    /// 
    /// The firmware versions included are PoE 2.2.37 and N 5.4.17. 
    /// </summary>
    public partial class MainWindow : Window
    {
        WaspCollection _wasps;
        Wasp _waspUT;
        EventWaitHandle _waspReady;

        /// <summary>
        /// Name of the WASP to be used for this test
        /// </summary>
        string[] _deviceNames = { "WASP-PoE_Test221", "WASP-N_Test221" };

        public MainWindow()
        {
            InitializeComponent();
            buttonPoE.IsEnabled = false;
            buttonOta.IsEnabled = false;
            _wasps = new WaspCollection();
            _wasps.CollectionVerbosity = Wasp.WaspLogLevel.None;
            _wasps.CollectionChanged += Wasps_CollectionChanged;
            _waspReady = new EventWaitHandle(false, EventResetMode.AutoReset);
            label.Content = "";
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
            _waspUT.UpdateFirmware(Bundle.GetBundle("WASP-PoE 2.2.37-Firmware-R1.zip"), progress, new CompletionCallback(ProgrammingComplete), AccessMethod.Ethernet);
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
                    if (((Wasp)e.NewItems[i]).Name == _deviceNames[0] )
                    {
                        _waspUT = (Wasp)e.NewItems[i];
                        _waspReady.Set();
                        buttonPoE.IsEnabled = true;
                        break;
                    }
                    else if (((Wasp)e.NewItems[i]).Name == _deviceNames[1])
                    {
                        _waspUT = (Wasp)e.NewItems[i];
                        _waspReady.Set();
                        buttonOta.IsEnabled = true;
                        break;
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
            _waspUT.UpdateFirmwareWithBundle(ManufacturingPackage.GetPackage("WASP-N 5.4.17-Firmware-R1.zip"), progress, new CompletionCallback(ProgrammingComplete), false, AccessMethod.WiFi);
        }
    }
}
