using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace AV.AsyncComputeTessellation
{
    public class DrawPipeline : MonoBehaviour
    {
        [SerializeField] private ComputeShader _updateCS;
        [SerializeField] private ComputeShader _copyDrawCS;
        [SerializeField] private ComputeShader _vsPrepassCS;
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;

        private int _pingPongCounter = 0;
        private int _subdCulledBuffIdx = 0;

        private ComputeBuffer _subdIn;
        private ComputeBuffer _subdOut;
        private ComputeBuffer _subdOutCulled;
        private ComputeBuffer[] _vsPrepassV = new ComputeBuffer[2];
        private ComputeBuffer[] _vsPrepassIdx = new ComputeBuffer[2];
        private ComputeBuffer[] _drawArgs = new ComputeBuffer[2];

        private ComputeBuffer _subdCounter;
        private ComputeBuffer _leafMeshVertex;
        private ComputeBuffer _leafMeshIndex;
        private ComputeBuffer _meshVertex;
        private ComputeBuffer _meshIndex;

        private ComputeBuffer _objectCB;
        private ComputeBuffer _tessellationCB;
        private ComputeBuffer _frameCB;

        private unsafe void Start()
        {
            int subdSize = 1000000;

            _subdIn = new ComputeBuffer(subdSize, sizeof(uint4), ComputeBufferType.Structured) { name = "SubdIn" };
            _subdOut = new ComputeBuffer(subdSize, sizeof(uint4), ComputeBufferType.Structured) { name = "SubdOut" };
            _subdOutCulled = new ComputeBuffer(subdSize, sizeof(uint4), ComputeBufferType.Structured) { name = "SubdOutCulled" };
            _subdCounter = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.Structured) { name = "SubdCounter" };

            for (int i = 0; i < 2; i++)
            {
                _vsPrepassV[i] = new ComputeBuffer(subdSize, sizeof(VertexOut), ComputeBufferType.Structured) { name = $"VsPrepassV-{i}" };
                _vsPrepassIdx[i] = new ComputeBuffer(subdSize, sizeof(uint), ComputeBufferType.Structured) { name = $"VsPrepassIdx-{i}" };
                _drawArgs[i] = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments) { name = $"DrawArgs-{i}" };
            }

            var triCount = _mesh.GetIndices(0).Length / 3;
            var subdData = new NativeArray<uint4>(triCount + 1, Allocator.Temp);

            for (int i = 0; i < triCount; i++)
                subdData[i] = new uint4(0, 0x1, (uint)i * 3, 1);

            _subdIn.SetData(subdData);

            for (int i = 0; i < triCount; i++)
                subdData[i] = new uint4(0, 0, 0, 0);

            _subdOut.SetData(subdData);
            subdData.Dispose();

            var counterData = new NativeArray<uint>(4, Allocator.Temp);
            counterData[0] = (uint)triCount;
            counterData[1] = counterData[2] = counterData[3] = 0;

            _subdCounter.SetData(counterData);
            counterData.Dispose();

            _meshVertex = new ComputeBuffer(_mesh.vertexCount, sizeof(Vertex), ComputeBufferType.Structured) { name = "MeshVertex" };

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

            _meshVertex.SetData(vertices);
            vertices.Dispose();

            _meshIndex = new ComputeBuffer(_mesh.GetIndices(0).Length, sizeof(uint));
            _meshIndex.name = nameof(_meshIndex);
            _meshIndex.SetData(_mesh.GetIndices(0));

            var leafVertices = LeafMesh.GetLeafVertices(0);
            var leafIndices = LeafMesh.GetLeafIndices(0);

            _leafMeshVertex = new ComputeBuffer(leafVertices.Count, sizeof(float2)) { name = "LeafMeshVertex" };
            _leafMeshIndex = new ComputeBuffer(leafIndices.Count, sizeof(uint)) { name = "LeafMeshIndex" };

            _leafMeshVertex.SetData(leafVertices);
            _leafMeshIndex.SetData(leafIndices);

            var objData = ObjectData.CreateDefault();
            var tessData = TessellationData.CreateDefault();

            PerFrameData perFrameData;
            perFrameData.ViewProj = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix;
            perFrameData.CamPosition = Camera.main.transform.position;
            perFrameData.PredictedCamPosition = Camera.main.transform.position;
            perFrameData.DeltaTime = Time.deltaTime;
            perFrameData.TotalTime = Time.time;
            // TODO: perFrameData.FrustrumPlanes = ...

            // TODO: Use NativeArray instead of List
            _objectCB = new ComputeBuffer(1, sizeof(ObjectData), ComputeBufferType.Constant) { name = "ObjectCB" };
            _objectCB.SetData(new List<ObjectData>() { objData });

            _frameCB = new ComputeBuffer(1, sizeof(PerFrameData), ComputeBufferType.Constant, ComputeBufferMode.Dynamic) { name = "FrameCB" };
            _frameCB.SetData(new List<PerFrameData>() { perFrameData });

            _tessellationCB = new ComputeBuffer(1, sizeof(TessellationData), ComputeBufferType.Constant) { name = "TessellationCB" };
            _tessellationCB.SetData(new List<TessellationData>() { tessData });

            _updateCS.EnableKeyword("USE_DISPLACE");
            _vsPrepassCS.EnableKeyword("USE_DISPLACE");
        }

        private unsafe void OnRenderObject()
        {
            int kernelHandleUpd = _updateCS.FindKernel("main");
            int kernelHandlePrepass = _vsPrepassCS.FindKernel("main");
            int kernelHandleCopyDraw = _copyDrawCS.FindKernel("main");

            CommandBuffer cmd = new CommandBuffer { name = "Adaptive Tessellation" };

            cmd.SetGlobalBuffer("SubdBufferIn", _pingPongCounter == 0 ? _subdIn : _subdOut);
            cmd.SetGlobalBuffer("SubdBufferOut", _pingPongCounter == 0 ? _subdOut : _subdIn);
            cmd.SetGlobalBuffer("SubdBufferOutCulled", _subdOutCulled);
            cmd.SetGlobalBuffer("PrepassVertexOut", _subdCulledBuffIdx == 0 ? _vsPrepassV[0] : _vsPrepassV[1]);
            cmd.SetGlobalBuffer("PrepassIndexOut", _subdCulledBuffIdx == 0 ? _vsPrepassIdx[0] : _vsPrepassIdx[1]);
            cmd.SetGlobalBuffer("SubdCounter", _subdCounter);
            cmd.SetGlobalBuffer("DrawArgs", _subdCulledBuffIdx == 0 ? _drawArgs[0] : _drawArgs[1]);
            cmd.SetGlobalBuffer("MeshDataVertex", _meshVertex);
            cmd.SetGlobalBuffer("MeshDataIndex", _meshIndex);
            cmd.SetGlobalBuffer("LeafVertex", _leafMeshVertex);
            cmd.SetGlobalBuffer("LeafIndex", _leafMeshIndex);

            PerFrameData perFrameData;
            perFrameData.ViewProj = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix;
            perFrameData.CamPosition = Camera.main.transform.position;
            perFrameData.PredictedCamPosition = Camera.main.transform.position;
            perFrameData.DeltaTime = Time.deltaTime;
            perFrameData.TotalTime = Time.time;

            _frameCB.SetData(new List<PerFrameData>() { perFrameData });

            cmd.SetGlobalConstantBuffer(_objectCB, "UnityObjectData", 0, sizeof(ObjectData));
            cmd.SetGlobalConstantBuffer(_tessellationCB, "UnityTessellationData", 0, sizeof(TessellationData));
            cmd.SetGlobalConstantBuffer(_frameCB, "UnityPerFrameData", 0, sizeof(PerFrameData));

            cmd.DispatchCompute(_updateCS, kernelHandleUpd, 10000, 1, 1);
            cmd.DispatchCompute(_vsPrepassCS, kernelHandlePrepass, 65000, 1, 1);
            cmd.DispatchCompute(_copyDrawCS, kernelHandleCopyDraw, 1, 1, 1);

            cmd.DrawProceduralIndirect(Matrix4x4.identity, _material, 0, MeshTopology.Triangles, _drawArgs[0]);

            Graphics.ExecuteCommandBuffer(cmd);

            _pingPongCounter = 1 - _pingPongCounter;

            cmd.Release();
        }

        private void OnDestroy()
        {
            _subdIn.Dispose();
            _subdOut.Dispose();
            _subdOutCulled.Dispose();

            for (int i = 0; i < 2; i++)
            {
                _vsPrepassV[i].Dispose();
                _vsPrepassIdx[i].Dispose();
                _drawArgs[i].Dispose();
            }

            _subdCounter.Dispose();
            _leafMeshVertex.Dispose();
            _leafMeshIndex.Dispose();
            _meshVertex.Dispose();
            _meshIndex.Dispose();
            _objectCB.Dispose();
            _tessellationCB.Dispose();
            _frameCB.Dispose();
        }
    }
}
