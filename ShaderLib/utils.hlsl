float random2(float2 uv)
{
	return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
}

float remap(float value, float from, float to, float targetFrom, float targetTo) {
    float t = (value - from) / (to - from);
    return lerp(targetFrom, targetTo, t);
}

float remapClamped(float value, float from, float to, float targetFrom, float targetTo) {
    float t = (value - from) / (to - from);
    return lerp(targetFrom, targetTo, saturate(t));
}

float4 monochrome(float4 color) {
    
}