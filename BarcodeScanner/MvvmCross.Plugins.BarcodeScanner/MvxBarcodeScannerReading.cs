// (c) Copyright Cirrious. http://www.cirrious.com
// This source is subject to the Microsoft Public License (Ms-PL)
// Please see license.txt on http://opensource.org/licenses/ms-pl.html
// All other rights reserved.
// </copyright>
//
// Project Lead - Stuart Lodge, Cirrious. http://www.cirrious.com - Hire me - I'm worth it!
// Implemented - Michael Bogaerts, 3Way. http://www.3Way.be - I'm worth it 2 ;-)!

namespace MvvmCross.Plugins.BarcodeScanner {
    public class MvxBarcodeScannerReading {
        public MvxBarcodeScannerReading(uint scanDataType, string rawData, string barcode, string symbology) {
            ScanDataType = scanDataType;
            RawData = rawData;
            Barcode = barcode;
            Symbology = symbology;
        }
        public string Barcode { get; private set; }
        public string RawData { get; private set; }
        public uint ScanDataType { get; private set; }
        public string Symbology { get; private set; }
    }
}