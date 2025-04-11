using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal class FrameCB : IDisposable
    {
        private readonly ComputeBuffer _frameCB;

        private Camera _camera;
        private Plane[] _frustumPlanes;
        
        public FrameCB()
        {
            _frameCB = new ComputeBuffer(
                    1,
                    Marshal.SizeOf<PerFrameData>(),
                    ComputeBufferType.Constant,
                    ComputeBufferMode.Dynamic)
                { name = "FrameCB" };

            _camera = Camera.main;
        }

        public unsafe void Update()
        {
            PerFrameData perFrameData = default;
            perFrameData.PredictedCamPosition = _camera.transform.position;

            _frustumPlanes ??= new Plane[6];

            Matrix4x4 viewProjMatrix = _camera.projectionMatrix * _camera.worldToCameraMatrix;
            GeometryUtility.CalculateFrustumPlanes(viewProjMatrix, _frustumPlanes);
            
            for (int i = 0; i < 6; i++)
            {
                Vector3 normal = _frustumPlanes[i].normal;
                float distance = _frustumPlanes[i].distance;
            
                int offset = i * 4;
                perFrameData.FrustrumPlanes[offset + 0] = normal.x;
                perFrameData.FrustrumPlanes[offset + 1] = normal.y;
                perFrameData.FrustrumPlanes[offset + 2] = normal.z;
                perFrameData.FrustrumPlanes[offset + 3] = distance;
            }

            var data = new NativeArray<PerFrameData>(1, Allocator.Temp);
            data[0] = perFrameData;
            _frameCB.SetData(data);
        }
        
        public void Dispose()
        {
            _frameCB?.Dispose();
        }
    }
}