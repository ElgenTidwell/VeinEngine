using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeinEngine.Engine.BaseComponents
{
	public class StaticMeshRenderer : Behaviour
	{
		public BoundingBox bounds;
		public Model mesh;
		public Texture2D texture;
		public override void Start()
		{
			//TODO: compute mesh boundaries
		}

		public override void Update()
		{
			//update is practically useless, as this is a static mesh that doesnt animate.
		}


		public override void Render()
		{
			if(!parentObject.useMatrix)
				Camera.main.RenderModel(mesh,texture,parentObject.position,parentObject.rotation);
			else
				Camera.main.RenderModel(mesh, texture, parentObject.transformationMatrix);
		}
	}
}
