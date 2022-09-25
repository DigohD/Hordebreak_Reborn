using FNZ.Shared.Utils;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FNZ.Shared.Model
{
	public class IdTranslator
	{
		private static IdTranslator T_Instance;

		public static IdTranslator Instance
		{
			get
			{
				if (T_Instance == null)
					T_Instance = new IdTranslator();

				return T_Instance;
			}
			private set
			{

			}
		}

		public IdTranslator()
		{

		}

		private Dictionary<Type, Tuple<Dictionary<string, ushort>, Dictionary<ushort, string>>> data = new Dictionary<Type, Tuple<Dictionary<string, ushort>, Dictionary<ushort, string>>>();
		private Dictionary<Type, ushort> idTrackers = new Dictionary<Type, ushort>();

		public void Init() { }

		public void GenerateIdIfNeeded<T>(string id) where T : DataDef
		{
			GenerateIdIfNeeded(typeof(T), id);
		}

		public void GenerateIdIfNeeded(Type type, string id)
		{
			if (!data.ContainsKey(type))
			{
				var item1 = new Dictionary<string, ushort>();
				var item2 = new Dictionary<ushort, string>();
				data.Add(type, new Tuple<Dictionary<string, ushort>, Dictionary<ushort, string>>(item1, item2));
				idTrackers.Add(type, 1);
			}

			if (data[type].Item1.ContainsKey(id)) return;

			data[type].Item1.Add(id, idTrackers[type]);
			data[type].Item2.Add(idTrackers[type], id);

			idTrackers[type] += 1;
		}

		public void GenerateMissingIds()
		{
			foreach (var type in DataBank.Instance.GetAllTypes())
			{
				foreach (var data in DataBank.Instance.GetAllDataDefsOfType(type))
				{
					GenerateIdIfNeeded(type, data.Id);
				}
			}
		}

		public void Clear()
		{
			data.Clear();
			idTrackers.Clear();
		}

		public void Serialize(NetBuffer nb)
		{
			nb.Write((ushort)data.Count);
			foreach (Type type in data.Keys)
			{

				nb.Write(type.ToString());

				nb.Write((ushort)data[type].Item1.Count);
				foreach (var id in data[type].Item1.Keys)
				{
					nb.Write(id);
					nb.Write(data[type].Item1[id]);
				}
			}
		}

		public void Deserialize(NetBuffer nb)
		{
			Clear();

			ushort typeCount = nb.ReadUInt16();

			IEnumerable<Type> dataTypes = FNEUtil.GetAllTypesOf<DataDef>("FNZ");

			/*IEnumerable<Type> dataTypes = typeof(DataDef)
				.Assembly.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(DataDef)) && !t.IsAbstract);*/

			var list = dataTypes.ToList();


			for (int i = 0; i < typeCount; i++)
			{
				string typeHash = nb.ReadString();
				foreach (var type in dataTypes)
				{
					var hash = type.ToString();

					if (hash == typeHash)
					{
						var item1 = new Dictionary<string, ushort>();
						var item2 = new Dictionary<ushort, string>();
						data.Add(type, new Tuple<Dictionary<string, ushort>, Dictionary<ushort, string>>(item1, item2));
						idTrackers.Add(type, 1);
						ushort dataCount = nb.ReadUInt16();
						for (int j = 0; j < dataCount; j++)
						{
							string id = nb.ReadString();
							ushort code = nb.ReadUInt16();
							data[type].Item1.Add(id, code);
							data[type].Item2.Add(code, id);
							if (idTrackers[type] <= code)
							{
								idTrackers[type] = (ushort)(code + 1);
							}
						}
					}
				}
			}
		}

		public ushort GetIdCode<T>(string id) where T : DataDef
		{
			if (string.IsNullOrEmpty(id)) return 0;
			if (!data.ContainsKey(typeof(T)))
			{
				Debug.LogError("Type list " + typeof(T) + " was missing from IdTranslator");
				return 0;
			}
			if (!data[typeof(T)].Item1.ContainsKey(id))
			{
				Debug.LogError("id " + id + " was missing from IdTranslator type list " + typeof(T));
				return 0;
			}

			return data[typeof(T)].Item1[id];
		}

		public string GetId<T>(ushort id) where T : DataDef
		{
			if (id == 0)
				return null;

			if (!data.ContainsKey(typeof(T))) Debug.LogError("Type list " + typeof(T) + " was missing from IdTranslator");
			if (!data[typeof(T)].Item2.ContainsKey(id)) Debug.LogError("id " + id + " was missing from IdTranslator type list " + typeof(T));
			return data[typeof(T)].Item2[id];
		}
	}
}

