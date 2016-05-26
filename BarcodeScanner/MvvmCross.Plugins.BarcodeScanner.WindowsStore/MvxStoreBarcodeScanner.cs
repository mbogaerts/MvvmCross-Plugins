// (c) Copyright Cirrious. http://www.cirrious.com
// This source is subject to the Microsoft Public License (Ms-PL)
// Please see license.txt on http://opensource.org/licenses/ms-pl.html
// All other rights reserved.
// </copyright>
//
// Project Lead - Stuart Lodge, Cirrious. http://www.cirrious.com - Hire me - I'm worth it!
// Implemented - Michael Bogaerts, 3Way. http://www.3Way.be - I'm worth it 2 ;-)!

using MvvmCross.Platform.Core;
using MvvmCross.Platform.Exceptions;
using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using MvvmCross.Platform;

namespace MvvmCross.Plugins.BarcodeScanner.WindowsStore
{
    public class MvxStoreBarcodeScanner : IMvxBarcodeScanner {
        private MvxBarcodeScannerReading _lastReading;

        private bool _started;
        private Windows.Devices.PointOfService.BarcodeScanner _barcodeScanner;
        private Windows.Devices.PointOfService.ClaimedBarcodeScanner _claimedBarcodeScanner;

        public async void Start() {
            if (_started) {
                throw new MvxException("Barcodescanner already started");
            }

            if (await CreateScanner()) {
                if (await ClaimScanner()) {
                    _started = true;
                    _claimedBarcodeScanner.ReleaseDeviceRequested += _claimedBarcodeScanner_ReleaseDeviceRequested;
                    _claimedBarcodeScanner.DataReceived += _claimedBarcodeScanner_DataReceived;
                    _claimedBarcodeScanner.IsDecodeDataEnabled = true;
                    if (await EnableScanner()) {
                        Mvx.Trace("Scanner Enabled");
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
            var symbology = Windows.Devices.PointOfService.BarcodeSymbologies.GetName(scanDataType);
            _lastReading = new MvxBarcodeScannerReading(scanDataType, rawData, barcode, symbology);
            ReadingAvailable?.Invoke(this, new MvxValueEventArgs<MvxBarcodeScannerReading>(_lastReading));
        }

        private void _claimedBarcodeScanner_ReleaseDeviceRequested(object sender, Windows.Devices.PointOfService.ClaimedBarcodeScanner e) {
            e.RetainDevice();
        }

        public void Stop() {
            _started = false;
            ReleaseScanner();
        }

        public MvxBarcodeScannerReading LastReading => _lastReading;

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
                //Scanner was allready claimed
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
            if (_claimedBarcodeScanner == null) {
                return false;
            } else {
                await _claimedBarcodeScanner.EnableAsync();
                return true;
            }
        }

        public void ReleaseScanner() {
            if (_claimedBarcodeScanner != null) {
                _claimedBarcodeScanner.DataReceived -= _claimedBarcodeScanner_DataReceived;
                _claimedBarcodeScanner.ReleaseDeviceRequested -= _claimedBarcodeScanner_ReleaseDeviceRequested;
                _claimedBarcodeScanner.Dispose();
                _claimedBarcodeScanner = null;
            }
            _barcodeScanner = null;
        }
    }
}