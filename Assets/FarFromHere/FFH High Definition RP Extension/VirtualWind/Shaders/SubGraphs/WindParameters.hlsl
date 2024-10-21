float4 _WindParameters;

void WindParameters_float(out float rotate, out float windSpeed, out float windAttenuation, out float windAmplitude)
{
    rotate = _WindParameters.x;
    windSpeed = _WindParameters.y;
    windAttenuation = _WindParameters.z;
    windAmplitude = _WindParameters.w;

}