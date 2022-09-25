using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Shared.Utils
{
	public struct FNETransform
	{
		public float posX;
		public float posY;
		public float posZ;

		public float rotX;
		public float rotY;
		public float rotZ;

		public float scaleX;
		public float scaleY;
		public float scaleZ;

		public string entityId;
	}

	public static class FNEUtil
	{
		public static float2 HalfFloat2 = new float2(0.5f, 0.5f);

		public static IEnumerable<Type> GetAllTypesOf<T>(string assemblyName)
		{
			List<Type> defList = new List<Type>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains(assemblyName));
			foreach (var a in assemblies)
			{
				var defs = a.GetTypes().Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract);
				foreach (var def in defs)
				{
					defList.Add(def);
				}
			}
			return defList;
		}

		public static byte[] Compress(byte[] data)
		{
			MemoryStream output = new MemoryStream();

			using (DeflateStream dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
			{
				dstream.Write(data, 0, data.Length);
			}

			return output.ToArray();
		}

		public static byte[] Decompress(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			MemoryStream output = new MemoryStream();

			using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
			{
				dstream.CopyTo(output);
			}

			return output.ToArray();
		}

		public static float ConvertAngleToBetween0And360(float angle)
		{
			angle = angle % 360;
			if (angle < 0)
				angle += 360;
			return angle;
		}

		public static ushort PackFloatAsShort(float value)
		{
			ushort x = (ushort)value;
			x *= 100;
			x += (ushort)((value - (ushort)value) * 100);
			return x;
		}

		public static short ConvertFloatToSignedShort(float value)
		{
			short x = (short)value;
			x *= 100;
			x += (short)((value - (short)value) * 100);
			return x;
		}

		public static float UnpackShortToFloat(ushort value)
		{
			float x = (value % 1000) / 100.0f;
			x += (value - (value % 1000)) / 100;
			return x;
		}

		public static float ConvertSignedShortToFloat(short value)
		{
			float x = (value % 1000) / 100.0f;
			x += (value - (value % 1000)) / 100;
			return x;
		}

		public static byte[] ReadAllBytes(BinaryReader reader)
		{
			const int bufferSize = 4096;

			using (var ms = new MemoryStream())
			{
				byte[] buffer = new byte[bufferSize];
				int count;
				while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
					ms.Write(buffer, 0, count);
				return ms.ToArray();
			}
		}

		/// <summary>
		/// Finds the MAC address of the NIC with maximum speed.
		/// </summary>
		/// <returns>The MAC address.</returns>
		public static string GetMacAddress()
		{
			const int MIN_MAC_ADDR_LENGTH = 12;
			string macAddress = string.Empty;
			long maxSpeed = -1;

			foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
			{
				UnityEngine.Debug.Log(
					"Found MAC Address: " + nic.GetPhysicalAddress() +
					" Type: " + nic.NetworkInterfaceType);

				string tempMac = nic.GetPhysicalAddress().ToString();

				if (nic.Speed > maxSpeed &&
					!string.IsNullOrEmpty(tempMac) &&
					tempMac.Length >= MIN_MAC_ADDR_LENGTH)
				{
					UnityEngine.Debug.Log("New Max Speed = " + nic.Speed + ", MAC: " + tempMac);
					maxSpeed = nic.Speed;
					macAddress = tempMac;
				}
			}

			return macAddress;
		}

		public static long NanoTime()
		{
			long nano = 10000L * Stopwatch.GetTimestamp();
			nano /= System.TimeSpan.TicksPerMillisecond;
			nano *= 100L;
			return nano;
		}

		public static Color32 ConvertHexStringToColor32(string color32)
		{
			byte r = byte.Parse(color32.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(color32.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(color32.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

			return new Color32(r, g, b, 255);
		}
		public static Color ConvertHexStringToColor(string color, float alpha = 1)
		{
			int r = int.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
			int g = int.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
			int b = int.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

			return new Color(r / 255f, g / 255f, b / 255f, alpha);
		}

		public static void ShuffleList<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = FNERandom.GetRandomIntInRange(0, n);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static float ScaleValueFloat(float value, float min, float max, float scaledMin, float scaledMax)
		{
			value = value <= min ? min : value;
			value = value > max ? max : value;

			return scaledMin + (value - min) / (max - min) * (scaledMax - scaledMin);
		}

		public static Color HexStringToColor(string hexColor)
		{
			string hc = ExtractHexDigits(hexColor);
			if (hc.Length != 6)
			{
				return Color.black;
			}
			string r = hc.Substring(0, 2);
			string g = hc.Substring(2, 2);
			string b = hc.Substring(4, 2);
			Color color = Color.black;
			try
			{
				int ri
				   = Int32.Parse(r, System.Globalization.NumberStyles.HexNumber);
				int gi
				   = Int32.Parse(g, System.Globalization.NumberStyles.HexNumber);
				int bi
				   = Int32.Parse(b, System.Globalization.NumberStyles.HexNumber);

				color = new Color((float)ri / 255f, (float)gi / 255f, (float)bi / 255f);
			}
			catch
			{
				return Color.black;
			}
			return color;
		}

		public static string ExtractHexDigits(string input)
		{
			// remove any characters that are not digits (like #)
			Regex isHexDigit
			   = new Regex("[abcdefABCDEF\\d]+", RegexOptions.Compiled);
			string newnum = "";
			foreach (char c in input)
			{
				if (isHexDigit.IsMatch(c.ToString()))
					newnum += c.ToString();
			}
			return newnum;
		}
	}
}