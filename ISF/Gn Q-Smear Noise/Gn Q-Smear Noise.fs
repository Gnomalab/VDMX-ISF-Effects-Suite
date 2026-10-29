/*{
    "CATEGORIES": [
        "DistortionEffect",
        "Glitch"
    ],
    "CREDIT": "Gnomalab",
    "DESCRIPTION": "Chaotic smear with angles quantized by brightness. Includes spatial/temporal noise and an unstable feedback engine that generates organic glitch textures similar to datamosh aesthetics.",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0.45,
            "LABEL": "Brightness threshold",
            "MAX": 1,
            "MIN": 0,
            "NAME": "brightThreshold",
            "TYPE": "float"
        },
        {
            "DEFAULT": 18,
            "LABEL": "Smear velocity",
            "MAX": 50,
            "MIN": -50,
            "NAME": "velocity",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.4,
            "LABEL": "Velocity sustain",
            "MAX": 0.6,
            "MIN": 0,
            "NAME": "velocityDecay",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.97,
            "LABEL": "Color sustain",
            "MAX": 1,
            "MIN": 0,
            "NAME": "colourDecay",
            "TYPE": "float"
        },
        {
            "DEFAULT": 24,
            "LABEL": "Max angular steps",
            "MAX": 64,
            "MIN": 1,
            "NAME": "maxAngleSteps",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.6,
            "LABEL": "Temporal Noise Amount",
            "MAX": 2,
            "MIN": 0,
            "NAME": "timeNoiseAmt",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1.2,
            "LABEL": "Temporal Noise Speed",
            "MAX": 5,
            "MIN": 0,
            "NAME": "timeNoiseSpeed",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.4,
            "LABEL": "Spatial noise amount",
            "MAX": 2,
            "MIN": 0,
            "NAME": "spatialNoiseAmt",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.15,
            "LABEL": "Feedback Instability",
            "MAX": 1,
            "MIN": 0,
            "NAME": "feedbackChaos",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2",
    "PASSES": [
        {
            "HEIGHT": "$HEIGHT",
            "PERSISTENT": true,
            "TARGET": "decayBuffer",
            "WIDTH": "$WIDTH"
        },
        {
            "PERSISTENT": true,
            "TARGET": "outputPass"
        }
    ],
    "VSN": "1.0.0"
}
*/

// ----------------------------------------------------
// Utils
// ----------------------------------------------------

float brightness(vec4 c)
{
    return (c.r + c.g + c.b) / 3.0;
}

float hash(vec2 p)
{
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float timeNoise(float t)
{
    return fract(sin(t * 13.37) * 43758.5453);
}

// ----------------------------------------------------
// Main
// ----------------------------------------------------

void main()
{
    vec4 inputPixel = IMG_THIS_PIXEL(inputImage);
    float inBright = brightness(inputPixel);
    float decayBright = brightness(IMG_THIS_PIXEL(decayBuffer));

    // ---------------- PASS 0 : velocity memory ----------------
    if (PASSINDEX == 0)
    {
        float v = max(inBright, decayBright) * velocityDecay;
        gl_FragColor = vec4(vec3(v), 1.0);
    }

    // ---------------- PASS 1 : output ----------------
    else
    {
        if (inBright > brightThreshold)
        {
            gl_FragColor = IMG_THIS_NORM_PIXEL(inputImage);
        }
        else
        {
            // --- base chaotic angle ---
            float angle = (inBright + decayBright) * 6.28318;

            // --- temporal noise ---
            angle += timeNoise(TIME * timeNoiseSpeed) * timeNoiseAmt;

            // --- spatial noise ---
            float sNoise = hash(gl_FragCoord.xy * 0.02) * spatialNoiseAmt;
            angle += sNoise;

            // --- brightness dependent quantize ---
            float steps = mix(maxAngleSteps, 1.0, inBright);
            float stepSize = 6.28318 / max(steps, 1.0);
            angle = floor(angle / stepSize) * stepSize;

            vec2 dir = vec2(cos(angle), sin(angle));

            // --- unstable feedback offset (datamosh-ish) ---
            vec2 chaosOffset = dir * decayBright * velocity;
            chaosOffset += feedbackChaos * vec2(
                hash(vec2(TIME, gl_FragCoord.y)),
                hash(vec2(gl_FragCoord.x, TIME))
            ) * 20.0;

            vec2 loc = gl_FragCoord.xy + chaosOffset;

            vec4 smear = IMG_PIXEL(outputPass, loc);
            vec4 base  = IMG_THIS_PIXEL(outputPass);

            float mixVal = inBright * colourDecay;
            gl_FragColor = mix(smear, base, mixVal);
        }
    }
}