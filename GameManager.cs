using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using VeinEngine.Engine;
using Newtonsoft.Json;

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

		protected Camera camera;

		public static bool Fullbright = false;
		protected static bool MouseLock = true;

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
			_graphics.ApplyChanges();

			camera = new Camera(new Vector3(0,1,0),new Vector3(90, 180, 0),MathHelper.ToRadians(90),0.1f,100f);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
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

			if (Keyboard.GetState().IsKeyDown(Keys.A))
			{
				camera.position.X -= MathF.Cos(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				camera.position.Z += MathF.Sin(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.D))
			{
				camera.position.X += MathF.Cos(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				camera.position.Z -= MathF.Sin(MathHelper.ToRadians(camera.rotation.Y)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.W))
			{
				float xsin = MathF.Cos(MathHelper.ToRadians(camera.rotation.X));
				camera.position.X -= MathF.Sin(MathHelper.ToRadians(camera.rotation.Y)) * xsin * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				camera.position.Z -= MathF.Cos(MathHelper.ToRadians(camera.rotation.Y)) * xsin * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				camera.position.Y += MathF.Sin(MathHelper.ToRadians(camera.rotation.X)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.S))
			{
				float xsin = MathF.Cos(MathHelper.ToRadians(camera.rotation.X));
				camera.position.X += MathF.Sin(MathHelper.ToRadians(camera.rotation.Y)) * xsin * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				camera.position.Z += MathF.Cos(MathHelper.ToRadians(camera.rotation.Y)) * xsin * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
				camera.position.Y -= MathF.Sin(MathHelper.ToRadians(camera.rotation.X)) * (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
			}
			if (KeyboardIN.HasBeenPressed(Keys.F))
			{
				Fullbright = !Fullbright;
			}
			if (KeyboardIN.HasBeenPressed(Keys.OemTilde))
			{
				MouseLock = !MouseLock;
			}

			camera.UpdateMatrices();

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// TODO: Add your drawing code here

			base.Draw(gameTime);
		}
	}
}
