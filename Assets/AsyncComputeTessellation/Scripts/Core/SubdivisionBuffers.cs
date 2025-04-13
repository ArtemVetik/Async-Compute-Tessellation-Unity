using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal class SubdivisionBuffers : IDisposable
    {
        private readonly ComputeBuffer _subdIn;
        private readonly ComputeBuffer _subdOut;
        private readonly ComputeBuffer _subdOutCulled;
        private readonly ComputeBuffer _subdCounter;
        private readonly ComputeBuffer[] _vsPrepassV = new ComputeBuffer[2];
        private readonly ComputeBuffer[] _vsPrepassIdx = new ComputeBuffer[2];
        private readonly ComputeBuffer[] _drawArgs = new ComputeBuffer[2];
        
        public SubdivisionBuffers()
        {
            int subdSize = 1000000;
            
            _subdIn = new ComputeBuffer(subdSize, Marshal.SizeOf<uint4>(), ComputeBufferType.Structured) { name = "SubdIn" };
            _subdOut = new ComputeBuffer(subdSize, Marshal.SizeOf<uint4>(), ComputeBufferType.Structured) { name = "SubdOut" };
            _subdOutCulled = new ComputeBuffer(subdSize, Marshal.SizeOf<uint4>(), ComputeBufferType.Structured) { name = "SubdOutCulled" };
            _subdCounter = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.Structured) { name = "SubdCounter" };
            
            for (int i = 0; i < 2; i++)
            {
                _vsPrepassV[i] = new ComputeBuffer(subdSize, Marshal.SizeOf<VertexOut>(), ComputeBufferType.Structured)
                    { name = $"VsPrepassV-{i}" };
                _vsPrepassIdx[i] = new ComputeBuffer(subdSize, sizeof(uint), ComputeBufferType.Structured)
                    { name = $"VsPrepassIdx-{i}" };
                _drawArgs[i] = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments)
                    { name = $"DrawArgs-{i}" };
            }
        }

        public ComputeBuffer SubdIn => _subdIn;
        public ComputeBuffer SubdOut => _subdOut;
        public ComputeBuffer SubdOutCulled => _subdOutCulled;
        public ComputeBuffer SubdCounter => _subdCounter;
        public ComputeBuffer PrepassV(int index) => _vsPrepassV[index];
        public ComputeBuffer PrepassIdx(int index) => _vsPrepassIdx[index];
        public ComputeBuffer DrawArgs(int index) => _drawArgs[index];
        
        public void Build(int trianglesCount)
        {
            var subdData = new NativeArray<uint4>(trianglesCount + 1, Allocator.Temp);

            for (int i = 0; i < trianglesCount; i++)
                subdData[i] = new uint4(0, 0x1, (uint)i * 3, 1);

            _subdIn.SetData(subdData, 0, 0, trianglesCount);

            for (int i = 0; i < trianglesCount; i++)
                subdData[i] = new uint4(0, 0, 0, 0);

            _subdOut.SetData(subdData, 0, 0, trianglesCount);

            var counterData = new NativeArray<uint>(4, Allocator.Temp);
            counterData[0] = (uint)trianglesCount;
            counterData[1] = 0;
            counterData[2] = 0;
            counterData[3] = 0;

            _subdCounter.SetData(counterData);
        }

        public void Dispose()
        {
            _subdIn?.Dispose();
            _subdOut?.Dispose();
            _subdOutCulled?.Dispose();
            _subdCounter?.Dispose();

            for (int i = 0; i < 2; i++)
            {
                _vsPrepassV[i].Dispose();
                _vsPrepassIdx[i].Dispose();
                _drawArgs[i].Dispose();
            }
        }
    }
}