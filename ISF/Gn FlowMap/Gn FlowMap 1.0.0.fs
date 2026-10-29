/*{
    "CATEGORIES": [
        "Glitch",
        "Gnomalab",
        "Studio"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "Flow map by input source",
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
            "DEFAULT": 40,
            "LABEL": "Master strength",
            "MAX": 100,
            "MIN": 0,
            "NAME": "pixelStrength",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.96,
            "LABEL": "Persistence",
            "MAX": 1,
            "MIN": 0,
            "NAME": "feedback",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Wall strength (edges)",
            "MAX": 2,
            "MIN": 0,
            "NAME": "edgeDetection",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "Wall threshold",
            "MAX": 1,
            "MIN": 0,
            "NAME": "edgeThreshold",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
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
            "DEFAULT": 10,
            "LABEL": "Chroma shift",
            "MAX": 40,
            "MIN": 0,
            "NAME": "chromaAbr",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.8,
            "LABEL": "Fringe brightness",
            "MAX": 2,
            "MIN": 0,
            "NAME": "fringeAmount",
            "TYPE": "float"
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

// --- FUNCIONES DE COLOR PARA EL BIAS ---
vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + 1e-10)), d / (q.x + 1e-10), q.x);
}

float getBias(vec4 col) {
    if (sortBiasMode == 0) return (col.r + col.g + col.b) / 3.0; // Suma RGB
    if (sortBiasMode == 1) return dot(col.rgb, vec3(0.299, 0.587, 0.114)); // Luma
    vec3 hsv = rgb2hsv(col.rgb);
    if (sortBiasMode == 2) return hsv.x; // Hue
    if (sortBiasMode == 3) return hsv.y; // Saturation
    if (sortBiasMode == 4) return (max(col.r, max(col.g, col.b)) + min(col.r, min(col.g, col.b))) * 0.5; // Lightness
    if (sortBiasMode == 5) return dot(col.rgb, vec3(0.596, -0.274, -0.322)); // Crominancia I (Naranja/Cian)
    if (sortBiasMode == 6) return dot(col.rgb, vec3(0.211, -0.523, 0.312)); // Crominancia Q (Púrpura/Verde)
    return col.a; // Alfa
}

// --- DETECCIÓN DE BORDES (SOBEL) ---
float getEdges(vec2 uv) {
    vec2 off = 1.0 / RENDERSIZE;
    float tleft = dot(IMG_NORM_PIXEL(inputImage, uv + vec2(-off.x, -off.y)).rgb, vec3(0.33));
    float left = dot(IMG_NORM_PIXEL(inputImage, uv + vec2(-off.x, 0.0)).rgb, vec3(0.33));
    float bleft = dot(IMG_NORM_PIXEL(inputImage, uv + vec2(-off.x, off.y)).rgb, vec3(0.33));
    float tright = dot(IMG_NORM_PIXEL(inputImage, uv + vec2(off.x, -off.y)).rgb, vec3(0.33));
    float right = dot(IMG_NORM_PIXEL(inputImage, uv + vec2(off.x, 0.0)).rgb, vec3(0.33));
    float bright = dot(IMG_NORM_PIXEL(inputImage, uv + vec2(off.x, off.y)).rgb, vec3(0.33));
    float top = dot(IMG_NORM_PIXEL(inputImage, uv + vec2(0.0, -off.y)).rgb, vec3(0.33));
    float bottom = dot(IMG_NORM_PIXEL(inputImage, uv + vec2(0.0, off.y)).rgb, vec3(0.33));

    float h = -tleft - 2.0 * left - bleft + tright + 2.0 * right + bright;
    float v = -tleft - 2.0 * top - tright + bleft + 2.0 * bottom + bright;
    return length(vec2(h, v));
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    vec2 res = RENDERSIZE.xy;
    
    // 1. OBTENER BORDES (MUROS)
    float edges = getEdges(uv) * edgeDetection;
    
    // 2. LÓGICA DE FLUJO
    vec4 flowCol = IMG_NORM_PIXEL(flowMapImage, uv);
    vec2 dir = vec2((flowCol.r * 2.0) - 1.0, (flowCol.g * 2.0) - 1.0);
    
    // 3. IMAGEN Y SESGO (BIAS)
    vec4 img = IMG_NORM_PIXEL(inputImage, uv);
    float bias = getBias(img); // <--- AHORA SÍ USA EL SELECTOR
    
    // 4. MOVIMIENTO (SE FRENA CON LOS BORDES)
    float speedMult = smoothstep(edgeThreshold + 0.1, edgeThreshold, edges);
    vec2 move = (dir * pixelStrength * bias * speedMult) / res.y;

    // 5. MUESTREO DE FEEDBACK CON CHROMA
    vec2 abr = (dir * chromaAbr) / res.y;
    vec2 prevUV = uv - move;

    vec4 prevFrame;
    prevFrame.r = IMG_NORM_PIXEL(bufferVariable, clamp(prevUV - abr, 0.0, 1.0)).r;
    prevFrame.g = IMG_NORM_PIXEL(bufferVariable, clamp(prevUV, 0.0, 1.0)).g;
    prevFrame.b = IMG_NORM_PIXEL(bufferVariable, clamp(prevUV + abr, 0.0, 1.0)).b;
    prevFrame.a = 1.0;

    // 6. FRINGE DINÁMICO
    float fringe = smoothstep(0.1, 0.5, edges) * fringeAmount;

    // 7. COMPOSICIÓN Y RUPTURA DE LÍNEAS
    vec4 sorting = mix(img, prevFrame, feedback);
    sorting.rgb += fringe * img.rgb;

    float breakLines = smoothstep(edgeThreshold, edgeThreshold + 0.2, edges);
    vec4 finalColor = mix(sorting, img, breakLines);

    if (clearBuffer) finalColor = img;

    gl_FragColor = finalColor;
}