using System.Collections.Generic;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal class TessellationParamUI : MonoBehaviour
    {
        [SerializeField] private DebugDropDown _meshMode;
        [SerializeField] private DebugSlider _cpuLodLevelS;
        [SerializeField] private DebugToggle _uniformT;
        [SerializeField] private DebugSlider _uniformLevelS;
        [SerializeField] private DebugSlider _edgeLengthS;
        [SerializeField] private DebugToggle _displaceT;
        [SerializeField] private DebugSlider _displaceFactorS;
        [SerializeField] private DebugToggle _animatedT;
        [SerializeField] private DebugSlider _displaceLacunarityS;
        [SerializeField] private DebugSlider _displacePosScaleS;
        [SerializeField] private DebugSlider _displaceHS;

        private DrawPipeline _drawPipeline;

        private void Update()
        {
            bool resetBuffers = false;
            bool buildPso = false;
            bool updateLeafMesh = false;
            bool initTessData = false;

            int meshModeInt = (int)_drawPipeline.TessellationParams.Mesh;
            if (_meshMode.UpdateValue(ref meshModeInt))
            {
                _drawPipeline.TessellationParams.Mesh = (TessellationParams.MeshMode)meshModeInt;
                buildPso = true;
                resetBuffers = true;
                initTessData = true;
            }
            
            if (_cpuLodLevelS.UpdateValue(ref _drawPipeline.TessellationParams.CPULodLevel))
            {
                updateLeafMesh = true;
                initTessData = true;
            }

            if (_uniformT.UpdateValue(ref _drawPipeline.TessellationParams.Uniform))
                buildPso = true;

            if (_uniformLevelS.UpdateValue(ref _drawPipeline.TessellationParams.CB.SubdivisionLevel))
                initTessData = true;

            var expo = Mathf.Log(_drawPipeline.TessellationParams.TargetLength, 2);
            if (_edgeLengthS.UpdateValue(ref expo))
            {
                _drawPipeline.TessellationParams.TargetLength = Mathf.Pow(2, expo);
                initTessData = true;
            }

            if (_displaceT.UpdateValue(ref _drawPipeline.TessellationParams.UseDisplaceMapping))
                buildPso = true;

            if (_displaceFactorS.UpdateValue(ref _drawPipeline.TessellationParams.CB.DisplaceFactor))
                initTessData = true;

            bool animated = _drawPipeline.TessellationParams.CB.WavesAnimationFlag != 0;
            if (_animatedT.UpdateValue(ref animated))
            {
                _drawPipeline.TessellationParams.CB.WavesAnimationFlag = animated ? 1u : 0u;
                initTessData = true;
            }

            if (_displaceLacunarityS.UpdateValue(ref _drawPipeline.TessellationParams.CB.DisplaceLacunarity))
                initTessData = true;

            if (_displacePosScaleS.UpdateValue(ref _drawPipeline.TessellationParams.CB.DisplacePosScale))
                initTessData = true;

            if (_displaceHS.UpdateValue(ref _drawPipeline.TessellationParams.CB.DisplaceH))
                initTessData = true;

            if (resetBuffers)
                _drawPipeline.ResetBuffers();
            if (buildPso)
                _drawPipeline.UpdateKeywords();
            if (updateLeafMesh)
                _drawPipeline.UpdateLeafMesh();
            if (initTessData)
                _drawPipeline.InitTessellationData();
        }

        public void Initialize(DrawPipeline drawPipeline)
        {
            _drawPipeline = drawPipeline;

            _meshMode.Setup("Mesh Mode", new List<string>() { "Terrain", "Mesh" });
            _cpuLodLevelS.Setup("CPU Lod Level", 0, 4, true);
            _uniformT.Setup("Uniform");
            _uniformLevelS.Setup("Uniform Level", 0, 16, true);
            _edgeLengthS.Setup("Edge Length", 1.2f, 10.0f);
            _displaceT.Setup("Displace Mapping");
            _displaceFactorS.Setup("Displace Factor", 1.0f, 20.0f);
            _animatedT.Setup("Animated");
            _displaceLacunarityS.Setup("Displace Lacunarity", 0.7f, 3.0f);
            _displacePosScaleS.Setup("Displace Pos Scale", 0.01f, 0.05f);
            _displaceHS.Setup("DisplaceH", 0.1f, 2.0f);
        }
    }
}