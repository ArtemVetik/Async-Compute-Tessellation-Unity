using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

namespace AV.AsyncComputeTessellation
{
    public class DrawPipeline : MonoBehaviour
    {
        [SerializeField] private ComputeShader _updateCS;
        [SerializeField] private ComputeShader _copyDrawCS;
        [SerializeField] private ComputeShader _vsPrepassCS;
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;
        [SerializeField] private TessellationParamUI _ui;

        private int _pingPongCounter = 0;
        private int _subdCulledBuffIdx = 0;

        private SubdivisionBuffers _subdBuffers;
        private TesselationMeshBuffer _tessellationMeshBuffer;
        private TessellationParamCB _tessellationParam;
        private LeafMesh _leafMesh;
        private TessellationShaderVariants _shaderVariants;
        private ObjectCB _objectCB;
        private FrameCB _frameCB;

        private void Start()
        {
            _subdBuffers = new SubdivisionBuffers();
            _subdBuffers.Build(_mesh.GetIndices(0).Length / 3);

            _tessellationMeshBuffer = new TesselationMeshBuffer();
            _tessellationMeshBuffer.Build(_mesh);
            
            _tessellationParam = new TessellationParamCB(_tessellationMeshBuffer);
            
            _leafMesh = new LeafMesh();
            _leafMesh.Build(_tessellationParam.Data.CPULodLevel);

            _shaderVariants = new TessellationShaderVariants(new[] { _updateCS, _vsPrepassCS, _copyDrawCS });
            _shaderVariants.UpdateKeywords(_tessellationParam);

            _objectCB = new ObjectCB();
            _frameCB = new FrameCB();
            
            _ui.Initialize(_tessellationParam, _leafMesh, _shaderVariants);
        }

        private void OnRenderObject()
        {
            int kernelHandleUpd = _updateCS.FindKernel("main");
            int kernelHandlePrepass = _vsPrepassCS.FindKernel("main");
            int kernelHandleCopyDraw = _copyDrawCS.FindKernel("main");

            CommandBuffer cmd = new CommandBuffer { name = "Adaptive Tessellation" };

            cmd.SetGlobalBuffer("SubdBufferIn", _pingPongCounter == 0 ? _subdBuffers.SubdIn : _subdBuffers.SubdOut);
            cmd.SetGlobalBuffer("SubdBufferOut", _pingPongCounter == 0 ? _subdBuffers.SubdOut : _subdBuffers.SubdIn);
            cmd.SetGlobalBuffer("SubdBufferOutCulled", _subdBuffers.SubdOutCulled);
            cmd.SetGlobalBuffer("PrepassVertexOut", _subdCulledBuffIdx == 0 ? _subdBuffers.PrepassV(0) : _subdBuffers.PrepassV(1));
            cmd.SetGlobalBuffer("PrepassIndexOut", _subdCulledBuffIdx == 0 ? _subdBuffers.PrepassIdx(0) : _subdBuffers.PrepassIdx(1));
            cmd.SetGlobalBuffer("SubdCounter", _subdBuffers.SubdCounter);
            cmd.SetGlobalBuffer("DrawArgs", _subdCulledBuffIdx == 0 ? _subdBuffers.DrawArgs(0) : _subdBuffers.DrawArgs(1));
            cmd.SetGlobalBuffer("MeshDataVertex", _tessellationMeshBuffer.VertexBuffer);
            cmd.SetGlobalBuffer("MeshDataIndex", _tessellationMeshBuffer.IndexBuffer);
            cmd.SetGlobalBuffer("LeafVertex", _leafMesh.Vertices);
            cmd.SetGlobalBuffer("LeafIndex", _leafMesh.Indices);

            _frameCB.Update();
            
            cmd.SetGlobalConstantBuffer(_objectCB.Cb, "UnityObjectData", 0, Marshal.SizeOf<ObjectData>());
            cmd.SetGlobalConstantBuffer(_tessellationParam.Buffer, "UnityTessellationData", 0, Marshal.SizeOf<TessellationParams.ConstantBuffer>());
            cmd.SetGlobalConstantBuffer(_frameCB.Buffer, "UnityPerFrameData", 0, Marshal.SizeOf<PerFrameData>());

            cmd.DispatchCompute(_updateCS, kernelHandleUpd, 10000, 1, 1);
            cmd.DispatchCompute(_vsPrepassCS, kernelHandlePrepass, 65000, 1, 1);
            cmd.DispatchCompute(_copyDrawCS, kernelHandleCopyDraw, 1, 1, 1);

            cmd.DrawProceduralIndirect(Matrix4x4.identity, _material, 0, MeshTopology.Triangles, _subdBuffers.DrawArgs(0));

            Graphics.ExecuteCommandBuffer(cmd);

            _pingPongCounter = 1 - _pingPongCounter;

            cmd.Release();
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
    }
}