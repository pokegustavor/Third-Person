using System;
using PulsarModLoader.Chat.Commands.CommandRouter;

namespace Third_Person
{
	public class ThirdPerson : ChatCommand
	{
		public override string[] CommandAliases()
		{
			return new string[]
			{
				"thirdperson"
			};
		}

		public override string Description()
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

		public override void Execute(string arguments)
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
		}
	}
}
