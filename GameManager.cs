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
using VeinEngine.Engine.Shadows;

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

namespace VeinEngine
{
	public class GameManager : Game
	{
		public static GameManager _instance;

		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		public Space space;

		public static bool Fullbright = false;
		protected static bool MouseLock = true;

		Model map;
		public Texture2D tex;

		Model defaultCube;

		public List<WorldObject> loadedEntities = new List<WorldObject>();
		WorldObject player;

		Listener3D listner;

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

			space = new Space();

			//space.Add(new Box(new BEPUutilities.Vector3(0,-3f,0),600,5,600));

			space.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			map = Content.Load<Model>("Models/testmodel");
			tex = Content.Load<Texture2D>("Textures/UVGrid");
			defaultCube = Content.Load<Model>("Models/untitled");

			FMODManager.Init(FMODMode.CoreAndStudio,"Content/Audio");

			var song = CoreSystem.LoadStreamedSound("missing.wav");
			//var channel = song.Play();
			//channel.Looping = true;
			//channel.Volume = 0.2f;

			listner = new Listener3D();

			listner.UpOrientation = Vector3.Up;
			listner.ForwardOrientation = Vector3.Forward;

			//hardcoded entity creation, hoping to remove this with the use of a scene/map system
			{
				player = new WorldObject();
				player.position = Vector3.Up * 10;
				player.AddBehaviour(new PlayerBehaviour());
				player.Start();

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

				WorldObject floor = new WorldObject();
				var smc = new StaticMeshCollider();
				smc.mesh = map;
				floor.AddBehaviour(smc);
				floor.Start();

				loadedEntities.Add(player);
				loadedEntities.Add(mapObj);
				loadedEntities.Add(rigidbodyTest);
				loadedEntities.Add(floor);
			}
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			space.Update();

			for(int i = loadedEntities.Count-1; i >= 0; i --)
			{
				loadedEntities[i].Update(gameTime);
			}

			listner.Position3D = player.GetBehaviour<PlayerBehaviour>().camera.position;
			listner.ForwardOrientation = player.GetBehaviour<PlayerBehaviour>().camForward;
			listner.UpOrientation = player.GetBehaviour<PlayerBehaviour>().camUp;

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.SetRenderTarget(null);
			GraphicsDevice.Clear(Color.SkyBlue);

			GraphicsDevice.DepthStencilState = DepthStencilState.Default;

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
