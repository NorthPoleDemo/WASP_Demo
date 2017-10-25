using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NorthPoleEngineering.WaspClassLibrary;
using NorthPoleEngineering.WaspClassLibrary.FitnessEquipmentControls;

namespace ControlAntRadio
{
    public partial class MainDialog : Form
    {
        WaspCollection _wasps;
        AntPlusDevices _sensors;
        FitnessEquipment _fitnessEquipment;

        public MainDialog()
        {
            InitializeComponent();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (checkBoxUsb.Checked)
            {
                _wasps = new WaspCollection(Handle);
            }
            else
            {
                _wasps = new WaspCollection();
            }
            _wasps.CollectionChanged += Wasps_CollectionChanged;
            // connect to ANT device notifications
            _sensors = new AntPlusDevices(_wasps);
            _sensors.AntPlusDeviceEvent += NewSensors_AntPlusDeviceEvent; ;

            buttonStart.Enabled = false;
        }

        private void NewSensors_AntPlusDeviceEvent(object sender, AntPlusDeviceEventArgs e)
        {
            ListViewItem item = new ListViewItem(e.DeviceType.ToString());
            item.SubItems.Add(e.ExtendedDeviceNumber.ToString());
            //item.SubItems.Add(e.DeviceNumber.ToString());
            //item.SubItems.Add(e.Device.RSSI.ToString());
            item.Tag = e.Device;
            listViewSensors.Items.Add(item);
        }

        private void Wasps_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
           /* if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object o in e.NewItems)
                {
                    listBox1.Items.Add((Wasp)o);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object o in e.NewItems)
                {
                    listBox1.Items.Remove((Wasp)o);
                }
            }*/
        }

        /// <summary>
        /// Called when the connection completes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wasp_ConnectionEvent(object sender, ConnectionEventArgs e)
        {
            if (e.Connection == NorthPoleEngineering.WaspClassLibrary.ConnectionState.Connected)
            {
                Wasp wasp = sender as Wasp;

                // check if not WASP-B
                if (wasp.ProductType != ProductTypes.WASP_B)
                {
                    // create a list of ANT radios we want to control
                    List<object> radios = new List<object>();
                    RadioTypes[] radioTypes = { wasp.Radio1Type, wasp.Radio2Type, wasp.Radio3Type, wasp.Radio4Type };

                    for (int i = 0; i < radioTypes.Length; i++)
                    {
                        if (radioTypes[i] == RadioTypes.ANT)
                        {
                            // order is radio index, radio mode
                            radios.AddRange(new object[] { i, Wasp.AntRadioModes.Controlled });

                            // connect to the radio reset event
                            AntRadio antRadio = wasp.GetAntRadio(i);
                            antRadio.ResetEvent += Radio_ResetEvent; ;
                            antRadio.Retries = 0;
                        }
                    }

                    if (radios.Count > 0)
                    {
                        // this will reset the radios
                        wasp.SetAntRadioMode(radios.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Called when a radio is reset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Radio_ResetEvent(object sender, EventArgs e)
        {
            AntRadio antRadio = sender as AntRadio;

            // disconnect from the reset event
            antRadio.ResetEvent -= Radio_ResetEvent;

            // the status event will occur when radio initialization is complete
            antRadio.StatusEvent += AntRadio_StatusEvent;

            // initialze the ANT radio for continuous scanning mode on channel 57
            antRadio.ConfigureContinuousScanningMode(AntDeviceType.UnknownSensor, 57);
        }

        private void AntRadio_StatusEvent(object sender, AntRadio.ChannelStatus e)
        {
            AntRadio antRadio = sender as AntRadio;
            // disconnect from the status event
            antRadio.StatusEvent -= AntRadio_StatusEvent;

            if (_fitnessEquipment != null)
            {
                SetBasicResistance();
            }
        }

        /// <summary>
        /// Called when a sensor is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewSensors_DoubleClick(object sender, EventArgs e)
        {
            AntDevice antDevice = (AntDevice)listViewSensors.SelectedItems[0].Tag;
            _fitnessEquipment = antDevice as FitnessEquipment;

            Wasp wasp = antDevice.GetStrongestWasp();
            wasp.ConnectionEvent += Wasp_ConnectionEvent; ;
            wasp.RequestConnection();
        }

        /// <summary>
        /// Sets the resistance of a Fitness Equipment Trainer
        /// </summary>
        private void SetBasicResistance()
        {
            if (_fitnessEquipment != null)
            {
                _fitnessEquipment.SetBasicResistance(10);
            }
        }
    }
}
