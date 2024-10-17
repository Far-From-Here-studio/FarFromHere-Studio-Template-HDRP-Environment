//-------------------------------------------------------------------------------------
// Defines
//-------------------------------------------------------------------------------------

#ifndef SHADER_STAGE_RAY_TRACING
// Use surface gradient normal mapping as it handle correctly triplanar normal mapping and multiple UVSet
#define SURFACE_GRADIENT
#endif

//-------------------------------------------------------------------------------------
// Fill SurfaceData/Builtin data function
//-------------------------------------------------------------------------------------

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Sampling/SampleUVMapping.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"




#ifndef UNITY_TERRAIN_CB_VARS
    #define UNITY_TERRAIN_CB_VARS
#endif

#ifndef UNITY_TERRAIN_CB_DEBUG_VARS
    #define UNITY_TERRAIN_CB_DEBUG_VARS
#endif

CBUFFER_START(UnityTerrain)
    UNITY_TERRAIN_CB_VARS

float2 _TerrainPosition;
float4 _BufferData;
float _FVOffset;
float _FVScale;

TEXTURE2D(_FadeBorder);
SAMPLER(sampler_FadeBorder);


#ifdef UNITY_INSTANCING_ENABLED
    float4 _TerrainHeightmapRecipSize;  // float4(1.0f/width, 1.0f/height, 1.0f/(width-1), 1.0f/(height-1))
    float4 _TerrainHeightmapScale;      // float4(hmScale.x, hmScale.y / (float)(kMaxHeight), hmScale.z, 0.0f)
#endif
#ifdef DEBUG_DISPLAY
    UNITY_TERRAIN_CB_DEBUG_VARS
#endif
#ifdef SCENESELECTIONPASS
    int _ObjectId;
    int _PassValue;
#endif


#ifdef _USECOLOR
TEXTURE2D(_ColorBuffer);
SAMPLER(sampler_ColorBuffer);
        #ifdef _USEFV
            TEXTURE2D(_FVColor);
            SAMPLER(sampler_FVColor);
        #endif   
#elif _USEALPHA
TEXTURE2D(_ColorBuffer);
SAMPLER(sampler_ColorBuffer);
        #ifdef _USEFV
            TEXTURE2D(_FVColor);
            SAMPLER(sampler_FVColor);
        #endif  
#endif
#ifdef _USEEMISSION
TEXTURE2D(_EmissionBuffer);
SAMPLER(sampler_EmissionBuffer);
        #ifdef _USEFV
            TEXTURE2D(_FVEmission);
            SAMPLER(sampler_FVEmission);
        #endif 
#endif
#ifdef _USECOVERAGE
TEXTURE2D(_CoverageBuffer);
SAMPLER(sampler_CoverageBuffer);
#endif

#ifdef _USEVERTEXHEIGHT
float _VertexHeightBufferStrenght;
TEXTURE2D(_VertexHeightBuffer);
SAMPLER(sampler_VertexHeightBuffer);
#endif

#ifdef _ALPHATEST_ON
TEXTURE2D(_TerrainHolesTexture);
SAMPLER(sampler_TerrainHolesTexture);
#endif


CBUFFER_END

#ifdef UNITY_INSTANCING_ENABLED
    TEXTURE2D(_TerrainHeightmapTexture);
    TEXTURE2D(_TerrainNormalmapTexture);

    #ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
        SAMPLER(sampler_TerrainNormalmapTexture);
    #endif
#endif


#if !defined(SHADER_STAGE_RAY_TRACING)

// Vertex height displacement
#ifdef HAVE_MESH_MODIFICATION

UNITY_INSTANCING_BUFFER_START(Terrain)
UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData)  // float4(xBase, yBase, skipScale, ~) 
UNITY_INSTANCING_BUFFER_END(Terrain)

float4 ConstructTerrainTangent(float3 normal, float3 positiveZ)
{
    // Consider a flat terrain. It should have tangent be (1, 0, 0) and bitangent be (0, 0, 1) as the UV of the terrain grid mesh is a scale of the world XZ position.
    // In CreateTangentToWorld function (in SpaceTransform.hlsl), it is cross(normal, tangent) * sgn for the bitangent vector.
    // It is not true in a left-handed coordinate system for the terrain bitangent, if we provide 1 as the tangent.w. It would produce (0, 0, -1) instead of (0, 0, 1).
    // Also terrain's tangent calculation was wrong in a left handed system because cross((0,0,1), terrainNormalOS) points to the wrong direction as negative X.
    // Therefore all the 4 xyzw components of the tangent needs to be flipped to correct the tangent frame.
    // (See TerrainLitData.hlsl - GetSurfaceAndBuiltinData)
    float3 tangent = cross(normal, positiveZ);
    return float4(tangent, -1);
}

AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
{

#ifdef UNITY_INSTANCING_ENABLED
  
    float2 patchVertex = input.positionOS.xy;
    float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);

    float2 sampleCoords = (patchVertex.xy + instanceData.xy) * instanceData.z; // (xy + float2(xBase,yBase)) * skipScale
    float height = UnpackHeightmap(_TerrainHeightmapTexture.Load(int3(sampleCoords, 0)));

    input.positionOS.xz = sampleCoords * _TerrainHeightmapScale.xz;

    #ifdef _USEVERTEXHEIGHT
        float x = input.positionOS.r + _BufferData.r + _TerrainPosition.x;
        float y = (1 - input.positionOS.b) + _BufferData.g + (-1*_TerrainPosition.y);    
        float2 UVproj = float2(x, y) * _BufferData.b;
        float AddVertexHeight = SAMPLE_TEXTURE2D_LOD(_VertexHeightBuffer, sampler_VertexHeightBuffer, UVproj,0).r;
        float SubVertexHeight = SAMPLE_TEXTURE2D_LOD(_VertexHeightBuffer, sampler_VertexHeightBuffer, UVproj,0).g;
        input.positionOS.y = (height * _TerrainHeightmapScale.y) + (AddVertexHeight* 5 * _VertexHeightBufferStrenght) + (-SubVertexHeight*5*_VertexHeightBufferStrenght);
    #else
        input.positionOS.y = (height * _TerrainHeightmapScale.y);
    #endif

    #ifdef ATTRIBUTES_NEED_NORMAL
        input.normalOS = _TerrainNormalmapTexture.Load(int3(sampleCoords, 0)).rgb * 2 - 1;
    #endif

    #if defined(VARYINGS_NEED_TEXCOORD0) || defined(VARYINGS_DS_NEED_TEXCOORD0)
        #ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
            input.uv0 = sampleCoords;
        #else
            input.uv0 = sampleCoords * _TerrainHeightmapRecipSize.zw;
        #endif
    #endif
#endif

#ifdef ATTRIBUTES_NEED_TANGENT
    input.tangentOS = ConstructTerrainTangent(input.normalOS, float3(0, 0, 1));
#endif
    return input;
}
#endif // HAVE_MESH_MODIFICATION
#endif // !defined(SHADER_STAGE_RAY_TRACING)

// We don't use emission for terrain
//#define _EmissiveColor float3(0,0,0)

float3 _EmissiveColor;
#define _AlbedoAffectEmissive 1
#define _EmissiveExposureWeight 1

float _EmissionStrenght;
float _AlphaBufferStrenght;

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitBuiltinData.hlsl"
/*
#undef _EmissiveColor
#undef _AlbedoAffectEmissive
#undef _EmissiveExposureWeight
*/

#if !defined(SHADER_STAGE_RAY_TRACING) || (SHADERPASS == SHADERPASS_PATH_TRACING)
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
#endif
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/TerrainLit/TerrainLitSurfaceData.hlsl"

//TO DO: Compute WS and add input.
void TerrainLitShade(float2 uv, in PositionInputs posInput, inout TerrainLitSurfaceData surfaceData);
void TerrainLitDebug(float2 uv, inout float3 baseColor);


void DoEmission(inout float3 _EmissiveColor, in float2 projectedUV, in float2 FVprojectedUV)
{
#ifdef _USEEMISSION
    float fadeborder = SAMPLE_TEXTURE2D(_FadeBorder, sampler_FadeBorder, projectedUV);
    float3 emission = SAMPLE_TEXTURE2D(_EmissionBuffer, sampler_EmissionBuffer, projectedUV);
    _EmissiveColor = emission.rgb * fadeborder * _EmissionStrenght;
    #ifdef _USEFV
         float3 FVemission = SAMPLE_TEXTURE2D(_FVEmission, sampler_FVEmission, FVprojectedUV);
         _EmissiveColor += (FVemission * (1-fadeborder)) * _EmissionStrenght;
    #endif
#else
    _EmissiveColor = (0,0,0);
#endif
}


float3 ConvertToNormalTS(float3 normalData, float3 tangentWS, float3 bitangentWS)
{
#ifdef _NORMALMAP
    #ifdef SURFACE_GRADIENT
        return SurfaceGradientFromTBN(normalData.xy, tangentWS, bitangentWS);
    #else
        return normalData;
    #endif
#else
    #ifdef SURFACE_GRADIENT
        return float3(0.0, 0.0, 0.0); // No gradient
    #else
        return float3(0.0, 0.0, 1.0);
    #endif
#endif
}

//#define _ALPHATEST_ON

void GetSurfaceAndBuiltinData(inout FragInputs input, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
{
    ZERO_INITIALIZE(SurfaceData, surfaceData);
    ZERO_INITIALIZE(BuiltinData, builtinData);
    
    float3 worldUV = posInput.positionWS;
    float3 absoluteworldpos = GetAbsolutePositionWS(worldUV);
    float2 PlanarUVabsWS = absoluteworldpos.rb;
    
    float x = absoluteworldpos.r + _BufferData.r;
    float y = (1 - absoluteworldpos.b) + _BufferData.g ;

    float xFV = absoluteworldpos.r + (_BufferData.r + _FVOffset);
    float yFV = (1 - absoluteworldpos.b) + (_BufferData.g + _FVOffset);
    
    float2 UVproj = float2(x, y) * _BufferData.b;
    float2 FVUVproj = float2(xFV, yFV) * (_BufferData.b * _FVScale);

    float3 emission = (0, 0, 0);

#ifdef _USEEMISSION
    DoEmission(_EmissiveColor, UVproj, FVUVproj);
    emission = _EmissiveColor;
#endif

#ifdef _USEALPHA
    float alphaBuffer = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, UVproj).a * _AlphaBufferStrenght;
#endif

#ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
    float2 terrainNormalMapUV = (input.texCoord0.xy + 0.5f) * _TerrainHeightmapRecipSize.xy;
    input.texCoord0.xy *= _TerrainHeightmapRecipSize.zw;
#endif
   // DoAlphaTest(alphaBuffer, 0.5);


#ifdef _ALPHATEST_ON
    #ifdef _USEALPHA
        float hole = SAMPLE_TEXTURE2D(_TerrainHolesTexture, sampler_TerrainHolesTexture, input.texCoord0.xy).r * (1-alphaBuffer);
    #else
        float hole = SAMPLE_TEXTURE2D(_TerrainHolesTexture, sampler_TerrainHolesTexture, input.texCoord0.xy).r;
    #endif

GENERIC_ALPHA_TEST(hole, 0.5);

#else
    #ifdef _USEALPHA
        GENERIC_ALPHA_TEST(1-alphaBuffer, 0.5);
    #endif        
#endif

    // terrain lightmap uvs are always taken from uv0
    input.texCoord1 = input.texCoord2 = input.texCoord0;
    

    builtinData.emissiveColor = emission * 1000;

    TerrainLitSurfaceData terrainLitSurfaceData;

    InitializeTerrainLitSurfaceData(terrainLitSurfaceData);

    TerrainLitShade(input.texCoord0.xy, posInput ,terrainLitSurfaceData);


#ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
    #ifdef TERRAIN_PERPIXEL_NORMAL_OVERRIDE
        float3 normalWS = terrainLitSurfaceData.normalData.xyz; // normalData directly contains normal in world space.
    #else
        float3 normalOS = SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, terrainNormalMapUV).rgb * 2 - 1;
        float3 normalWS = mul((float3x3)GetObjectToWorldMatrix(), normalOS);
    #endif
    float4 tangentWS = ConstructTerrainTangent(normalWS, GetObjectToWorldMatrix()._13_23_33);
    input.tangentToWorld = BuildTangentToWorld(tangentWS, normalWS);
    surfaceData.normalWS = normalWS;
#else
    surfaceData.normalWS = float3(0.0, 0.0, 0.0);
#endif
    surfaceData.tangentWS = normalize(input.tangentToWorld[0].xyz); // The tangent is not normalize in tangentToWorld for mikkt. Tag: SURFACE_GRADIENT

    surfaceData.geomNormalWS = input.tangentToWorld[2];

    surfaceData.baseColor = terrainLitSurfaceData.albedo;
    surfaceData.perceptualSmoothness = terrainLitSurfaceData.smoothness;
    surfaceData.metallic = terrainLitSurfaceData.metallic;
    surfaceData.ambientOcclusion = terrainLitSurfaceData.ao;

    surfaceData.subsurfaceMask = 0;
    surfaceData.transmissionMask = 0;
    surfaceData.thickness = 1;
    surfaceData.diffusionProfileHash = 0;

    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

    // Init other parameters
    surfaceData.anisotropy = 0.0;
    surfaceData.specularColor = float3(0.0, 0.0, 0.0);
    surfaceData.coatMask = 0.0;
    surfaceData.iridescenceThickness = 0.0;
    surfaceData.iridescenceMask = 0.0;

    // Transparency parameters
    // Use thickness from SSS
    surfaceData.ior = 1.0;
    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
    surfaceData.atDistance = 1000000.0;
    surfaceData.transmittanceMask = 0.0;

    // This need to be init here to quiet the compiler in case of decal, but can be override later.
    surfaceData.specularOcclusion = 1.0;

#if !defined(ENABLE_TERRAIN_PERPIXEL_NORMAL) || !defined(TERRAIN_PERPIXEL_NORMAL_OVERRIDE)
    float3 normalTS = ConvertToNormalTS(terrainLitSurfaceData.normalData, input.tangentToWorld[0], input.tangentToWorld[1]);

    #ifdef DECAL_NORMAL_BLENDING
    if (_EnableDecals)
    {
        #ifndef SURFACE_GRADIENT
        normalTS = SurfaceGradientFromTangentSpaceNormalAndFromTBN(normalTS,
                input.tangentToWorld[0], input.tangentToWorld[1]);
        #endif

        float alpha = 1.0; // unused
        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, input, alpha);
        ApplyDecalToSurfaceData(decalSurfaceData, input.tangentToWorld[2], surfaceData, normalTS);
    }

    GetNormalWS_SG(input, normalTS, surfaceData.normalWS, float3(1.0, 1.0, 1.0));
    #else
    GetNormalWS(input, normalTS, surfaceData.normalWS, float3(1.0, 1.0, 1.0));

    #if HAVE_DECALS
    if (_EnableDecals)
    {
        float alpha = 1.0; // unused
                           // Both uses and modifies 'surfaceData.normalWS'.
        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, input, alpha);
        ApplyDecalToSurfaceData(decalSurfaceData, input.tangentToWorld[2], surfaceData);
    }
    #endif
    #endif
#elif HAVE_DECALS
    if (_EnableDecals)
    {
        float alpha = 1.0; // unused
        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, input, alpha);

        #ifdef DECAL_NORMAL_BLENDING
        float3 normalTS = SurfaceGradientFromPerturbedNormal(input.tangentToWorld[2], surfaceData.normalWS);
        ApplyDecalToSurfaceData(decalSurfaceData, input.tangentToWorld[2], surfaceData, normalTS);
        GetNormalWS_SG(input, normalTS, surfaceData.normalWS, float3(1.0, 1.0, 1.0));
        #else
        ApplyDecalToSurfaceData(decalSurfaceData, input.tangentToWorld[2], surfaceData);
        #endif
    }
#endif

    float3 bentNormalWS = surfaceData.normalWS;

#if defined(DEBUG_DISPLAY) && !defined(SHADER_STAGE_RAY_TRACING)
    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
    {
        TerrainLitDebug(input.texCoord0.xy, surfaceData.baseColor);
        surfaceData.metallic = 0;
    }
    // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
    // as it can modify attribute use for static lighting
    ApplyDebugToSurfaceData(input.tangentToWorld, surfaceData);
#endif

    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
    // Don't do spec occ from Ambient if there is no mask mask
#if defined(_MASKMAP) && !defined(_SPECULAR_OCCLUSION_NONE)
    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
#endif

    GetBuiltinData(input, V, posInput, surfaceData, 1, bentNormalWS, 0, builtinData);

    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
}
