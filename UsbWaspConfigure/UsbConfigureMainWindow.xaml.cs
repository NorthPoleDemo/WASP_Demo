////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  UsbConfigureMainWindow.xaml.cs
//
//  Copyright © 2012-2016 by North Pole Engineering, Inc.  All rights reserved.
//
//  Printed in the United States of America.  Except as permitted under the United States
//  Copyright Act of 1976, no part of this software may be reproduced or distributed in
//  any form or by any means, without the prior written permission of North Pole
//  Engineering, Inc., unless such copying is expressly permitted by federal copyright law.
//
//  Address copying inquiries to:
//  North Pole Engineering, Inc.
//  Joe Tretter
//  221 North First St. Ste. 310
//  Minneapolis, Minnesota 55401
//
//  Information contained in this software has been created or obtained by North Pole Engineering,
//  Inc. from sources believed to be reliable.  However, North Pole Engineering, Inc. does not
//  guarantee the accuracy or completeness of the information published herein nor shall
//  North Pole Engineering, Inc. be liable for any errors, omissions, or damages arising
//  from the use of this software.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Interop;
using NorthPoleEngineering.WaspClassLibrary;

namespace UsbWaspConfigure
{
    /// <summary>
    /// This program demonstrates how to update the WASP configuration over a USB connection
    /// and use that to change the SSID and passphrase
    /// </summary>
    public partial class UsbConfigureMainWindow : Window
    {
        WaspCollection _wasps;
        WaspConfiguration _config;
        Wasp _thisWasp;
        public UsbConfigureMainWindow()
        {
            InitializeComponent();

            WindowInteropHelper w = new WindowInteropHelper(this);
            _wasps = new WaspCollection(w.Handle);
            // Register for notification of a new WASP added or removed
            _wasps.CollectionChanged += WASP_CollectionChangedEvent;
        }

        /// <summary>
        /// WASP Collection Changed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>WASP comes up, get a connection to it</remarks>
        private void WASP_CollectionChangedEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                _thisWasp = e.NewItems[0] as Wasp;
                Title += " connected to " + _thisWasp.Name;
                _thisWasp.ConnectionEvent += WaspConnected;
                _thisWasp.RequestConnection();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                Title = "Configure WASP-N";
                Dispatcher.Invoke(new Action(() => { buttonConf.IsEnabled = false; }));
            }
        }

        /// <summary>
        /// Handler for the connection event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaspConnected(object sender, ConnectionEventArgs e)
        {
            if (e.Connection == ConnectionState.Connected)
            {
                _config = e.Configuration;
                Dispatcher.Invoke(new Action(() => { buttonConf.IsEnabled = true; }));                
            }
        }

        /// <summary>
        /// Handler for Configuration button.
        /// Updates SSID & passphrase
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConf_Click(object sender, RoutedEventArgs e)
        {
            if (_config != null)
            {
                _config.SSIDName = textBoxSsid.Text;
                _config.SecurityPassphrase = passwordBox.Password;
                _thisWasp.SetConfiguration(_config, true);
                MessageBox.Show("WASP Configuration updated");
            }
        }
    }
}
