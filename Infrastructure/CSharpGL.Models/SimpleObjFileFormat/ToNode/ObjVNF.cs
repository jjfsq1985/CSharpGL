﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpGL
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ObjVNF : IBufferSource
    {
        private ObjVNFMesh mesh;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public vec3 GetSize()
        {
            return mesh.Size;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public vec3 GetPosition()
        {
            return mesh.Position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        public ObjVNF(ObjVNFMesh mesh)
        {
            this.mesh = mesh;
        }

        #region IBufferSource 成员

        public const string strPosition = "position";
        private VertexBuffer positionBuffer;
        public const string strTexCoord = "texCoord";
        private VertexBuffer texCoordBuffer;
        public const string strNormal = "normal";
        private VertexBuffer normalBuffer;

        private IDrawCommand drawCmd;

        public IEnumerable<VertexBuffer> GetVertexAttribute(string bufferName)
        {
            if (bufferName == strPosition)
            {
                if (this.positionBuffer == null)
                {
                    this.positionBuffer = mesh.vertexes.GenVertexBuffer(VBOConfig.Vec3, BufferUsage.StaticDraw);
                }

                yield return this.positionBuffer;
            }
            else if (bufferName == strTexCoord)
            {
                if (this.texCoordBuffer == null)
                {
                    this.texCoordBuffer = mesh.texCoords.GenVertexBuffer(VBOConfig.Vec2, BufferUsage.StaticDraw);
                }

                yield return this.texCoordBuffer;
            }
            else if (bufferName == strNormal)
            {
                if (this.normalBuffer == null)
                {
                    this.normalBuffer = mesh.normals.GenVertexBuffer(VBOConfig.Vec3, BufferUsage.StaticDraw);
                }

                yield return this.normalBuffer;
            }
            else
            {
                throw new ArgumentException("bufferName");
            }
        }

        public IEnumerable<IDrawCommand> GetDrawCommand()
        {
            if (this.drawCmd == null)
            {
                int polygon = (this.mesh.faces[0] is ObjVNFTriangle) ? 3 : 4;
                DrawMode mode = (this.mesh.faces[0] is ObjVNFTriangle) ? DrawMode.Triangles : DrawMode.Quads;
                int index = 0;
                var array = new uint[polygon * this.mesh.faces.Length];
                foreach (var face in this.mesh.faces)
                {
                    foreach (var vertexIndex in face.VertexIndexes())
                    {
                        array[index++] = vertexIndex;
                    }
                }
                IndexBuffer buffer = array.GenIndexBuffer(BufferUsage.StaticDraw);

                this.drawCmd = new DrawElementsCmd(buffer, mode);
            }

            yield return this.drawCmd;
        }

        #endregion

        /// <summary>
        /// Write the model into an obj file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="modelName"></param>
        public void DumpObjFile(string filename, string modelName = null)
        {
            vec3[] positions = this.mesh.vertexes;
            vec3[] normals = this.mesh.normals;
            int polygon = (this.mesh.faces[0] is ObjVNFTriangle) ? 3 : 4;
            DrawMode mode = (this.mesh.faces[0] is ObjVNFTriangle) ? DrawMode.Triangles : DrawMode.Quads;
            var indexes = new uint[polygon * this.mesh.faces.Length];
            {
                int t = 0;
                foreach (var face in this.mesh.faces)
                {
                    foreach (var vertexIndex in face.VertexIndexes())
                    {
                        indexes[t++] = vertexIndex;
                    }
                }
            }
            using (var stream = new System.IO.StreamWriter(filename))
            {
                stream.WriteLine(string.Format("# Generated by CSharpGL.ObjVNF {0}", DateTime.Now));
                stream.WriteLine("# " + (modelName == null ? "" : modelName));
                for (int i = 0; i < positions.Length; i++)
                {
                    stream.WriteLine();
                    var pos = positions[i];
                    stream.Write(string.Format("v {0} {1} {2}", pos.x, pos.y, pos.z));
                }
                for (int i = 0; i < normals.Length; i++)
                {
                    stream.WriteLine();
                    var normal = normals[i];
                    stream.Write(string.Format("vn {0} {1} {2}", normal.x, normal.y, normal.z));
                }
                for (int i = 0; i < indexes.Length; i += 3)
                {
                    stream.WriteLine();
                    stream.Write(string.Format("f {0}//{0} {1}//{1} {2}//{2}", indexes[i + 0] + 1, indexes[i + 1] + 1, indexes[i + 2] + 1));
                }
            }
        }
    }

}
