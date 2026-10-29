/*{
    "CATEGORIES": [
        "Glitch",
        "Distortion"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "Luminance displacement - fine grain version",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "Luma Threshold",
            "MAX": 1,
            "MIN": 0,
            "NAME": "threshold",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert threshold",
            "NAME": "invertLuma",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0.05,
            "LABEL": "Distortion H",
            "MAX": 0.3,
            "MIN": 0,
            "NAME": "displacement",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Blend mode",
            "LABELS": [
                "Replace",
                "Multiply"
            ],
            "NAME": "blendMode",
            "TYPE": "long",
            "VALUES": [
                0,
                1
            ]
        },
        {
            "DEFAULT": 0,
            "LABEL": "Pattern style",
            "LABELS": [
                "Fine Grain",
                "Ultra Thin V",
                "Ultra Thin H"
            ],
            "NAME": "patternStyle",
            "TYPE": "long",
            "VALUES": [
                0,
                1,
                2
            ]
        },
        {
            "DEFAULT": 100,
            "LABEL": "Density / Size",
            "MAX": 200,
            "MIN": 10,
            "NAME": "patternDensity",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2",
    "VSN": "1.0.0"
}
*/

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void main() {
    // 1. Coordenadas y Color Base
    vec2 uv = isf_FragNormCoord;
    vec4 src = IMG_NORM_PIXEL(inputImage, uv);
    
    // 2. Luminancia y Máscara
    float luma = dot(src.rgb, vec3(0.299, 0.587, 0.114));
    float mask = invertLuma ? step(luma, threshold) : step(threshold, luma);
    
    if (mask > 0.5) {
        // 3. Desplazamiento Fino por Filas
        // Usamos una frecuencia alta para que las filas desplazadas sean delgadas
        float row = floor(uv.y * patternDensity * 1.5);
        float noiseVal = rand(vec2(row, TIME));
        float shift = (noiseVal - 0.5) * displacement;
        
        vec2 distortedUV = fract(uv + vec2(shift, 0.0));
        vec4 distortedImg = IMG_NORM_PIXEL(inputImage, distortedUV);
        
        // 4. Patrones Ultra Finos
        float pat = 0.0;
        if (patternStyle == 0) {
            // Grano de pixel puro
            pat = rand(gl_FragCoord.xy + TIME);
        } 
        else if (patternStyle == 1) {
            // Líneas Verticales: step muy alto (0.95) para que sean hilos
            pat = step(0.95, fract(uv.x * patternDensity));
        } 
        else if (patternStyle == 2) {
            // Líneas Horizontales
            pat = step(0.95, fract(uv.y * patternDensity));
        }
        
        // 5. Mezcla Final
        if (blendMode == 0) {
            // Replace (Mezcla suave con blanco)
            gl_FragColor = mix(distortedImg, vec4(vec3(pat), 1.0), 0.3);
        } else {
            // Multiply (La imagen solo se ve donde hay patrón o se oscurece)
            gl_FragColor = distortedImg * (1.0 - (pat * 0.7));
        }
    } 
    else {
        // Fuera de la zona de luma: Imagen limpia
        gl_FragColor = src;
    }
}