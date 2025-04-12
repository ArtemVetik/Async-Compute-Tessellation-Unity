using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal class TessellationShaderVariants
    {
        private ComputeShader[] _shaders;
        
        public TessellationShaderVariants(ComputeShader[] shaders)
        {
            _shaders = shaders;
        }
        
        public void UpdateKeywords(in TessellationParams tessellationParams)
        {
            foreach (var shader in _shaders)
            {
                if (tessellationParams.UseDisplaceMapping)
                    shader.EnableKeyword("USE_DISPLACE");
                else
                    shader.DisableKeyword("USE_DISPLACE");
                
                if (tessellationParams.Uniform)
                    shader.EnableKeyword("UNIFORM_TESSELLATION");
                else
                    shader.DisableKeyword("UNIFORM_TESSELLATION");
            }
        }
    }
}