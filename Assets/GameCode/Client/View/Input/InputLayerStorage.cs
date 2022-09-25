using System;
using System.Collections.Generic;
using System.Linq;

namespace FNZ.Client.View.Input
{

	public static class InputLayerStorage
	{
		private static List<InputLayer> m_InputLayers = new List<InputLayer>();

		public static void Init()
		{
			IEnumerable<Type> inputLayerTypes = typeof(InputLayer)
			.Assembly.GetTypes()
			.Where(t => t.IsSubclassOf(typeof(InputLayer)) && !t.IsAbstract);

			foreach (var layerType in inputLayerTypes)
			{
				var layerInstance = (InputLayer)Activator.CreateInstance(layerType);
				m_InputLayers.Add(layerInstance);
			}

			//m_InputLayers = new List<InputLayer>
			//{
			//    new PlayerDefaultInputLayer(),
			//    new UiInputLayer(),
			//    new BuildModeInputLayer(),
			//    new ExitMenuLayer(),
			//    new DeathScreenLayer(),
			//    new ChatInputLayer(),
			//    new VehicleInputLayer(),
			//    new VehiclePassengerInputLayer()
			//};
		}

		public static T GetInputLayer<T>() where T : InputLayer
		{
			foreach (var layer in m_InputLayers)
			{
				if (layer.GetType() == typeof(T))
				{
					return layer as T;
				}
			}

			return null;
		}
	}
}