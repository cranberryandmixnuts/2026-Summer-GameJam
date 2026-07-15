#ifndef LAYEREDNOISEHSLS_INCLUDED
#define LAYEREDNOISEHSLS_INCLUDED

float2 Unity_GradientNoise_Dir_float(float2 p) {
	p = p % 289;
	float x = (34 * p.x + 1) * p.x % 289 + p.y;
	x = (34 * x + 1) * x % 289;
	x = frac(x / 41) * 2 - 1;
	return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}

float Unity_GradientNoise_float(float2 UV, float Scale) {

	float2 p = UV * Scale;
	float2 ip = floor(p);
	float2 fp = frac(p);

	float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
	float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
	float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
	float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));

	fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);

	return lerp(
		lerp(d00, d10, fp.x),
		lerp(d01, d11, fp.x),
		fp.y
	) + 0.5;
}

void LayeredNoise_float(float2 UV, float LayerCount, float Scale, float StepDivider, out float Noise) {

    float result = 0;
    float total = 0;
    float step = 1;

    for (int i = 0; i < LayerCount; i++) {
        result += Unity_GradientNoise_float(UV, Scale * pow(2, i)) * step;
        total += step;
        step /= StepDivider;
    }

    Noise = result / total;

}

void LayeredNoise_half(half2 UV, half LayerCount, half Scale, half StepDivider, out half Noise) {

	float noise;
	LayeredNoise_float(
		(float2)UV,
		(float)LayerCount,
		(float)Scale,
		(float)StepDivider,
		noise
	);

	Noise = (half)noise;

}

#endif