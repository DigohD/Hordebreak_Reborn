using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FNZ.Shared.Model
{

	public class XMLValidation
	{
		public static void ValidateId<T>(
			List<Tuple<string, string>> errorMessages,
			string IdToTest,
			bool nullValueAcceptable,
			string fileName,
			string dataDefId,
			string IdXMLLabel,
			string compName = "def"
		) where T : DataDef
		{
			if (!nullValueAcceptable && string.IsNullOrEmpty(IdToTest))
				errorMessages.Add(
					new Tuple<string, string>(
						$"Error in '{fileName}'",
						$"{IdXMLLabel} '{IdToTest}' in {compName} of '{dataDefId}' must be specified."
					)
				);
			else if (!string.IsNullOrEmpty(IdToTest) && !DataBank.Instance.DoesIdExist<T>(IdToTest))
			{
				errorMessages.Add(
					new Tuple<string, string>(
						$"Error in '{fileName}'",
						$"{IdXMLLabel} '{IdToTest}' in {compName} of '{dataDefId}' was not found in database."
					)
				);
			}
		}

		public static void ValidateIdList<T>(
			List<Tuple<string, string>> errorMessages,
			List<string> Ids,
			bool nullOrEmptyAcceptable,
			string fileName,
			string dataDefId,
			string IdXMLLabel,
			string compName = "def"
		) where T : DataDef
		{
			foreach (var Id in Ids)
			{
				ValidateId<T>(
					errorMessages,
					Id,
					nullOrEmptyAcceptable,
					fileName,
					dataDefId,
					"Entry in " + IdXMLLabel,
					compName
				);
			}
		}

		public static void ValidateColor(
			List<Tuple<string, string>> errorMessages,
			string ColorCode,
			bool acceptNullOrEmpty,
			string fileName,
			string dataDefId,
			string IdXMLLabel,
			string compName = "def"
		)
		{
			if (!acceptNullOrEmpty && string.IsNullOrEmpty(ColorCode))
			{
				errorMessages.Add(new Tuple<string, string>(
					$"Error in {fileName}",
					$"property '{IdXMLLabel}' of '{compName}' of '{dataDefId}' must be specified."));
			}
			else if (!string.IsNullOrEmpty(ColorCode) && !Regex.IsMatch(ColorCode, @"^#[0-9, a-f, A-F]{6}$"))
			{
				errorMessages.Add(new Tuple<string, string>(
					$"Error in {fileName}",
					$"property '{IdXMLLabel}' of '{compName}' of {dataDefId} is not a valid hex color format (#XXXXXX)."));
			}
		}

		public static bool ValidateMeshPath(
			List<Tuple<string, string>> errorMessages,
			string path,
			string fileName,
			string dataDefId,
			string IdXMLLabel,
			string compName = "def"
		)
		{
			if (string.IsNullOrEmpty(path))
			{
				errorMessages.Add(
					new Tuple<string, string>(
						$"Error in {fileName}",
						$"'{IdXMLLabel}' of '{compName}' for '{dataDefId}' must exist and not be empty."
					)
				);
				return false;
			}
			else
			{
				if (File.Exists(Application.streamingAssetsPath + "/" + path) && (path.Contains(".gltf") || path.Contains(".glb")))
					return true;

				errorMessages.Add(new Tuple<string, string>($"Error in {fileName}", $"'{IdXMLLabel}' of '{compName}' for '{dataDefId}' is not a valid mesh path."));
				return false;
			}
		}

		public static bool ValidateTexturePath(
			List<Tuple<string, string>> errorMessages,
			string path,
			bool acceptsNullOrEmpty,
			string fileName,
			string dataDefId,
			string IdXMLLabel,
			string compName = "def"
		)
		{
			if (!acceptsNullOrEmpty && string.IsNullOrEmpty(path))
			{
				errorMessages.Add(
					new Tuple<string, string>(
						$"Error in {fileName}",
						$"'{IdXMLLabel}' of '{compName}' for '{dataDefId}' must exist and not be empty."
					)
				);
				return false;
			}
			else if (!string.IsNullOrEmpty(path))
			{

				if (!File.Exists(Application.streamingAssetsPath + "/" + path))
					errorMessages.Add(
						new Tuple<string, string>(
							$"Error in {fileName}",
							$"'{IdXMLLabel}' of '{compName}' for '{dataDefId}' is not a valid texture path."
						)
					);

			}

			return true;
		}
	}
}