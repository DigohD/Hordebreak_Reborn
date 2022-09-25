
namespace FNZ.Shared.Constants
{
	public class HordeEntityPacketHelperConstants
	{
		public static readonly int s_MaxBytesPerPacket = 1388;
		public static readonly int s_NetIdBits = 17;
		public static readonly int s_ChunkIndexBits = 8;
		public static readonly int s_PositionIntegerBits = 5;
		public static readonly int s_PositionDecimalBits = 3;

		public static readonly int bitsPerEnemy = s_NetIdBits
									   + (2 * s_ChunkIndexBits)
									   + (2 * s_PositionIntegerBits)
									   + (2 * s_PositionDecimalBits);

		public static readonly int NumberOfEnemiesPerPacket = (s_MaxBytesPerPacket * 8) / bitsPerEnemy;
	}
}