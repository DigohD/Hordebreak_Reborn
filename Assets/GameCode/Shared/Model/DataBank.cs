using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Profiling;

namespace FNZ.Shared.Model
{
	public abstract class DataDef
	{
		[XmlElement("id")]
		public string Id { get; set; }

		public string fileName
		{
			get { return DataBank.Instance.GetIdFileName(Id); }
		}

		public abstract bool ValidateXMLData(out List<Tuple<string, string>> errorMessages);
	}

	[XmlType("DataComponent")]
	public abstract class DataComponent
	{
		public abstract Type GetComponentType();

        public abstract bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName);

    }

	public class DataBank
	{
		private static DataBank instance = null;

		private Dictionary<Type, Dictionary<string, DataDef>> data = new Dictionary<Type, Dictionary<string, DataDef>>();

		private static List<Tuple<string, string>> m_IdFileName = new List<Tuple<string, string>>();
		private static List<Tuple<string, string>> m_ErrorList = new List<Tuple<string, string>>();

		public static DataBank Instance => instance ?? (instance = new DataBank());

		private DataBank()
		{
			var directoryInfo = new DirectoryInfo(Application.streamingAssetsPath + "/Data/XML");
			FileInfo[] allfiles = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
			
			Profiler.BeginSample("XML-Deserialize");
			foreach (var file in allfiles)
			{
				
				try
				{
					List<DataDef> input = XMLReader<DataDef>.Load(file.FullName);

					if (input == null)
						continue;

					foreach (var itemData in input)
					{
						if (!data.ContainsKey(itemData.GetType()))
						{
							data.Add(itemData.GetType(), new Dictionary<string, DataDef>());
						}

						data[itemData.GetType()].Add(itemData.Id, itemData);

						m_IdFileName.Add(new Tuple<string, string>(itemData.Id, file.Name));
					}
				}
				catch (Exception e)
				{
					Debug.LogError(file + " NOPE!");
					throw e;
				}
			}
			Profiler.EndSample();
		}

		public List<Tuple<string, string>> GetDataBankErrors()
		{
			return m_ErrorList;
		}

		public string GetIdFileName(string id)
		{
			return m_IdFileName.Find(tuple => tuple.Item1 == id).Item2;
		}

		public void ValidateDataBank()
		{
			foreach (var type in data.Keys)
			{
				foreach (var dataDef in data[type].Values)
				{
                    if (!Regex.Match(dataDef.Id, "^[a-z0-9_]*$").Success)
                        m_ErrorList.Add(
                            new Tuple<string, string>(
                                "Warning in " + dataDef.fileName,
                                $"Id of '{dataDef.Id}' is wrong: it should only contain numbers, lower case letters and '_'"
                            )    
                        );
                    if (dataDef.ValidateXMLData(out List<Tuple<string, string>> errors))
					{
                        if(errors != null)
						    foreach (var error in errors)
						    {
							    m_ErrorList.Add(error);
						    }
					}
				}
			}
		}

		public void ReloadDataBank()
		{
			instance = null;
			instance = new DataBank();
		}

		public T GetData<T>(string id) where T : DataDef
		{
			if (!data.ContainsKey(typeof(T)))
			{
				Debug.LogError("Couldn't find data type list " + typeof(T) + " in DataBank");
				return null;
			}

			try
			{
				if (!data[typeof(T)].ContainsKey(id))
				{
					return null;
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Error in Databank.cs GetData for id: {id}. Stacktrace: " + e);
			}
			
			return data[typeof(T)][id] as T;
		}

		public List<Type> GetAllTypes()
		{
			List<Type> types = new List<Type>();
			foreach (var type in data.Keys)
			{
				types.Add(type);
			}
			return types;
		}

		public List<T> GetAllDataIdsOfType<T>() where T : DataDef
		{
			List<T> dataDefs = new List<T>();

			if (!data.ContainsKey(typeof(T))) return dataDefs;

			foreach (var dataDef in data[typeof(T)].Values)
			{
				T d = dataDef as T;
				dataDefs.Add(d);
			}
			return dataDefs;
		}

		public bool IsIdOfType<T>(string id) where T : DataDef
		{
			return DataBank.Instance.GetAllDataIdsOfType<T>().Find(
				t => t.Id.Equals(id)
			) != null;
		}

		public bool DoesIdExist<T>(string id) where T : DataDef
		{
			if (!data.ContainsKey(typeof(T)))
			{
				return false;
			}

			return data[typeof(T)].ContainsKey(id);
		}

		public List<DataDef> GetAllDataDefsOfType(Type type)
		{
			if (!data.ContainsKey(type)) return null;

			List<DataDef> dataDefs = new List<DataDef>();
			foreach (var dataDef in data[type].Values)
			{
				dataDefs.Add(dataDef);
			}
			return dataDefs;
		}

    }
}

