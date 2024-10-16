using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XrSystemTrackingProperties
	{
		private bool _orientationTracking;
		private bool _positionTracking;

		public bool OrientationTracking => _orientationTracking;
		public bool PositionTracking => _positionTracking;
	}
}
