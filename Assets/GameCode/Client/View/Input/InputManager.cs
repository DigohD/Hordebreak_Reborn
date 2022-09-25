using FNZ.Client.Utils;
using FNZ.Client.View.Camera;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Client.View.Input
{
	public enum InputActionType
	{
		PRESS,
		RELEASE,
		HOLD,
		NONE
	}

	public enum MouseButton
	{
		LEFT = 0,
		RIGHT = 1,
		MIDDLE = 2
	}

	public enum GamepadButton
	{
		FACE_BTN_DOWN,
		FACE_BTN_RIGHT,
		FACE_BTN_LEFT,
		FACE_BTN_UP,
		RIGHT_BUMPER,
		LEFT_BUMPER,
		START,
		BACK,
		LEFT_STICK_BTN,
		RIGHT_STICK_BTN,
		RIGHT_TRIGGER,
		LEFT_TRIGGER
	}

	public class InputManager : FNESingleton<InputManager>
	{
		private List<InputLayer> m_LayerStack;

		private KeyCode[] m_KeyCodes;
		private MouseButton[] m_MouseButtons;
		private GamepadButton[] m_GamepadButtons;

		private Dictionary<GamepadButton, string> m_GamepadMappings;

		private Vector3 m_lastMousePosition;

		private void Awake()
		{
			m_KeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));
			m_MouseButtons = (MouseButton[])Enum.GetValues(typeof(MouseButton));
			m_GamepadButtons = (GamepadButton[])Enum.GetValues(typeof(GamepadButton));

			m_GamepadMappings = new Dictionary<GamepadButton, string>
			{
				{ GamepadButton.FACE_BTN_DOWN,   "Face_Btn_Down" },
				{ GamepadButton.FACE_BTN_RIGHT,  "Face_Btn_Right"},
				{ GamepadButton.FACE_BTN_LEFT,   "Face_Btn_Left" },
				{ GamepadButton.FACE_BTN_UP,     "Face_Btn_Up" },
				{ GamepadButton.LEFT_BUMPER,     "Left_Bumper" },
				{ GamepadButton.RIGHT_BUMPER,    "Right_Bumper"},
				{ GamepadButton.BACK,            "Back" },
				{ GamepadButton.START,           "Start" },
				{ GamepadButton.LEFT_STICK_BTN,  "Left_Stick_Btn"},
				{ GamepadButton.RIGHT_STICK_BTN, "Right_Stick_Btn" },
				{ GamepadButton.RIGHT_TRIGGER,   "Right_Trigger" },
				{ GamepadButton.LEFT_TRIGGER,    "Left_Trigger" }
			};

			InputLayerStorage.Init();

			m_LayerStack = new List<InputLayer>();
		}

		private void Start()
		{
			m_lastMousePosition = UnityEngine.Input.mousePosition;
		}

		public void ResetLayerStack()
		{
			m_LayerStack.Clear();
			PushInputLayer<PlayerDefaultInputLayer>();
		}

		public void PushInputLayer<T>() where T : InputLayer
		{
			var layer = InputLayerStorage.GetInputLayer<T>();
			PushInputLayer(layer);
		}

		public void PushInputLayer(InputLayer layer)
		{
			foreach (var l in m_LayerStack)
				if (l.GetType() == layer.GetType())
					return;

			var currentLayer = PeekInputLayer();

			currentLayer?.OnDeactivated();

			layer.OnActivated();
			m_LayerStack.Add(layer);
		}

		public void PopInputLayer()
		{
			if (m_LayerStack.Count == 1)
				return;

			var layer = PeekInputLayer();

			layer.OnDeactivated();
			m_LayerStack.RemoveAt(m_LayerStack.Count - 1);
			PeekInputLayer().OnActivated();
		}

		public InputLayer PeekInputLayer()
		{
			return m_LayerStack.Count > 0 ? m_LayerStack[m_LayerStack.Count - 1] : null;
		}

		private void Update()
		{
			//PollAxisEvents();
			PollMouseEvents();
			PollGamepadEvents();
			PollKeyEvents();
		}

		private void PollAxisEvents()
		{
			for (int j = m_LayerStack.Count - 1; j >= 0; j--)
			{
				var inputLayer = m_LayerStack[j];

				float hAxisValue = UnityEngine.Input.GetAxis("Horizontal");
				float vAxisValue = UnityEngine.Input.GetAxis("Vertical");

				float xDir = UnityEngine.Input.GetAxis("Horizontal_Turn");
				float yDir = UnityEngine.Input.GetAxis("Vertical_Turn");

				if (hAxisValue != 0.0f)
				{
					inputLayer.InvokeAction(ActionIdentifiers.MOVE_RIGHT, hAxisValue);
				}

				if (vAxisValue != 0.0f)
				{
					inputLayer.InvokeAction(ActionIdentifiers.MOVE_FORWARD, vAxisValue);
				}

				if (xDir != 0.0f || yDir != 0.0f)
				{
					float angle = Mathf.Atan2(yDir, xDir) * Mathf.Rad2Deg;
					inputLayer.InvokeAction(ActionIdentifiers.YAW, angle);
				}

				if (inputLayer.IsBlocking) break;
			}
		}

		private void PollMouseEvents()
		{
			if (UnityEngine.Input.mousePosition != m_lastMousePosition)
			{
				m_lastMousePosition = UnityEngine.Input.mousePosition;
			}

			for (int i = 0; i < m_MouseButtons.Length; i++)
			{
				for (int j = m_LayerStack.Count - 1; j >= 0; j--)
				{
					var inputLayer = m_LayerStack[j];

					if (inputLayer.IsUIBlockingMouse && CameraScript.DidRaycastHitUI()) break;

					if (UnityEngine.Input.GetMouseButtonDown(i))
					{
						if (inputLayer.InvokeMouseAction(m_MouseButtons[i], InputActionType.PRESS)) break;
					}
					else if (UnityEngine.Input.GetMouseButton(i))
					{
						if (inputLayer.InvokeMouseAction(m_MouseButtons[i], InputActionType.HOLD)) break;
					}
					else if (UnityEngine.Input.GetMouseButtonUp(i))
					{
						if (inputLayer.InvokeMouseAction(m_MouseButtons[i], InputActionType.RELEASE)) break;
					}

					if (inputLayer.IsBlocking) break;
				}
			}
		}

		private void PollGamepadEvents()
		{
			for (int i = 0; i < m_GamepadButtons.Length; i++)
			{
				for (int j = m_LayerStack.Count - 1; j >= 0; j--)
				{
					var inputLayer = m_LayerStack[j];

					if (UnityEngine.Input.GetButtonDown(m_GamepadMappings[(GamepadButton)i]))
					{
						if (inputLayer.InvokeGamepadAction(m_GamepadButtons[i], InputActionType.PRESS)) break;
					}
					else if (UnityEngine.Input.GetButton(m_GamepadMappings[(GamepadButton)i]))
					{
						if (inputLayer.InvokeGamepadAction(m_GamepadButtons[i], InputActionType.HOLD)) break;
					}
					else if (UnityEngine.Input.GetButtonUp(m_GamepadMappings[(GamepadButton)i]))
					{
						if (inputLayer.InvokeGamepadAction(m_GamepadButtons[i], InputActionType.RELEASE)) break;
					}

					if (inputLayer.IsBlocking) break;
				}
			}
		}

		private void PollKeyEvents()
		{
			for (int i = 0; i < m_KeyCodes.Length; i++)
			{
				if ((int)m_KeyCodes[i] > 319) break;

				for (int j = m_LayerStack.Count - 1; j >= 0; j--)
				{
					var inputLayer = m_LayerStack[j];

					if (UnityEngine.Input.GetKeyDown(m_KeyCodes[i]))
					{
						if (inputLayer.InvokeKeyAction(m_KeyCodes[i], InputActionType.PRESS)) break;
					}
					else if (UnityEngine.Input.GetKey(m_KeyCodes[i]))
					{
						if (inputLayer.InvokeKeyAction(m_KeyCodes[i], InputActionType.HOLD)) break;
					}
					else if (UnityEngine.Input.GetKeyUp(m_KeyCodes[i]))
					{
						if (inputLayer.InvokeKeyAction(m_KeyCodes[i], InputActionType.RELEASE)) break;
					}

					if (inputLayer.IsBlocking) break;
				}
			}
		}
	}
}