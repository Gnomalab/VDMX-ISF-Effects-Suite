/*{
    "CATEGORIES": [
        "Gnomalab",
        "Glitch",
        "Compression"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "fake Datamosh ",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0.7,
            "LABEL": "Fuerza Derretido",
            "MAX": 1,
            "MIN": 0,
            "NAME": "mosh_strength",
            "TYPE": "float"
        },
        {
            "DEFAULT": 16,
            "LABEL": "Tamaño Macrobloque",
            "MAX": 64,
            "MIN": 2,
            "NAME": "block_size",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "Sensibilidad Mov.",
            "MAX": 1,
            "MIN": 0,
            "NAME": "motion_threshold",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Solo en Bordes",
            "MAX": 1,
            "MIN": 0,
            "NAME": "edge_boost",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Luma Min",
            "MAX": 1,
            "MIN": 0,
            "NAME": "luma_key_min",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Luma Max",
            "MAX": 1,
            "MIN": 0,
            "NAME": "luma_key_max",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.98,
            "LABEL": "Persistencia Escena",
            "MAX": 1,
            "MIN": 0,
            "NAME": "persistence",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "Limpieza Fondo",
            "MAX": 1,
            "MIN": 0,
            "NAME": "background_clean",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Fuerza Gravedad",
            "MAX": 0.2,
            "MIN": 0,
            "NAME": "gravity_force",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.75,
            "LABEL": "Direccion Gravedad",
            "MAX": 1,
            "MIN": 0,
            "NAME": "gravity_dir",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.3,
            "LABEL": "Ensuciar Color",
            "MAX": 1,
            "MIN": 0,
            "NAME": "color_corruption",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Limpiar I-Frame",
            "NAME": "reset",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0.15,
            "LABEL": "Velocidad Recuperacion",
            "MAX": 1,
            "MIN": 0,
            "NAME": "sync_speed",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2",
    "PASSES": [
        {
            "PERSISTENT": true,
            "TARGET": "lastFrame"
        }
    ],
    "VSN": "1.0.0"
}
*/

vec3 rgb2yuv(vec3 rgb) {
    float y = dot(rgb, vec3(0.299, 0.587, 0.114));
    float u = dot(rgb, vec3(-0.147, -0.289, 0.436));
    float v = dot(rgb, vec3(0.615, -0.515, -0.100));
    return vec3(y, u, v);
}

vec3 yuv2rgb(vec3 yuv) {
    float r = yuv.x + 1.140 * yuv.z;
    float g = yuv.x - 0.395 * yuv.y - 0.581 * yuv.z;
    float b = yuv.x + 2.032 * yuv.y;
    return vec3(r, g, b);
}

void main() {
    vec2 uv = isf_FragNormCoord;
    float b = max(block_size, 1.0);
    vec2 bUV = floor(uv * RENDERSIZE / b) * b / RENDERSIZE;
    vec2 off = vec2(b) / RENDERSIZE;
    
    vec4 current = IMG_NORM_PIXEL(inputImage, uv);
    vec4 previous = IMG_NORM_PIXEL(lastFrame, uv);
    
    // Estimación de movimiento y bordes
    float lC = dot(IMG_NORM_PIXEL(inputImage, bUV).rgb, vec3(0.3, 0.59, 0.11));
    float lR = dot(IMG_NORM_PIXEL(inputImage, bUV + vec2(off.x, 0.0)).rgb, vec3(0.3, 0.59, 0.11));
    float lU = dot(IMG_NORM_PIXEL(inputImage, bUV + vec2(0.0, off.y)).rgb, vec3(0.3, 0.59, 0.11));
    float edge = abs(lC - lR) + abs(lC - lU);
    
    // Luma Keyer
    float pixelLuma = dot(current.rgb, vec3(0.299, 0.587, 0.114));
    bool inLumaRange = (pixelLuma >= luma_key_min && pixelLuma <= luma_key_max);
    
    // Gravedad
    vec2 motion = vec2(lC - lR, lC - lU);
    float angle = gravity_dir * 6.28318;
    motion += vec2(cos(angle), sin(angle)) * gravity_force;

    // Activación inteligente
    float motion_mag = length(motion);
    float edge_factor = mix(1.0, edge * 10.0, edge_boost);
    bool shouldMosh = (motion_mag * edge_factor > motion_threshold) && inLumaRange;

    float activePersistence = reset ? mix(persistence, 0.0, sync_speed) : persistence;
    vec4 finalCol;
    
    if (shouldMosh) {
        vec2 moshUV = uv + (motion * mosh_strength * 0.25);
        
        // Muestreo YUV Real
        vec4 sampleLuma = IMG_NORM_PIXEL(lastFrame, moshUV);
        float bChroma = b * 2.0; // Chroma subsampling factor
        vec2 chromaUV = floor(moshUV * RENDERSIZE / bChroma) * bChroma / RENDERSIZE;
        vec4 sampleChroma = IMG_NORM_PIXEL(lastFrame, chromaUV);
        
        vec3 compositeRGB = yuv2rgb(vec3(rgb2yuv(sampleLuma.rgb).x, rgb2yuv(sampleChroma.rgb).y, rgb2yuv(sampleChroma.rgb).z));

        // Corrupción de color suave (anti-quemado)
        if (color_corruption > 0.0) {
            float lVal = dot(compositeRGB, vec3(0.3, 0.59, 0.11));
            vec3 sat = mix(vec3(lVal), compositeRGB, 1.0 + color_corruption * 0.5);
            float levels = mix(256.0, 16.0, color_corruption * color_corruption);
            compositeRGB = mix(compositeRGB, floor(sat * levels) / levels, color_corruption);
        }
        
        finalCol.rgb = mix(current.rgb, clamp(compositeRGB, 0.0, 1.0), activePersistence);
    } else {
        // Anti-Smear Logic
        float bg_persistence = mix(activePersistence, 0.0, background_clean);
        finalCol = mix(current, previous, bg_persistence);
    }

    if (reset) finalCol = mix(finalCol, current, sync_speed);

    gl_FragColor = vec4(finalCol.rgb, 1.0);
}