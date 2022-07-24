using System;
using UnityEngine;

namespace FarmGrid.Patches
{
	// Token: 0x02000003 RID: 3
	public static class Vector3Ext
	{
		// Token: 0x06000003 RID: 3 RVA: 0x0000225E File Offset: 0x0000045E
		public static Vector3 xz(this Vector3 v)
		{
			return new Vector3(v.x, 0f, v.z);
		}
	}
}
