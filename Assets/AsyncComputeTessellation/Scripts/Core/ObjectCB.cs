using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal class ObjectCB : IDisposable
    {
        private readonly ComputeBuffer _objectCB;

        public ObjectCB()
        {
            var objData = new ObjectData()
            {
                World = float4x4.identity,
                TexTransform = float4x4.identity,
            };

            var objDataArr = new NativeArray<ObjectData>(1, Allocator.Temp);
            objDataArr[0] = objData;
            _objectCB = new ComputeBuffer(1, Marshal.SizeOf<ObjectData>(), ComputeBufferType.Constant) { name = "ObjectCB" };
            _objectCB.SetData(objDataArr);
        }
        
        public ComputeBuffer Cb => _objectCB;
        
        public void Dispose()
        {
            _objectCB?.Dispose();
        }
    }
}