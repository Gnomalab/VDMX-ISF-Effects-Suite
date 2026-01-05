/*{
    "CATEGORIES": [
        "Gnomalab",
        "Stylize"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "\"Pixel sorting de una sola pasada optimizado para mantener fondos limpios. Permite ordenar por Luma, Hue o Sat y realizar un fundido suave hacia la imagen original mediante un sistema de límites de rango.\"",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Direction",
            "LABELS": [
                "Vertical",
                "Horizontal"
            ],
            "NAME": "direction",
            "TYPE": "long",
            "VALUES": [
                0,
                1
            ]
        },
        {
            "DEFAULT": 0,
            "LABEL": "Sort By",
            "LABELS": [
                "Luma",
                "Hue",
                "Sat",
                "Bright"
            ],
            "NAME": "sortBy",
            "TYPE": "long",
            "VALUES": [
                0,
                1,
                2,
                3
            ]
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "Range Low",
            "MAX": 1,
            "MIN": 0,
            "NAME": "rangeLow",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.8,
            "LABEL": "Range High",
            "MAX": 1,
            "MIN": 0,
            "NAME": "rangeHigh",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "Stretch Amount",
            "MAX": 1,
            "MIN": 0,
            "NAME": "stretch",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "Fade to Original",
            "MAX": 1,
            "MIN": 0,
            "NAME": "fade",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert Sort Order",
            "NAME": "invertMode",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0.1,
            "LABEL": "Smooth Jitter",
            "MAX": 1,
            "MIN": 0,
            "NAME": "jitter",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2",
    "NAME": "Gnomalab PixelSort Clean",
    "VSN": "1.0.0"
}
*/

float random (vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float getVal(vec4 col) {
    if (sortBy == 0) return dot(col.rgb, vec3(0.299, 0.587, 0.114));
    vec3 hsv = rgb2hsv(col.rgb);
    if (sortBy == 1) return hsv.x;
    if (sortBy == 2) return hsv.y;
    return hsv.z;
}

void main() {
    vec2 uv = isf_FragNormCoord;
    vec2 stepDir = (direction == 0) ? vec2(0.0, 1.0 / RENDERSIZE.y) : vec2(1.0 / RENDERSIZE.x, 0.0);
    
    // Guardamos el color original
    vec4 originalCol = IMG_NORM_PIXEL(inputImage, uv);
    float originalVal = getVal(originalCol);
    
    // Si el píxel original no está en el rango, dibujamos original y salimos
    if (originalVal < rangeLow || originalVal > rangeHigh) {
        gl_FragColor = originalCol;
        return;
    }

    float noise = random(uv) * jitter * 0.05;
    vec2 bestCoord = uv;
    float bestVal = originalVal;
    float distToBest = 0.0;
    
    const int iterations = 80; 
    for (int i = 1; i < iterations; i++) {
        float p = float(i) / float(iterations);
        // El estiramiento se calcula hacia atrás
        float offset = p * stretch * 250.0;
        vec2 testUV = uv - (stepDir * (offset + noise));
        
        if (testUV.x < 0.0 || testUV.x > 1.0 || testUV.y < 0.0 || testUV.y > 1.0) break;
        
        vec4 testCol = IMG_NORM_PIXEL(inputImage, testUV);
        float testVal = getVal(testCol);
        
        // Verificamos si el píxel muestreado califica para el sort
        if (testVal >= rangeLow && testVal <= rangeHigh) {
            bool condition = invertMode ? (testVal < bestVal) : (testVal > bestVal);
            if (condition) {
                bestCoord = testUV;
                bestVal = testVal;
                distToBest = p; 
            }
        } else {
            // Si el camino se corta por un píxel fuera de rango, paramos
            break; 
        }
    }

    vec4 sortedCol = IMG_NORM_PIXEL(inputImage, bestCoord);
    
    // CORRECCIÓN: El fade ahora mezcla entre el color ordenado y el original
    // En lugar de multiplicar por negro, hacemos un mix suave.
    float fadeAmount = pow(1.0 - distToBest, fade * 4.0);
    
    // Si no encontramos un píxel mejor (distToBest = 0), el mix dará el original.
    gl_FragColor = mix(originalCol, sortedCol, fadeAmount);
}