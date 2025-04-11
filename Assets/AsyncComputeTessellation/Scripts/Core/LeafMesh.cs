using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal class LeafMesh : IDisposable
    {
        private ComputeBuffer _leafMeshVertex;
        private ComputeBuffer _leafMeshIndex;

        private int _level;

        public ComputeBuffer Vertices => _leafMeshVertex;
        public ComputeBuffer Indices => _leafMeshIndex;

        public void Build(int level, ref uint indicesCount, ref uint trianglesCount)
        {
            _level = level;

            _leafMeshVertex?.Dispose();
            _leafMeshIndex?.Dispose();

            var leafVertices = GetLeafVertices();
            var leafIndices = GetLeafIndices();

            indicesCount = (uint)leafIndices.Count;
            trianglesCount = (uint)leafVertices.Count;
            
            _leafMeshVertex = new ComputeBuffer(leafVertices.Count, Marshal.SizeOf<float2>()) { name = "LeafMeshVertex" };
            _leafMeshIndex = new ComputeBuffer(leafIndices.Count, sizeof(uint)) { name = "LeafMeshIndex" };

            _leafMeshVertex.SetData(leafVertices);
            _leafMeshIndex.SetData(leafIndices);
        }

        private List<float2> GetLeafVertices()
        {
            var vertices = new List<float2>();

            float numRow = 1 << _level;
            float col = 0.0f, row = 0.0f;
            float d = 1.0f / numRow;

            while (row <= numRow)
            {
                while (col <= row)
                {
                    vertices.Add(new float2(col * d, 1.0f - row * d));
                    col++;
                }

                row++;
                col = 0;
            }

            return vertices;
        }

        private List<uint> GetLeafIndices()
        {
            var indices = new List<uint>();
            uint col = 0, row = 0;
            uint elem = 0, numCol = 1;
            uint orientation;
            int numRow = 1 << _level;

            Func<uint, uint3> newTriangle = (uint orientation) =>
            {
                if (orientation == 0)
                    return new uint3(elem, elem + numCol, elem + numCol + 1);
                else if (orientation == 1)
                    return new uint3(elem, elem - 1, elem + numCol);
                else if (orientation == 2)
                    return new uint3(elem, elem + numCol, elem + 1);
                else if (orientation == 3)
                    return new uint3(elem, elem + numCol - 1, elem + numCol);
                else
                    throw new Exception("Bad orientation error");
            };

            while (row < numRow)
            {
                orientation = (row % 2 == 0) ? 0u : 2u;
                while (col < numCol)
                {
                    var t = newTriangle(orientation);
                    indices.Add(t.x);
                    indices.Add(t.y);
                    indices.Add(t.z);
                    orientation = (orientation + 1) % 4;

                    if (col > 0)
                    {
                        t = newTriangle(orientation);
                        indices.Add(t.x);
                        indices.Add(t.y);
                        indices.Add(t.z);
                        orientation = (orientation + 1) % 4;
                    }

                    col++;
                    elem++;
                }

                col = 0;
                numCol++;
                row++;
            }

            return indices;
        }

        public void Dispose()
        {
            _leafMeshVertex?.Dispose();
            _leafMeshIndex?.Dispose();
        }
    }
}