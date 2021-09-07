using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using UnityEngine.Rendering;

namespace Procedural
{
    public struct NativeMeshBuilder<TVertex> : IDisposable where TVertex : struct
    {
        public NativeList<TVertex> Vertices;
        public NativeList<uint> Indices;

        public int VertexCount => Vertices.Length;
        public int IndexCount => Indices.Length;

        private static FixedList4096<VertexAttributeDescriptor> _staticAttributes;
        private uint _indexOffset;
        
        static NativeMeshBuilder()
        {
            var type = typeof(TVertex);
            var attrs = new List<VertexAttributeDescriptor>();

            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var c = fieldInfo.GetCustomAttribute<VertexAttributeAttribute>();
                if (c == null)
                {
                    throw new ArgumentException("Must define VertexAttributeAttribute");
                }
                
                attrs.Add(c.ToDescriptor());
            }

            _staticAttributes = new FixedList4096<VertexAttributeDescriptor>();
            foreach (var vertexAttributeDescriptor in attrs)
            {
                _staticAttributes.Add(vertexAttributeDescriptor);
            }
        }

        public NativeMeshBuilder(Allocator allocator)
        {
            Vertices = new NativeList<TVertex>(allocator);
            Indices = new NativeList<uint>(allocator);
            _indexOffset = 0;
        }

        public void AddVertex(TVertex vertex)
        {
            Vertices.Add(vertex);
        }

        public void EndPart()
        {
            _indexOffset = (uint) Vertices.Length;
        }

        public void AddIndex(int i)
        {
            var index = Indices.Length;
            Indices.Resize(Indices.Length + 3, NativeArrayOptions.UninitializedMemory);
            Indices[index] = (uint) (_indexOffset + i);
        }

        public void AddTriangleIndices(int i0, int i1, int i2)
        {
            var index = Indices.Length;
            Indices.Resize(Indices.Length + 3, NativeArrayOptions.UninitializedMemory);
            Indices[index] = (uint) (_indexOffset + i0);
            Indices[index + 1] = (uint) (_indexOffset + i1);
            Indices[index + 2] = (uint) (_indexOffset + i2);
        }

        public void AddQuadIndices(int i0, int i1, int i2, int i3)
        {
            var index = Indices.Length;
            Indices.Resize(Indices.Length + 6, NativeArrayOptions.UninitializedMemory);
            Indices[index] = (uint) (_indexOffset + i0);
            Indices[index + 1] = (uint) (_indexOffset + i1);
            Indices[index + 2] = (uint) (_indexOffset + i2);
            Indices[index + 3] = (uint) (_indexOffset + i0);
            Indices[index + 4] = (uint) (_indexOffset + i2);
            Indices[index + 5] = (uint) (_indexOffset + i3);
        }

        public void Dispose()
        {
            Vertices.Dispose();
            Indices.Dispose();
        }
    }
}