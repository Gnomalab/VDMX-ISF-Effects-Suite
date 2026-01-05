/*{
    "CATEGORIES": [
        "Gnomalab",
        "Glitch"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "Pixel sorter persistente con modulación rítmica de velocidad. Incluye controles de cadencia (pulse), desvanecimiento de color (desaturación/oscurecimiento) y selección por rangos de luminancia.\"",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": false,
            "LABEL": "Modo Horizontal",
            "NAME": "horizontalMode",
            "TYPE": "bool"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invertir Dirección",
            "NAME": "invertDirection",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0.25,
            "LABEL": "Umbral Mínimo",
            "MAX": 1,
            "MIN": 0,
            "NAME": "thresholdMin",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.8,
            "LABEL": "Umbral Máximo",
            "MAX": 1,
            "MIN": 0,
            "NAME": "thresholdMax",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invertir Selección FX",
            "NAME": "invertSelection",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0.005,
            "LABEL": "Velocidad Base",
            "MAX": 0.05,
            "MIN": 0,
            "NAME": "meltSpeed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Ritmo Cadencia",
            "MAX": 10,
            "MIN": 0,
            "NAME": "cadenceRate",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.01,
            "LABEL": "Desvanecer (Fade)",
            "MAX": 0.2,
            "MIN": 0,
            "NAME": "colorFade",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.98,
            "LABEL": "Persistencia (Feedback)",
            "MAX": 1,
            "MIN": 0,
            "NAME": "feedback",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2",
    "NAME": "Gnomalab_PixelSorter_V3",
    "PASSES": [
        {
            "PERSISTENT": true,
            "TARGET": "bufferAccum"
        }
    ],
    "VSN": "1.0.0"
}
*/

void main() {
    vec2 uv = isf_FragNormCoord;
    vec4 img = IMG_NORM_PIXEL(inputImage, uv);
    
    // Luminancia para el umbral
    float bright = dot(img.rgb, vec3(0.299, 0.587, 0.114));
    
    // Lógica de Selección
    bool isInRange = (bright >= thresholdMin && bright <= thresholdMax);
    if (invertSelection) isInRange = !isInRange;

    // Cálculo de movimiento y cadencia
    float pulse = sin(TIME * cadenceRate * 6.28318);
    float dynamicSpeed = meltSpeed * (1.0 + pulse);
    float finalSpeed = invertDirection ? -dynamicSpeed : dynamicSpeed;
    vec2 offset = horizontalMode ? vec2(finalSpeed, 0.0) : vec2(0.0, finalSpeed);

    if (isInRange) {
        // Obtenemos el color que viene de la dirección del movimiento
        vec4 prev = IMG_NORM_PIXEL(bufferAccum, uv + offset);
        
        // --- NUEVA LÓGICA DE FADE (MÁS AGRESIVA) ---
        // 1. Desaturamos
        float gray = dot(prev.rgb, vec3(0.3, 0.59, 0.11));
        prev.rgb = mix(prev.rgb, vec3(gray), colorFade * 2.0);
        
        // 2. Oscurecemos (para que la estela desaparezca visualmente)
        prev.rgb *= (1.0 - colorFade);
        
        // Mezclamos con la imagen actual según la persistencia
        gl_FragColor = mix(img, prev, feedback);
    } else {
        gl_FragColor = img;
    }
}