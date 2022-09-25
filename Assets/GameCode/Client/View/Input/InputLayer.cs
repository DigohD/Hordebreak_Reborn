using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FNZ.Client.View.Input
{
	public abstract class InputLayer
	{
		public bool IsActive;

		// if true then all input layers below this layer will be blocked
		public bool IsBlocking = false;

		// if true, mouse events will be blocked if hovering interactable UI 
		public bool IsUIBlockingMouse = false;

		private Dictionary<string, KeyCode> m_KeyActionMappings;
		private Dictionary<string, MouseButton> m_MouseActionMappings;
		private Dictionary<string, GamepadButton> m_GamepadActionMappings;

		private List<Tuple<string, InputActionType, Action>> m_KeyActionEvents;
		private List<Tuple<string, InputActionType, Action>> m_MouseActionEvents;
		private List<Tuple<string, InputActionType, Action>> m_GamepadActionEvents;

		private Dictionary<string, Action<float>> m_AxisEventMappings;

		//private Dictionary<string, List<Tuple<KeyCode, float>>> m_AxisMappings;

		private List<Tuple<string, KeyCode, float>> m_KeyAxisMappings;
		private List<Tuple<string, string, float>> m_AxisMappings;

		public InputLayer(bool isBlocking)
		{
			this.IsBlocking = isBlocking;

			IsActive = false;

			m_KeyActionMappings = new Dictionary<string, KeyCode>();
			m_MouseActionMappings = new Dictionary<string, MouseButton>();
			m_GamepadActionMappings = new Dictionary<string, GamepadButton>();

			m_KeyActionEvents = new List<Tuple<string, InputActionType, Action>>();
			m_MouseActionEvents = new List<Tuple<string, InputActionType, Action>>();
			m_GamepadActionEvents = new List<Tuple<string, InputActionType, Action>>();

			m_KeyAxisMappings = new List<Tuple<string, KeyCode, float>>();
			m_AxisEventMappings = new Dictionary<string, Action<float>>();

			AddActionMappings();
			BindActions();
		}
		protected abstract void AddActionMappings();
		protected virtual void BindActions() { }
		public virtual void OnActivated()
		{
			IsActive = true;
		}
		public virtual void OnDeactivated() { IsActive = false; }

		public void AddActionMapping(string actionName, KeyCode keyCode)
		{
			if (!m_KeyActionMappings.ContainsKey(actionName))
			{
				m_KeyActionMappings.Add(actionName, keyCode);
			}
			else
			{
				m_KeyActionMappings[actionName] = keyCode;
			}
		}

		public void AddActionMapping(string actionName, MouseButton button)
		{
			if (!m_MouseActionMappings.ContainsKey(actionName))
			{
				m_MouseActionMappings.Add(actionName, button);
			}
			else
			{
				m_MouseActionMappings[actionName] = button;
			}
		}

		public void AddActionMapping(string actionName, GamepadButton button)
		{
			if (!m_GamepadActionMappings.ContainsKey(actionName))
			{
				m_GamepadActionMappings.Add(actionName, button);
			}
			else
			{
				m_GamepadActionMappings[actionName] = button;
			}
		}

		public void AddAxisMapping(string axisName, KeyCode keyCode, float axisValue)
		{
			foreach (var mapping in m_KeyAxisMappings)
			{
				if (mapping.Item1 == axisName && mapping.Item2 == keyCode
					&& mapping.Item3 == axisValue)
				{
					Debug.LogError("Axismapping already exists!");
					return;
				}
			}

			m_KeyAxisMappings.Add(new Tuple<string, KeyCode, float>(axisName, keyCode, axisValue));
		}

		public void BindAxis(string axisName, Action<float> func)
		{
			foreach (var axisBinding in m_KeyAxisMappings)
			{
				if (axisBinding.Item1 == axisName)
				{
					m_AxisEventMappings.Add(axisName, func);
					break;
				}
			}
		}

		public void InvokeAction(string axisName, float axisValue)
		{

		}

		public void BindAction(string actionName, InputActionType actionType, Action func)
		{
			if (!IsActionAlreadyBound(actionName, actionType, m_KeyActionEvents))
			{
				var mapping = new Tuple<string, InputActionType, Action>(actionName, actionType, func);
				m_KeyActionEvents.Add(mapping);
			}

			if (!IsActionAlreadyBound(actionName, actionType, m_MouseActionEvents))
			{
				var mapping = new Tuple<string, InputActionType, Action>(actionName, actionType, func);
				m_MouseActionEvents.Add(mapping);
			}

			if (!IsActionAlreadyBound(actionName, actionType, m_GamepadActionEvents))
			{
				var mapping = new Tuple<string, InputActionType, Action>(actionName, actionType, func);
				m_GamepadActionEvents.Add(mapping);
			}
		}

		public bool InvokeKeyAction(KeyCode keyCode, InputActionType actionType)
		{
			// TODO(Anders): if action was hold or press then check if the keycode is bound to an axis
			// and invoke the action mapped to it

			//if (actionType == InputActionType.HOLD 
			//    || actionType == InputActionType.PRESS)
			//{
			//    foreach (var mapping in m_KeyAxisMappings)
			//    {
			//        if (mapping.Item2 == keyCode)
			//        {
			//            m_AxisEventMappings[mapping.Item1](mapping.Item3);
			//            break;
			//        }
			//    }
			//}

			foreach (var eventMapping in m_KeyActionEvents)
			{
				if (m_KeyActionMappings.ContainsValue(keyCode))
				{
					var kvp = m_KeyActionMappings.First(entry => entry.Value == keyCode);

					if (eventMapping.Item1 == kvp.Key &&
						eventMapping.Item2 == actionType)
					{
						eventMapping.Item3();
						return true;
					}
				}
			}

			return false;
		}

		public bool InvokeMouseAction(MouseButton button, InputActionType actionType)
		{
			foreach (var eventMapping in m_MouseActionEvents)
			{
				if (m_MouseActionMappings.ContainsValue(button))
				{
					var kvp = m_MouseActionMappings.First(entry => entry.Value == button);

					if (kvp.Key == eventMapping.Item1
						&& eventMapping.Item2 == actionType)
					{
						eventMapping.Item3();
						return true;
					}
				}
			}

			return false;
		}

		public bool InvokeGamepadAction(GamepadButton button, InputActionType actionType)
		{
			foreach (var eventMapping in m_GamepadActionEvents)
			{
				if (m_GamepadActionMappings.ContainsValue(button))
				{
					var kvp = m_GamepadActionMappings.First(entry => entry.Value == button);

					if (kvp.Key == eventMapping.Item1
						&& eventMapping.Item2 == actionType)
					{
						eventMapping.Item3();
						return true;
					}
				}
			}

			return false;
		}

		public void UpdateKeyBinds()
		{
			foreach (var kvp in InputKeybinds.Instance.Keybinds)
			{
				AddActionMapping(kvp.Key, kvp.Value);
			}
		}

		private bool IsActionAlreadyBound(string actionName, InputActionType actionType,
			List<Tuple<string, InputActionType, Action>> eventMappings)
		{
			foreach (var elem in eventMappings)
			{
				string name = elem.Item1;
				InputActionType type = elem.Item2;

				if (actionName == name && actionType == type)
				{
					return true;
				}
			}

			return false;
		}
	}
}