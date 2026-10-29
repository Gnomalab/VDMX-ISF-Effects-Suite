/*{
    "CATEGORIES": [
        "Gnomalab",
        "Distortion",
        "Video Synth"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "Analog scan synthesizer inspired by the Rutt-Etra. Turns the image into a 3D topographic sculpture through luminance deflection, with CRT instability, scan drift and chromatic aberration.",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 100,
            "LABEL": "Number of lines",
            "MAX": 256,
            "MIN": 20,
            "NAME": "line_density",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Relief height",
            "MAX": 2.5,
            "MIN": 0,
            "NAME": "z_extrude",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "Rotate X (360)",
            "MAX": 1,
            "MIN": -1,
            "NAME": "rot_x",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.3,
            "LABEL": "Rotate Y (360)",
            "MAX": 1,
            "MIN": -1,
            "NAME": "rot_y",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1.2,
            "LABEL": "Zoom",
            "MAX": 3,
            "MIN": 0.1,
            "NAME": "zoom",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.05,
            "LABEL": "Scan drift (roll)",
            "MAX": 1,
            "MIN": -1,
            "NAME": "scan_drift",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.003,
            "LABEL": "CRT aberration",
            "MAX": 0.02,
            "MIN": 0,
            "NAME": "rgb_offset",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "LABEL": "Scan jitter",
            "MAX": 1,
            "MIN": 0,
            "NAME": "jitter",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.8,
            "LABEL": "Thread thickness",
            "MAX": 2.5,
            "MIN": 0.1,
            "NAME": "thickness",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Audio kick",
            "MAX": 1,
            "MIN": 0,
            "NAME": "audio_mod",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Saturation",
            "MAX": 1,
            "MIN": 0,
            "NAME": "saturation",
            "TYPE": "float"
        },
        {
            "DEFAULT": 2,
            "LABEL": "Signal gain",
            "MAX": 4,
            "MIN": 0,
            "NAME": "brightness",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.2,
            "LABEL": "Persistence",
            "MAX": 0.95,
            "MIN": 0,
            "NAME": "feedback",
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

// Función de ruido para el Jitter
float rand(float n){ return fract(sin(n) * 43758.5453123); }

void main() {
    vec2 st = (isf_FragNormCoord - 0.5) * 2.0;
    vec3 finalCol = vec3(0.0);
    
    // 1. Parametros de inestabilidad
    float timeJitter = (rand(TIME) - 0.5) * jitter * 0.02;
    float drift = TIME * scan_drift;
    
    // 2. Parametros 3D
    float dynZ = z_extrude + (audio_mod * 0.8);
    float angX = rot_x * 3.14159265;
    float angY = rot_y * 3.14159265;
    
    float cx = cos(angX), sx = sin(angX);
    float cy = cos(angY), sy = sin(angY);
    float camDist = 4.0;

    const int max_steps = 128;
    int current_steps = int(clamp(line_density, 20.0, float(max_steps)));

    for (int i = 0; i < max_steps; i++) {
        if (i >= current_steps) break;

        float y_src = float(i) / float(current_steps);
        float y_norm = (y_src - 0.5) * 2.0;

        // Proyeccion horizontal
        float x_obj = st.x / (zoom * max(abs(cy), 0.05));
        if (cy < 0.0) x_obj *= -1.0;
        if (abs(x_obj) > 1.0) continue;

        // Aplicamos Drift y Jitter al muestreo
        // El jitter horizontal por linea le da ese look electrico
        float lineJitter = (rand(y_src + TIME) - 0.5) * jitter * 0.01;
        float y_drifted = fract(y_src + drift + timeJitter);
        
        // Muestreo RGB con Aberracion
        float redX = x_obj * 0.5 + 0.5 + rgb_offset + lineJitter;
        float greenX = x_obj * 0.5 + 0.5 + lineJitter;
        float blueX = x_obj * 0.5 + 0.5 - rgb_offset + lineJitter;

        float texR = IMG_NORM_PIXEL(inputImage, vec2(redX, y_drifted)).r;
        float texG = IMG_NORM_PIXEL(inputImage, vec2(greenX, y_drifted)).g;
        float texB = IMG_NORM_PIXEL(inputImage, vec2(blueX, y_drifted)).b;
        
        vec3 rawColor = vec3(texR, texG, texB);
        float luma = dot(rawColor, vec3(0.299, 0.587, 0.114));

        // TRANSFORMACION 3D
        float z0 = (luma - 0.5) * dynZ;
        float x1 = x_obj * cy + z0 * sy;
        float z1 = -x_obj * sy + z0 * cy;
        float y2 = y_norm * cx - z1 * sx;
        float z2 = y_norm * sx + z1 * cx;

        float pFactor = camDist / (camDist - z2);
        float y_proj = y2 * pFactor * zoom;

        // DIBUJO
        float dist_y = abs(y_proj - st.y);
        float density_weight = 100.0 / line_density;
        float current_thickness = 0.006 * thickness * pFactor * density_weight;
        
        float line_mask = smoothstep(current_thickness, 0.0, dist_y);

        if (line_mask > 0.0) {
            vec3 rgb = mix(vec3(luma), rawColor, saturation);
            float signal_intensity = brightness * (0.7 + luma * 0.5); 
            finalCol = max(finalCol, rgb * line_mask * signal_intensity * pFactor);
        }
    }

    vec3 prevFrame = IMG_THIS_NORM_PIXEL(lastFrame).rgb;
    gl_FragColor = vec4(mix(finalCol, prevFrame, feedback), 1.0);
}