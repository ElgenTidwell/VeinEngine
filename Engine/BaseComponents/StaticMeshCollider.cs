using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeinEngine.Engine.BaseComponents
{
	public class StaticMeshCollider : Behaviour
	{
		public Model mesh;
		List<Box> boxes;

		public override void Render()
		{
			
		}

		public override void Start()
		{
			boxes = new List<Box>();
			var i = 0;
			foreach(ModelMesh m in mesh.Meshes)
			{
				foreach (ModelMeshPart p in m.MeshParts)
				{
					BoundingBox computed = ModelMath.GetMeshPartBounds(p);
					Vector3 bounds = computed.Max*2;
					Vector3 center = (computed.Min + computed.Max);
					
					boxes.Add(new Box(new BEPUutilities.Vector3(center.X,center.Y, center.Z),bounds.X,bounds.Y,bounds.Z));

					GameManager._instance.space.Add(boxes[i]);
					i++;
				}
			}
		}

		public override void Update(GameTime gameTime)
		{
			
		}
	}
}
