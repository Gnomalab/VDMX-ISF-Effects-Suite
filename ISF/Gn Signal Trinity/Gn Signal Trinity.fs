/*{
    "CATEGORIES": [
        "Signal",
        "Gnomalab",
        "Glitch",
        "Modulation",
        "Feedback"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "\"Procesador triple por bandas de luminancia. Rutea dinámicamente tres motores (Síntesis Analógica, Glitch Radial y Feedback Geométrico) entre negros, medios y blancos con control de orden conmutable.\"",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0.3,
            "LABEL": "MASK: Umbral NEGROS/MED",
            "MAX": 1,
            "MIN": 0,
            "NAME": "threshold_min",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.7,
            "LABEL": "MASK: Umbral MED/BLANCOS",
            "MAX": 1,
            "MIN": 0,
            "NAME": "threshold_max",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "LABEL": "MASK: Suavizado",
            "MAX": 0.5,
            "MIN": 0,
            "NAME": "softness",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "MASK: Invertir Todo",
            "NAME": "invert_mask",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "ORDEN: Conmutador",
            "LABELS": [
                "N:Synth / M:Glitch / B:FB",
                "N:FB / M:Synth / B:Glitch",
                "N:Glitch / M:FB / B:Synth",
                "FULL FEEDBACK"
            ],
            "NAME": "processing_order",
            "TYPE": "long",
            "VALUES": [
                0,
                1,
                2,
                3
            ]
        },
        {
            "DEFAULT": 100,
            "LABEL": "SYNTH: Omega (Freq)",
            "MAX": 1000,
            "MIN": 0,
            "NAME": "omega",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "SYNTH: Fase",
            "MAX": 6.28,
            "MIN": 0,
            "NAME": "phase",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "SYNTH: Distorsión",
            "MAX": 10,
            "MIN": 0.1,
            "NAME": "distortion",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "SYNTH: Orientacion",
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
            "DEFAULT": 1,
            "LABEL": "SYNTH: Modo Color",
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
            "DEFAULT": 0.2,
            "LABEL": "SYNTH: Lowpass Signal",
            "MAX": 1,
            "MIN": 0,
            "NAME": "lowpass",
            "TYPE": "float"
        },
        {
            "DEFAULT": [
                0.5,
                0.5
            ],
            "LABEL": "GLITCH: RGB Pos (Center 0.5)",
            "NAME": "pos_rgb",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": 5,
            "LABEL": "GLITCH: Copias",
            "MAX": 20,
            "MIN": 1,
            "NAME": "glit_copies",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "GLITCH: Angulo",
            "MAX": 1,
            "MIN": 0,
            "NAME": "glit_angle",
            "TYPE": "float"
        },
        {
            "DEFAULT": 15,
            "LABEL": "GLITCH: Offset",
            "MAX": 100,
            "MIN": 0,
            "NAME": "glit_offset",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.9,
            "LABEL": "FB: Amount",
            "MAX": 1,
            "MIN": 0,
            "NAME": "fb_amt",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "FB: Modo Geo",
            "LABELS": [
                "Linear",
                "Zoom",
                "Rotation"
            ],
            "NAME": "fb_mode",
            "TYPE": "long",
            "VALUES": [
                0,
                1,
                2
            ]
        },
        {
            "DEFAULT": 0.005,
            "LABEL": "FB: Linear Offset",
            "MAX": 0.05,
            "MIN": 0,
            "NAME": "fb_offset",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "FB: Linear Angle",
            "MAX": 1,
            "MIN": 0,
            "NAME": "fb_angle",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.01,
            "LABEL": "FB: Zoom Speed",
            "MAX": 0.1,
            "MIN": -0.1,
            "NAME": "fb_zoom",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.01,
            "LABEL": "FB: Rotate Speed",
            "MAX": 0.1,
            "MIN": -0.1,
            "NAME": "fb_rotate",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "FB: Update Speed",
            "MAX": 1,
            "MIN": 0.05,
            "NAME": "fb_speed",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "FB: RESET (Panic)",
            "NAME": "reset_fb",
            "TYPE": "bool"
        }
    ],
    "ISFVSN": "2",
    "PASSES": [
        {
            "PERSISTENT": true,
            "TARGET": "fbBuffer"
        }
    ],
    "VSN": "1.0.0"
}
*/

const float PI = 3.14159265359;

// --- FUNCIONES DE APOYO ---
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float modulate(float carrier, float signal) {
    return clamp(sin(carrier + (signal * 12.0)) * signal, -1.0, 1.0);
}

// --- MOTOR 1: SYNTH MODULATION PRO ---
vec4 getSynth(vec2 uv, vec4 lp) {
    float t = (orientation == 0) ? uv.x : uv.y;
    float carrier = t * omega + phase;
    vec3 fore = vec3(0.0);
    float lW = 0.6;
    float s_sig = dot(lp.rgb, vec3(0.299, 0.587, 0.114));
    
    if (colorMode == 0) fore = vec3(smoothstep(lW, 1.0, 1.0 - abs(modulate(carrier, s_sig * distortion))));
    else if (colorMode == 1) fore = smoothstep(vec3(lW), vec3(1.0), 1.0 - abs(vec3(modulate(carrier, lp.r * distortion), modulate(carrier, lp.g * distortion), modulate(carrier, lp.b * distortion))));
    else if (colorMode == 2) fore = 1.0 - smoothstep(vec3(lW), vec3(1.0), 1.0 - abs(vec3(modulate(carrier, (1.0-lp.r)*distortion), modulate(carrier, (1.0-lp.g)*distortion), modulate(carrier, (1.0-lp.b)*distortion))));
    else if (colorMode == 3) {
        float k = modulate(carrier, (1.0 - s_sig) * distortion);
        vec3 cmy = 1.0 - smoothstep(vec3(lW), vec3(1.0), 1.0 - abs(vec3(modulate(carrier, (1.0-lp.r)*distortion), modulate(carrier, (1.0-lp.g)*distortion), modulate(carrier, (1.0-lp.b)*distortion))));
        fore = cmy * (1.0 - smoothstep(lW, 1.0, 1.0 - abs(k)));
    }
    else fore = hsv2rgb(vec3(fract(s_sig + phase/6.28), 0.8, smoothstep(lW, 1.0, 1.0 - abs(modulate(carrier, s_sig * distortion)))));
    
    return vec4(fore, 1.0);
}

// --- MOTOR 2: GLITCH CONTROL RADIAL ---
vec4 getGlitch(vec2 uv) {
    vec2 off = (pos_rgb - 0.5) * 0.25;
    float rad = glit_angle * 2.0 * PI;
    vec2 dir = vec2(cos(rad), sin(rad));
    vec4 acc = vec4(0.0);
    int copies = int(glit_copies);
    for (int i = 0; i < 20; ++i) {
        if (i >= copies) break;
        vec2 offset = float(i) * glit_offset * dir * (1.0/RENDERSIZE);
        vec2 uvOff = fract(uv - offset);
        acc += vec4(IMG_NORM_PIXEL(inputImage, fract(uvOff + off)).r, IMG_NORM_PIXEL(inputImage, uvOff).g, IMG_NORM_PIXEL(inputImage, fract(uvOff - off)).b, 1.0);
    }
    return acc / max(1.0, float(copies));
}

// --- MOTOR 3: GEOMETRIC FEEDBACK ---
vec4 getFeedback(vec2 uv) {
    vec2 fb_uv = uv;
    vec2 center = vec2(0.5, 0.5);
    if (fb_mode == 0) {
        vec2 dir = vec2(cos(fb_angle * 2.0 * PI), sin(fb_angle * 2.0 * PI));
        fb_uv = fract(uv - dir * fb_offset);
    } else if (fb_mode == 1) {
        fb_uv = (uv - center) * (1.0 - fb_zoom) + center;
    } else {
        float rad = fb_rotate * PI * 0.1;
        float c = cos(rad), s = sin(rad);
        vec2 rot = uv - center;
        fb_uv = vec2(rot.x * c - rot.y * s, rot.x * s + rot.y * c) + center;
    }
    return mix(IMG_NORM_PIXEL(inputImage, uv), IMG_NORM_PIXEL(fbBuffer, fb_uv), fb_amt);
}

// --- MAIN ---
void main() {
    vec2 uv = isf_FragNormCoord;
    vec4 baseCol = IMG_NORM_PIXEL(inputImage, uv);
    
    // Panic / Update Speed Logic
    if (reset_fb) { gl_FragColor = baseCol; return; }
    float interval = floor(mix(30.0, 1.0, fb_speed));
    if (mod(float(FRAMEINDEX), interval) >= 1.0) { gl_FragColor = IMG_NORM_PIXEL(fbBuffer, uv); return; }

    // Lowpass para la señal del Synth
    float lpSize = lowpass * 0.02;
    vec4 lp = (baseCol + IMG_NORM_PIXEL(inputImage, uv+vec2(lpSize,0)) + IMG_NORM_PIXEL(inputImage, uv-vec2(lpSize,0))) / 3.0;

    // Mascaras
    float luma = dot(baseCol.rgb, vec3(0.299, 0.587, 0.114));
    if (invert_mask) luma = 1.0 - luma;
    float m_low = 1.0 - smoothstep(threshold_min - softness, threshold_min + softness, luma);
    float m_mid = smoothstep(threshold_min - softness, threshold_min + softness, luma) * (1.0 - smoothstep(threshold_max - softness, threshold_max + softness, luma));
    float m_high = smoothstep(threshold_max - softness, threshold_max + softness, luma);

    // Procesado
    vec4 resSynth = getSynth(uv, lp);
    vec4 resGlitch = getGlitch(uv);
    vec4 resFB = getFeedback(uv);

    // Salida según orden
    vec4 finalCol = baseCol;
    if (processing_order == 3) finalCol = resFB;
    else if (processing_order == 0) { finalCol = mix(finalCol, resSynth, m_low); finalCol = mix(finalCol, resGlitch, m_mid); finalCol = mix(finalCol, resFB, m_high); }
    else if (processing_order == 1) { finalCol = mix(finalCol, resFB, m_low); finalCol = mix(finalCol, resSynth, m_mid); finalCol = mix(finalCol, resGlitch, m_high); }
    else if (processing_order == 2) { finalCol = mix(finalCol, resGlitch, m_low); finalCol = mix(finalCol, resFB, m_mid); finalCol = mix(finalCol, resSynth, m_high); }

    finalCol.a = 1.0;
    gl_FragColor = finalCol;
}