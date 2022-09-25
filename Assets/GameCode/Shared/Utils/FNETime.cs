using System.Diagnostics;

namespace FNZ.Shared.Utils
{
	public static class FNETime
	{
		public static long NanoTime()
		{
			long nano = 10000L * Stopwatch.GetTimestamp();
			nano /= System.TimeSpan.TicksPerMillisecond;
			nano *= 100L;
			return nano;
		}
	}
}

