/*{
    "CATEGORIES": [
        "Glitch",
        "Distortion",
        "Gnomalab"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "Digital trail processor with bitwise XOR corruption. Isolates specific luminance ranges and switches between rough or solid scan textures through its two-stage routing.",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Threshold MIN",
            "MAX": 1,
            "MIN": 0,
            "NAME": "threshold_min",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Threshold MAX",
            "MAX": 1,
            "MIN": 0,
            "NAME": "threshold_max",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "LABEL": "Softness",
            "MAX": 0.5,
            "MIN": 0,
            "NAME": "softness",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert mask",
            "NAME": "mask_invert",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "ORDER",
            "LABELS": [
                "1: XOR > Smear",
                "2: Smear > XOR"
            ],
            "NAME": "processing_order",
            "TYPE": "long",
            "VALUES": [
                0,
                1
            ]
        },
        {
            "DEFAULT": 10,
            "LABEL": "XOR Amount",
            "MAX": 255,
            "MIN": 0,
            "NAME": "xor_val",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.4,
            "LABEL": "Smear length",
            "MAX": 1,
            "MIN": 0,
            "NAME": "smear_len",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Smear angle",
            "MAX": 1,
            "MIN": 0,
            "NAME": "smear_angle",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2",
    "VSN": "1.0.0"
}
*/

// --- FUNCION XOR MANUAL ---
float manual_xor(float a, float b) {
    float result = 0.0;
    for(int i = 0; i < 8; ++i) {
        float p = pow(2.0, float(i));
        float a_bit = mod(floor(a / p), 2.0);
        float b_bit = mod(floor(b / p), 2.0);
        if (abs(a_bit - b_bit) > 0.5) result += p;
    }
    return result;
}

vec4 applyXOR(vec4 col, float xVal) {
    float r = manual_xor(floor(col.r * 255.0), xVal) / 255.0;
    float g = manual_xor(floor(col.g * 255.0), xVal) / 255.0;
    float b = manual_xor(floor(col.b * 255.0), xVal) / 255.0;
    return vec4(r, g, b, col.a);
}

float getLuma(vec4 col) {
    return dot(col.rgb, vec3(0.299, 0.587, 0.114));
}

void main() {
    vec2 uv = isf_FragNormCoord;
    vec4 baseCol = IMG_NORM_PIXEL(inputImage, uv);
    
    // 1. Calcular Mascara de Banda (Min/Max)
    float luma = getLuma(baseCol);
    
    // Suavizado en el minimo y en el maximo
    float mask_low = smoothstep(threshold_min - softness, threshold_min + softness, luma);
    float mask_high = 1.0 - smoothstep(threshold_max - softness, threshold_max + softness, luma);
    float mask = mask_low * mask_high;
    
    if (mask_invert) mask = 1.0 - mask;

    vec4 finalCol;
    vec2 dir = vec2(cos(smear_angle * 6.2831), sin(smear_angle * 6.2831));
    float samples = 16.0; 
    float distMult = 0.8; 

    if (processing_order == 0) {
        // --- MODO 1: XOR > SMEAR ---
        vec4 acc = vec4(0.0);
        for(float i = 0.0; i < 16.0; i++) {
            vec2 offset = dir * (i / samples) * smear_len * distMult;
            vec4 pix = IMG_NORM_PIXEL(inputImage, fract(uv - offset));
            acc += applyXOR(pix, xor_val);
        }
        finalCol = mix(baseCol, acc / samples, mask);

    } else {
        // --- MODO 2: SMEAR > XOR ---
        vec4 acc = vec4(0.0);
        for(float i = 0.0; i < 16.0; i++) {
            vec2 offset = dir * (i / samples) * smear_len * distMult;
            acc += IMG_NORM_PIXEL(inputImage, fract(uv - offset));
        }
        vec4 smearClean = acc / samples;
        vec4 xorResult = applyXOR(smearClean, xor_val);
        finalCol = mix(baseCol, xorResult, mask);
    }

    gl_FragColor = finalCol;
}