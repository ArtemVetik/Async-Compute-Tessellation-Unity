using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal unsafe class SubdivisionBuffers : IDisposable
    {
        private readonly ComputeBuffer _subdIn;
        private readonly ComputeBuffer _subdOut;
        private readonly ComputeBuffer _subdOutCulled;
        private readonly ComputeBuffer _subdCounter;

        public SubdivisionBuffers()
        {
            int subdSize = 1000000;
            
            _subdIn = new ComputeBuffer(subdSize, sizeof(uint4), ComputeBufferType.Structured) { name = "SubdIn" };
            _subdOut = new ComputeBuffer(subdSize, sizeof(uint4), ComputeBufferType.Structured) { name = "SubdOut" };
            _subdOutCulled = new ComputeBuffer(subdSize, sizeof(uint4), ComputeBufferType.Structured) { name = "SubdOutCulled" };
            _subdCounter = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.Structured) { name = "SubdCounter" };   
        }

        public ComputeBuffer SubdIn => _subdIn;
        public ComputeBuffer SubdOut => _subdOut;
        public ComputeBuffer SubdOutCulled => _subdOutCulled;
        public ComputeBuffer SubdCounter => _subdCounter;

        public void Rebuild(int trianglesCount)
        {
            var subdData = new NativeArray<uint4>(trianglesCount + 1, Allocator.Temp);

            for (int i = 0; i < trianglesCount; i++)
                subdData[i] = new uint4(0, 0x1, (uint)i * 3, 1);

            _subdIn.SetData(subdData);

            for (int i = 0; i < trianglesCount; i++)
                subdData[i] = new uint4(0, 0, 0, 0);

            _subdOut.SetData(subdData);

            var counterData = new NativeArray<uint>(4, Allocator.Temp);
            counterData[0] = (uint)trianglesCount;
            counterData[1] = counterData[2] = counterData[3] = 0;

            _subdCounter.SetData(counterData);
        }

        public void Dispose()
        {
            _subdIn?.Dispose();
            _subdOut?.Dispose();
            _subdOutCulled?.Dispose();
            _subdCounter?.Dispose();
        }
    }
}