using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NorthPoleEngineering.WaspClassLibrary;
using NorthPoleEngineering.WaspClassLibrary.FitnessEquipmentControls;

namespace BasicWasp
{
    /// <summary>
    /// This class receives a specific ANT object and formats it for display
    /// </summary>
    class DeviceFormatter
    {
        /// <summary>
        ///  List of known ANT/BLE devices
        /// </summary>
        readonly List<string> _knownDevices = new List<string> {
        "BikeCadenceSensor",
        "BikePowerSensor",
        "BikeSpeedAndCadenceSensor",
        "BikeSpeedSensor",
        "EnvironmentSensor",
        "FitnessEquipment",
        "Geocache",
        "HeartRateMonitor",
        "PairingPod",
        "PolarH7HeartRateMonitors",
        "SuuntoHeartRateMonitors",
        "SdmPod",
        "UnknownDevice",
        "WFKickr",
        "MuscleOxygenMonitor"
        };

        /// <summary>
        /// Returns a dictionary of data from the device passed in
        /// </summary>
        /// <param name="device">Device sending data</param>
        /// <param name="args">arguments provided</param>
        /// <returns></returns>
        public static List<string> FormatDevice(AntDevice device, EventArgs args)
        {
            try
            {
                object[] parameters = { device, args };
                return (List<string>)typeof(DeviceFormatter).InvokeMember(
                    "Format" + device.DeviceType.ToString(),
                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                    null,
                    null,
                    parameters);
            }
            catch(MissingMethodException )
            {
                return new List<string> { "Unknown device" ,  "Type: "+ device.DeviceType.ToString() };
            }
        }

        /// <summary>
        /// Returns a list of user-pertinent data from the heart rate monitor
        /// </summary>
        /// <param name="h">HeartRateMonitor device</param>
        /// <param name="a">Device arguments</param>
        /// <returns></returns>
        public static List<string> FormatHeartRateMonitor(object h, object a)
        {
            HeartRateMonitor hrm = h as HeartRateMonitor;
            List<string> data = new List<string>();
            data.Add("ID: " + hrm.ExtendedDeviceNumber.ToString());
            data.Add("Type: " + hrm.DeviceType.ToString());
            if (hrm.Manufacturer != null)
            {
                data.Add(string.Format("Mfr: {0}", hrm.Manufacturer.ManufacturerName));
                data.Add(string.Format("Model: {0}", hrm.Manufacturer.ModelNumber));
            }

            HeartRateMonitorMessageEventArgs args = a as HeartRateMonitorMessageEventArgs;
            if (args != null)
            {
                data.Add("Heartrate: "+ args.FormattedHeartRate(true));
            }

            return data;
        }

        /// <summary>
        /// Returns a list of user-pertinent data from the BikeSpeedSensor
        /// </summary>
        /// <param name="h">BikeSpeedSensor device</param>
        /// <param name="a">Device arguments</param>
        /// <returns></returns>
        public static List<string> FormatBikeSpeedSensor(object h, object a)
        {
            BikeSpeedSensor hrm = h as BikeSpeedSensor;
            List<string> data = new List<string>();
            data.Add("ID: " + hrm.ExtendedDeviceNumber.ToString());
            data.Add("Type: " + hrm.DeviceType.ToString());
            if (hrm.Manufacturer != null)
            {
                data.Add(string.Format("Mfr: {0}", hrm.Manufacturer.ManufacturerName));
                data.Add(string.Format("Model: {0}", hrm.Manufacturer.ModelNumber));
            }

            BikeSpeedMessageEventArgs args = a as BikeSpeedMessageEventArgs;
            if (args != null)
            {
                data.Add(string.Format("Speed: {0:F1}", args.InstantaneousSpeed));
                data.Add("Revs: " + args.AccumulatedRevolutionCount.ToString());
            }

            return data;
        }

        /// <summary>
        /// Returns a list of user-pertinent data from the BikeSpeedSensor
        /// </summary>
        /// <param name="h">BikeSpeedSensor device</param>
        /// <param name="a">Device arguments</param>
        /// <returns></returns>
        public static List<string> FormatBikePowerSensor(object h, object a)
        {
            BikePowerSensor bp = h as BikePowerSensor;
            List<string> data = new List<string>();
            data.Add("ID: " + bp.ExtendedDeviceNumber.ToString());
            data.Add("Type: " + bp.DeviceType.ToString());
            if (bp.Manufacturer != null)
            {
                data.Add(string.Format("Mfr: {0}", bp.Manufacturer.ManufacturerName));
                data.Add(string.Format("Model: {0}", bp.Manufacturer.ModelNumber));
            }

            PowerOnlyEventArgs args = a as PowerOnlyEventArgs;
            if (args != null)
            {
                data.Add(string.Format("Cadence: {0:F1}", args.InstantaneousCadence));
                data.Add(string.Format("Avg Power: {0:F1}", args.AveragePower));
                data.Add(string.Format("Pedal Power: {0:F1}", args.PedalPower));
                data.Add(string.Format("Accum Power: {0:F1}", args.AccumulatedPower));
            }

            return data;
        }

        /// <summary>
        /// Returns a list of user-pertinent data from the BikeSpeedSensor
        /// </summary>
        /// <param name="h">BikeSpeedSensor device</param>
        /// <param name="a">Device arguments</param>
        /// <returns></returns>
        public static List<string> FormatMuscleOxygenMonitor(object h, object a)
        {
            MuscleOxygenMonitor moxy = h as MuscleOxygenMonitor;
            List<string> data = new List<string>();
            data.Add("ID: " + moxy.ExtendedDeviceNumber.ToString());
            data.Add("Type: " + moxy.DeviceType.ToString());
            if (moxy.Manufacturer != null)
            {
                data.Add(string.Format("Mfr: {0}", moxy.Manufacturer.ManufacturerName));
                data.Add(string.Format("Model: {0}", moxy.Manufacturer.ModelNumber));
            }

            data.Add(string.Format("THb: {0}", moxy.TotalHemoglobinConcentration));
            data.Add(string.Format("Current SMO2: {0}%", moxy.CurrentSaturatedHemoglobin));
            data.Add(string.Format("Previous SMO2: {0}%", moxy.PreviousSaturatedHemoglobin));
            string tooHigh = moxy.AmbientLightTooHigh ? "too high" : "OK";
            data.Add(string.Format("Ambient light: {0}", tooHigh));
            if (moxy.Battery != null) 
            {
                data.Add(string.Format("Battery: {0:0.0}V", moxy.Battery.Voltage));
            }

            return data;
        }

        /// <summary>
        /// Returns a list of user-pertinent data from the FitnessEquipment
        /// </summary>
        /// <param name="h">FitnessEquipment device</param>
        /// <param name="a">Device arguments</param>
        /// <returns></returns>
        public static List<string> FormatFitnessEquipment(object h, object a)
        {
            FitnessEquipment fe = h as FitnessEquipment;
            List<string> data = new List<string>();
            data.Add("ID: " + fe.ExtendedDeviceNumber.ToString());
            data.Add("Type: " + fe.DeviceType.ToString());
            if (fe.Manufacturer != null)
            {
                data.Add(string.Format("Mfr: {0}", fe.Manufacturer.ManufacturerName));
                data.Add(string.Format("Model: {0}", fe.Manufacturer.ModelNumber));
            }

            TrainerMessageEventArgs tr = a as TrainerMessageEventArgs;
            if (tr != null)
            {
                data.Add(string.Format("Cadence: {0}", tr.InstantaneousCadence));
                data.Add(string.Format("Power: {0}", tr.AccumulatedPower));
            }
            else
            {
                RowerMessageEventArgs r = a as RowerMessageEventArgs;
                if (r != null)
                {
                    data.Add(string.Format("Cadence: {0}", r.InstantaneousCadence));
                    data.Add(string.Format("Strokes: {0}", r.AccumulatedStrokeCount));
                }
            }
            return data;
        }

        /// <summary>
        /// Display data for unknown ANT sensor types
        /// </summary>
        /// <param name="j"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static List<string> FormatUnknownSensor(object j, object a)
        {
            return new List<string> { "Unknown device", "Type: Unknown"};
        }

    }
}
