/*{
    "CATEGORIES": [
        "Glitch",
        "Gnomalab",
        "Destruction"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "XOR BitCrush + Pixel Sorting chained, V13 - with order selector",
    "INPUTS": [
        {
            "LABEL": "Source image",
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert order (Sort -> XOR)",
            "NAME": "swapOrder",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Global intensity",
            "MAX": 1,
            "MIN": 0,
            "NAME": "masterIntensity",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "XOR Threshold",
            "MAX": 1,
            "MIN": 0,
            "NAME": "xorThreshold",
            "TYPE": "float"
        },
        {
            "DEFAULT": 2.11,
            "LABEL": "XOR Bit Depth",
            "MAX": 8,
            "MIN": 1,
            "NAME": "xorBitDepth",
            "TYPE": "float"
        },
        {
            "DEFAULT": 59.39,
            "LABEL": "XOR Block Size",
            "MAX": 200,
            "MIN": 1,
            "NAME": "xorBlockSize",
            "TYPE": "float"
        },
        {
            "DEFAULT": 128,
            "LABEL": "XOR red",
            "MAX": 255,
            "MIN": 0,
            "NAME": "xorRed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 64,
            "LABEL": "XOR green",
            "MAX": 255,
            "MIN": 0,
            "NAME": "xorGreen",
            "TYPE": "float"
        },
        {
            "DEFAULT": 32,
            "LABEL": "XOR blue",
            "MAX": 255,
            "MIN": 0,
            "NAME": "xorBlue",
            "TYPE": "float"
        },
        {
            "DEFAULT": 90,
            "LABEL": "Sort angle",
            "MAX": 360,
            "MIN": 0,
            "NAME": "angle",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.3,
            "LABEL": "Sort hue min",
            "MAX": 1,
            "MIN": 0,
            "NAME": "sortThresholdMin",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.8,
            "LABEL": "Sort hue max",
            "MAX": 1,
            "MIN": 0,
            "NAME": "sortThresholdMax",
            "TYPE": "float"
        },
        {
            "DEFAULT": 46.8,
            "LABEL": "Sort strength",
            "MAX": 100,
            "MIN": 0,
            "NAME": "pixelStrength",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.95,
            "LABEL": "Sort persistence",
            "MAX": 1,
            "MIN": 0,
            "NAME": "feedback",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1.53,
            "LABEL": "Sort chroma aberr.",
            "MAX": 10,
            "MIN": 0,
            "NAME": "chromaAbr",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.17,
            "LABEL": "Sort glow",
            "MAX": 1,
            "MIN": 0,
            "NAME": "glow",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.3,
            "LABEL": "Dithering",
            "MAX": 1,
            "MIN": 0,
            "NAME": "dither",
            "TYPE": "float"
        },
        {
            "LABEL": "Clear buffer",
            "NAME": "clearBuffer",
            "TYPE": "event"
        }
    ],
    "ISFVSN": "2",
    "PASSES": [
        {
            "FLOAT": true,
            "PERSISTENT": true,
            "TARGET": "bufferVariable"
        }
    ],
    "VSN": "1.0.0"
}
*/

float xor_op(float a, float b) {
    float res = 0.0; float fac = 1.0;
    for (int i = 0; i < 8; i++) {
        bool ap = mod(a, 2.0) >= 1.0; bool bp = mod(b, 2.0) >= 1.0;
        if (ap != bp) res += fac;
        a = floor(a / 2.0); b = floor(b / 2.0); fac *= 2.0;
    }
    return res;
}

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + 1e-10)), d / (q.x + 1e-10), q.x);
}

// Función modular para XOR
vec3 applyXor(vec3 colorIn, vec2 uv, vec2 res) {
    vec2 xorUV = (xorBlockSize > 1.0) ? (floor(uv * res / xorBlockSize) * xorBlockSize) / res : uv;
    vec4 xorSample = IMG_NORM_PIXEL(inputImage, xorUV);
    float luma = dot(colorIn, vec3(0.299, 0.587, 0.114));
    float xorMask = smoothstep(xorThreshold - 0.1, xorThreshold + 0.1, luma);
    float bLevels = pow(2.0, xorBitDepth);
    vec3 rgbBits = floor(xorSample.rgb * bLevels) * (255.0 / bLevels);
    vec3 xorRes;
    xorRes.r = xor_op(rgbBits.r, xorRed);
    xorRes.g = xor_op(rgbBits.g, xorGreen);
    xorRes.b = xor_op(rgbBits.b, xorBlue);
    return mix(colorIn, xorRes / 255.0, xorMask);
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    vec2 res = RENDERSIZE.xy;
    vec4 src = IMG_NORM_PIXEL(inputImage, uv);
    vec3 finalEffect;

    if (!swapOrder) {
        // --- ORDEN: XOR -> SORT ---
        vec3 stage1 = applyXor(src.rgb, uv, res);
        
        vec3 hsv = rgb2hsv(stage1);
        float sortMask = smoothstep(sortThresholdMin - 0.05, sortThresholdMin, hsv.x) * (1.0 - smoothstep(sortThresholdMax, sortThresholdMax + 0.05, hsv.x));
        
        vec2 dir = vec2(cos(radians(angle)), sin(radians(angle)));
        vec2 move = (dir * pixelStrength) / res;
        vec2 abr = (dir * chromaAbr) / res;
        
        vec4 prev;
        prev.r = IMG_NORM_PIXEL(bufferVariable, clamp(uv - move - abr, 0.0, 1.0)).r;
        prev.g = IMG_NORM_PIXEL(bufferVariable, clamp(uv - move, 0.0, 1.0)).g;
        prev.b = IMG_NORM_PIXEL(bufferVariable, clamp(uv - move + abr, 0.0, 1.0)).b;
        prev.a = 1.0;
        
        prev.rgb += (fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453) - 0.5) * dither * 0.1;
        vec3 combined = mix(stage1, prev.rgb, feedback);
        combined += (prev.rgb * prev.rgb) * glow * 0.2;
        finalEffect = mix(stage1, combined, sortMask);

    } else {
        // --- ORDEN: SORT -> XOR ---
        vec3 sortVal = rgb2hsv(src.rgb);
        float sortMask = smoothstep(sortThresholdMin - 0.05, sortThresholdMin, sortVal.x) * (1.0 - smoothstep(sortThresholdMax, sortThresholdMax + 0.05, sortVal.x));
        
        vec2 dir = vec2(cos(radians(angle)), sin(radians(angle)));
        vec2 move = (dir * pixelStrength) / res;
        vec2 abr = (dir * chromaAbr) / res;
        
        vec4 prev;
        prev.r = IMG_NORM_PIXEL(bufferVariable, clamp(uv - move - abr, 0.0, 1.0)).r;
        prev.g = IMG_NORM_PIXEL(bufferVariable, clamp(uv - move, 0.0, 1.0)).g;
        prev.b = IMG_NORM_PIXEL(bufferVariable, clamp(uv - move + abr, 0.0, 1.0)).b;
        prev.a = 1.0;
        
        prev.rgb += (fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453) - 0.5) * dither * 0.1;
        vec3 combinedSort = mix(src.rgb, prev.rgb, feedback);
        combinedSort += (prev.rgb * prev.rgb) * glow * 0.2;
        vec3 stage1 = mix(src.rgb, combinedSort, sortMask);
        
        finalEffect = applyXor(stage1, uv, res);
    }

    vec4 finalColor = vec4(mix(src.rgb, finalEffect, masterIntensity), src.a);
    if (clearBuffer) finalColor = src;
    gl_FragColor = finalColor;
}