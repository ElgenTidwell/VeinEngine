using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeinEngine.Engine
{
	public abstract class Entity
	{
		public Vector3 position, rotation;
		public abstract void Start();
		public abstract void Update();
		public abstract void Render();

        public BoundingBox bounds;
        public Model model;
    }
}
