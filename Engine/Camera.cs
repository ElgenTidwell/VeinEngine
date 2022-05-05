using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VeinEngine.Engine
{
	public class Camera
	{
		public Vector3 position, rotation;

		public Matrix worldMatrix, viewMatrix, projectionMatrix;

		public BasicEffect basicEffect;

		BlendState _blendState;

		public Camera(Vector3 position, Vector3 rotation, float fov, float nearPlane, float farPlane)
		{
			this.position = position;
			this.rotation = rotation;

			projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fov,GameManager._instance.GraphicsDevice.Viewport.AspectRatio,nearPlane,farPlane);
			viewMatrix = Matrix.CreateTranslation(position) * Matrix.CreateFromYawPitchRoll(rotation.X,rotation.Y,rotation.Z);
			worldMatrix = Matrix.CreateWorld(Vector3.Zero,Vector3.Forward,Vector3.Up);

			_blendState = new BlendState
			{
				ColorSourceBlend = Blend.One, // multiplier of the source color
				ColorBlendFunction = BlendFunction.Add, // function to combine colors
				ColorDestinationBlend = Blend.InverseSourceAlpha, // multiplier of the destination color
				AlphaSourceBlend = Blend.One, // multiplier of the source alpha
				AlphaBlendFunction = BlendFunction.ReverseSubtract, // function to combine alpha
				AlphaDestinationBlend = Blend.One, // multiplier of the destination alpha
			};

			basicEffect = new BasicEffect(GameManager._instance.GraphicsDevice);
			basicEffect.Alpha = 1f;
			basicEffect.TextureEnabled = false;
			basicEffect.LightingEnabled = false;
		}

		public void UpdateMatrices()
		{
			viewMatrix = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(rotation.Y), MathHelper.ToRadians(rotation.X), MathHelper.ToRadians(rotation.Z));
			viewMatrix.Translation = position;

			viewMatrix = Matrix.Invert(viewMatrix);

			worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

			basicEffect.Projection = projectionMatrix;
			basicEffect.View = viewMatrix;
			basicEffect.World = worldMatrix;
		}

		public void RenderBuffer(VertexBuffer vb, int length)
		{
			if (vb == null || length <= 0) return;

			GraphicsDevice g = GameManager._instance.GraphicsDevice;

			g.SetVertexBuffer(vb);

			foreach (EffectPass effect in basicEffect.CurrentTechnique.Passes)
			{
				effect.Apply();

				g.DrawPrimitives(PrimitiveType.TriangleList, 0, length);
			}
		}
		public void RenderModel(Model m,Texture2D tex,Vector3 pos)
		{
			foreach(ModelMesh mesh in m.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.LightingEnabled = !GameManager.Fullbright;
					
					effect.DirectionalLight0.DiffuseColor = new Vector3(1.2f,1.1f,0.9f);
					effect.DirectionalLight0.Direction = new Vector3(-0.2f,-1f,0.6f);

					effect.AmbientLightColor = new Vector3(0.1f, 0.04f, 0.04f);
					effect.TextureEnabled = true;
					//effect.Texture = tex;
					effect.View = viewMatrix;
					Matrix localworld = worldMatrix;
					localworld.Translation = pos;
					effect.World = localworld;
					effect.Projection = projectionMatrix;
				}
				mesh.Draw();
			}
		}
	}
}
