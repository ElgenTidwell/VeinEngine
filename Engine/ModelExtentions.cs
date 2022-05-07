using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeinEngine.Engine
{
	public static class ModelMath
	{
        public static BoundingBox GetBounds(Model m)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (ModelMesh mesh in m.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    int vertexDataSize = vertexBufferSize / sizeof(float);
                    float[] vertexData = new float[vertexDataSize];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    for (int i = 0; i < vertexDataSize; i += vertexStride / sizeof(float))
                    {
                        Vector3 vertex = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);
                        min = Vector3.Min(min, vertex);
                        max = Vector3.Max(max, vertex);
                    }
                }
            }

            return new BoundingBox(min, max);
        }
        public static BoundingBox GetMeshPartBounds(ModelMeshPart meshPart)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
            int vertexBufferSize = meshPart.NumVertices * vertexStride;

            int vertexDataSize = vertexBufferSize / sizeof(float);
            float[] vertexData = new float[vertexDataSize];
            meshPart.VertexBuffer.GetData<float>(vertexData);

            for (int i = 0; i < vertexDataSize; i += vertexStride / sizeof(float))
            {
                Vector3 vertex = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }

            return new BoundingBox(min, max);
        }
    }
}
