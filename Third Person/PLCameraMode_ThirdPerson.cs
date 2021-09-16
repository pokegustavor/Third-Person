using System;
using UnityEngine;
using UnityEngine.Rendering;

public class PLCameraMode_ThirdPerson : PLCameraMode_GameBase
{
	public PLCameraMode_ThirdPerson()
	{
		SunShaftsEnabledInThisMode = false;
	}

	public override string GetModeString()
	{
		return "LocalPawn";
	}

	public override void BeginMode(PLCameraSystem inSys)
	{
		base.BeginMode(inSys);
	}

	public override void GetLocalPawnLayerClipRanges(out float outNearPlane, out float outFarPlane)
	{
		if (PLNetworkManager.Instance.LocalPlayer != null && PLNetworkManager.Instance.LocalPlayer.CurrentlyInLiarsDiceGame != null)
		{
			outNearPlane = 0.02f;
			outFarPlane = 200f;
			return;
		}
		base.GetLocalPawnLayerClipRanges(out outNearPlane, out outFarPlane);
	}

	public override float GetFOV(bool isMainCamera)
	{
		if (PLNetworkManager.Instance.ViewedPawn != null && PLNetworkManager.Instance.ViewedPawn.ExteriorViewActive())
		{
			return 60f;
		}
		return base.GetFOV(isMainCamera);
	}

	public override int GetLocalPawnCameraCullingMask()
	{
		if (PLTabMenu.Instance.DialogueMenu.CurrentActorInstance != null && PLTabMenu.Instance.DialogueMenu.CurrentActorInstance.DialogueCameraShouldBeUsed)
		{
			return base.GetLocalPawnCameraCullingMask() & -8193 & -16385 & -16777217;
		}
		return base.GetLocalPawnCameraCullingMask();
	}

	public override void Tick(PLCameraSystem inSys)
	{
		base.Tick(inSys);
		if (Input.GetKeyUp(KeyCode.Equals))
		{
			NextViewPawn();
		}
		if (Input.GetKeyUp(KeyCode.Minus))
		{
			PrevViewPawn();
		}
	}

	private PLPawn GetAvailableViewPawn()
	{
		foreach (PLPawn plpawn in PLGameStatic.Instance.AllPawns)
		{
			if (plpawn != null && !plpawn.IsDead && plpawn.GetPlayer() == PLNetworkManager.Instance.LocalPlayer)
			{
				return plpawn;
			}
		}
		foreach (PLPawn plpawn2 in PLGameStatic.Instance.AllPawns)
		{
			if (plpawn2 != null && plpawn2.IsDead && plpawn2.GetPlayer() == PLNetworkManager.Instance.LocalPlayer)
			{
				return plpawn2;
			}
		}
		return null;
	}

	private void NextViewPawn()
	{
		AdvancePawnID(true);
	}

	private void PrevViewPawn()
	{
		AdvancePawnID(false);
	}

	private void AdvancePawnID(bool inForward)
	{
		int count = PLGameStatic.Instance.AllPawns.Count;
		int viewPawnID = ViewPawnID;
		CurrentViewPawn = null;
		for (int i = 1; i < count; i++)
		{
			int num;
			if (inForward)
			{
				num = (viewPawnID + i) % count;
			}
			else
			{
				num = (viewPawnID - i) % count;
			}
			if (num < 0)
			{
				num += count;
			}
			PLPawn plpawn = PLGameStatic.Instance.AllPawns[num];
			if (plpawn != null && !plpawn.IsDead && plpawn.GetPlayer() != null && plpawn.GetPlayer().TeamID == 0)
			{
				ViewPawnID = num;
				CurrentViewPawn = plpawn;
				return;
			}
		}
	}

	public override bool ShouldShowInterior()
	{
		return true;
	}

	private void UpdateDeathCamera(PLCameraSystem inSys, PLPawn inPawn)
	{
		TimeSinceTargetChange += Time.deltaTime;
		if (DeathCamera_LastTarget != inPawn)
		{
			DeathCamera_LastTarget = inPawn;
			DeathCamera_Rotation = new Vector3(0f, 0f, 0f);
			DeathCamera_Dist = 2f;
			TimeSinceTargetChange = 0f;
		}
		float num = 0.4f * Mathf.Lerp(0.1f, 5f, PLXMLOptionsIO.Instance.CurrentOptions.GetFloatValue("MouseSensitivity_Foot"));
		float num2 = PLInput.Instance.GetAxis(PLInputBase.EInputActionName.view_x, false) * num + PLInput.Instance.GetAxis(PLInputBase.EInputActionName.joystick_view_x, false) * PLInput.Instance.JoystickSensitivity(true);
		float x = PLInput.Instance.GetAxis(PLInputBase.EInputActionName.view_y, false) * num + PLInput.Instance.GetAxis(PLInputBase.EInputActionName.joystick_view_y, false) * PLInput.Instance.JoystickSensitivity(true);
		if (PLServer.Instance != null && PLServer.Instance.IsReflection_FlipIsActiveLocal)
		{
			num2 *= -1f;
		}
		DeathCamera_Rotation += new Vector3(x, num2, 0f);
		DeathCamera_Rotation = new Vector3(Mathf.Clamp(DeathCamera_Rotation.x, -85f, -5f), DeathCamera_Rotation.y, DeathCamera_Rotation.z);
		DeathCamera_Dist -= PLInput.Instance.GetAxis(PLInputBase.EInputActionName.mouse_scrollwheel, false);
		DeathCamera_Dist = Mathf.Clamp(DeathCamera_Dist, 1f, 5f);
		Vector3 normalized = (Quaternion.Euler(DeathCamera_Rotation) * new Vector3(0f, 0f, 1f)).normalized;
		if (Time.time - LastDeathCameraRaycastTime > 0.05f)
		{
			LastDeathCameraRaycastTime = Time.time;
			RaycastHit raycastHit;
			if (Physics.Raycast(new Ray(inPawn.transform.position, normalized.normalized), out raycastHit, DeathCamera_Dist, inPawn.GetCurrentRaycastCollisionMask()))
			{
				DeathCamera_Dist = Vector3.Distance(inPawn.transform.position, raycastHit.point);
			}
		}
		inSys.CurrentTransformPositionOffsetNear = inPawn.transform.position + normalized * Mathf.Clamp(DeathCamera_Dist, 0.5f, Mathf.Clamp(TimeSinceTargetChange, 0f, 5f));
		inSys.CurrentTransformRotationOffsetNear = Quaternion.LookRotation((inPawn.transform.position - inSys.CurrentTransformPositionOffsetNear).normalized);
		if (PLNetworkManager.Instance.ViewedPawn.CurrentShip != null)
		{
			Vector3 currentTransformPositionOffsetFar = PLNetworkManager.Instance.ViewedPawn.CurrentShip.InteriorDynamic.transform.InverseTransformPoint(PLNetworkManager.Instance.ViewedPawn.transform.position);
			inSys.CurrentTransformPositionOffsetFar = currentTransformPositionOffsetFar;
			inSys.CurrentTransformRotationOffsetFar = inSys.CurrentTransformRotationOffsetNear;
			return;
		}
		if (PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI != null && PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO != null)
		{
			Vector3 currentTransformPositionOffsetFar2 = PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO.InteriorParent.transform.InverseTransformPoint(PLNetworkManager.Instance.ViewedPawn.transform.position);
			inSys.CurrentTransformPositionOffsetFar = currentTransformPositionOffsetFar2;
			inSys.CurrentTransformRotationOffsetFar = Quaternion.Euler(PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO.FarCameraRotationOffset) * inSys.CurrentTransformRotationOffsetNear;
			if (PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO.InteriorIsAttachedToExterior)
			{
				inSys.CurrentTransformRotationOffsetFar = Quaternion.Euler(PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO.FarCameraRotationOffset) * Quaternion.Inverse(PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO.transform.rotation) * inSys.CurrentTransformRotationOffsetNear;
				return;
			}
		}
		else
		{
			inSys.CurrentTransformPositionOffsetFar = inSys.CurrentTransformPositionOffsetNear;
			inSys.CurrentTransformRotationOffsetFar = inSys.CurrentTransformRotationOffsetNear;
		}
	}

	public override void UpdateCameraSystem(PLCameraSystem inSys)
	{
		if (inSys.CurrentCameraMode != null)
		{
			foreach (Camera camera in inSys.CurrentSubSystem.MainCameras)
			{
			}
			foreach (Camera camera2 in inSys.CurrentSubSystem.LocalPawnCameras)
			{
			}
		}
		if (PLNetworkManager.Instance.MyLocalPawn != null)
		{
			//Make head visible
			PLNetworkManager.Instance.ViewedPawn = PLNetworkManager.Instance.MyLocalPawn;
			PLNetworkManager.Instance.MyLocalPawn.CustomPawn.HeadRenderer.enabled = true;
			PLCustomPawn customPawn = PLNetworkManager.Instance.MyLocalPawn.CustomPawn;
			PLCustomPawn_Male malepawn = PLNetworkManager.Instance.MyLocalPawn.CustomPawn as PLCustomPawn_Male;
			if (customPawn.CurrentHairObjRenderer != null)
			{
				customPawn.CurrentHairObjRenderer.shadowCastingMode = ShadowCastingMode.On;
			}
			if(malepawn != null) 
			{
				if (malepawn.CurrentFacialHairObjRenderer != null)
				{
					malepawn.CurrentFacialHairObjRenderer.shadowCastingMode = ShadowCastingMode.On;
				}
				if (malepawn.RobotNeckRenderer != null)
				{
					malepawn.RobotNeckRenderer.shadowCastingMode = ShadowCastingMode.On;
				}
			}
			if (customPawn.HeadRenderer != null)
			{
				customPawn.HeadRenderer.shadowCastingMode = ShadowCastingMode.On;
			}
			if (customPawn.EyebrowRenderer != null)
			{
				customPawn.EyebrowRenderer.shadowCastingMode = ShadowCastingMode.On;
			}
			if (customPawn.SlyvassiHelmetRenderer != null)
			{
				customPawn.SlyvassiHelmetRenderer.shadowCastingMode = ShadowCastingMode.On;
			}
			if (customPawn.SlyvassiHelmetGlassRenderer != null)
			{
				customPawn.SlyvassiHelmetGlassRenderer.shadowCastingMode = ShadowCastingMode.On;
			}
			if (customPawn.SlyvassiPackRenderer != null)
			{
				customPawn.SlyvassiPackRenderer.shadowCastingMode = ShadowCastingMode.On;
			}
			if (customPawn.MyPawn.PawnLightMeshRenderer != null)
			{
				customPawn.MyPawn.PawnLightMeshRenderer.shadowCastingMode = ShadowCastingMode.On;
			}
			if (customPawn.CurrentTechObjRenderer != null)
			{
				customPawn.CurrentTechObjRenderer.shadowCastingMode = ShadowCastingMode.On;
			}
			
		}
		else
		{
			PLNetworkManager.Instance.ViewedPawn = GetAvailableViewPawn();
		}
		Transform transform = null;
		if (PLNetworkManager.Instance.ViewedPawn != null)
		{
			transform = PLNetworkManager.Instance.ViewedPawn.PawnCamera;
		}
		if (PLNetworkManager.Instance.MyLocalPawn != null && PLNetworkManager.Instance.MyLocalPawn.ExteriorViewActive())
		{
			if (PLCustomPawnMenu.LastTargetedPlayer != null && PLCustomPawnMenu.LastTargetedPlayer.GetPawn() != null)
			{
				transform = PLCustomPawnMenu.LastTargetedPlayer.GetPawn().PawnCustomAppearanceCamera;
			}
			else
			{
				transform = PLNetworkManager.Instance.MyLocalPawn.PawnCustomAppearanceCamera;
			}
		}
		if (PLEncounterManager.Instance.GetCPEI() != null && PLEncounterManager.Instance.GetCPEI().ShouldOverrideUpdateCameraSystem())
		{
			PLEncounterManager.Instance.GetCPEI().UpdateCameraSystemOverride(inSys);
			return;
		}
		if (PLLCChair.Instance != null && PLLCChair.Instance.PlayerIDInChair == PLNetworkManager.Instance.LocalPlayerID)
		{
			inSys.SetCurrentTransformBoth(PLLCChair.Instance.CagedMindslaver_CameraTransform);
			inSys.SetCurrentTransformPositionOffsetBoth(Vector3.zero);
			inSys.SetCurrentTransformRotationOffsetBoth(Quaternion.identity);
			return;
		}
		if (PLTabMenu.Instance.DialogueMenu.CurrentActorInstance != null && PLTabMenu.Instance.DialogueMenu.CurrentActorInstance.DialogueCameraShouldBeUsed)
		{
			QualitySettings.shadowDistance = 200f;
			PLTabMenu.Instance.DialogueMenu.CurrentActorInstance.transform.TransformPoint(Vector3.up * 1.1f);
			inSys.SetCurrentTransformBoth(null);
			inSys.SetCurrentTransformPositionOffsetBoth(PLTabMenu.Instance.DialogueMenu.CurrentActorInstance.transform.TransformPoint(Vector3.up * 1.4f + Vector3.forward + Vector3.right * 0.4f));
			inSys.SetCurrentTransformRotationOffsetBoth(Quaternion.LookRotation(-PLTabMenu.Instance.DialogueMenu.CurrentActorInstance.transform.forward) * Quaternion.Euler(8f, -18f, 0f));
			return;
		}
		if (PLNetworkManager.Instance.MyLocalPawn != null && PLNetworkManager.Instance.MyLocalPawn.MyPlayer != null && PLNetworkManager.Instance.MyLocalPawn.MyPlayer.CurrentlyInLiarsDiceGame != null)
		{
			QualitySettings.shadowDistance = 200f;
			inSys.SetCurrentTransformBoth(null);
			Quaternion rhs = Quaternion.Euler(-PLNetworkManager.Instance.ViewedPawn.CameraRotationScale * PLNetworkManager.Instance.ViewedPawn.VerticalMouseLook.RotationY, PLNetworkManager.Instance.ViewedPawn.CameraRotationScale * PLNetworkManager.Instance.ViewedPawn.HorizontalMouseLook.RotationX, 0f);
			Vector3 currentTransformPositionOffsetBoth;
			Quaternion lhs;
			PLLiarsDiceGame.GetCameraPosAndRotForClassID(PLNetworkManager.Instance.MyLocalPawn.MyPlayer.CurrentlyInLiarsDiceGame, PLNetworkManager.Instance.MyLocalPawn.MyPlayer.GetClassID(), out currentTransformPositionOffsetBoth, out lhs);
			inSys.SetCurrentTransformPositionOffsetBoth(currentTransformPositionOffsetBoth);
			inSys.SetCurrentTransformRotationOffsetBoth(lhs * rhs);
			return;
		}
		float num = 0.25f;
		if (Input.GetKeyDown(KeyCode.PageDown))
		{
			ThirdPerson_Dist += num;
		}
		else if (Input.GetKeyDown(KeyCode.PageUp))
		{
			ThirdPerson_Dist -= num;
		}
		else if (Input.GetKeyDown(KeyCode.Home))
		{
			ThirdPerson_Side += num;
		}
		else if (Input.GetKeyDown(KeyCode.End))
		{
			ThirdPerson_Side -= num;
		}
		ThirdPerson_Dist = Mathf.Clamp(ThirdPerson_Dist, 0f, 5f);
		ThirdPerson_Side = Mathf.Clamp(ThirdPerson_Side, -1.5f, 1.5f);
		float num2 = ThirdPerson_Dist;
		float num3 = ThirdPerson_Side;
		if (PLNetworkManager.Instance != null && PLNetworkManager.Instance.ViewedPawn != null && PLNetworkManager.Instance.ViewedPawn.GetPlayer() != null && PLNetworkManager.Instance.ViewedPawn.GetPlayer().OnPlanet && !PLNetworkManager.Instance.ViewedPawn.IsDead)
		{
			QualitySettings.shadowDistance = 200f;
			if (PLNetworkManager.Instance.ViewedPawn != null)
			{
				PLNetworkManager.Instance.ViewedPawn.CameraRotationPoint.transform.localRotation = Quaternion.Euler(-1f * PLNetworkManager.Instance.ViewedPawn.CameraRotationScale * PLNetworkManager.Instance.ViewedPawn.VerticalMouseLook.RotationY, 0f, 0f);
				inSys.SetCurrentTransformBoth(null);
				PLPawn viewedPawn = PLNetworkManager.Instance.ViewedPawn;
				RaycastHit raycastHit;
				if (Physics.Raycast(new Ray(viewedPawn.transform.position, - transform.forward), out raycastHit, num2, viewedPawn.GetCurrentRaycastCollisionMask()))
				{
					num2 = Vector3.Distance(viewedPawn.transform.position, raycastHit.point);
				}
				if (Physics.Raycast(new Ray(viewedPawn.transform.position, - transform.right), out raycastHit, num3, viewedPawn.GetCurrentRaycastCollisionMask()))
				{
					num3 = Vector3.Distance(viewedPawn.transform.position, raycastHit.point);
				}
				inSys.SetCurrentTransformPositionOffsetBoth(transform.position - transform.forward * num2 - transform.right * num3);
				inSys.SetCurrentTransformRotationOffsetBoth(transform.rotation);
				return;
			}
		}
		else
		{
			QualitySettings.shadowDistance = 800f;
			if (PLNetworkManager.Instance.ViewedPawn != null)
			{
				inSys.CurrentTransformNear = null;
				if (PLNetworkManager.Instance.ViewedPawn.CurrentShip != null)
				{
					inSys.CurrentTransformFar = PLNetworkManager.Instance.ViewedPawn.CurrentShip.Exterior.transform;
				}
				else if (PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI != null && PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO != null)
				{
					inSys.CurrentTransformFar = PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO.transform;
				}
				if (PLNetworkManager.Instance.ViewedPawn.MyController != null)
				{
					PLNetworkManager.Instance.ViewedPawn.MyController.LateCameraChildTransformUpdate();
				}
				if (!PLNetworkManager.Instance.ViewedPawn.IsDead)
				{
					if (PLNetworkManager.Instance.ViewedPawn.CameraRotationPoint != null)
					{
						PLNetworkManager.Instance.ViewedPawn.CameraRotationPoint.transform.localRotation = Quaternion.Euler(-1f * PLNetworkManager.Instance.ViewedPawn.CameraRotationScale * PLNetworkManager.Instance.ViewedPawn.VerticalMouseLook.RotationY, 0f, 0f);
						inSys.CurrentTransformPositionOffsetNear = transform.position - transform.forward * num2 - transform.right * num3;
						inSys.CurrentTransformRotationOffsetNear = transform.rotation;
						if (PLNetworkManager.Instance.ViewedPawn.CurrentShip != null)
						{
							Vector3 a = PLNetworkManager.Instance.ViewedPawn.CurrentShip.InteriorDynamic.transform.InverseTransformPoint(PLNetworkManager.Instance.ViewedPawn.transform.position);
							inSys.CurrentTransformPositionOffsetFar = a - transform.forward * num2 - transform.right * num3;
							inSys.CurrentTransformRotationOffsetFar = transform.rotation;
							return;
						}
						if (!(PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI != null) || !(PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO != null))
						{
							inSys.CurrentTransformPositionOffsetFar = Vector3.zero;
							inSys.CurrentTransformRotationOffsetFar = Quaternion.identity;
							return;
						}
						Vector3 currentTransformPositionOffsetFar = PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO.InteriorParent.transform.InverseTransformPoint(PLNetworkManager.Instance.ViewedPawn.transform.position);
						inSys.CurrentTransformPositionOffsetFar = currentTransformPositionOffsetFar;
						inSys.CurrentTransformRotationOffsetFar = Quaternion.Euler(PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO.FarCameraRotationOffset) * transform.rotation;
						if (PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO.InteriorIsAttachedToExterior)
						{
							inSys.CurrentTransformRotationOffsetFar = Quaternion.Euler(PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO.FarCameraRotationOffset) * Quaternion.Inverse(PLNetworkManager.Instance.ViewedPawn.MyCurrentTLI.MyBSO.transform.rotation) * transform.rotation;
							return;
						}
					}
				}
				else if (PLServer.Instance != null)
				{
					if (PLEncounterManager.Instance.PlayerShip == null || PLEncounterManager.Instance.PlayerShip.HasBeenDestroyed || PLServer.Instance.PlayerShipIsDestroyed)
					{
						PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_GameOver());
						return;
					}
					UpdateDeathCamera(inSys, PLNetworkManager.Instance.ViewedPawn);
					return;
				}
			}
			else if (PLEncounterManager.Instance.PlayerShip != null)
			{
				inSys.CurrentTransformNear = PLEncounterManager.Instance.PlayerShip.BridgeCameraTransform;
				inSys.CurrentTransformPositionOffsetNear = Vector3.zero;
				inSys.CurrentTransformRotationOffsetNear = Quaternion.identity;
				inSys.CurrentTransformFar = PLEncounterManager.Instance.PlayerShip.Exterior.transform;
				if (PLEncounterManager.Instance.PlayerShip.InteriorDynamic != null)
				{
					Vector3 currentTransformPositionOffsetFar2 = PLEncounterManager.Instance.PlayerShip.InteriorDynamic.transform.InverseTransformPoint(PLEncounterManager.Instance.PlayerShip.BridgeCameraTransform.position);
					Vector3 lookDir = PLEncounterManager.Instance.PlayerShip.InteriorDynamic.transform.InverseTransformDirection(PLEncounterManager.Instance.PlayerShip.BridgeCameraTransform.forward);
					inSys.CurrentTransformPositionOffsetFar = currentTransformPositionOffsetFar2;
					inSys.CurrentTransformRotationOffsetFar = PLGlobal.SafeLookRotation(lookDir);
				}
				if (PLNetworkManager.Instance.LocalPlayer != null && PLNetworkManager.Instance.LocalPlayer.MyCurrentTLI != PLEncounterManager.Instance.PlayerShip.MyTLI)
				{
					PLNetworkManager.Instance.LocalPlayer.SetSubHubAndTTIID(PLEncounterManager.Instance.PlayerShip.MyTLI.SubHubID, 0);
					return;
				}
			}
			else if (!PLUIClassSelectionMenu.Instance.Visuals.activeSelf)
			{
				PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_GameOver());
			}
		}
	}

	private bool shownInitialShip;

	private int ViewPawnID;

	private PLPawn CurrentViewPawn;

	private float LastDeathCameraRaycastTime;

	private Vector3 DeathCamera_Rotation;

	private float DeathCamera_Dist = 1f;

	private PLPawn DeathCamera_LastTarget;

	private float TimeSinceTargetChange;

	private float ThirdPerson_Dist = 1f;

	private float ThirdPerson_Side = 0f;
}

