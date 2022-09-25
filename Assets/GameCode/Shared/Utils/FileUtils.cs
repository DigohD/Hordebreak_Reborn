using System.IO;
using UnityEngine.Profiling;

namespace FNZ.Shared.Utils
{
	public class FileUtils
	{
		// DO NOT CHANGE THESE ENUM NAMES!
		public enum FNS_File_Version_Code
		{
			FNS_FILE_VERSION_1 = 11111
		};

		public static byte[] ReadFile(string filePath)
		{
			return FNEUtil.Decompress(File.ReadAllBytes(filePath));
		}

		public static void WriteFile(string filePath, byte[] data)
		{
			Profiler.BeginSample("Write File");
			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				using (var writer = new BinaryWriter(stream))
				{
					var compressedData = FNEUtil.Compress(data);
					writer.Write(compressedData, 0, compressedData.Length);
				}
			}
			Profiler.EndSample();
		}
	}
}