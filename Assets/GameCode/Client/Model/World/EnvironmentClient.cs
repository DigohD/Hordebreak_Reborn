using FNZ.Shared.Model.World;

namespace FNZ.Client.Model.World
{

	public class EnvironmentClient : EnvironmentShared
	{
		public byte SecondsPerHour
		{
			get
			{
				return SECONDS_PER_HOUR;
			}
		}
		public byte Hour
		{
			get
			{
				return m_Hour;
			}
		}
	}
}