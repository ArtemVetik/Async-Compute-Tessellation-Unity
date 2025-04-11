using System;
using Unity.Collections;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal unsafe class TessellationParamBuffer : IDisposable
    {
        private readonly ComputeBuffer _constantBuffer;

        private TesselationMeshBuffer _meshBuffer;
        private TessellationParams _data;

        public TessellationParamBuffer(TesselationMeshBuffer meshBuffer)
        {
            _meshBuffer = meshBuffer;

            _data.WireframeMode = true;
            _data.FlatNormals = false;
            _data.CPULodLevel = 0;
            _data.Uniform = false;
            _data.TargetLength = 25;
            _data.UseDisplaceMapping = true;
            _data.Freeze = false;

            _data.CB.SubdivisionLevel = 5;
            _data.CB.ScreenRes = (uint)Screen.width;
            _data.CB.DisplaceFactor = 10.0f;
            _data.CB.WavesAnimationFlag = 0;
            _data.CB.DisplaceLacunarity = 1.99f;
            _data.CB.DisplacePosScale = 0.02f;
            _data.CB.DisplaceH = 0.96f;
            _data.CB.LodFactor = 0.00008f;
            _data.CB.IndicesCount = 3;
            _data.CB.TrianglesCount = 3;

            _constantBuffer = new ComputeBuffer(
                1,
                sizeof(TessellationParams.ConstantBuffer),
                ComputeBufferType.Constant) { name = "TessellationCB" };

            UploadData();
        }

        public ref TessellationParams Data => ref _data;
        public ComputeBuffer Buffer => _constantBuffer;

        public void UploadData(bool updateLod = true)
        {
            if (updateLod)
            {
                float l = 2.0f * Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad / 2.0f)
                               * _data.TargetLength
                               * (1 << _data.CPULodLevel)
                          / _data.CB.ScreenRes;

                const float cap = 0.43f;
                if (l > cap)
                    l = cap;

                _data.CB.LodFactor = l / _meshBuffer.GetAvgEdgeLength();
            }

            var container = new NativeArray<TessellationParams.ConstantBuffer>(1, Allocator.Temp);
            container[0] = _data.CB;
            _constantBuffer.SetData(container);
        }

        public void Dispose()
        {
            _constantBuffer?.Dispose();
        }
    }
}