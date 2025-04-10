#ifndef CONSTANT_BUFFERS
#define CONSTANT_BUFFERS

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

CBUFFER_START(UnityObjectData)
    float4x4 gWorld;
    float4x4 gTexTransform;
CBUFFER_END

CBUFFER_START(UnityTessellationData)
    uint gSubdivisionLevel;
    uint gScreenRes;
    float gDisplaceFactor;
    uint gWavesAnimationFlag;
    float gDisplaceLacunarity;
    float gDisplacePosScale;
    float gDisplaceH;
    float gLodFactor;
    uint gIndicesCount;
    uint gTrianglesCount;
    uint2 gPadding0;
CBUFFER_END

CBUFFER_START(UnityPerFrameData)
    float3 gPredictedCamPosition;
    uint gPadding1;
    float4 gFrustrumPlanes[6];
CBUFFER_END

CBUFFER_START(UnityShadowMapData)
    float4x4 gShadowViewProj;
    float3 gLightPos;
    float gPadding2;
CBUFFER_END

#endif