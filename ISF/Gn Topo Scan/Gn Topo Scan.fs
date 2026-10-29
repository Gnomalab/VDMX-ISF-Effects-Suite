/*{
    "CATEGORIES": [
        "Geometry",
        "Stylize",
        "Gnomalab"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "Topographic contour-line generator driven by luminance. Includes a Blur/Sharpen pre-processing stage to soften or focus the map, rhythmic erosion animation and a HUD-style blend mode.",
    "INPUTS": [
        {
            "LABEL": "Source image",
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 20,
            "LABEL": "Number of levels",
            "MAX": 100,
            "MIN": 2,
            "NAME": "levels",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.08,
            "LABEL": "Line thickness",
            "MAX": 0.5,
            "MIN": 0.001,
            "NAME": "lineThickness",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "Edge softness",
            "MAX": 1,
            "MIN": 0,
            "NAME": "softness",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "Erosion speed",
            "MAX": 2,
            "MIN": -2,
            "NAME": "speed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Height offset",
            "MAX": 1,
            "MIN": 0,
            "NAME": "offset",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Definition (Blur <-> Sharpen)",
            "MAX": 1,
            "MIN": -1,
            "NAME": "preProcess",
            "TYPE": "float"
        },
        {
            "DEFAULT": [
                1,
                1,
                1,
                1
            ],
            "LABEL": "Line color",
            "NAME": "lineColor",
            "TYPE": "color"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Mix with original (HUD)",
            "MAX": 1,
            "MIN": 0,
            "NAME": "showSource",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert map",
            "NAME": "invert",
            "TYPE": "bool"
        }
    ],
    "ISFVSN": "2",
    "VSN": "1.0.0"
}
*/

// Función auxiliar para calcular luminancia
float getLuma(vec4 col) {
    return dot(col.rgb, vec3(0.299, 0.587, 0.114));
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    vec2 texel = 1.0 / RENDERSIZE.xy;
    vec4 src = IMG_NORM_PIXEL(inputImage, uv);
    float centerLuma = getLuma(src);
    
    // --- ETAPA DE PRE-PROCESADO (BLUR / SHARPEN) ---
    float finalLuma = centerLuma;
    
    // Solo ejecutamos el bucle si el deslizador no está en el centro
    if (abs(preProcess) > 0.01) {
        // 1. Calculamos una versión desenfocada (Blur de 9 muestras)
        // El radio del blur depende de cuánto movamos el deslizador
        float radius = abs(preProcess) * 4.0; 
        vec2 offX = vec2(texel.x * radius, 0.0);
        vec2 offY = vec2(0.0, texel.y * radius);
        
        float accumLuma = 0.0;
        for(int i = -1; i <= 1; i++){
            for(int j = -1; j <= 1; j++){
                 vec2 sampleUV = clamp(uv + vec2(float(i)*offX.x, float(j)*offY.y), 0.0, 1.0);
                 float sampleLuma = getLuma(IMG_NORM_PIXEL(inputImage, sampleUV));
                 accumLuma += sampleLuma;
            }
        }
        float blurredLuma = accumLuma / 9.0;

        // 2. Aplicamos la lógica según la dirección del deslizador
        if (preProcess < 0.0) {
            // Modo BLUR: Mezclamos la luminancia original con la desenfocada
            finalLuma = mix(centerLuma, blurredLuma, abs(preProcess));
        } else {
            // Modo SHARPEN (Técnica "Unsharp Mask"):
            // Restamos el blur del original para aislar los detalles, y los sumamos de nuevo.
            float detail = centerLuma - blurredLuma;
            // Multiplicamos por 3.0 para darle agresividad al sharpen
            finalLuma = centerLuma + detail * preProcess * 3.0; 
        }
    }

    // --- ETAPA TOPOGRÁFICA ---
    float lumaMap = invert ? 1.0 - finalLuma : finalLuma;
    
    // Erosión temporal
    float height = lumaMap + (TIME * speed * 0.1) + offset;
    
    // Generación de curvas
    // Usamos fract() en lugar de sin() para líneas más técnicas y precisas
    float topo = fract(height * levels);
    
    // Definición del grosor
    float halfThick = lineThickness * 0.5;
    float s = softness * 0.1 + 0.001; // Suavizado mínimo para evitar aliasing
    
    // Creamos la línea detectando cuando el valor fract() está cerca de 0 o 1
    // Usamos dos smoothsteps para crear ambos lados de la línea
    float maskA = smoothstep(halfThick + s, halfThick, topo);
    float maskB = smoothstep(1.0 - halfThick - s, 1.0 - halfThick, topo);
    float mask = max(maskA, maskB);
    
    // --- COMPOSICIÓN FINAL ---
    vec3 finalRGB = mix(vec3(0.0), lineColor.rgb, mask);
    
    // Mezcla tipo HUD con la imagen original
    finalRGB = mix(finalRGB, src.rgb + finalRGB, showSource);
    
    gl_FragColor = vec4(finalRGB, 1.0);
}