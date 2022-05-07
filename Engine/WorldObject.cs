using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeinEngine.Engine
{
	public class WorldObject
	{
		public WorldObject()
		{
			components = new List<Behaviour>();
			position = Vector3.Zero;
			rotation = Vector3.Zero;
		}

		List<Behaviour> components;

		public Vector3 position, rotation;
		public Matrix transformationMatrix;
		public bool useMatrix;

		public void Start()
		{
			foreach (var b in components) 
			{
				b.parentObject = this;
				b.Start();
			}
		}
		public void Update(GameTime gameTime)
		{
			foreach (var b in components) b.Update(gameTime);
		}
		public void Render()
		{
			foreach (var b in components) b.Render();
		}
		public Behaviour GetBehaviour(Type t)
		{
			Behaviour be = Array.Find(components.ToArray(),check => check.GetType() == t);

			return be;
		}
		public T GetBehaviour<T>()
		{
			object be = Array.Find(components.ToArray(), check => check.GetType() == typeof(T));

			return (T)be;
		}
		public void AddBehaviour(Behaviour b)
		{
			components.Add(b);
		}
	}
	public abstract class Behaviour
	{
		public WorldObject parentObject;

		public abstract void Start();
		public abstract void Update(GameTime gameTime);
		public abstract void Render();
	}
}
