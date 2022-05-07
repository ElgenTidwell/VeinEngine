using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeinEngine.Engine.BaseComponents
{
	public class PlayerBehaviour : Behaviour
	{
		public Camera camera;
		public Vector3 camForward, camRight, camUp;
		public Capsule collisionBox;

		Texture2D tex;
		Model defaultCube;
		Vector3 wishdir;
		bool MouseLock = true;

		public override void Start()
		{
			tex = GameManager._instance.Content.Load<Texture2D>("Textures/UVGrid");
			defaultCube = GameManager._instance.Content.Load<Model>("Models/untitled");

			camera = new Camera(new Vector3(0, 1, 0), new Vector3(90, 180, 0), MathHelper.ToRadians(90), 0.03f, 100f);

			collisionBox = new Capsule(new BEPUutilities.Vector3(parentObject.position.X, parentObject.position.Y, parentObject.position.Z), 2, 0.2f, 0.5f);
			collisionBox.LocalInertiaTensorInverse = new BEPUutilities.Matrix3x3();

			camera.viewOffset = Vector3.Up;

			GameManager._instance.space.Add(collisionBox);
		}

		public override void Update(GameTime gameTime)
		{
			MouseState state = Mouse.GetState();

			Point mouseRelativeToCenter = new Point(state.X - GameManager._instance.GraphicsDevice.Viewport.Width / 2, state.Y - GameManager._instance.GraphicsDevice.Viewport.Height / 2);

			if (MouseLock)
			{
				camera.rotation.Y -= mouseRelativeToCenter.X * 10f * (float)gameTime.ElapsedGameTime.TotalSeconds;
				camera.rotation.X -= mouseRelativeToCenter.Y * 10f * (float)gameTime.ElapsedGameTime.TotalSeconds;

				Mouse.SetPosition(GameManager._instance.GraphicsDevice.Viewport.Width / 2, GameManager._instance.GraphicsDevice.Viewport.Height / 2);
				GameManager._instance.IsMouseVisible = false;
			}
			else
			{
				GameManager._instance.IsMouseVisible = true;
			}

			camera.rotation.X = MathF.Max(MathF.Min(camera.rotation.X, 90), -90);

			KeyboardIN.GetState();
			wishdir = Vector3.Lerp(wishdir, Vector3.Zero, (float)gameTime.ElapsedGameTime.TotalSeconds * 10);

			if (Keyboard.GetState().IsKeyDown(Keys.A))
			{
				wishdir.X += -MathF.Cos(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				wishdir.Z += MathF.Sin(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.D))
			{
				wishdir.X += MathF.Cos(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				wishdir.Z += -MathF.Sin(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.W))
			{
				wishdir.X += -MathF.Sin(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				wishdir.Z += -MathF.Cos(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				//wishdir.Y = MathF.Sin(MathHelper.ToRadians(camera.rotation.X)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.S))
			{
				wishdir.X += MathF.Sin(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				wishdir.Z += MathF.Cos(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				//wishdir.Y = -MathF.Sin(MathHelper.ToRadians(camera.rotation.X)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
			}
			if (KeyboardIN.HasBeenPressed(Keys.F))
			{
				GameManager.Fullbright = !GameManager.Fullbright;
			}
			if (KeyboardIN.HasBeenPressed(Keys.OemTilde))
			{
				MouseLock = !MouseLock;
			}
			if (state.RightButton == ButtonState.Pressed)
			{

			}
			if (state.RightButton == ButtonState.Released)
			{

			}

			float xsin = MathF.Cos(MathHelper.ToRadians(camera.rotation.X));

			camForward.X = -MathF.Sin(MathHelper.ToRadians(camera.rotation.Y)) * xsin;
			camForward.Z = -MathF.Cos(MathHelper.ToRadians(camera.rotation.Y)) * xsin;
			camForward.Y = +MathF.Sin(MathHelper.ToRadians(camera.rotation.X));

			camRight.X = MathF.Cos(MathHelper.ToRadians(camera.rotation.Y));
			camRight.Y = MathF.Sin(MathHelper.ToRadians(camera.rotation.Z));
			camRight.Z = -MathF.Sin(MathHelper.ToRadians(camera.rotation.Y));

			camForward.Normalize();
			camRight.Normalize();

			camUp = Vector3.Cross(camForward, camRight);

			camUp.Normalize();

			if (KeyboardIN.HasBeenPressed(Keys.Space))
			{
				WorldObject rigidbodyTest = new WorldObject();
				rigidbodyTest.position = camera.position + Vector3.Up + camForward * 3;
				rigidbodyTest.rotation = camForward;

				StaticMeshRenderer smr2 = new StaticMeshRenderer();
				smr2.mesh = defaultCube;
				smr2.texture = tex;

				RigidBodyBehaviour rbb = new RigidBodyBehaviour();
				rbb.mass = 1.5f;

				rigidbodyTest.AddBehaviour(smr2);
				rigidbodyTest.AddBehaviour(rbb);

				rigidbodyTest.Start();

				GameManager._instance.loadedEntities.Add(rigidbodyTest);
			}


			wishdir = Vector3.Clamp(wishdir, -Vector3.One, Vector3.One);

			collisionBox.LinearVelocity = new BEPUutilities.Vector3(wishdir.X * 12, collisionBox.LinearVelocity.Y, wishdir.Z * 12);

			Vector3 p = new Vector3(collisionBox.Position.X, collisionBox.Position.Y, collisionBox.Position.Z);
			camera.position = p;
			parentObject.position = p;
			camera.UpdateMatrices();

			collisionBox.Position = new BEPUutilities.Vector3(parentObject.position.X, parentObject.position.Y, parentObject.position.Z);
		}

		//render is useless
		public override void Render() { }
	}
}
