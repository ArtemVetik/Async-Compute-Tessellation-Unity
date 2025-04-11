using System;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal class TessellationParamUI : MonoBehaviour
    {
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

        private TessellationParamCB _cb;
        private LeafMesh _leafMesh;
        private TessellationShaderVariants _variants;

        private void Update()
        {
            bool buildPso = false;
            bool updateLeafMesh = false;
            bool initTessData = false;

            if (_cpuLodLevelS.UpdateValue(ref _cb.Data.CPULodLevel))
            {
                updateLeafMesh = true;
                initTessData = true;
            }

            if (_uniformT.UpdateValue(ref _cb.Data.Uniform))
                buildPso = true;

            if (_uniformLevelS.UpdateValue(ref _cb.Data.CB.SubdivisionLevel))
                initTessData = true;

            var expo = Mathf.Log(_cb.Data.TargetLength, 2);
            if (_edgeLengthS.UpdateValue(ref expo))
            {
                _cb.Data.TargetLength = Mathf.Pow(2, expo);
                initTessData = true;
            }

            if (_displaceT.UpdateValue(ref _cb.Data.UseDisplaceMapping))
                buildPso = true;
            
            if (_displaceFactorS.UpdateValue(ref _cb.Data.CB.DisplaceFactor))
                initTessData = true;

            bool animated = _cb.Data.CB.WavesAnimationFlag != 0;
            if (_animatedT.UpdateValue(ref animated))
            {
                _cb.Data.CB.WavesAnimationFlag = animated ? 1u : 0u;
                initTessData = true;
            }
            
            if (_displaceLacunarityS.UpdateValue(ref _cb.Data.CB.DisplaceLacunarity))
                initTessData = true;
            
            if (_displacePosScaleS.UpdateValue(ref _cb.Data.CB.DisplacePosScale))
                initTessData = true;
            
            if (_displaceHS.UpdateValue(ref _cb.Data.CB.DisplaceH))
                initTessData = true;
            
            if (buildPso) _variants.UpdateKeywords(_cb);
            if (updateLeafMesh) _leafMesh.Build(_cb.Data.CPULodLevel, ref _cb.Data.CB.IndicesCount, ref _cb.Data.CB.TrianglesCount);
            if (initTessData) _cb.UploadData();
        }
        
        public void Initialize(TessellationParamCB cb, LeafMesh leafMesh, TessellationShaderVariants variants)
        {
            if (_cb != null || _leafMesh != null)
                throw new InvalidOperationException($"{nameof(TessellationParamUI)} already initialized");
            
            _cb = cb;
            _leafMesh = leafMesh;
            _variants = variants;

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