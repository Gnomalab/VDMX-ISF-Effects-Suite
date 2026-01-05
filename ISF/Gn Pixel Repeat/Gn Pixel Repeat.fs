/*{
    "CATEGORIES": [
        "Glitch",
        "Geometry",
        "Repeat",
        "Gnomalab"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "\"Generador de repetición de píxeles basado en máscaras. Crea estelas geométricas rítmicas con control de dirección, espejo (ping-pong) y búsqueda por ray-casting de hasta 70 pasos.\"",
    "INPUTS": [
        {
            "LABEL": "Source Image",
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "LABEL": "Mask Layer",
            "NAME": "maskImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": false,
            "LABEL": "Usar Capa de Máscara",
            "NAME": "useMaskLayer",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Matte Channel",
            "LABELS": [
                "Luminance",
                "Red",
                "Green",
                "Blue",
                "Alpha",
                "Sat x Luma"
            ],
            "NAME": "channel",
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
            "DEFAULT": 0.5,
            "LABEL": "Threshold",
            "MAX": 1,
            "MIN": 0,
            "NAME": "threshold",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "LABEL": "Threshold Softness",
            "MAX": 1,
            "MIN": 0,
            "NAME": "softness",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Invert Threshold",
            "NAME": "invertThreshold",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 90,
            "LABEL": "Dirección",
            "MAX": 360,
            "MIN": 0,
            "NAME": "direction",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "Streak Length",
            "MAX": 1,
            "MIN": 0,
            "NAME": "streakLength",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.15,
            "LABEL": "Repeat Length",
            "MAX": 1,
            "MIN": 0.01,
            "NAME": "repeatLength",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Offset",
            "MAX": 1,
            "MIN": -1,
            "NAME": "offset",
            "TYPE": "float"
        },
        {
            "DEFAULT": false,
            "LABEL": "Mirror (Ping-Pong)",
            "NAME": "mirror",
            "TYPE": "bool"
        },
        {
            "DEFAULT": false,
            "LABEL": "Transparent (Matte Only)",
            "NAME": "transparent",
            "TYPE": "bool"
        },
        {
            "DEFAULT": true,
            "LABEL": "Override When Brighter",
            "NAME": "overrideBrighter",
            "TYPE": "bool"
        },
        {
            "DEFAULT": true,
            "LABEL": "Include Shape Interior",
            "NAME": "includeInterior",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Show Output",
            "LABELS": [
                "Final Result",
                "Effect Only",
                "Visualization"
            ],
            "NAME": "show",
            "TYPE": "long",
            "VALUES": [
                0,
                1,
                2
            ]
        }
    ],
    "ISFVSN": "2",
    "VSN": "1.0.0"
}
*/

float getChannelValue(vec4 col, int ch) {
    if (ch == 0) return dot(col.rgb, vec3(0.299, 0.587, 0.114));
    if (ch == 1) return col.r;
    if (ch == 2) return col.g;
    if (ch == 3) return col.b;
    if (ch == 4) return col.a;
    if (ch == 5) {
        float maxC = max(col.r, max(col.g, col.b));
        float minC = min(col.r, min(col.g, col.b));
        float luma = dot(col.rgb, vec3(0.299, 0.587, 0.114));
        return ((maxC - minC) / (maxC + 0.0001)) * luma;
    }
    return 0.0;
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    vec2 res = RENDERSIZE.xy;
    vec4 originalCol = IMG_NORM_PIXEL(inputImage, uv);
    
    float sRange = softness * 0.5 + 0.001;
    
    // 1. CHEQUEO INTERIOR
    vec4 currentCheckCol = useMaskLayer ? IMG_NORM_PIXEL(maskImage, uv) : originalCol;
    float currentRawVal = getChannelValue(currentCheckCol, int(channel));
    float currentMask = smoothstep(threshold - sRange, threshold + sRange, currentRawVal);
    if (invertThreshold) currentMask = 1.0 - currentMask;
    
    if (includeInterior && currentMask > 0.5 && show == 0) {
        gl_FragColor = originalCol;
        return;
    }

    // 2. CONFIGURAR RAYO
    float rad = radians(direction + 180.0); 
    vec2 dirVec = vec2(cos(rad), sin(rad)) / res; 
    float maxDist = (streakLength == 0.0) ? 2.0 : streakLength;
    
    vec2 sourceUV = uv;
    bool found = false;
    float distToSource = 0.0;
    float currentBestMask = 0.0;
    
    // 3. BÚSQUEDA DEL ORIGEN
    for (int i = 0; i < 70; i++) {
        float stepDist = float(i) / 69.0 * maxDist;
        vec2 testUV = uv + dirVec * stepDist * res.y; 
        
        if (testUV.x < 0.0 || testUV.x > 1.0 || testUV.y < 0.0 || testUV.y > 1.0) break;
        
        vec4 testCol = useMaskLayer ? IMG_NORM_PIXEL(maskImage, testUV) : IMG_NORM_PIXEL(inputImage, testUV);
        float testRawVal = getChannelValue(testCol, int(channel));
        float testMask = smoothstep(threshold - sRange, threshold + sRange, testRawVal);
        if (invertThreshold) testMask = 1.0 - testMask;
        
        if (testMask > 0.01) {
            if (overrideBrighter) {
                if (testMask > currentBestMask) {
                    currentBestMask = testMask;
                    sourceUV = testUV;
                    distToSource = stepDist;
                    found = true;
                }
            } else {
                sourceUV = testUV;
                distToSource = stepDist;
                currentBestMask = testMask;
                found = true;
                break; 
            }
        }
    }

    // 4. RESULTADO
    vec4 finalFxCol = originalCol;
    float visValue = 0.0;

    if (found) {
        float phase = distToSource + offset;
        float repeatedDist = mirror ? abs(fract((phase / repeatLength) * 0.5) * 2.0 - 1.0) * repeatLength : fract(phase / repeatLength) * repeatLength;
        vec2 sampleUV = sourceUV - (dirVec * repeatedDist * res.y);
        finalFxCol = mix(originalCol, IMG_NORM_PIXEL(inputImage, clamp(sampleUV, 0.0, 1.0)), currentBestMask);
        visValue = currentBestMask * (1.0 - (distToSource / maxDist));
    }

    // 5. SALIDA FINAL
    if (show == 2) {
        gl_FragColor = vec4(vec3(visValue), 1.0);
    } else {
        vec4 outCol = found ? finalFxCol : (transparent ? vec4(0.0) : originalCol);
        if (show == 1) gl_FragColor = (found ? finalFxCol : vec4(0.0));
        else gl_FragColor = outCol;
    }
}