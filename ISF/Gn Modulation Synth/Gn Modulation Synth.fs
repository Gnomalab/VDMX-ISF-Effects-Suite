/*{
    "CATEGORIES": [
        "Signal",
        "Gnomalab",
        "Modulation"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "\"Procesador de modulación de señal inspirado en la síntesis analógica. Transforma la imagen en patrones de onda rítmicos con soporte para espacios de color CMYK/HSV, filtrado Lowpass y 11 modos de fusión extrema.\"",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Umbral de Efecto",
            "MAX": 1,
            "MIN": 0,
            "NAME": "threshold",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "LABEL": "Suavizado Umbral",
            "MAX": 1,
            "MIN": 0,
            "NAME": "threshSoft",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invertir Máscara (Afectar Sombras)",
            "NAME": "invertMask",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 100,
            "LABEL": "Omega (Frecuencia)",
            "MAX": 1000,
            "MIN": 0,
            "NAME": "omega",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Fase",
            "MAX": 6.28,
            "MIN": 0,
            "NAME": "phase",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Distorsión (Gain)",
            "MAX": 10,
            "MIN": 0.1,
            "NAME": "distortion",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Orientación",
            "LABELS": [
                "Horizontal",
                "Vertical"
            ],
            "NAME": "orientation",
            "TYPE": "long",
            "VALUES": [
                0,
                1
            ]
        },
        {
            "DEFAULT": 0,
            "LABEL": "Dirección",
            "LABELS": [
                "Forward",
                "Backward"
            ],
            "NAME": "direction",
            "TYPE": "long",
            "VALUES": [
                0,
                1
            ]
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "Lowpass Signal",
            "MAX": 1,
            "MIN": 0,
            "NAME": "lowpass",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Modo de Color",
            "LABELS": [
                "Greyscale",
                "RGB",
                "CMY",
                "CMYK",
                "HSV"
            ],
            "NAME": "colorMode",
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
            "DEFAULT": 0,
            "LABEL": "Switch Channel",
            "LABELS": [
                "Luma",
                "Rojo",
                "Verde",
                "Azul"
            ],
            "NAME": "channelSwitch",
            "TYPE": "long",
            "VALUES": [
                0,
                1,
                2,
                3
            ]
        },
        {
            "DEFAULT": 0.4,
            "LABEL": "Grosor",
            "MAX": 1,
            "MIN": 0,
            "NAME": "lineWidth",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Modo de Fusión",
            "LABELS": [
                "Normal Mix",
                "Add",
                "Screen",
                "Luma Mask",
                "Difference",
                "Color Dodge (Extreme)",
                "Vivid Light (Extreme)",
                "Hard Mix (Extreme)",
                "Divide",
                "Exclusion",
                "Overlay"
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
                6,
                7,
                8,
                9,
                10
            ]
        },
        {
            "DEFAULT": 1,
            "LABEL": "Mezcla Final (Wet)",
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

const float PI = 3.14159265359;

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float modulate(float carrier, float signal) {
    return clamp(sin(carrier + (signal * 12.0)) * signal, -1.0, 1.0);
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    vec2 res = RENDERSIZE.xy;
    
    // 0. CAPTURA IMAGEN BASE
    vec4 baseImg = IMG_NORM_PIXEL(inputImage, uv);
    
    // 1. LOWPASS (Filtrado para la señal de control)
    vec4 lp = baseImg;
    if (lowpass > 0.0) {
        float p = lowpass * 0.02;
        lp = (baseImg + 
              IMG_NORM_PIXEL(inputImage, uv + vec2(p, 0.0)) + 
              IMG_NORM_PIXEL(inputImage, uv - vec2(p, 0.0)) + 
              IMG_NORM_PIXEL(inputImage, uv + vec2(0.0, p)) + 
              IMG_NORM_PIXEL(inputImage, uv - vec2(0.0, p)) +
              IMG_NORM_PIXEL(inputImage, uv + vec2(p * 0.7, p * 0.7)) +
              IMG_NORM_PIXEL(inputImage, uv - vec2(p * 0.7, p * 0.7))
             ) / 7.0;
    }

    // 2. LÓGICA DE MÁSCARA POR UMBRAL (Aquí está el cambio)
    float lumaForMask = dot(lp.rgb, vec3(0.299, 0.587, 0.114));
    float softEdge = threshSoft * 0.5 + 0.001;
    // Máscara base: 1 en zonas claras, 0 en oscuras
    float baseMask = smoothstep(threshold - softEdge, threshold + softEdge, lumaForMask);
    // Si invertMask está activo, invertimos la lógica (1 en zonas oscuras)
    float effectMask = invertMask ? (1.0 - baseMask) : baseMask;

    // 3. SELECCIÓN DE CANAL Y CARRIER
    float s;
    if (channelSwitch == 1) s = lp.r;
    else if (channelSwitch == 2) s = lp.g;
    else if (channelSwitch == 3) s = lp.b;
    else s = dot(lp.rgb, vec3(0.299, 0.587, 0.114));

    vec3 modSources = (channelSwitch == 0) ? lp.rgb : vec3(s);
    float t = (orientation == 0) ? uv.x : uv.y;
    if (direction == 1) t = 1.0 - t;
    float carrier = t * omega + phase;

    // 4. GENERACIÓN DE COLOR MODULADO
    vec3 fore = vec3(0.0);
    float lW = 1.0 - lineWidth;

    if (colorMode == 0) { // Greyscale
        fore = vec3(smoothstep(lW, 1.0, 1.0 - abs(modulate(carrier, s * distortion))));
    } else if (colorMode == 1) { // RGB
        vec3 waves = vec3(modulate(carrier, modSources.r * distortion),
                          modulate(carrier, modSources.g * distortion),
                          modulate(carrier, modSources.b * distortion));
        fore = smoothstep(vec3(lW), vec3(1.0), 1.0 - abs(waves));
    } else if (colorMode == 2) { // CMY
        vec3 waves = vec3(modulate(carrier, (1.0 - modSources.r) * distortion),
                          modulate(carrier, (1.0 - modSources.g) * distortion),
                          modulate(carrier, (1.0 - modSources.b) * distortion));
        fore = 1.0 - smoothstep(vec3(lW), vec3(1.0), 1.0 - abs(waves));
    } else if (colorMode == 3) { // CMYK
        float k = modulate(carrier, (1.0 - s) * distortion);
        float kVal = smoothstep(lW, 1.0, 1.0 - abs(k));
        vec3 waves = vec3(modulate(carrier, (1.0 - modSources.r) * distortion),
                          modulate(carrier, (1.0 - modSources.g) * distortion),
                          modulate(carrier, (1.0 - modSources.b) * distortion));
        vec3 cmy = 1.0 - smoothstep(vec3(lW), vec3(1.0), 1.0 - abs(waves));
        fore = cmy * (1.0 - kVal); 
    } else { // HSV
        float w = modulate(carrier, s * distortion);
        fore = hsv2rgb(vec3(fract(s + phase/6.28), 0.8, smoothstep(lW, 1.0, 1.0 - abs(w))));
    }

    // 5. MOTOR DE FUSIÓN
    vec3 A = baseImg.rgb;
    vec3 B = fore;
    vec3 blended;

    if (blendMode == 0) blended = mix(A, B, blendMix);
    else if (blendMode == 1) blended = A + B * blendMix;
    else if (blendMode == 2) blended = 1.0 - (1.0 - A) * (1.0 - B * blendMix);
    else if (blendMode == 3) blended = mix(A, B, dot(A, vec3(0.299, 0.587, 0.114)) * blendMix);
    else if (blendMode == 4) blended = abs(A - B * blendMix);
    else if (blendMode == 5) blended = A / (1.00001 - B * blendMix);
    else if (blendMode == 6) { // Vivid Light
        vec3 vl;
        for(int i=0; i<3; i++) vl[i] = (B[i] < 0.5) ? (1.0 - (1.0 - A[i]) / (2.0 * B[i] + 0.0001)) : (A[i] / (2.0 * (1.0 - B[i]) + 0.0001));
        blended = mix(A, vl, blendMix);
    }
    else if (blendMode == 7) blended = mix(A, floor(A + B + 0.5), blendMix);
    else if (blendMode == 8) blended = (A / (B + 0.01)) * blendMix;
    else if (blendMode == 9) blended = (A + B - 2.0 * A * B) * blendMix;
    else { // Overlay
        vec3 ov;
        for(int i=0; i<3; i++) ov[i] = (A[i] < 0.5) ? (2.0 * A[i] * B[i]) : (1.0 - 2.0 * (1.0 - A[i]) * (1.0 - B[i]));
        blended = mix(A, ov, blendMix);
    }

    // Aplicamos la máscara final (normal o invertida)
    gl_FragColor = vec4(mix(baseImg.rgb, blended, effectMask), 1.0);
}