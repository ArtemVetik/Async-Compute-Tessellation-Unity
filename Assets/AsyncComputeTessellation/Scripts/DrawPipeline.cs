using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

namespace AV.AsyncComputeTessellation
{
    internal enum RenderMode
    {
        Direct = 0,
        AsyncCompute = 1
    }
    
    internal class DrawPipeline : MonoBehaviour
    {
        [SerializeField] private ComputeShader _updateCS;
        [SerializeField] private ComputeShader _copyDrawCS;
        [SerializeField] private ComputeShader _vsPrepassCS;
        [SerializeField] private Mesh _terrainMesh;
        [SerializeField] private Mesh _modelMesh;
        [SerializeField] private Material _material;
        [SerializeField] private TessellationParamUI _tessellationParamUi;
        [SerializeField] private RenderParamUI _renderParamUi;

        public RenderMode RenderMode = RenderMode.Direct;
        
        private int _pingPongCounter = 0;
        private int _subdCulledBuffIdx = 0;

        private SubdivisionBuffers _subdBuffers;
        private TesselationMeshBuffer _tessellationMeshBuffer;
        private TessellationParamCB _tessellationParam;
        private LeafMesh _leafMesh;
        private TessellationShaderVariants _shaderVariants;
        private ObjectCB _objectCB;
        private FrameCB _frameCB;

        public ref TessellationParams TessellationParams => ref _tessellationParam.Data;
        
        private void Start()
        {
            _subdBuffers = new SubdivisionBuffers();
            _tessellationMeshBuffer = new TesselationMeshBuffer();
            _tessellationParam = new TessellationParamCB(_tessellationMeshBuffer);

            ResetBuffers();
            _tessellationParam.UploadData();
            
            _leafMesh = new LeafMesh();
            _leafMesh.Build(_tessellationParam.Data.CPULodLevel, ref _tessellationParam.Data.CB.IndicesCount, ref _tessellationParam.Data.CB.TrianglesCount);

            _shaderVariants = new TessellationShaderVariants(new[] { _updateCS, _vsPrepassCS, _copyDrawCS });
            _shaderVariants.UpdateKeywords(_tessellationParam.Data);

            _objectCB = new ObjectCB();
            _frameCB = new FrameCB();
            
            _tessellationParamUi.Initialize(this);
            _renderParamUi.Initialize(this);
        }

        private void OnRenderObject()
        {
            if (RenderMode == RenderMode.AsyncCompute)
                _subdCulledBuffIdx = 1 - _subdCulledBuffIdx;
            else
                _subdCulledBuffIdx = 0;
            
            int kernelHandleUpd = _updateCS.FindKernel("main");
            int kernelHandlePrepass = _vsPrepassCS.FindKernel("main");
            int kernelHandleCopyDraw = _copyDrawCS.FindKernel("main");

            CommandBuffer computeCmd = new CommandBuffer { name = "Adaptive Tessellation Compute" };

            if (RenderMode == RenderMode.AsyncCompute)
                computeCmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);
            
            computeCmd.SetGlobalBuffer("SubdBufferIn", _pingPongCounter == 0 ? _subdBuffers.SubdIn : _subdBuffers.SubdOut);
            computeCmd.SetGlobalBuffer("SubdBufferOut", _pingPongCounter == 0 ? _subdBuffers.SubdOut : _subdBuffers.SubdIn);
            computeCmd.SetGlobalBuffer("SubdBufferOutCulled", _subdBuffers.SubdOutCulled);
            computeCmd.SetGlobalBuffer("PrepassVertexOut", _subdCulledBuffIdx == 0 ? _subdBuffers.PrepassV(0) : _subdBuffers.PrepassV(1));
            computeCmd.SetGlobalBuffer("PrepassIndexOut", _subdCulledBuffIdx == 0 ? _subdBuffers.PrepassIdx(0) : _subdBuffers.PrepassIdx(1));
            computeCmd.SetGlobalBuffer("SubdCounter", _subdBuffers.SubdCounter);
            computeCmd.SetGlobalBuffer("DrawArgs", _subdCulledBuffIdx == 0 ? _subdBuffers.DrawArgs(0) : _subdBuffers.DrawArgs(1));
            computeCmd.SetGlobalBuffer("MeshDataVertex", _tessellationMeshBuffer.VertexBuffer);
            computeCmd.SetGlobalBuffer("MeshDataIndex", _tessellationMeshBuffer.IndexBuffer);
            computeCmd.SetGlobalBuffer("LeafVertex", _leafMesh.Vertices);
            computeCmd.SetGlobalBuffer("LeafIndex", _leafMesh.Indices);

            _frameCB.Update();
            
            computeCmd.DispatchCompute(_updateCS, kernelHandleUpd, 10000, 1, 1);
            computeCmd.DispatchCompute(_vsPrepassCS, kernelHandlePrepass, 65535, 1, 1);
            computeCmd.DispatchCompute(_copyDrawCS, kernelHandleCopyDraw, 1, 1, 1);

            if (RenderMode == RenderMode.AsyncCompute)
                Graphics.ExecuteCommandBufferAsync(computeCmd, ComputeQueueType.Urgent);

            CommandBuffer drawCmd = null;
            
            if (RenderMode == RenderMode.AsyncCompute)
                drawCmd = new CommandBuffer() { name = "Adaptive Tessellation Draw" };
            else
                drawCmd = computeCmd;

            if (RenderMode != RenderMode.AsyncCompute)
                _subdCulledBuffIdx = 1;
            
            drawCmd.SetGlobalBuffer("PrepassVertexOut", _subdCulledBuffIdx == 0 ? _subdBuffers.PrepassV(1) : _subdBuffers.PrepassV(0));
            drawCmd.SetGlobalBuffer("PrepassIndexOut", _subdCulledBuffIdx == 0 ? _subdBuffers.PrepassIdx(1) : _subdBuffers.PrepassIdx(0));
            
            drawCmd.SetGlobalConstantBuffer(_objectCB.Cb, "UnityObjectData", 0, Marshal.SizeOf<ObjectData>());
            drawCmd.SetGlobalConstantBuffer(_tessellationParam.Buffer, "UnityTessellationData", 0, Marshal.SizeOf<TessellationParams.ConstantBuffer>());
            drawCmd.SetGlobalConstantBuffer(_frameCB.Buffer, "UnityPerFrameData", 0, Marshal.SizeOf<PerFrameData>());
            
            drawCmd.DrawProceduralIndirect(Matrix4x4.identity, _material, 0, MeshTopology.Triangles,
                _subdCulledBuffIdx == 0 ? _subdBuffers.DrawArgs(1) : _subdBuffers.DrawArgs(0));

            Graphics.ExecuteCommandBuffer(drawCmd);
            
            _pingPongCounter = 1 - _pingPongCounter;

            computeCmd.Release();
            
            if (RenderMode == RenderMode.AsyncCompute)
                drawCmd.Release();
        }

        private void OnDestroy()
        {
            _subdBuffers.Dispose();
            _tessellationMeshBuffer.Dispose();
            _tessellationParam.Dispose();
            _leafMesh.Dispose();
            _objectCB.Dispose();
            _frameCB.Dispose();
        }

        public void ResetBuffers()
        {
            var mesh = _tessellationParam.Data.Mesh == TessellationParams.MeshMode.Mesh
                ? _modelMesh
                : _terrainMesh;
            
            _subdBuffers.Build(mesh.GetIndices(0).Length / 3);
            _tessellationMeshBuffer.Build(mesh);

            _pingPongCounter = 0;
        }

        public void UpdateKeywords()
        {
            _shaderVariants.UpdateKeywords(_tessellationParam.Data);
        }

        public void UpdateLeafMesh()
        {
            _leafMesh.Build(_tessellationParam.Data.CPULodLevel, ref _tessellationParam.Data.CB.IndicesCount, ref _tessellationParam.Data.CB.TrianglesCount);
        }

        public void InitTessellationData()
        {
            _tessellationParam.UploadData();
        }
    }
}