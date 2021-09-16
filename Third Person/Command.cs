using System;
using PulsarPluginLoader.Chat.Commands;

namespace Third_Person
{
	public class ThirdPerson : IChatCommand
	{
		public string[] CommandAliases()
		{
			return new string[]
			{
				"thirdperson"
			};
		}

		public string Description()
		{
			return "Enables/Disables Third Person";
		}

		public string UsageExample()
		{
			return "/" + this.CommandAliases()[0];
		}

		public bool PublicCommand()
		{
			return false;
		}

		public static bool activated = false;

		public bool Execute(string arguments, int executor)
		{
			if (PLCameraSystem.Instance.CurrentCameraMode != null && PLCameraSystem.Instance.CurrentCameraMode.GetType() == typeof(PLCameraMode_ThirdPerson))
			{
				PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_LocalPawn());
				activated = false;
			}
			else
			{
				PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_ThirdPerson());
				activated = true;
			}
			return false;
		}
	}
}
