using FNZ.Shared.Model.Effect.RealEffect;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.QuestType;
using FNZ.Shared.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using FNZ.Shared.Model.WorldEvent;
using UnityEngine;

namespace FNZ.Shared.Model
{
	public class XMLReader<T> where T : DataDef
	{
		static IEnumerable<Type> dataTypes = FNEUtil.GetAllTypesOf<DataDef>("FNZ");
		static IEnumerable<Type> componentDataTypes = FNEUtil.GetAllTypesOf<DataComponent>("FNZ");
		static IEnumerable<Type> itemComponentDataTypes = FNEUtil.GetAllTypesOf<ItemComponentData>("FNZ");
		static IEnumerable<Type> questDataTypes = FNEUtil.GetAllTypesOf<QuestTypeData>("FNZ");
		static IEnumerable<Type> realEffectDataTypes = FNEUtil.GetAllTypesOf<RealEffectData>("FNZ");

		static Type[] typesArr;

		static XmlRootAttribute xRoot = new XmlRootAttribute();


		static XmlSerializer serializer;


		static XMLReader()
		{
			xRoot.ElementName = "Defs";
			xRoot.IsNullable = true;

			int i = 0;
			foreach (var type in dataTypes)
			{
				i++;
			}

			foreach (var type in componentDataTypes)
			{
				i++;
			}

			foreach (var type in itemComponentDataTypes)
			{
				i++;
			}

			foreach (var type in questDataTypes)
			{
				i++;
			}

			foreach (var type in realEffectDataTypes)
			{
				i++;
			}

			typesArr = new Type[i];

			i = 0;

			foreach (var type in dataTypes)
			{
				typesArr[i] = type;
				i++;
			}

			foreach (var type in componentDataTypes)
			{
				typesArr[i] = type;
				i++;
			}

			foreach (var type in itemComponentDataTypes)
			{
				typesArr[i] = type;
				i++;
			}

			foreach (var type in questDataTypes)
			{
				typesArr[i] = type;
				i++;
			}

			foreach (var type in realEffectDataTypes)
			{
				typesArr[i] = type;
				i++;
			}

			serializer = new XmlSerializer(typeof(List<T>), null, typesArr, xRoot, null);
		}

	public static List<T> Load(string path)
		{
			StreamReader reader = new StreamReader(path);

			try
			{
				var obj = (List<T>)serializer.Deserialize(reader);

				return obj;
			}
			catch (InvalidOperationException ioe)
			{

				Debug.LogError(path);
				Debug.LogError(ioe.Message);
				Debug.LogError(ioe.InnerException.Message);
				Debug.LogError(ioe.StackTrace);

				return null;
			}
		}
	}
}

