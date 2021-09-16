using HarmonyLib;

namespace Third_Person
{
    [HarmonyPatch(typeof(PLTurret), "DeActivate")]
    class Turret
    {
        static void Postfix() 
        {
            if (ThirdPerson.activated) PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_ThirdPerson());
        }
    }
    [HarmonyPatch(typeof(PLPawn), "Revive")]
    class Revive
    {
        static void Postfix()
        {
            if (ThirdPerson.activated) PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_ThirdPerson());
        }
    }
    [HarmonyPatch(typeof(PLPilotingSystem), "DeActivate")]
    class Pilot
    {
        static void Postfix()
        {
            if (ThirdPerson.activated) PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_ThirdPerson());
        }
    }
}
