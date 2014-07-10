using System;
using System.Collections;
using UnityEngine;
using InControl;


namespace CustomProfileExample
{
	// This custom profile is enabled by adding it to the Custom Profiles list
	// on the InControlManager script, which is attached to the InControl 
	// game object in this example scene.
	// 
	public class GenesesKeyboardExampleP2 : UnityInputDeviceProfile
	{
		public GenesesKeyboardExampleP2()
		{
			Name = "Keyboard/Mouse";
			Meta = "Geneses custom KB mouse control for testing";
			
			// This profile only works on desktops.
			SupportedPlatforms = new[]
			{
				"Windows",
				"Mac",
				"Linux"
			};
			
			Sensitivity = 1.0f;
			LowerDeadZone = 0.0f;
			UpperDeadZone = 1.0f;
			
			ButtonMappings = new[]
			{
				new InputControlMapping
				{
					Handle = "Build2",
					Target = InputControlType.Action1,
					Source = KeyCodeButton(KeyCode.Slash)
				}
				
			};
			
			AnalogMappings = new[]
			{
				new InputControlMapping
				{
					Handle = "Move X2",
					Target = InputControlType.LeftStickX,
					Source = KeyCodeAxis( KeyCode.LeftArrow, KeyCode.RightArrow )
				},
				new InputControlMapping
				{
					Handle = "Move Y2",
					Target = InputControlType.LeftStickY,
					Source = KeyCodeAxis( KeyCode.DownArrow, KeyCode.UpArrow )
				}
				
			};
		}
	}
}

