using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;

namespace VeinEngine.Engine.Shadows
{
	public class ShadowRenderer
	{
		public Vector3 direction = new Vector3(-0.2f, -1f, 0.6f);
		public int texelsresolution = 80;
		public Texture2D map;
		
		public void CalculateShadows()
		{
			map = new Texture2D(GameManager._instance.GraphicsDevice,texelsresolution,texelsresolution);

			// Assume you have a Texture2D called texture

			Color[] data = new Color[map.Width * map.Height];
			map.GetData(data);

			for (int x = 0; x < texelsresolution; x++)
			{
				for (int z = 0; z < texelsresolution; z++)
				{
					data[z * map.Width + x] = Color.White;

					RayCastResult hit;
					BEPUutilities.Ray ray = new BEPUutilities.Ray(new BEPUutilities.Vector3((x - texelsresolution / 2f), 2,(z - texelsresolution / 2f)),new BEPUutilities.Vector3(direction.X,direction.Y,direction.Z));

					if(GameManager._instance.space.RayCast(ray, out hit))
					{
						data[z * map.Width + x] = new Color(hit.HitData.T, hit.HitData.T, hit.HitData.T);
					}
				}
			}

			// Once you have finished changing data, set it back to the texture:

			map.SetData(data);
		}
	}
}
