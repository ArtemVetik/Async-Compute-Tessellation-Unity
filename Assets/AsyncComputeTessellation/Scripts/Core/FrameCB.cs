using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
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

        public ComputeBuffer Buffer => _frameCB;

        public void Update()
        {
            PerFrameData perFrameData = default;
            perFrameData.PredictedCamPosition = _camera.transform.position;
            
            _frustumPlanes ??= new Plane[6];

            Matrix4x4 viewProjMatrix = _camera.projectionMatrix * _camera.worldToCameraMatrix;
            GeometryUtility.CalculateFrustumPlanes(viewProjMatrix, _frustumPlanes);
            
            perFrameData.FrustrumPlane1 = new float4(_frustumPlanes[0].normal.x, _frustumPlanes[0].normal.y, _frustumPlanes[0].normal.z, _frustumPlanes[0].distance);
            perFrameData.FrustrumPlane2 = new float4(_frustumPlanes[1].normal.x, _frustumPlanes[1].normal.y, _frustumPlanes[1].normal.z, _frustumPlanes[2].distance);
            perFrameData.FrustrumPlane3 = new float4(_frustumPlanes[2].normal.x, _frustumPlanes[2].normal.y, _frustumPlanes[2].normal.z, _frustumPlanes[2].distance);
            perFrameData.FrustrumPlane4 = new float4(_frustumPlanes[3].normal.x, _frustumPlanes[3].normal.y, _frustumPlanes[3].normal.z, _frustumPlanes[3].distance);
            perFrameData.FrustrumPlane5 = new float4(_frustumPlanes[4].normal.x, _frustumPlanes[4].normal.y, _frustumPlanes[4].normal.z, _frustumPlanes[4].distance);
            perFrameData.FrustrumPlane6 = new float4(_frustumPlanes[5].normal.x, _frustumPlanes[5].normal.y, _frustumPlanes[5].normal.z, _frustumPlanes[5].distance);
            
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