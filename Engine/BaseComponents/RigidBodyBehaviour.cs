using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeinEngine.Engine.BaseComponents
{
	public class RigidBodyBehaviour : Behaviour
	{
		Box box;
		public float mass = 0.3f;
		public Vector3 bounds = Vector3.One * 2;
		public override void Render()
		{
		}

		public override void Start()
		{
			box = new Box(new BEPUutilities.Vector3(parentObject.position.X, parentObject.position.Y, parentObject.position.Z), bounds.X, bounds.Y, bounds.Z, mass);

			var q = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(parentObject.rotation.Y), MathHelper.ToRadians(parentObject.rotation.X), MathHelper.ToRadians(parentObject.rotation.Z));

			box.Orientation = new BEPUutilities.Quaternion(q.X,q.Y,q.Z,q.W);
			
			GameManager._instance.space.Add(box);
		}

		public override void Update()
		{
			BEPUutilities.Matrix w = box.WorldTransform;

			Matrix mat = new Matrix(w.M11,w.M12,w.M13,w.M14,w.M21,w.M22,w.M23,w.M24,w.M31,w.M32,w.M33,w.M34,w.M41,w.M42,w.M43,w.M44);

			parentObject.transformationMatrix = mat;
			parentObject.useMatrix = true;

			parentObject.position = new Vector3(w.Translation.X, w.Translation.Y, w.Translation.Z);

			UpdateRotations();
		}

		void UpdateRotations()
		{
			BEPUutilities.Quaternion r = box.Orientation;

			Quaternion q = new Quaternion(r.X, r.Y, r.Z, r.W);

			parentObject.rotation = CreateEulerFromYawPitchRoll(q);
		}

		public static Vector3 CreateEulerFromYawPitchRoll(Quaternion r)
		{
			Vector3 rot;

			rot.Y = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
			rot.X = MathF.Asin(2.0f * (r.X * r.W - r.Y * r.Z));
			rot.Z = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
			return rot;
		}
	}
}
