void RotateAndStretchUV_float(in float2 uv, in float2 scrollDirection, in float stretchAmount, out float2 rotatedUV)
{
    // Get the angle from the scroll direction
    float2 dir = normalize(scrollDirection);
    float angle = atan2(dir.y, dir.x);
    
    // Center UVs
    float2 centered = uv - 0.5;
    
    // Create rotation matrix
    float sinA = sin(angle);
    float cosA = cos(angle);
    float2x2 rotationMatrix = float2x2(
        cosA, -sinA,
        sinA, cosA
    );
    
    // Rotate UVs
    rotatedUV = mul(rotationMatrix, centered);
    
    // Apply stretching
    // X axis is perpendicular to wind direction (waves direction)
    // Y axis is parallel to wind direction
    rotatedUV.x *= stretchAmount;
    
    // Uncenter UVs
    rotatedUV += 0.5;
}