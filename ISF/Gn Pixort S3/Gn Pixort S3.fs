/*{
    "CATEGORIES": [
        "Glitch",
        "Gnomalab"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "Advanced vector pixel sorting engine with 8 flow modes (Radial, Spiral, Dipole, etc.). Includes magnetic pole control, Sobel edge detection for masking, chromatic aberration and temporal persistence.",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Flow mode",
            "LABELS": [
                "Linear",
                "Radial (Pole A)",
                "Spiral (Pole A)",
                "Concentric (Pole A)",
                "Metaphase-Attract",
                "Metaphase-Repel",
                "Dipole-Attract",
                "Dipole-Repel"
            ],
            "NAME": "flowMode",
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
            "DEFAULT": [
                0.5,
                0.5
            ],
            "LABEL": "Pole A / flow center",
            "NAME": "poleA",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": [
                0.75,
                0.5
            ],
            "LABEL": "Pole B (dipole)",
            "NAME": "poleB",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": false,
            "LABEL": "Constrain axes",
            "NAME": "constrainFlow",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 90,
            "LABEL": "Angle / Rotation",
            "MAX": 360,
            "MIN": 0,
            "NAME": "angle",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Auto speed",
            "MAX": 2,
            "MIN": -2,
            "NAME": "flowSpeed",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert direction",
            "NAME": "invertFlow",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Threshold based on:",
            "LABELS": [
                "Luminance",
                "Hue",
                "Saturation",
                "Lightness",
                "Edges (Sobel)",
                "Red",
                "Green",
                "Blue",
                "Alpha"
            ],
            "NAME": "thresholdMode",
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
                8
            ]
        },
        {
            "DEFAULT": 0.1,
            "LABEL": "Threshold min",
            "MAX": 1,
            "MIN": 0,
            "NAME": "thresholdMin",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Threshold max",
            "MAX": 1,
            "MIN": 0,
            "NAME": "thresholdMax",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.05,
            "LABEL": "Threshold softness",
            "MAX": 0.5,
            "MIN": 0,
            "NAME": "thresholdSmooth",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert mask",
            "NAME": "invertMask",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Sort bias:",
            "LABELS": [
                "None",
                "RGB sum",
                "RGBA sum",
                "Luminance",
                "Hue",
                "Saturation",
                "Lightness",
                "Chrominance I",
                "Chrominance Q"
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
                7,
                8
            ]
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert order (bias)",
            "NAME": "reverseSort",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 15,
            "LABEL": "Displacement strength",
            "MAX": 80,
            "MIN": 0,
            "NAME": "pixelStrength",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.95,
            "LABEL": "Persistence",
            "MAX": 1,
            "MIN": 0,
            "NAME": "feedback",
            "TYPE": "float"
        },
        {
            "DEFAULT": 2,
            "LABEL": "Chromatic aberration",
            "MAX": 20,
            "MIN": 0,
            "NAME": "chromaAbr",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "LABEL": "Glow (subtle)",
            "MAX": 1,
            "MIN": 0,
            "NAME": "glow",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.05,
            "LABEL": "Dithering (grain)",
            "MAX": 0.3,
            "MIN": 0,
            "NAME": "ditherAmount",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Bias red",
            "MAX": 0.02,
            "MIN": -0.02,
            "NAME": "biasR",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Bias green",
            "MAX": 0.02,
            "MIN": -0.02,
            "NAME": "biasG",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Bias blue",
            "MAX": 0.02,
            "MIN": -0.02,
            "NAME": "biasB",
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

float getNoise(vec2 p) {
    return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453123 + TIME);
}

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + 1e-10)), d / (q.x + 1e-10), q.x);
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    vec4 img = IMG_NORM_PIXEL(inputImage, uv);
    float aspect = RENDERSIZE.x / RENDERSIZE.y;

    // 1. OBTENER VALOR PARA LA MASCARA
    float val = 0.0;
    if (thresholdMode == 0) val = dot(img.rgb, vec3(0.299, 0.587, 0.114));
    else if (thresholdMode == 1) val = rgb2hsv(img.rgb).x;
    else if (thresholdMode == 2) val = rgb2hsv(img.rgb).y;
    else if (thresholdMode == 3) val = (max(img.r, max(img.g, img.b)) + min(img.r, min(img.g, img.b))) * 0.5;
    else if (thresholdMode == 4) {
        vec2 off = 1.0 / RENDERSIZE;
        float h = dot(IMG_NORM_PIXEL(inputImage, uv + vec2(-off.x, 0)).rgb, vec3(0.33)) - dot(IMG_NORM_PIXEL(inputImage, uv + vec2(off.x, 0)).rgb, vec3(0.33));
        float v = dot(IMG_NORM_PIXEL(inputImage, uv + vec2(0, -off.y)).rgb, vec3(0.33)) - dot(IMG_NORM_PIXEL(inputImage, uv + vec2(0, off.y)).rgb, vec3(0.33));
        val = length(vec2(h, v)) * 12.0;
    }
    else if (thresholdMode == 5) val = img.r;
    else if (thresholdMode == 6) val = img.g;
    else if (thresholdMode == 7) val = img.b;
    else if (thresholdMode == 8) val = img.a;

    float mask = smoothstep(thresholdMin - thresholdSmooth, thresholdMin, val) * (1.0 - smoothstep(thresholdMax, thresholdMax + thresholdSmooth, val));
    if (invertMask) mask = 1.0 - mask;

    // 2. SESGO DE ORDENACION (SORT BIAS)
    float sBias = 1.0;
    if (sortBiasMode == 1) sBias = (img.r + img.g + img.b) / 3.0;
    else if (sortBiasMode == 2) sBias = (img.r + img.g + img.b + img.a) / 4.0;
    else if (sortBiasMode == 3) sBias = dot(img.rgb, vec3(0.299, 0.587, 0.114));
    else if (sortBiasMode == 4) sBias = rgb2hsv(img.rgb).x;
    else if (sortBiasMode == 5) sBias = rgb2hsv(img.rgb).y;
    else if (sortBiasMode == 7) sBias = dot(img.rgb, vec3(0.596, -0.274, -0.322));
    else if (sortBiasMode == 8) sBias = dot(img.rgb, vec3(0.211, -0.523, 0.312));
    if (reverseSort) sBias = 1.0 - sBias;

    // 3. CALCULO DE VECTORES (FIX TOTAL)
    vec2 dir = vec2(0.0);
    vec2 loc = uv - poleA; 
    loc.x *= aspect; // Corrección visual para que el círculo sea REDONDO
    
    float rot = radians(angle + (TIME * flowSpeed * 60.0));

    if (flowMode == 0) { // Lineal
        dir = vec2(cos(rot), sin(rot));
    }
    else if (flowMode == 1) { // Radial
        dir = normalize(loc + 0.00001);
    }
    else if (flowMode == 2) { // Espiral
        vec2 tangential = vec2(-loc.y, loc.x);
        dir = normalize(tangential + loc * 0.5);
    }
    else if (flowMode == 3) { // Concentrico
        dir = normalize(vec2(-loc.y, loc.x));
    }
    else if (flowMode == 4 || flowMode == 5) { // Metafase
        dir = vec2(0.0, uv.y - poleA.y);
        if (flowMode == 4) dir.y *= -1.0;
        dir = normalize(dir + 0.00001);
    }
    else if (flowMode == 6 || flowMode == 7) { // Dipolo
        vec2 locB = uv - poleB; locB.x *= aspect;
        dir = normalize(loc + 0.00001) - normalize(locB + 0.00001);
        if (flowMode == 6) dir *= -1.0;
        dir = normalize(dir + 0.00001);
    }

    // MAPEAR DIRECCION DE VUELTA AL ESPACIO UV (Importante)
    dir.x /= aspect; 
    if (invertFlow) dir *= -1.0;

    if (constrainFlow) {
        if (abs(dir.x) > abs(dir.y)) dir = vec2(sign(dir.x), 0.0);
        else dir = vec2(0.0, sign(dir.y));
    }

    // 4. MUESTREO DE FEEDBACK
    vec2 move = (dir * pixelStrength * sBias) / RENDERSIZE;
    vec2 prevUV = uv - move;
    
    vec2 abr = (dir * chromaAbr) / RENDERSIZE;
    vec4 prevFrame;
    prevFrame.r = IMG_NORM_PIXEL(bufferVariable, clamp(prevUV - abr, 0.0, 1.0)).r;
    prevFrame.g = IMG_NORM_PIXEL(bufferVariable, clamp(prevUV, 0.0, 1.0)).g;
    prevFrame.b = IMG_NORM_PIXEL(bufferVariable, clamp(prevUV + abr, 0.0, 1.0)).b;
    prevFrame.a = 1.0;

    prevFrame.rgb += vec3(biasR, biasG, biasB);
    prevFrame.rgb += (getNoise(uv * 100.0) - 0.5) * ditherAmount;
    prevFrame = clamp(prevFrame, 0.0, 1.0);

    // 5. COMPOSICION
    vec4 sorting = mix(img, prevFrame, feedback);
    sorting.rgb += (prevFrame.rgb * prevFrame.rgb) * (glow * 0.1) * mask;

    vec4 finalColor = mix(img, sorting, mask);
    if (clearBuffer) finalColor = img;

    gl_FragColor = finalColor;
}