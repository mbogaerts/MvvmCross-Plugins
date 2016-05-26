// <copyright file="MvxStoreAccelerometer.cs" company="Cirrious">
// (c) Copyright Cirrious. http://www.cirrious.com
// This source is subject to the Microsoft Public License (Ms-PL)
// Please see license.txt on http://opensource.org/licenses/ms-pl.html
// All other rights reserved.
// </copyright>
//
// Project Lead - Stuart Lodge, Cirrious. http://www.cirrious.com - Hire me - I'm worth it!

using System;
using MvvmCross.Platform.Core;
using MvvmCross.Platform.Exceptions;
using Windows.Devices.Sensors;
//using Windows.Devices.PointOfService = POS;


namespace MvvmCross.Plugins.BarcodeScanner.WindowsStore {
    public class MvxBarcodeScanner : IMvxBarcodeScanner {

        private MvxBarcodeScannerReading _lastReading;

        private bool _started;
        private Windows.Devices.PointOfService.BarcodeScanner _barcodeScanner;
        private Windows.Devices.PointOfService.ClaimedBarcodeScanner _claimedBarcodeScanner;

        public async void Start() {
            if (_started) {
                throw new MvxException("Barcodescanner already started");
            }

            if (await CreateScanner()) {
                Log.Trace("BarcodeScanner Registration Successfull");
                if (await ClaimScanner()) {
                    Log.Trace("BarcodeScanner Claimed Successfull");
                    _started = true;
                    // Make sure the scanner remains claimed for our app
                    _claimedBarcodeScanner.ReleaseDeviceRequested += _claimedBarcodeScanner_ReleaseDeviceRequested;
                    _claimedBarcodeScanner.DataReceived += _claimedBarcodeScanner_DataReceived;
                    _claimedBarcodeScanner.IsDecodeDataEnabled = true;
                    if (await EnableScanner()) {
                        Log.Trace("Barcode Scanner ready to Scan");
                    }
                }
            }
        }

        private void _claimedBarcodeScanner_DataReceived(Windows.Devices.PointOfService.ClaimedBarcodeScanner sender, Windows.Devices.PointOfService.BarcodeScannerDataReceivedEventArgs args) {
            var rawdata = Windows.Storage.Streams.DataReader.FromBuffer(args.Report.ScanData);
            var barcodedata = Windows.Storage.Streams.DataReader.FromBuffer(args.Report.ScanDataLabel);

            // Remove the checksum
            string strippedbc = barcodedata.ReadString(args.Report.ScanDataLabel.Length);
            strippedbc = strippedbc.Remove(strippedbc.Length - 2);
            // Trim the end
            strippedbc = strippedbc.TrimEnd();

            OnBarcodeScanned(args.Report.ScanDataType, rawdata.ReadString(args.Report.ScanData.Length), strippedbc);
        }


        protected virtual void OnBarcodeScanned(uint scanDataType, string rawData, string barcode) {
            Log.MethodEnter();
            Log.Debug($"scanDataType {scanDataType}");
            Log.Debug($"rawData {rawData}");
            Log.Debug($"barcode {barcode}");
            ReadingAvailable?.Invoke(this, new MvxValueEventArgs<MvxBarcodeScannerReading>(new MvxBarcodeScannerReading(scanDataType, rawData, barcode)));
            Log.MethodExit();
        }

        private void _claimedBarcodeScanner_ReleaseDeviceRequested(object sender, Windows.Devices.PointOfService.ClaimedBarcodeScanner e) {
            e.RetainDevice();
            Log.Trace("Event ReleaseDeviceRequest received. Retaining the Barcode Scanner.");

        }

        public void Stop() {
            _started = false;
            ReleaseScanner();
        }

        public MvxBarcodeScannerReading LastReading
        {
            get { return _lastReading; }
        }

        public event EventHandler<MvxValueEventArgs<MvxBarcodeScannerReading>> ReadingAvailable;

        /// <summary>
        /// Create a barcodescanner object
        /// </summary>
        /// <returns>true if system has a barcodescanner</returns>
        public async Task<bool> CreateScanner() {
            _barcodeScanner = await Windows.Devices.PointOfService.BarcodeScanner.GetDefaultAsync();

            if (_barcodeScanner == null) {
                DeviceInformationCollection col = await DeviceInformation.FindAllAsync(Windows.Devices.PointOfService.BarcodeScanner.GetDeviceSelector());
                if (col.Count > 0) {
                    _barcodeScanner = await Windows.Devices.PointOfService.BarcodeScanner.FromIdAsync(col[0].Id);
                }
            }

            return _barcodeScanner != null;
        }
        /// <summary>
        /// Claim the barcode scanner
        /// </summary>
        /// <returns>true if claimed</returns>
        private async Task<bool> ClaimScanner() {
            if (_claimedBarcodeScanner != null) {
                Log.Trace("Scanner was allready claimed");
                return true;

            } else {
                // claim the barcode scanner
                _claimedBarcodeScanner = await _barcodeScanner.ClaimScannerAsync();
                // enable the claimed barcode scanner
                if (_claimedBarcodeScanner == null) {
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> EnableScanner() {
            // enable the claimed barcode scanner
            if (_claimedBarcodeScanner == null) {
                return false;
            } else {
                await _claimedBarcodeScanner.EnableAsync();

                Log.Trace("Enable Barcode Scanner succeeded.");

                return true;
            }

        }

        public void ReleaseScanner() {
            if (_claimedBarcodeScanner != null) {
                // Detach the event handlers
                _claimedBarcodeScanner.DataReceived -= _claimedBarcodeScanner_DataReceived;
                _claimedBarcodeScanner.ReleaseDeviceRequested -= _claimedBarcodeScanner_ReleaseDeviceRequested;
                // Release the Barcode Scanner and set to null
                _claimedBarcodeScanner.Dispose();
                _claimedBarcodeScanner = null;
            }
            _barcodeScanner = null;
        }
    }
}

namespace MvvmCross.Plugins.Accelerometer.WindowsCommon
{
    public class MvxWindowsCommonBarcodeScanner : IMvxAccelerometer
    {
        private bool _started;
        private Windows.Devices.Sensors.Accelerometer _accelerometer;

        public void Start()
        {
            if (_started)
            {
                throw new MvxException("Accelerometer already started");
            }
            _accelerometer = Windows.Devices.Sensors.Accelerometer.GetDefault();
            if (_accelerometer != null)
            {
                _accelerometer.ReadingChanged += AccelerometerOnReadingChanged;
            }
            _started = true;
        }

        public void Stop()
        {
            if (!_started)
            {
                throw new MvxException("Accelerometer not started");
            }
            if (_accelerometer != null)
            {
                _accelerometer.ReadingChanged -= AccelerometerOnReadingChanged;
                _accelerometer = null;
            }
            _started = false;
        }

        private void AccelerometerOnReadingChanged(Windows.Devices.Sensors.Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            var handler = ReadingAvailable;

            if (handler == null)
                return;

            var reading = ToReading(args.Reading);

            handler(this, new MvxValueEventArgs<MvxAccelerometerReading>(reading));
        }

        private static MvxAccelerometerReading ToReading(AccelerometerReading sensorReading)
        {
            var reading = new MvxAccelerometerReading
            {
                X = sensorReading.AccelerationX,
                Y = sensorReading.AccelerationY,
                Z = sensorReading.AccelerationZ
            };
            return reading;
        }

        public bool Started => _accelerometer != null;

        public MvxAccelerometerReading LastReading
        {
            get
            {
                try
                {
                    var reading = ToReading(_accelerometer.GetCurrentReading());
                    return reading;
                }
                catch (Exception exception)
                {
                    throw exception.MvxWrap("Problem getting current Accelerometer reading");
                }
            }
        }

        public event EventHandler<MvxValueEventArgs<MvxAccelerometerReading>> ReadingAvailable;
    }
}