/*{
    "CATEGORIES": [
        "Gnomalab",
        "Feedback"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "Feedback Pro con Noise, Vórtice y Edge Color. Versión estable y optimizada.",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Blend Mode",
            "LABELS": [
                "Mix",
                "Screen",
                "Overlay",
                "Soft Light",
                "Max"
            ],
            "NAME": "blendMode",
            "TYPE": "long",
            "VALUES": [
                0,
                1,
                2,
                3,
                4
            ]
        },
        {
            "DEFAULT": 1.2,
            "LABEL": "Edge Strength",
            "MAX": 3,
            "MIN": 0,
            "NAME": "edgeStrength",
            "TYPE": "float"
        },
        {
            "DEFAULT": [
                0.8,
                0.2,
                0.2,
                1
            ],
            "LABEL": "Edge Color",
            "NAME": "edgeColor",
            "TYPE": "color"
        },
        {
            "DEFAULT": 0.02,
            "LABEL": "Noise Turbulence",
            "MAX": 0.1,
            "MIN": 0,
            "NAME": "noiseStrength",
            "TYPE": "float"
        },
        {
            "DEFAULT": 8,
            "LABEL": "Noise Scale",
            "MAX": 20,
            "MIN": 1,
            "NAME": "noiseScale",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert Threshold (Shadows)",
            "NAME": "invertThreshold",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "Luma Threshold",
            "MAX": 1,
            "MIN": 0,
            "NAME": "lumaThreshold",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "Source Gain",
            "MAX": 1,
            "MIN": 0,
            "NAME": "sourceGain",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1.005,
            "LABEL": "Scale (Zoom)",
            "MAX": 1.1,
            "MIN": 0.9,
            "NAME": "scale",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.02,
            "LABEL": "Rotation Speed",
            "MAX": 0.5,
            "MIN": -0.5,
            "NAME": "rotationSpeed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.02,
            "LABEL": "RGB Split Speed",
            "MAX": 0.1,
            "MIN": 0,
            "NAME": "rgbSplit",
            "TYPE": "float"
        },
        {
            "DEFAULT": 5,
            "LABEL": "Blur Feedback",
            "MAX": 10,
            "MIN": 0,
            "NAME": "blurFeedback",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1.02,
            "LABEL": "Feedback Gain",
            "MAX": 1.1,
            "MIN": 0,
            "NAME": "feedbackGain",
            "TYPE": "float"
        },
        {
            "DEFAULT": -1,
            "LABEL": "Hue Shift",
            "MAX": 5,
            "MIN": -5,
            "NAME": "hueShift",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.9,
            "LABEL": "Dry / Wet Mix",
            "MAX": 1,
            "MIN": 0,
            "NAME": "dryWet",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2",
    "NAME": "Feedback Edge Gnomalab Ultimate",
    "PASSES": [
        {
            "PERSISTENT": true,
            "TARGET": "fbBuffer"
        },
        {
        }
    ],
    "VSN": "1.0.0"
}
*/

// --- RUIDO ---
float noise(vec2 p) {
    return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

float smoothNoise(vec2 p) {
    vec2 i = floor(p); vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = noise(i);
    float b = noise(i + vec2(1.0, 0.0));
    float c = noise(i + vec2(0.0, 1.0));
    float d = noise(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

// --- MEZCLAS ---
vec3 blendScreen(vec3 a, vec3 b) { return 1.0 - ((1.0 - a) * (1.0 - b)); }
vec3 blendOverlay(vec3 a, vec3 b) {
    return vec3(
        (a.r < 0.5) ? (2.0 * a.r * b.r) : (1.0 - 2.0 * (1.0 - a.r) * (1.0 - b.r)),
        (a.g < 0.5) ? (2.0 * a.g * b.g) : (1.0 - 2.0 * (1.0 - a.g) * (1.0 - b.g)),
        (a.b < 0.5) ? (2.0 * a.b * b.b) : (1.0 - 2.0 * (1.0 - a.b) * (1.0 - b.b))
    );
}

// --- UTILIDADES ---
float getLuma(vec3 c) { return dot(c, vec3(0.299, 0.587, 0.114)); }

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + 1.0e-10)), d / (q.x + 1.0e-10), q.x);
}

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void main() {
    vec2 uv = isf_FragNormCoord;

    if (PASSINDEX == 0) {
        // 1. RUIDO
        float t = TIME * 0.5;
        float nX = smoothNoise(uv * noiseScale + t) - 0.5;
        float nY = smoothNoise(uv * noiseScale - t + 5.0) - 0.5;
        vec2 noiseOffset = vec2(nX, nY) * noiseStrength;

        // 2. EDGE + LUMA MASK
        float d = 0.002;
        vec3 tRaw = IMG_NORM_PIXEL(inputImage, uv).rgb;
        vec3 tL = IMG_NORM_PIXEL(inputImage, uv + vec2(-d, 0.0)).rgb;
        vec3 tR = IMG_NORM_PIXEL(inputImage, uv + vec2(d, 0.0)).rgb;
        vec3 tU = IMG_NORM_PIXEL(inputImage, uv + vec2(0.0, d)).rgb;
        vec3 tD = IMG_NORM_PIXEL(inputImage, uv + vec2(0.0, -d)).rgb;
        float edgeMask = length(tL - tR) + length(tU - tD);
        float luma = getLuma(tRaw);
        float mask = (invertThreshold) ? (1.0 - smoothstep(lumaThreshold - 0.1, lumaThreshold + 0.1, luma)) : smoothstep(lumaThreshold - 0.1, lumaThreshold + 0.1, luma);
        
        vec3 edgePart = edgeColor.rgb * edgeMask * edgeStrength * mask;
        vec3 sourcePart = tRaw * sourceGain * mask;
        vec3 injection = blendScreen(sourcePart, edgePart);

        // 3. TRANSFORM (ROTATION + ZOOM + NOISE)
        vec2 centeredUV = uv - 0.5;
        float sa = sin(rotationSpeed * 0.1); float ca = cos(rotationSpeed * 0.1);
        mat2 rotMat = mat2(ca, -sa, sa, ca);
        vec2 transformedUV = (rotMat * centeredUV) + noiseOffset;

        vec2 uvR = transformedUV * (1.0 / scale) + 0.5;
        vec2 uvG = transformedUV * (1.0 / (scale + rgbSplit * 0.5)) + 0.5;
        vec2 uvB = transformedUV * (1.0 / (scale + rgbSplit)) + 0.5;
        
        float b = blurFeedback * 0.001;
        float r = (IMG_NORM_PIXEL(fbBuffer, uvR).r + IMG_NORM_PIXEL(fbBuffer, uvR + vec2(b,b)).r + IMG_NORM_PIXEL(fbBuffer, uvR - vec2(b,b)).r) / 3.0;
        float g = (IMG_NORM_PIXEL(fbBuffer, uvG).g + IMG_NORM_PIXEL(fbBuffer, uvG + vec2(b,-b)).g + IMG_NORM_PIXEL(fbBuffer, uvG - vec2(b,-b)).g) / 3.0;
        float bb = (IMG_NORM_PIXEL(fbBuffer, uvB).b + IMG_NORM_PIXEL(fbBuffer, uvB + vec2(0.0,b)).b + IMG_NORM_PIXEL(fbBuffer, uvB - vec2(0.0,b)).b) / 3.0;
        
        vec3 fb = vec3(r, g, bb) * feedbackGain;
        fb = max(fb - vec3(0.02), vec3(0.0));
        vec3 hsv = rgb2hsv(fb);
        hsv.x = fract(hsv.x + (hueShift * 0.01));
        fb = hsv2rgb(hsv);

        gl_FragColor = vec4(blendScreen(fb, injection), 1.0);
    } 
    else {
        vec4 src = IMG_NORM_PIXEL(inputImage, uv);
        vec3 w = IMG_NORM_PIXEL(fbBuffer, uv).rgb;
        vec3 finalOut;

        if (blendMode == 0) finalOut = mix(src.rgb, w, dryWet);
        else if (blendMode == 1) finalOut = blendScreen(src.rgb, w * dryWet);
        else if (blendMode == 2) finalOut = blendOverlay(src.rgb, w * dryWet);
        else if (blendMode == 3) { finalOut = (1.0 - 2.0 * w) * src.rgb * src.rgb + 2.0 * w * src.rgb; } // Soft Light
        else if (blendMode == 4) finalOut = max(src.rgb, w * dryWet);

        gl_FragColor = vec4(clamp(finalOut, 0.0, 1.0), src.a);
    }
}