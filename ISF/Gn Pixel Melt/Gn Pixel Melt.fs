/*{
    "CATEGORIES": [
        "Organic",
        "Gnomalab",
        "Melt"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "Organic melt effect based on a persistent buffer. Displaces pixels by luminance, saturation or color at any angle, with viscosity and drip-jitter control.",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Melt based on:",
            "LABELS": [
                "Luminance",
                "Saturation",
                "Hue",
                "Red",
                "Green",
                "Blue"
            ],
            "NAME": "meltMode",
            "TYPE": "long",
            "VALUES": [
                0,
                1,
                2,
                3,
                4,
                5
            ]
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert bias (melt shadows)",
            "NAME": "invertBias",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "Blend threshold",
            "MAX": 1,
            "MIN": 0,
            "NAME": "meltThreshold",
            "TYPE": "float"
        },
        {
            "DEFAULT": 15,
            "LABEL": "Fall speed",
            "MAX": 50,
            "MIN": 0,
            "NAME": "meltSpeed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.98,
            "LABEL": "Viscosity (trail)",
            "MAX": 1,
            "MIN": 0.8,
            "NAME": "viscosity",
            "TYPE": "float"
        },
        {
            "DEFAULT": 270,
            "LABEL": "Gravity angle",
            "MAX": 360,
            "MIN": 0,
            "NAME": "angle",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.4,
            "LABEL": "Drip noise",
            "MAX": 1,
            "MIN": 0,
            "NAME": "jitter",
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
            "TARGET": "meltBuffer"
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

float getMeltBias(vec4 col) {
    float b = 0.0;
    if (meltMode == 0) b = dot(col.rgb, vec3(0.299, 0.587, 0.114));
    else if (meltMode == 1) b = rgb2hsv(col.rgb).y;
    else if (meltMode == 2) b = rgb2hsv(col.rgb).x;
    else if (meltMode == 3) b = col.r;
    else if (meltMode == 4) b = col.g;
    else b = col.b;
    
    // Aplicamos la inversión si el toggle está activo
    return invertBias ? (1.0 - b) : b;
}

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    vec2 res = RENDERSIZE.xy;
    
    vec4 currentImg = IMG_NORM_PIXEL(inputImage, uv);
    float bias = getMeltBias(currentImg);

    // 1. Gravedad y Jitter
    float rad = radians(angle);
    vec2 dir = vec2(cos(rad), sin(rad));
    float colNoise = hash(vec2(uv.x * 0.8, 0.0)) * jitter;
    
    // 2. Cálculo del movimiento basado en el bias corregido
    float force = smoothstep(meltThreshold, meltThreshold + 0.1, bias);
    vec2 move = (dir * (meltSpeed + colNoise * 30.0) * force) / res.y;

    // 3. Muestreo del Buffer
    vec2 sampleUV = uv - move;
    vec4 prevBuffer = IMG_NORM_PIXEL(meltBuffer, clamp(sampleUV, 0.0, 1.0));

    // 4. Composición viscosa
    vec4 finalColor;
    if (bias > meltThreshold) {
        finalColor = mix(currentImg, prevBuffer, viscosity);
    } else {
        // En zonas que no se derriten, el líquido fluye con menos fuerza
        finalColor = mix(currentImg, prevBuffer, viscosity * 0.4);
    }

    if (clearBuffer || FRAMEINDEX < 2) {
        finalColor = currentImg;
    }

    gl_FragColor = finalColor;
}