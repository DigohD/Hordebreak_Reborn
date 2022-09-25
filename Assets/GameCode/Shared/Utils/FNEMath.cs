using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Shared.Utils 
{

	public static class FNEMath
	{
		// https://math.stackexchange.com/questions/462562/need-a-formula-for-curve
		public static float ExpoCurve(float maxValue, float expo, float x)
		{
			var result = x / maxValue;
			result = Mathf.Pow(result, expo);
			result = result * maxValue;
			return result;
		}
	}
}