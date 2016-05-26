### BarcodeScanner

The `BarcodeScanner` plugin provides access to a platforms BarcodeScanner using a singleton implementing the API:

    public interface IMvxBarcodeScanner
    {
        void Start();
        void Stop();
        bool Started { get; }
        MvxBarcodeScannerReading LastReading { get; }
        event EventHandler<MvxValueEventArgs<MvxBarcodeScannerReading>> ReadingAvailable;
    }
    
This plugin for now only available WindowsStore.

Please note that this implementation is 