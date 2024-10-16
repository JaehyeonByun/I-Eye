using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrSystemProperties
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _systemId;
        private uint _vendorId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] private string systemName;
        private XrSystemGraphicsProperties _graphicsProperties;
        private XrSystemTrackingProperties _trackingProperties;

        internal XrSystemProperties(ulong systemId)
        {
            _type = XrStructureType.XR_TYPE_SYSTEM_PROPERTIES;
            _next = IntPtr.Zero;
            _systemId = systemId;
            _vendorId = 0;
            systemName = string.Empty;
            _graphicsProperties = new XrSystemGraphicsProperties();
            _trackingProperties = new XrSystemTrackingProperties();
        }

        public XrSystemGraphicsProperties GetGraphicsProperties()
        {
            return _graphicsProperties;
        }

        public XrSystemTrackingProperties GetTrackingProperties()
        {
            return _trackingProperties;
        }
    }
}
