/*{
    "CATEGORIES": [
        "Glitch",
        "Distortion"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "Glitch Control - switchable processing order",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": true,
            "LABEL": "Enable RGB",
            "NAME": "enable_rgb",
            "TYPE": "bool"
        },
        {
            "DEFAULT": [
                0.5,
                0.5
            ],
            "LABEL": "Red Position",
            "NAME": "pos_r",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": [
                0.5,
                0.5
            ],
            "LABEL": "Green Position",
            "NAME": "pos_g",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": [
                0.5,
                0.5
            ],
            "LABEL": "Blue Position",
            "NAME": "pos_b",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": false,
            "LABEL": "Enable displacement",
            "NAME": "enable_disp",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "ORDER",
            "LABELS": [
                "1: RGB > Displacement",
                "2: Displacement > RGB"
            ],
            "NAME": "processing_order",
            "TYPE": "long",
            "VALUES": [
                0,
                1
            ]
        },
        {
            "DEFAULT": 0,
            "LABEL": "Mask source",
            "LABELS": [
                "RGB sum",
                "Luminance",
                "Hue",
                "Saturation",
                "Lightness",
                "Chrom. I",
                "Chrom. Q",
                "Alpha"
            ],
            "NAME": "disp_mask_mode",
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
            "DEFAULT": 0.5,
            "LABEL": "Disp. threshold",
            "MAX": 1,
            "MIN": 0,
            "NAME": "disp_threshold",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "LABEL": "Disp. softness",
            "MAX": 0.5,
            "MIN": 0,
            "NAME": "disp_softness",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert mask",
            "NAME": "disp_mask_invert",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 5,
            "LABEL": "Number of copies",
            "MAX": 20,
            "MIN": 1,
            "NAME": "disp_copies",
            "TYPE": "float"
        },
        {
            "DEFAULT": 15,
            "LABEL": "Displace offset",
            "MAX": 100,
            "MIN": 0,
            "NAME": "disp_offset",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Displace angle",
            "MAX": 1,
            "MIN": 0,
            "NAME": "disp_angle",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Enable stretch",
            "NAME": "enable_stretch",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Axis (0:H, 1:V)",
            "LABELS": [
                "Horizontal",
                "Vertical"
            ],
            "NAME": "str_axis",
            "TYPE": "long",
            "VALUES": [
                0,
                1
            ]
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert stretch",
            "NAME": "str_invert",
            "TYPE": "bool"
        },
        {
            "DEFAULT": [
                0.5,
                0.5
            ],
            "LABEL": "Stretch position",
            "NAME": "str_pos",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": false,
            "LABEL": "Enable color cycle",
            "NAME": "enable_cycle",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Red Amount",
            "MAX": 1,
            "MIN": 0,
            "NAME": "amt_r",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Green Amount",
            "MAX": 1,
            "MIN": 0,
            "NAME": "amt_g",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Blue Amount",
            "MAX": 1,
            "MIN": 0,
            "NAME": "amt_b",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2",
    "VSN": "1.0.0"
}
*/

// --- UTILIDADES DE COLOR ---
vec3 rgb2hls(vec3 c) {
    float minVal = min(min(c.r, c.g), c.b);
    float maxVal = max(max(c.r, c.g), c.b);
    float delta = maxVal - minVal;
    float l = (maxVal + minVal) / 2.0;
    float s = 0.0, h = 0.0;
    if (delta > 0.0) {
        s = l < 0.5 ? delta / (maxVal + minVal) : delta / (2.0 - maxVal - minVal);
        if (c.r == maxVal) h = (c.g - c.b) / delta + (c.g < c.b ? 6.0 : 0.0);
        else if (c.g == maxVal) h = (c.b - c.r) / delta + 2.0;
        else h = (c.r - c.g) / delta + 4.0;
        h /= 6.0;
    }
    return vec3(h, l, s);
}

vec2 rgb2yiq(vec3 c) {
    return vec2(0.596 * c.r - 0.274 * c.g - 0.322 * c.b, 0.211 * c.r - 0.523 * c.g + 0.312 * c.b);
}

float getMaskValue(vec4 col) {
    if (disp_mask_mode == 0) return (col.r + col.g + col.b) / 3.0;
    if (disp_mask_mode == 1) return dot(col.rgb, vec3(0.299, 0.587, 0.114));
    vec3 hls = rgb2hls(col.rgb);
    if (disp_mask_mode == 2) return hls.x;
    if (disp_mask_mode == 3) return hls.z;
    if (disp_mask_mode == 4) return hls.y;
    vec2 iq = rgb2yiq(col.rgb);
    if (disp_mask_mode == 5) return iq.x;
    if (disp_mask_mode == 6) return iq.y;
    return col.a;
}

// --- LOGICA CORE ---
vec2 normPoint(vec2 p) {
    return (p.x > 1.0 || p.y > 1.0 || p.x < -1.0 || p.y < -1.0) ? p / RENDERSIZE : p;
}

vec2 applyStretch(vec2 uv) {
    if (!enable_stretch) return uv;
    vec2 s_pos = normPoint(str_pos);
    vec2 res = uv;
    if (str_axis == 0) {
        if (!str_invert) { if (uv.x > s_pos.x) res.x = s_pos.x; }
        else { if (uv.x < s_pos.x) res.x = s_pos.x; }
    } else {
        if (!str_invert) { if (uv.y > s_pos.y) res.y = s_pos.y; }
        else { if (uv.y < s_pos.y) res.y = s_pos.y; }
    }
    return res;
}

// Devuelve el color con RGB aplicado (sin desplazamiento)
vec4 getRGBOnly(vec2 uv) {
    if (!enable_rgb) return IMG_NORM_PIXEL(inputImage, applyStretch(uv));
    float scale = 0.15;
    vec2 r_off = (normPoint(pos_r) - 0.5) * scale;
    vec2 g_off = (normPoint(pos_g) - 0.5) * scale;
    vec2 b_off = (normPoint(pos_b) - 0.5) * scale;
    float r = IMG_NORM_PIXEL(inputImage, applyStretch(fract(uv + r_off))).r;
    float g = IMG_NORM_PIXEL(inputImage, applyStretch(fract(uv + g_off))).g;
    float b = IMG_NORM_PIXEL(inputImage, applyStretch(fract(uv + b_off))).b;
    return vec4(r, g, b, IMG_NORM_PIXEL(inputImage, applyStretch(uv)).a);
}

// Devuelve el color con Displacement aplicado (sin RGB)
vec4 getDispOnly(vec2 uv) {
    vec2 pSize = 1.0 / RENDERSIZE;
    vec2 s_uv = applyStretch(uv);
    vec4 base = IMG_NORM_PIXEL(inputImage, s_uv);
    if (!enable_disp) return base;
    
    float mask = smoothstep(disp_threshold - disp_softness, disp_threshold + disp_softness, getMaskValue(base));
    if (disp_mask_invert) mask = 1.0 - mask;
    if (mask <= 0.0) return base;
    
    vec2 dir = vec2(cos(disp_angle * 6.2831), sin(disp_angle * 6.2831));
    vec4 acc = vec4(0.0);
    int copies = int(disp_copies);
    for (int i = 0; i < 20; ++i) {
        if (i >= copies) break;
        acc += IMG_NORM_PIXEL(inputImage, applyStretch(fract(uv - float(i) * disp_offset * dir * pSize)));
    }
    return mix(base, acc / max(1.0, float(copies)), mask);
}

void main() {
    vec2 uv = isf_FragNormCoord;
    vec4 finalCol;

    if (processing_order == 0) { 
        // --- MODO 1: RGB -> DISP ---
        vec4 base = getRGBOnly(uv);
        float mask = smoothstep(disp_threshold - disp_softness, disp_threshold + disp_softness, getMaskValue(base));
        if (disp_mask_invert) mask = 1.0 - mask;
        
        if (!enable_disp || mask <= 0.0) {
            finalCol = base;
        } else {
            vec2 dir = vec2(cos(disp_angle * 6.2831), sin(disp_angle * 6.2831));
            vec4 acc = vec4(0.0);
            int copies = int(disp_copies);
            for (int i = 0; i < 20; ++i) {
                if (i >= copies) break;
                acc += getRGBOnly(fract(uv - float(i) * disp_offset * dir * (1.0/RENDERSIZE)));
            }
            finalCol = mix(base, acc / max(1.0, float(copies)), mask);
        }
    } else {
        // --- MODO 2: DISP -> RGB ---
        if (!enable_rgb) {
            finalCol = getDispOnly(uv);
        } else {
            float scale = 0.15;
            vec2 r_off = (normPoint(pos_r) - 0.5) * scale;
            vec2 g_off = (normPoint(pos_g) - 0.5) * scale;
            vec2 b_off = (normPoint(pos_b) - 0.5) * scale;
            finalCol.r = getDispOnly(fract(uv + r_off)).r;
            finalCol.g = getDispOnly(fract(uv + g_off)).g;
            finalCol.b = getDispOnly(fract(uv + b_off)).b;
            finalCol.a = getDispOnly(uv).a;
        }
    }

    if (enable_cycle) {
        finalCol.rgb = fract(finalCol.rgb + vec3(amt_r, amt_g, amt_b));
    }
    gl_FragColor = finalCol;
}