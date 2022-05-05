using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeinEngine.Engine
{
	/// <summary>
	/// The structure for maps in VEIN
	/// </summary>
	[Serializable]
	public struct MapInfo
	{
		public int formatVerstion;
		public Brush[] brushes;
		public TriggerBrush[] triggers;
		public EntityDefinition[] definitions;
	}
	[Serializable]
	public struct TriggerBrush
	{
		public Vector3 start, end;
		public int target;
	}
	[Serializable]
	public struct Brush
	{
		public Vector3 start, end;
		public Vector2[] uvs;
	}
	[Serializable]
	public struct EntityDefinition
	{
		public int entity;
		public int placementInHierarchy;
		public Vector3 position;
	}
}
