using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal class TesselationMeshBuffer : IDisposable
    {
        private Mesh _mesh;

        private ComputeBuffer _vertexBuffer;
        private ComputeBuffer _indexBuffer;
        
        public ComputeBuffer VertexBuffer => _vertexBuffer;
        public ComputeBuffer IndexBuffer => _indexBuffer;
        
        public void Build(Mesh mesh)
        {
            _mesh = mesh;

            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            
            _vertexBuffer = new ComputeBuffer(_mesh.vertexCount, Marshal.SizeOf<Vertex>(), ComputeBufferType.Structured);
            _vertexBuffer.name = "MeshVertex";
            
            var vertices = new NativeArray<Vertex>(_mesh.vertexCount, Allocator.Temp);
            for (int i = 0; i < _mesh.vertexCount; i++)
            {
                vertices[i] = new Vertex()
                {
                    Position = new float4(_mesh.vertices[i].x, _mesh.vertices[i].y, _mesh.vertices[i].z, 0),
                    Normal = new float4(_mesh.normals[i].x, _mesh.normals[i].y, _mesh.normals[i].z, 0),
                    TangentU = _mesh.tangents[i],
                    TexC = new float4(_mesh.uv[i].x, _mesh.uv[i].y, 0, 0)
                };
            }

            _vertexBuffer.SetData(vertices);
            vertices.Dispose();

            _indexBuffer = new ComputeBuffer(_mesh.GetIndices(0).Length, sizeof(uint));
            _indexBuffer.name = "MeshIndex";
            _indexBuffer.SetData(_mesh.GetIndices(0));
        }
        
        public float GetAvgEdgeLength()
        {
            if (_mesh == null || _mesh.vertexCount == 0 || _mesh.triangles.Length == 0)
                return 0f;

            Vector3[] vertices = _mesh.vertices;
            int[] triangles = _mesh.triangles;

            float totalLength = 0f;
            int edgeCount = 0;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                totalLength += Vector3.Distance(v0, v1);
                totalLength += Vector3.Distance(v1, v2);
                totalLength += Vector3.Distance(v2, v0);
                edgeCount += 3;
            }

            return totalLength / edgeCount;
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }
    }
}