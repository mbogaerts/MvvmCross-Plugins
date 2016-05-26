// (c) Copyright Cirrious. http://www.cirrious.com
// This source is subject to the Microsoft Public License (Ms-PL)
// Please see license.txt on http://opensource.org/licenses/ms-pl.html
// All other rights reserved.
// </copyright>
//
// Project Lead - Stuart Lodge, Cirrious. http://www.cirrious.com - Hire me - I'm worth it!
// Implemented - Michael Bogaerts, 3Way. http://www.3Way.be - I'm worth it 2 ;-)!

using MvvmCross.Platform;
using MvvmCross.Platform.Plugins;

namespace MvvmCross.Plugins.BarcodeScanner {
    public class PluginLoader
        : IMvxPluginLoader
    {
        public static readonly PluginLoader Instance = new PluginLoader();

        public void EnsureLoaded()
        {
            var manager = Mvx.Resolve<IMvxPluginManager>();
            manager.EnsurePlatformAdaptionLoaded<PluginLoader>();
        }
    }
}