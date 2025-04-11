using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal class TessellationShaderVariants
    {
        private ComputeShader[] _shaders;
        private TessellationParamBuffer _tessellationParams;
        
        public TessellationShaderVariants(ComputeShader[] shaders, TessellationParamBuffer tessellationParams)
        {
            _shaders = shaders;
            _tessellationParams = tessellationParams;
        }
        
        public void UpdateKeywords()
        {
            foreach (var shader in _shaders)
            {
                if (_tessellationParams.Data.UseDisplaceMapping)
                    shader.EnableKeyword("USE_DISPLACE");
                else
                    shader.DisableKeyword("USE_DISPLACE");
                
                if (_tessellationParams.Data.Uniform)
                    shader.EnableKeyword("UNIFORM_TESSELLATION");
                else
                    shader.DisableKeyword("UNIFORM_TESSELLATION");
            }
        }
    }
}