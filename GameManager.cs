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
using System.Threading;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.BroadPhaseEntries;

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
		public static bool SoftShadows = true;
		protected static bool MouseLock = true;

		static bool TestLight = false;

		Model map;
		Texture2D smap;
		int texelsresolution = 12;
		public Texture2D tex;

		Model defaultCube;

		public List<WorldObject> loadedEntities = new List<WorldObject>();
		WorldObject player;

		int castX, castZ;

		Listener3D listner;

		// Make a render target, 
		RenderTarget2D rt2d;

		// Make a global or static Depth stencil state.
		// Note don't just make it and then set it in draw. *** It should be premade like so.*** 
		// Re-making every frame would cause a ton of garbage collections.
		DepthStencilState ds_depth_on_less_than = new DepthStencilState() { DepthBufferEnable = true, DepthBufferFunction = CompareFunction.Less };


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
			rt2d = new RenderTarget2D(GraphicsDevice, texelsresolution, (int)(texelsresolution / (GraphicsDevice.Viewport.AspectRatio)));
			space.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);
			smap = new Texture2D(GraphicsDevice, (int)(GraphicsDevice.Viewport.Width / texelsresolution), (int)(GraphicsDevice.Viewport.Height/texelsresolution));
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

		void ShadowCast()
		{
			Color[] data = new Color[smap.Width * smap.Height];
			if (Fullbright)
			{
				smap.SetData(data);
				return;
			}
			smap.GetData(data);

			SetColors(data);
		}

		void SetColors(Color[] data)
		{
			for (int x = 0; x < smap.Width; x++)
			{
				for (int z = 0; z < smap.Height; z++)
				{
					castX = x;
					castZ = z;

					Color col = ColorCast();

					data[z * smap.Width + x] = col;
				}
			}
			if(SoftShadows)
			{
				for (int x = 1; x < smap.Width - 1; x++)
				{
					for (int z = 1; z < smap.Height - 1; z++)
					{
						int averageR = (data[z * smap.Width + x].R + data[z * smap.Width + (x - 1)].R +
							data[z * smap.Width + (x + 1)].R + data[(z - 1) * smap.Width + x].R + data[(z + 1) * smap.Width + x].R) / 5;

						int averageG = (data[z * smap.Width + x].G + data[z * smap.Width + (x - 1)].G +
							data[z * smap.Width + (x + 1)].G + data[(z - 1) * smap.Width + x].G + data[(z + 1) * smap.Width + x].G) / 5;

						int averageB = (data[z * smap.Width + x].B + data[z * smap.Width + (x - 1)].B +
							data[z * smap.Width + (x + 1)].B + data[(z - 1) * smap.Width + x].B + data[(z + 1) * smap.Width + x].B) / 5;

						int averageA = (data[z * smap.Width + x].A + data[z * smap.Width + (x - 1)].A +
							data[z * smap.Width + (x + 1)].A + data[(z - 1) * smap.Width + x].A + data[(z + 1) * smap.Width + x].A);

						averageA += (data[(z - 1) * smap.Width + x].A + data[(z - 1) * smap.Width + (x - 1)].A +
							data[(z - 1) * smap.Width + (x + 1)].A + data[(z - 1) * smap.Width + x - 1].A + data[(z + 1) * smap.Width + x - 1].A);

						averageA += (data[(z - 1) * smap.Width + x].A + data[(z - 1) * smap.Width + (x - 1)].A +
							data[(z - 1) * smap.Width + (x + 1)].A + data[(z - 1) * smap.Width + x + 1].A + data[(z + 1) * smap.Width + x + 1].A);

						averageA /= 15;

						data[z * smap.Width + x] = new Color(averageR, averageG, averageB, averageA);
					}
				}
			}
			smap.SetData(data);

			_spriteBatch.Begin();

			_spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

			_spriteBatch.Draw(smap, GraphicsDevice.Viewport.Bounds, Color.White);

			_spriteBatch.End();
		}
		Color ColorCast()
		{
			Color _color;
			_color = new Color(0, 0, 0, 0);

			Vector2 p = new Vector2(((float)castX / (float)smap.Width) * GraphicsDevice.Viewport.Width,
												((float)castZ / ((float)smap.Height)) * GraphicsDevice.Viewport.Height);

			//Ray screenRay = GetScreenVector2AsRayInto3dWorld(p, Camera.main.projectionMatrix, Camera.main.viewMatrix, Camera.main.worldMatrix, 0.3f, GraphicsDevice);

			Vector3 nearWorldPoint = GraphicsDevice.Viewport.Unproject(new Vector3(p.X, p.Y, 0), Camera.main.projectionMatrix, Camera.main.viewMatrix, Camera.main.worldMatrix);
			Vector3 farWorldPoint = GraphicsDevice.Viewport.Unproject(new Vector3(p.X, p.Y, 1), Camera.main.projectionMatrix, Camera.main.viewMatrix, Camera.main.worldMatrix);

			Ray screenRay = new Ray(nearWorldPoint, farWorldPoint - nearWorldPoint);
			screenRay.Position -= (Camera.main.viewOffset / 2f);
			RayCastResult hit;
			BEPUutilities.Ray ray = new BEPUutilities.Ray(new BEPUutilities.Vector3(screenRay.Position.X, screenRay.Position.Y, screenRay.Position.Z), new BEPUutilities.Vector3(screenRay.Direction.X, screenRay.Direction.Y, screenRay.Direction.Z));

			if (space.RayCast(ray, 1, RayCastFilter, out hit))
			{
				RayCastResult hit2;

				Vector3 direction = TestLight ? Vector3.UnitY * 8 - new Vector3(hit.HitData.Location.X, hit.HitData.Location.Y, hit.HitData.Location.Z) : new Vector3(1f, 1f, -0.6f);

				Ray toLight = new Ray(new Vector3(hit.HitData.Location.X + hit.HitData.Normal.X * 0.1f, hit.HitData.Location.Y + hit.HitData.Normal.Y * -0.1f,
					hit.HitData.Location.Z + hit.HitData.Normal.Z * 0.1f), direction);
				ray = new BEPUutilities.Ray(new BEPUutilities.Vector3(toLight.Position.X, toLight.Position.Y, toLight.Position.Z), new BEPUutilities.Vector3(toLight.Direction.X, toLight.Direction.Y, toLight.Direction.Z));

				if (space.RayCast(ray, 100, RayCastFilter2, out hit2))
				{
					_color = new Color(0, 0, 0, 150);
				}
			}
			return _color;
		}
		bool RayCastFilter(BroadPhaseEntry entry)
		{
			return entry != player.GetBehaviour<PlayerBehaviour>().collisionBox.CollisionInformation && entry.CollisionRules.Personal <= CollisionRule.Normal;
		}
		bool RayCastFilter2(BroadPhaseEntry entry)
		{
			return entry.CollisionRules.Personal <= CollisionRule.Normal;
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.SkyBlue);

			GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			foreach (WorldObject ent in loadedEntities) ent.Render();

			ShadowCast();

			base.Draw(gameTime);
		}

		protected override void UnloadContent()
		{
			loadedEntities.Clear();

			GraphicsDevice.Dispose();

			FMODManager.Unload();

			base.UnloadContent();
		}

		/// <summary>
		/// Near plane is typically just 0 in this function or some extremely small value. Far plane cant be more then one and so i just folded it automatically. In truth this isnt the near plane its the min clip but whatever.
		/// </summary>
		public Ray GetScreenVector2AsRayInto3dWorld(Vector2 screenPosition, Matrix projectionMatrix, Matrix viewMatrix, Matrix cameraWorld, float near, GraphicsDevice device)
		{
			//if (far > 1.0f) // this is actually a misnomer which caused me a headache this is supposed to be the max clip value not the far plane.
			//    throw new ArgumentException("Far Plane can't be more then 1f or this function will fail to work in many cases");
			Vector3 nearScreenPoint = new Vector3(screenPosition.X, screenPosition.Y, near); // must be more then zero.
			Vector3 nearWorldPoint = Unproject(nearScreenPoint, projectionMatrix, viewMatrix, Matrix.Identity, device);

			Vector3 farScreenPoint = new Vector3(screenPosition.X, screenPosition.Y, 1f); // the projection matrice's far plane value.
			Vector3 farWorldPoint = Unproject(farScreenPoint, projectionMatrix, viewMatrix, Matrix.Identity, device);

			Vector3 worldRaysNormal = Vector3.Normalize((farWorldPoint + nearWorldPoint) - nearWorldPoint);
			return new Ray(nearWorldPoint, worldRaysNormal);
		}

		/// <summary>
		/// Note the source position internally expects a Vector3 with a z value.
		/// That Z value can Not excced 1.0f or the function will error. I leave it as is for future advanced depth selection functionality which should be apparent.
		/// </summary>
		public Vector3 Unproject(Vector3 position, Matrix projection, Matrix view, Matrix world, GraphicsDevice gd)
		{
			if (position.Z > gd.Viewport.MaxDepth)
				throw new Exception("Source Z must be less than MaxDepth ");
			Matrix wvp = Matrix.Multiply(view, projection);
			Matrix inv = Matrix.Invert(wvp);
			Vector3 clipSpace = position;
			clipSpace.X = (((position.X - gd.Viewport.X) / ((float)gd.Viewport.Width)) * 2f) - 1f;
			clipSpace.Y = -((((position.Y - gd.Viewport.Y) / ((float)gd.Viewport.Height)) * 2f) - 1f);
			clipSpace.Z = (position.Z - gd.Viewport.MinDepth) / (gd.Viewport.MaxDepth - gd.Viewport.MinDepth); // >> Oo <<
			Vector3 invsrc = Vector3.Transform(clipSpace, inv);
			float a = (((clipSpace.X * inv.M14) + (clipSpace.Y * inv.M24)) + (clipSpace.Z * inv.M34)) + inv.M44;
			return invsrc / a;
		}
	}
}
