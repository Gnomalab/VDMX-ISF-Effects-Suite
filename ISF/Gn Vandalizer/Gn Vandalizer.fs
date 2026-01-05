/*{
    "CATEGORIES": [
        "Glitch",
        "Destruction",
        "Gnomalab",
        "Vandalizer"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "\"Procesador de destrucción quirúrgica. Aplica Slices rítmicos, Melt vertical, TV Warp y RGB Split mediante un sistema de máscaras de precisión (Luma, RGB, Saturación). Incluye 7 modos de fusión para una integración estética del error.\"",
    "INPUTS": [
        {
            "LABEL": "Source Image",
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 3,
            "LABEL": "Glitch Speed",
            "MAX": 10,
            "MIN": 0,
            "NAME": "flickerSpeed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 4,
            "LABEL": "Matte Channel",
            "LABELS": [
                "Luminance",
                "Red",
                "Green",
                "Blue",
                "Sat x Luma"
            ],
            "NAME": "channel",
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
            "DEFAULT": 0.3,
            "LABEL": "Matte Threshold",
            "MAX": 1,
            "MIN": 0,
            "NAME": "threshold",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "Matte Softness",
            "MAX": 1,
            "MIN": 0,
            "NAME": "matteSoftness",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert Threshold",
            "NAME": "invertThreshold",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 20,
            "LABEL": "Slice Count",
            "MAX": 100,
            "MIN": 2,
            "NAME": "sliceCount",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.4,
            "LABEL": "Slice Displacement",
            "MAX": 1,
            "MIN": 0,
            "NAME": "sliceDisplace",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Melt Stretch",
            "MAX": 1,
            "MIN": 0,
            "NAME": "meltStrength",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "TV Warp",
            "MAX": 1,
            "MIN": 0,
            "NAME": "badTvWarp",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.05,
            "LABEL": "RGB Split",
            "MAX": 0.3,
            "MIN": 0,
            "NAME": "rgbShift",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Modo de Fusión",
            "LABELS": [
                "Normal Mix",
                "Luma Mask",
                "Difference",
                "Exclusion",
                "Overlay",
                "Soft Light",
                "Multiply"
            ],
            "NAME": "blendMode",
            "TYPE": "long",
            "VALUES": [
                0,
                1,
                2,
                3,
                4,
                5,
                6
            ]
        },
        {
            "DEFAULT": 1,
            "LABEL": "Mezcla Fusión (Wet/Dry)",
            "MAX": 1,
            "MIN": 0,
            "NAME": "blendMix",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2",
    "VSN": "1.0.0"
}
*/

float hash(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

float getChannelValue(vec4 col, int ch) {
    if (ch == 0) return dot(col.rgb, vec3(0.299, 0.587, 0.114));
    if (ch == 1) return col.r;
    if (ch == 2) return col.g;
    if (ch == 3) return col.b;
    if (ch == 4) {
        float maxC = max(col.r, max(col.g, col.b));
        float minC = min(col.r, min(col.g, col.b));
        float luma = dot(col.rgb, vec3(0.299, 0.587, 0.114));
        return ((maxC - minC) / (maxC + 0.0001)) * luma;
    }
    return 0.0;
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    float t = floor(TIME * flickerSpeed);
    vec4 baseImg = IMG_NORM_PIXEL(inputImage, uv);
    
    // 1. MATTE MASK CON OPCIÓN DE INVERSIÓN
    float val = getChannelValue(baseImg, int(channel));
    float soft = matteSoftness * 0.5 + 0.001;
    float matteMask = smoothstep(threshold - soft, threshold + soft, val);
    
    // Si invertThreshold está activo, restamos el valor de 1.0
    if (invertThreshold) {
        matteMask = 1.0 - matteMask;
    }

    // 2. SLICES & DISPLACEMENT
    vec2 sliceUV = uv;
    float sliceID = floor(uv.y * sliceCount);
    float sliceNoise = hash(vec2(sliceID, t));
    if (sliceNoise < 0.5) {
        sliceUV.x += (hash(vec2(sliceID, t + 0.1)) - 0.5) * sliceDisplace * 0.5;
    }

    // 3. MELT
    vec2 meltUV = sliceUV;
    if (meltStrength > 0.0) {
        float columnNoise = hash(vec2(floor(uv.x * 120.0), t));
        meltUV.y -= columnNoise * meltStrength * 0.2;
    }

    // 4. TV WARP
    vec2 finalUV = meltUV;
    if (badTvWarp > 0.0) {
        float wave = sin(uv.y * 20.0 + TIME * 10.0) * 0.01 * badTvWarp;
        finalUV.x += wave;
    }

    // 5. RGB SAMPLING (EL EFECTO)
    float shift = rgbShift * 0.1;
    vec4 fore;
    fore.r = IMG_NORM_PIXEL(inputImage, clamp(finalUV + vec2(shift, 0.0), 0.0, 1.0)).r;
    fore.g = IMG_NORM_PIXEL(inputImage, clamp(finalUV, 0.0, 1.0)).g;
    fore.b = IMG_NORM_PIXEL(inputImage, clamp(finalUV - vec2(shift, 0.0), 0.0, 1.0)).b;
    fore.a = 1.0;

    // 6. MOTOR DE FUSIÓN REFINADO
    vec3 A = baseImg.rgb;
    vec3 B = mix(A, fore.rgb, matteMask);
    vec3 blended;

    if (blendMode == 0) { // Normal Mix
        blended = mix(A, B, blendMix);
    } else if (blendMode == 1) { // Luma Mask
        blended = mix(A, B, dot(A, vec3(0.299, 0.587, 0.114)) * blendMix);
    } else if (blendMode == 2) { // Difference
        blended = mix(A, abs(A - B), blendMix);
    } else if (blendMode == 3) { // Exclusion
        blended = mix(A, A + B - 2.0 * A * B, blendMix);
    } else if (blendMode == 4) { // Overlay
        vec3 ov;
        for(int i=0; i<3; i++) ov[i] = (A[i] < 0.5) ? (2.0 * A[i] * B[i]) : (1.0 - 2.0 * (1.0 - A[i]) * (1.0 - B[i]));
        blended = mix(A, ov, blendMix);
    } else if (blendMode == 5) { // Soft Light
        vec3 sl;
        for(int i=0; i<3; i++) sl[i] = (1.0 - 2.0 * B[i]) * A[i] * A[i] + 2.0 * B[i] * A[i];
        blended = mix(A, sl, blendMix);
    } else if (blendMode == 6) { // Multiply
        blended = mix(A, A * B, blendMix);
    }

    gl_FragColor = vec4(blended, 1.0);
}