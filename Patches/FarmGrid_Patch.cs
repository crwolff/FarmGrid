using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

//FarmGrid.logger.LogInfo(String.Format("DrawFarmGrid({0},{1},{2}", pos, gridDir, gridSize));
//FarmGrid.logger.LogInfo(GenericToDataString.ObjectDumper.Dump(plantObject));

namespace FarmGrid.Patches
{
	// Token: 0x02000004 RID: 4
	[HarmonyPatch]
	internal class FarmGrid_Patch
	{
		// Token: 0x06000004 RID: 4 RVA: 0x00002278 File Offset: 0x00000478
		[HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> UpdatePlacementGhost(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].opcode == OpCodes.Brtrue)
				{
					num = i;
				}
				if (list[i].Calls(AccessTools.Method(typeof(Player), "FindClosestSnapPoints", null, null)))
				{
					list.InsertRange(num + 1, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FarmGrid_Patch), "GetFarmSnapPoints", null, null)),
						new CodeInstruction(OpCodes.Brtrue, list[num].operand)
					});
					break;
				}
			}
			return list;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002334 File Offset: 0x00000534
		[HarmonyPatch(typeof(Player), "PlacePiece")]
		[HarmonyPrefix]
		private static bool PlacePiece(ref Player __instance, ref bool __result, Piece piece)
		{
			FarmGrid_Patch.PlantObject plantObject;
			if (FarmGrid_Patch.GetPlantObject(__instance.m_placementGhost.GetComponent<Collider>(), out plantObject) && FarmGrid_Patch.HasOverlappingPlants(plantObject.position, plantObject.growthSize))
			{
				__result = false;
				return false;
			}
			FarmGrid_Patch.otherPlantCollider = Vector3.zero;
			return true;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000237C File Offset: 0x0000057C
		[HarmonyPatch(typeof(Humanoid), "SetupVisEquipment")]
		[HarmonyPostfix]
		private static void SetupVisEquipment(ref Humanoid __instance)
		{
			if (__instance == Player.m_localPlayer && (Player.m_localPlayer == null || Player.m_localPlayer.m_rightItem == null || Player.m_localPlayer.m_rightItem.m_shared == null || Player.m_localPlayer.m_rightItem.m_shared.m_name != "$item_cultivator"))
			{
				FarmGrid_Patch.HideFarmGrid();
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000023E8 File Offset: 0x000005E8
		private static void DrawFarmGrid(Vector3 pos, Vector3 gridDir, float gridSize)
		{
			if (FarmGrid_Patch.farmGrid == null || FarmGrid_Patch.farmGrid[0] == null)
			{
				FarmGrid_Patch.InitFarmGrid();
			}
			Vector3 vector = new Vector3(gridDir.z, gridDir.y, -gridDir.x);
			ZoneSystem instance = ZoneSystem.instance;
			for (int i = -FarmGrid_Patch.farmGridSections; i <= FarmGrid_Patch.farmGridSections; i++)
			{
				Vector3 vector2 = pos + gridDir * (float)i * gridSize;
				GameObject gameObject = FarmGrid_Patch.farmGrid[i + FarmGrid_Patch.farmGridSections];
				gameObject.transform.position = vector2;
				LineRenderer component = gameObject.GetComponent<LineRenderer>();
				component.widthMultiplier = 0.015f;
				component.enabled = true;
				for (int j = -FarmGrid_Patch.farmGridSections; j <= FarmGrid_Patch.farmGridSections; j++)
				{
					Vector3 vector3 = vector2 + vector * (float)j * gridSize;
					float groundHeight = instance.GetGroundHeight(vector3);
					vector3.y = groundHeight + FarmGrid_Patch.farmGridYOffset;
					component.SetPosition(j + FarmGrid_Patch.farmGridSections, vector3);
				}
			}
			for (int k = -FarmGrid_Patch.farmGridSections; k <= FarmGrid_Patch.farmGridSections; k++)
			{
				Vector3 vector4 = pos + vector * (float)k * gridSize;
				GameObject gameObject2 = FarmGrid_Patch.farmGrid[k + FarmGrid_Patch.farmGridSections + (FarmGrid_Patch.farmGridSections * 2 + 1)];
				gameObject2.transform.position = vector4;
				LineRenderer component2 = gameObject2.GetComponent<LineRenderer>();
				component2.widthMultiplier = 0.015f;
				component2.enabled = true;
				for (int l = -FarmGrid_Patch.farmGridSections; l <= FarmGrid_Patch.farmGridSections; l++)
				{
					Vector3 vector5 = vector4 + gridDir * (float)l * gridSize;
					float groundHeight2 = instance.GetGroundHeight(vector5);
					vector5.y = groundHeight2 + FarmGrid_Patch.farmGridYOffset;
					component2.SetPosition(l + FarmGrid_Patch.farmGridSections, vector5);
				}
			}
			FarmGrid_Patch.farmGridVisible = true;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000025C8 File Offset: 0x000007C8
		private static void InitFarmGrid()
		{
			FarmGrid_Patch.farmGrid = new GameObject[(FarmGrid_Patch.farmGridSections * 2 + 1) * 2];
			for (int i = 0; i < FarmGrid_Patch.farmGrid.Length; i++)
			{
				GameObject gameObject = new GameObject();
				LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
				lineRenderer.material = Resources.FindObjectsOfTypeAll<Material>().First((Material k) => k.name == "Default-Line");
				lineRenderer.startColor = FarmGrid_Patch.farmGridColor;
				lineRenderer.endColor = FarmGrid_Patch.farmGridColor;
				lineRenderer.positionCount = FarmGrid_Patch.farmGridSections * 2 + 1;
				FarmGrid_Patch.farmGrid[i] = gameObject;
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002664 File Offset: 0x00000864
		private static void HideFarmGrid()
		{
			if (FarmGrid_Patch.farmGridVisible && FarmGrid_Patch.farmGrid != null && FarmGrid_Patch.farmGrid[0] != null)
			{
				for (int i = 0; i < FarmGrid_Patch.farmGrid.Length; i++)
				{
					FarmGrid_Patch.farmGrid[i].GetComponent<LineRenderer>().enabled = false;
				}
				FarmGrid_Patch.farmGridVisible = false;
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000026B8 File Offset: 0x000008B8
		private static FarmGrid_Patch.PlantObject GetGhostPlant()
		{
			Player localPlayer = Player.m_localPlayer;
			Collider component = localPlayer.m_placementGhost.GetComponent<Collider>();
			FarmGrid_Patch.PlantObject plantObject;
			if (component != null && FarmGrid_Patch.GetPlantObject(component, out plantObject))
			{
				return plantObject;
			}
			if (component == null)
			{
				Piece componentInParent = localPlayer.m_placementGhost.GetComponentInParent<Piece>();
				float num;
				if (componentInParent != null && FarmGrid_Patch.customPlants.TryGetValue(componentInParent.name, out num))
				{
					return new FarmGrid_Patch.PlantObject(componentInParent.transform.position, num);
				}
			}
			return null;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002734 File Offset: 0x00000934
		private static bool GetFarmSnapPoints()
		{
			Player localPlayer = Player.m_localPlayer;
			GameObject gameObject = ((localPlayer != null) ? localPlayer.m_placementGhost : null);
			if (gameObject == null)
			{
				return false;
			}
			FarmGrid_Patch.PlantObject plantGhost = FarmGrid_Patch.GetGhostPlant();
			if (plantGhost == null)
			{
				FarmGrid_Patch.HideFarmGrid();
				return false;
			}
			if (FarmGrid_Patch.plantGhostPosition == gameObject.transform.position)
			{
				if (FarmGrid_Patch.plantSnapPoint != Vector3.zero)
				{
					gameObject.transform.position = FarmGrid_Patch.plantSnapPoint;
				}
				return true;
			}
			FarmGrid_Patch.plantSnapPoint = Vector3.zero;
			FarmGrid_Patch.plantGhostPosition = plantGhost.position;
			List<FarmGrid_Patch.PlantObject> otherPlants = FarmGrid_Patch.GetOtherPlants(plantGhost.position, plantGhost.growthSize * 3.5f);
			otherPlants.OrderBy((FarmGrid_Patch.PlantObject k) => (k.position - plantGhost.position).sqrMagnitude);
			FarmGrid_Patch.PlantObject firstPlant = ((otherPlants.Count > 0) ? otherPlants[0] : null);
			if (otherPlants.Count >= 1)
			{
				if (!FarmGrid_Patch.otherPlantCollider.Equals(otherPlants[0].position))
				{
					FarmGrid_Patch.otherPlantCollider = otherPlants[0].position;
					FarmGrid_Patch.otherPlantList = FarmGrid_Patch.GetOtherPlants(otherPlants[0].position, plantGhost.growthSize * 4.5f);
					FarmGrid_Patch.otherPlantList = FarmGrid_Patch.otherPlantList.OrderBy((FarmGrid_Patch.PlantObject k) => (k.position - firstPlant.position).sqrMagnitude).ToList<FarmGrid_Patch.PlantObject>();
				}
				otherPlants = FarmGrid_Patch.otherPlantList;
			}
			if (otherPlants.Count == 1)
			{
				Vector3 vector = plantGhost.position.xz() - otherPlants[0].position.xz();
				vector.Normalize();
				float num = Mathf.Max(plantGhost.growthSize, otherPlants[0].growthSize) * 2f + FarmGrid_Patch.plantSpacing;
				FarmGrid_Patch.plantSnapPoint = otherPlants[0].position + vector * num;
				float groundHeight = ZoneSystem.instance.GetGroundHeight(FarmGrid_Patch.plantSnapPoint);
				FarmGrid_Patch.plantSnapPoint.y = groundHeight;
				gameObject.transform.position = FarmGrid_Patch.plantSnapPoint;
				FarmGrid_Patch.DrawFarmGrid(FarmGrid_Patch.plantSnapPoint, vector, num);
				return true;
			}
			if (otherPlants.Count > 1)
			{
				Vector3 gridDir = FarmGrid_Patch.GetGridDir(otherPlants, plantGhost);
				Vector3 vector2 = new Vector3(gridDir.z, gridDir.y, -gridDir.x);
				float num2 = Mathf.Max(Mathf.Max(otherPlants[1].growthSize, otherPlants[0].growthSize), plantGhost.growthSize) * 2f + FarmGrid_Patch.plantSpacing;
				List<Vector3> list = new List<Vector3>();
				for (int i = -2; i <= 2; i++)
				{
					for (int j = -2; j <= 2; j++)
					{
						if (i != 0 || j != 0)
						{
							Vector3 vector3 = firstPlant.position + gridDir * (float)i * num2 + vector2 * (float)j * num2;
							list.Add(vector3);
						}
					}
				}
				list = list.OrderBy((Vector3 k) => (k - plantGhost.position).sqrMagnitude).ToList<Vector3>();
				FarmGrid_Patch.plantSnapPoint = Vector3.zero;
				for (int l = 0; l < list.Count; l++)
				{
					if (!FarmGrid_Patch.HasOverlappingPlants(list[l], plantGhost.growthSize))
					{
						FarmGrid_Patch.plantSnapPoint = list[l];
						break;
					}
				}
				if (FarmGrid_Patch.plantSnapPoint == Vector3.zero)
				{
					FarmGrid_Patch.plantSnapPoint = list[0];
				}
				float groundHeight2 = ZoneSystem.instance.GetGroundHeight(FarmGrid_Patch.plantSnapPoint);
				FarmGrid_Patch.plantSnapPoint.y = groundHeight2;
				gameObject.transform.position = FarmGrid_Patch.plantSnapPoint;
				FarmGrid_Patch.DrawFarmGrid(FarmGrid_Patch.plantSnapPoint, gridDir, num2);
				return true;
			}
			FarmGrid_Patch.HideFarmGrid();
			return true;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002B14 File Offset: 0x00000D14
		private static Vector3 GetGridDir(List<FarmGrid_Patch.PlantObject> plantList, FarmGrid_Patch.PlantObject plantGhost)
		{
			Vector3 vector;
			for (int i = 0; i < plantList.Count - 1; i++)
			{
				for (int j = 1 + i; j < plantList.Count; j++)
				{
					vector = plantList[i].position.xz() - plantList[j].position.xz();
					float num = Mathf.Max(Mathf.Max(plantList[i].growthSize, plantList[j].growthSize), plantGhost.growthSize) * 2f + FarmGrid_Patch.plantSpacing;
					float magnitude = vector.magnitude;
					float num2 = Mathf.Floor(magnitude / (num - 0.01f));
					if (Mathf.Abs(magnitude - num2 * num) <= num2 * 0.01f)
					{
						vector.Normalize();
						return vector;
					}
				}
			}
			vector = plantList[0].position.xz() - plantList[1].position.xz();
			vector.Normalize();
			return vector;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002C14 File Offset: 0x00000E14
		private static List<FarmGrid_Patch.PlantObject> GetOtherPlants(Vector3 pos, float collisionRadius)
		{
			List<FarmGrid_Patch.PlantObject> list = new List<FarmGrid_Patch.PlantObject>();
			if (FarmGrid_Patch.plantObjectMask == 0)
			{
				FarmGrid_Patch.plantObjectMask = LayerMask.GetMask(FarmGrid_Patch.plantObjectMasks.ToArray());
			}
			int num = Physics.OverlapSphereNonAlloc(pos, collisionRadius, Piece.pieceColliders, FarmGrid_Patch.plantObjectMask);
			for (int i = 0; i < num; i++)
			{
				FarmGrid_Patch.PlantObject plantObject;
				if (FarmGrid_Patch.GetPlantObject(Piece.pieceColliders[i], out plantObject))
				{
					list.Add(plantObject);
				}
			}
			return list;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002C78 File Offset: 0x00000E78
		private static bool HasOverlappingPlants(Vector3 pos, float collisionRadius)
		{
			if (FarmGrid_Patch.plantObjectMask == 0)
			{
				FarmGrid_Patch.plantObjectMask = LayerMask.GetMask(FarmGrid_Patch.plantObjectMasks.ToArray());
			}
			int num = Physics.OverlapSphereNonAlloc(pos, collisionRadius, Piece.pieceColliders, FarmGrid_Patch.plantObjectMask);
			for (int i = 0; i < num; i++)
			{
				FarmGrid_Patch.PlantObject plantObject;
				if (FarmGrid_Patch.GetPlantObject(Piece.pieceColliders[i], out plantObject) && (pos - plantObject.position).magnitude <= collisionRadius + plantObject.growthSize)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002CF0 File Offset: 0x00000EF0
		private static bool GetPlantObject(Collider collider, out FarmGrid_Patch.PlantObject plantObject)
		{
			if (collider == null)
			{
				plantObject = null;
				return false;
			}
			Plant componentInParent = collider.GetComponentInParent<Plant>();
			if (componentInParent != null)
			{
				plantObject = new FarmGrid_Patch.PlantObject(collider.transform.position, componentInParent.m_growRadius);
				return true;
			}
			Piece componentInParent2 = collider.GetComponentInParent<Piece>();
			float contactOffset;
			if (componentInParent2 != null && FarmGrid_Patch.customPlants.TryGetValue(componentInParent2.name, out contactOffset))
			{
				if (contactOffset == 0f)
				{
					contactOffset = collider.contactOffset;
				}
				plantObject = new FarmGrid_Patch.PlantObject(collider.transform.position, contactOffset);
				return true;
			}
			plantObject = null;
			return false;
		}

		// Token: 0x0400000D RID: 13
		public static float plantSpacing = 0.01f;

		// Token: 0x0400000E RID: 14
		public static int farmGridSections = 2;

		// Token: 0x0400000F RID: 15
		public static float farmGridYOffset = 0.2f;

		// Token: 0x04000010 RID: 16
		public static Color farmGridColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);

		// Token: 0x04000011 RID: 17
		public static Dictionary<string, float> customPlants = new Dictionary<string, float>();

		// Token: 0x04000012 RID: 18
		public static List<string> plantObjectMasks = new List<string> { "piece", "piece_nonsolid" };

		// Token: 0x04000013 RID: 19
		private static Vector3 plantSnapPoint;

		// Token: 0x04000014 RID: 20
		private static GameObject[] farmGrid = null;

		// Token: 0x04000015 RID: 21
		private static bool farmGridVisible = false;

		// Token: 0x04000016 RID: 22
		private static Vector3 plantGhostPosition;

		// Token: 0x04000017 RID: 23
		private static Vector3 otherPlantCollider;

		// Token: 0x04000018 RID: 24
		private static List<FarmGrid_Patch.PlantObject> otherPlantList;

		// Token: 0x04000019 RID: 25
		private static int plantObjectMask;

		// Token: 0x02000005 RID: 5
		private class PlantObject
		{
			// Token: 0x06000012 RID: 18 RVA: 0x00002E07 File Offset: 0x00001007
			public PlantObject(Vector3 position, float growthSize)
			{
				this.position = position;
				this.growthSize = growthSize;
			}

			// Token: 0x0400001A RID: 26
			public Vector3 position;

			// Token: 0x0400001B RID: 27
			public float growthSize;
		}
	}
}
