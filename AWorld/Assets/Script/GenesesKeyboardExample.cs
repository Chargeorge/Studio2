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
	public class GenesesKeyboardExample : UnityInputDeviceProfile
	{
		public GenesesKeyboardExample()
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
					Handle = "Build",
					Target = InputControlType.Action1,
					Source = KeyCodeButton(KeyCode.Space)
				}
				
			};
			
			AnalogMappings = new[]
			{
				new InputControlMapping
				{
					Handle = "Move X",
					Target = InputControlType.LeftStickX,
					Source = KeyCodeAxis( KeyCode.A, KeyCode.D )
				},
				new InputControlMapping
				{
					Handle = "Move Y",
					Target = InputControlType.LeftStickY,
					Source = KeyCodeAxis( KeyCode.S, KeyCode.W )
				}
				
			};
		}
	}
}

