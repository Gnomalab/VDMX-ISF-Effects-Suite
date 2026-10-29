/*{
    "CATEGORIES": [
        "Glitch",
        "Gnomalab",
        "Flow"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "PixSort Flow Mapper - movement driven by a second image",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "LABEL": "Flow map (R=X, G=Y)",
            "NAME": "flowMapImage",
            "TYPE": "image"
        },
        {
            "LABEL": "External mask (clip)",
            "NAME": "maskImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": false,
            "LABEL": "Use external mask",
            "NAME": "useExternalMask",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 3,
            "LABEL": "Sort bias",
            "LABELS": [
                "RGB sum",
                "Luminance",
                "Hue",
                "Saturation",
                "Lightness",
                "Chrominance I",
                "Chrominance Q",
                "Alpha"
            ],
            "NAME": "sortBiasMode",
            "TYPE": "long",
            "VALUES": [
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7
            ]
        },
        {
            "DEFAULT": 1,
            "LABEL": "Map influence",
            "MAX": 2,
            "MIN": 0,
            "NAME": "flowMapStrength",
            "TYPE": "float"
        },
        {
            "DEFAULT": 30,
            "LABEL": "Master strength",
            "MAX": 100,
            "MIN": 0,
            "NAME": "pixelStrength",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.96,
            "LABEL": "Persistence (trail)",
            "MAX": 1,
            "MIN": 0,
            "NAME": "feedback",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "Bias influence on trail",
            "MAX": 1,
            "MIN": 0,
            "NAME": "biasToFeedback",
            "TYPE": "float"
        },
        {
            "DEFAULT": 10,
            "LABEL": "Color shift (Chroma)",
            "MAX": 40,
            "MIN": 0,
            "NAME": "chromaAbr",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Block size",
            "MAX": 50,
            "MIN": 1,
            "NAME": "blocky",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.3,
            "LABEL": "Noise / jitter",
            "MAX": 1,
            "MIN": 0,
            "NAME": "jitter",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "Threshold min",
            "MAX": 1,
            "MIN": 0,
            "NAME": "thresholdMin",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.8,
            "LABEL": "Threshold max",
            "MAX": 1,
            "MIN": 0,
            "NAME": "thresholdMax",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "Fringe intensity",
            "MAX": 2,
            "MIN": 0,
            "NAME": "fringeAmount",
            "TYPE": "float"
        },
        {
            "DEFAULT": [
                1,
                1,
                1,
                1
            ],
            "LABEL": "Fringe tint",
            "NAME": "fringeColor",
            "TYPE": "color"
        },
        {
            "LABEL": "Reset",
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

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + 1e-10)), d / (q.x + 1e-10), q.x);
}

float getBias(vec4 col) {
    if (sortBiasMode == 0) return (col.r + col.g + col.b) / 3.0;
    if (sortBiasMode == 1) return dot(col.rgb, vec3(0.299, 0.587, 0.114));
    vec3 hsv = rgb2hsv(col.rgb);
    if (sortBiasMode == 2) return hsv.x;
    if (sortBiasMode == 3) return hsv.y;
    if (sortBiasMode == 4) return (max(col.r, max(col.g, col.b)) + min(col.r, min(col.g, col.b))) * 0.5;
    if (sortBiasMode == 5) return dot(col.rgb, vec3(0.596, -0.274, -0.322));
    if (sortBiasMode == 6) return dot(col.rgb, vec3(0.211, -0.523, 0.312));
    return col.a;
}

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    vec2 res = RENDERSIZE.xy;
    
    // 1. MÁSCARA EXTERNA (Recorte)
    float extMaskVal = 1.0;
    if (useExternalMask) {
        vec4 mCol = IMG_NORM_PIXEL(maskImage, uv);
        extMaskVal = dot(mCol.rgb, vec3(0.299, 0.587, 0.114)) * mCol.a;
    }

    // 2. BLOCKINESS
    vec2 blockUV = uv;
    if (blocky > 1.1) {
        vec2 bRes = res / blocky;
        blockUV = floor(uv * bRes) / bRes;
    }

    vec4 img = IMG_NORM_PIXEL(inputImage, uv);
    vec4 blockImg = IMG_NORM_PIXEL(inputImage, blockUV);

    // 3. CÁLCULO DEL BIAS Y UMBRAL
    float bias = getBias(blockImg);
    float luma = dot(blockImg.rgb, vec3(0.299, 0.587, 0.114));
    float mask = smoothstep(thresholdMin - 0.05, thresholdMin, luma) * (1.0 - smoothstep(thresholdMax, thresholdMax + 0.05, luma));
    mask *= extMaskVal;

    // --- 4. DIRECCIÓN BASADA EN FLOW MAP (EL NÚCLEO DEL EFECTO) ---
    // Muestreado del mapa de flujo usando las coordenadas de bloque
    vec4 flowCol = IMG_NORM_PIXEL(flowMapImage, blockUV);

    // Conversión de color (0.0 a 1.0) a vector (-1.0 a 1.0)
    // R = Eje X, G = Eje Y. Restamos 0.5 y multiplicamos por 2.
    vec2 flowDir = vec2((flowCol.r * 2.0) - 1.0, (flowCol.g * 2.0) - 1.0);

    // Aplicamos la fuerza específica del mapa
    vec2 dir = flowDir * flowMapStrength;

    // 5. CÁLCULO FINAL DEL MOVIMIENTO
    float noise = hash(blockUV + TIME * 0.005);
    float force = pixelStrength * (1.0 + (noise - 0.5) * jitter);
    
    // El movimiento combina la dirección del mapa, la fuerza global y el bias
    vec2 move = (dir * force * bias * extMaskVal) / res.y;

    // 6. FEEDBACK CON CHROMA
    vec2 abr = (dir * chromaAbr) / res.y;
    vec2 prevUV = uv - move;

    vec4 prevFrame;
    prevFrame.r = IMG_NORM_PIXEL(bufferVariable, clamp(prevUV - abr, 0.0, 1.0)).r;
    prevFrame.g = IMG_NORM_PIXEL(bufferVariable, clamp(prevUV, 0.0, 1.0)).g;
    prevFrame.b = IMG_NORM_PIXEL(bufferVariable, clamp(prevUV + abr, 0.0, 1.0)).b;
    prevFrame.a = 1.0;

    float dynamicFeedback = mix(feedback, feedback * bias, biasToFeedback);

    // 7. FRINGE
    // Usamos la dirección del mapa para calcular dónde está el "atrás"
    vec2 sampleBehind = uv - (dir * 2.0 / res.y);
    float maskBehind = smoothstep(thresholdMin - 0.05, thresholdMin, dot(IMG_NORM_PIXEL(inputImage, sampleBehind).rgb, vec3(0.3)));
    float fringe = max(0.0, mask - (maskBehind * extMaskVal));
    
    // 8. COMPOSICIÓN
    vec4 sorting = mix(img, prevFrame, dynamicFeedback);
    sorting.rgb += fringe * fringeColor.rgb * fringeAmount;

    vec4 finalColor = mix(img, sorting, mask);
    if (clearBuffer) finalColor = img;

    gl_FragColor = finalColor;
}