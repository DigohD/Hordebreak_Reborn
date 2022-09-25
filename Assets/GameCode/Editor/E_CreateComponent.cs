using System.IO;
using UnityEditor;
using UnityEngine;

namespace FNZ.Editor
{
	public class ShowPopupExample : EditorWindow
	{
		protected string componentName = "";

		[MenuItem("FarNorth / Create Entity Component")]
		static void Init()
		{
			ShowPopupExample window = ScriptableObject.CreateInstance<ShowPopupExample>();
			window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
			window.ShowPopup();
		}

		void OnGUI()
		{
			EditorGUILayout.LabelField("What is the name of the component. e.g. \"Crop\" in CropComponent", EditorStyles.wordWrappedLabel);
			GUILayout.Space(50);
			componentName = GUILayout.TextField(componentName);

			if (GUILayout.Button("Create"))
			{
				CreateSharedComponent();
				CreateClientComponent();
				CreateServerComponent();
				this.Close();
			}

			if (GUILayout.Button("Cancel")) Close();
		}

		void CreateSharedComponent()
		{
			string dir = "Assets/GameCode/Shared/Model/Entity/Components/" + componentName + "/";
			string copyPath = dir + componentName + "ComponentShared.cs";

			Debug.Log("Creating Classfile: " + copyPath);
			if (File.Exists(copyPath) == false)
			{ // do not overwrite
				Directory.CreateDirectory(dir);
				using (StreamWriter outfile =
					new StreamWriter(copyPath))
				{
					outfile.WriteLine("using System;");
					outfile.WriteLine("using UnityEngine;");
					outfile.WriteLine("using System.Collections;");
					outfile.WriteLine("using System.Xml.Serialization;");
					outfile.WriteLine("using Lidgren.Network;");
					outfile.WriteLine("using System.Collections.Generic;");

					outfile.WriteLine(" ");
					outfile.WriteLine("namespace FNZ.Shared.Model.Entity.Components." + componentName);
					outfile.WriteLine("{");

					outfile.WriteLine("\t[XmlType(\"" + componentName + "ComponentData\")]");
					outfile.WriteLine("\tpublic class " + componentName + "ComponentData : DataComponent ");
					outfile.WriteLine("\t{");
					outfile.WriteLine("\t\tpublic override Type GetComponentType()");
					outfile.WriteLine("\t\t{");
					outfile.WriteLine("\t\t\treturn typeof(" + componentName + "ComponentShared);");
					outfile.WriteLine("\t\t}");
					outfile.WriteLine("\t}");

					outfile.WriteLine(" ");
					outfile.WriteLine("\tpublic class " + componentName + "ComponentShared : FNEComponent");
					outfile.WriteLine("\t{");

					outfile.WriteLine("\t\tnew public " + componentName + "ComponentData m_Data {");
					outfile.WriteLine("\t\t\tget");
					outfile.WriteLine("\t\t\t{");
					outfile.WriteLine("\t\t\t\treturn (" + componentName + "ComponentData) base.m_Data;");
					outfile.WriteLine("\t\t\t}");
					outfile.WriteLine("\t\t}");

					outfile.WriteLine("\t\tpublic override void Init(){}");

					outfile.WriteLine(" ");
					outfile.WriteLine("\t\tpublic override void Serialize(NetBuffer bw){}");

					outfile.WriteLine(" ");
					outfile.WriteLine("\t\tpublic override void Deserialize(NetBuffer br){}");

					outfile.WriteLine(" ");
					outfile.WriteLine("\t\tpublic override ushort GetSizeInBytes(){ return 0; }");

					outfile.WriteLine("\t}");
					outfile.WriteLine("}");
				}//File written
			}
			AssetDatabase.Refresh();
		}

		void CreateClientComponent()
		{
			string dir = "Assets/GameCode/Client/Model/Entity/Components/" + componentName + "/";
			string copyPath = dir + componentName + "ComponentClient.cs";

			Debug.Log("Creating Classfile: " + copyPath);
			if (File.Exists(copyPath) == false)
			{ // do not overwrite
				Directory.CreateDirectory(dir);
				using (StreamWriter outfile =
					new StreamWriter(copyPath))
				{
					outfile.WriteLine("using FNZ.Shared.Model.Entity.Components." + componentName + ";");
					outfile.WriteLine("using UnityEngine;");
					outfile.WriteLine("using System.Collections;");
					outfile.WriteLine("using System.Collections.Generic;");

					outfile.WriteLine(" ");
					outfile.WriteLine("namespace FNZ.Client.Model.Entity.Components." + componentName);
					outfile.WriteLine("{");

					outfile.WriteLine("\tpublic class " + componentName + "ComponentClient : " + componentName + "ComponentShared");
					outfile.WriteLine("\t{");
					outfile.WriteLine(" ");
					outfile.WriteLine("\t}");
					outfile.WriteLine("}");
				}//File written
			}
			AssetDatabase.Refresh();
		}

		void CreateServerComponent()
		{
			string dir = "Assets/GameCode/Server/Model/Entity/Components/" + componentName + "/";
			string copyPath = dir + componentName + "ComponentServer.cs";

			Debug.Log("Creating Classfile: " + copyPath);
			if (File.Exists(copyPath) == false)
			{ // do not overwrite
				Directory.CreateDirectory(dir);
				using (StreamWriter outfile =
					new StreamWriter(copyPath))
				{
					outfile.WriteLine("using FNZ.Shared.Model.Entity.Components." + componentName + ";");
					outfile.WriteLine("using UnityEngine;");
					outfile.WriteLine("using System.Collections;");
					outfile.WriteLine("using System.Collections.Generic;");

					outfile.WriteLine(" ");
					outfile.WriteLine("namespace FNZ.Server.Model.Entity.Components." + componentName);
					outfile.WriteLine("{");

					outfile.WriteLine("\tpublic class " + componentName + "ComponentServer : " + componentName + "ComponentShared");
					outfile.WriteLine("\t{");
					outfile.WriteLine(" ");
					outfile.WriteLine("\t}");
					outfile.WriteLine("}");
				}//File written
			}
			AssetDatabase.Refresh();
		}
	}
}

