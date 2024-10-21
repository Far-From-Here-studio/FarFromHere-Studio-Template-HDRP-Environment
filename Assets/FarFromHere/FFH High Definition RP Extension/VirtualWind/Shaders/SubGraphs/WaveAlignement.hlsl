void StretchUV_float(in float2 uv, in float2 scrollDirection, in float stretchAmount, out float2 stretchedUV)
{
    // Center UVs
    float2 centered = uv - 0.5;

    stretchedUV = centered;
    // Apply stretching
    // X axis is perpendicular to wind direction (waves direction)
    // Y axis is parallel to wind direction
    stretchedUV.x *= scrollDirection.x * stretchAmount;
    stretchedUV.y *= scrollDirection.y;

}