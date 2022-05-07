using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using VeinEngine.Engine;
using Newtonsoft.Json;
using ChaiFoxes.FMODAudio; //thank you, you are a godsend
using System.Collections.Generic;
using VeinEngine.Engine.BaseComponents;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;

namespace VeinEngine
{
	public class KeyboardIN
	{
		static KeyboardState currentKeyState;
		static KeyboardState previousKeyState;

		public static KeyboardState GetState()
		{
			previousKeyState = currentKeyState;
			currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
			return currentKeyState;
		}

		public static bool IsPressed(Keys key)
		{
			return currentKeyState.IsKeyDown(key);
		}

		public static bool HasBeenPressed(Keys key)
		{
			return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
		}
	}
	public class GameManager : Game
	{
		public static GameManager _instance;

		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		public Space space;

		protected Camera camera;

		public static bool Fullbright = false;
		protected static bool MouseLock = true;

		Model map;
		Texture2D tex;

		Model defaultCube;

		List<WorldObject> loadedEntities = new List<WorldObject>();

		Listener3D listner;
		Vector3 wishdir = Vector3.Zero;

		public GameManager()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			Window.AllowAltF4 = true;

			_instance = this;
		}

		protected override void Initialize()
		{
			_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			_graphics.IsFullScreen = true;
			_graphics.PreferMultiSampling = true;

			_graphics.ApplyChanges();

			camera = new Camera(new Vector3(0,1,0),new Vector3(90, 180, 0),MathHelper.ToRadians(90),0.03f,100f);

			space = new Space();

			space.Add(new Box(new BEPUutilities.Vector3(0,-3f,0),600,5,600));

			space.Add(camera.collisionBox);

			space.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			map = Content.Load<Model>("Models/trainstation_test_notex");
			tex = Content.Load<Texture2D>("Textures/UVGrid");
			defaultCube = Content.Load<Model>("Models/untitled");

			FMODManager.Init(FMODMode.CoreAndStudio,"Content/Audio");

			var song = CoreSystem.LoadStreamedSound("missing.wav");
			var channel = song.Play();
			channel.Looping = true;
			channel.Volume = 0.2f;

			listner = new Listener3D();

			listner.UpOrientation = Vector3.Up;
			listner.ForwardOrientation = Vector3.Forward;

			WorldObject mapObj = new WorldObject();

			StaticMeshRenderer smr = new StaticMeshRenderer();
			smr.mesh = map;
			smr.texture = tex;

			mapObj.AddBehaviour(smr);

			mapObj.Start();

			WorldObject rigidbodyTest = new WorldObject();
			rigidbodyTest.position = new Vector3(1, 12, 2);

			StaticMeshRenderer smr2 = new StaticMeshRenderer();
			smr2.mesh = defaultCube;
			smr2.texture = tex;

			RigidBodyBehaviour rbb = new RigidBodyBehaviour();
			rbb.mass = 1.5f;

			rigidbodyTest.AddBehaviour(smr2);
			rigidbodyTest.AddBehaviour(rbb);

			rigidbodyTest.Start();

			loadedEntities.Add(mapObj);
			loadedEntities.Add(rigidbodyTest);
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			MouseState state = Mouse.GetState();

			Point mouseRelativeToCenter = new Point(state.X - GraphicsDevice.Viewport.Width / 2, state.Y - GraphicsDevice.Viewport.Height / 2);

			if(MouseLock)
			{
				camera.rotation.Y -= mouseRelativeToCenter.X * 10f * (float)gameTime.ElapsedGameTime.TotalSeconds;
				camera.rotation.X -= mouseRelativeToCenter.Y * 10f * (float)gameTime.ElapsedGameTime.TotalSeconds;

				Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
				IsMouseVisible = false;
			}
			else
			{
				IsMouseVisible = true;
			}

			camera.rotation.X = MathF.Max(MathF.Min(camera.rotation.X,90),-90);

			KeyboardIN.GetState();
			wishdir = Vector3.Lerp(wishdir,Vector3.Zero, (float)gameTime.ElapsedGameTime.TotalSeconds * 10);

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
				Fullbright = !Fullbright;
			}
			if (KeyboardIN.HasBeenPressed(Keys.OemTilde))
			{
				MouseLock = !MouseLock;
			}

			float xsin = MathF.Cos(MathHelper.ToRadians(camera.rotation.X));
			Vector3 camForward,camRight,camUp;
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
				rigidbodyTest.position = camera.position + Vector3.Up + camForward*18;
				rigidbodyTest.rotation = camForward;

				StaticMeshRenderer smr2 = new StaticMeshRenderer();
				smr2.mesh = defaultCube;
				smr2.texture = tex;

				RigidBodyBehaviour rbb = new RigidBodyBehaviour();
				rbb.mass = 1.5f;

				rigidbodyTest.AddBehaviour(smr2);
				rigidbodyTest.AddBehaviour(rbb);

				rigidbodyTest.Start();

				loadedEntities.Add(rigidbodyTest);
			}

			wishdir = Vector3.Clamp(wishdir, -Vector3.One, Vector3.One);

			camera.collisionBox.LinearVelocity = new BEPUutilities.Vector3(wishdir.X * 12, camera.collisionBox.LinearVelocity.Y, wishdir.Z * 12);

			Vector3 p = new Vector3(camera.collisionBox.Position.X, camera.collisionBox.Position.Y, camera.collisionBox.Position.Z);
			camera.position = p;

			camera.UpdateMatrices();

			space.Update();

			foreach (WorldObject ent in loadedEntities) ent.Update();

			listner.Position3D = camera.position;
			listner.ForwardOrientation = camForward;
			listner.UpOrientation = camUp;

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.SkyBlue);

			//camera.RenderModel(map,tex,Vector3.Zero);

			foreach (WorldObject ent in loadedEntities) ent.Render();

			base.Draw(gameTime);
		}

		protected override void UnloadContent()
		{
			loadedEntities.Clear();

			GraphicsDevice.Dispose();

			FMODManager.Unload();

			base.UnloadContent();
		}
	}
}
