Gn Glitch FlowMap
[EN] Description
An advanced vector-based displacement engine that uses a secondary "Flow Map" (where Red and Green channels represent X and Y coordinates) to drive pixel movement. This shader treats the image like a fluid, where edges act as physical barriers and colors define the flow's intensity.

Custom Flow Input: Use any image or texture as a vector map to dictate the direction of the glitch, allowing for organic, circular, or chaotic liquid movements.

Edge "Walls" Detection: Features a built-in Sobel Edge Detection motor that acts as a physical boundary. The flow "breaks" or stops when it hits high-contrast areas, preserving the structural integrity of the subjects.

8 Sort Bias Modes: Unique selector to decide which image property drives the flow's speed:

RGB Sum, 2. Luminance, 3. Hue, 4. Saturation, 5. Lightness, 6. Chrominance I, 7. Chrominance Q, 8. Alpha.

Liquid Persistence: High-precision feedback loop for smooth trails, complemented by Chroma Shift and Fringe Brightness for a prismatic, electric look on the edges.

Reset Event: Dedicated trigger to clear the buffer instantly.

[ES] Descripción
Un avanzado motor de desplazamiento basado en vectores que utiliza un "Mapa de Flujo" secundario (donde los canales Rojo y Verde representan las coordenadas X e Y) para dirigir el movimiento de los píxeles. Este shader trata la imagen como un fluido, donde los bordes actúan como barreras físicas y los colores definen la intensidad del flujo.

Entrada de Flujo Personalizada: Utiliza cualquier imagen o textura como mapa de vectores para dictar la dirección del glitch, permitiendo movimientos líquidos orgánicos, circulares o caóticos.

Detección de Bordes (Muros): Incorpora un motor de detección de bordes Sobel que actúa como frontera física. El flujo se "rompe" o detiene al chocar con zonas de alto contraste, preservando la estructura de los sujetos.

8 Modos de Sesgo (Bias): Selector único para decidir qué propiedad de la imagen impulsa la velocidad del flujo:

Suma RGB, 2. Luminancia, 3. Matiz (Hue), 4. Saturación, 5. Ligereza, 6. Crominancia I, 7. Crominancia Q, 8. Alfa.

Persistencia Líquida: Bucle de feedback de alta precisión para estelas fluidas, complementado con Chroma Shift (aberración) y Brillo Fringe para un aspecto prismático y eléctrico en los contornos.

Evento de Reinicio: Disparador dedicado para limpiar el buffer de forma instantánea.

Credits: Gnomalab + Gemini (AI)

Web: www.gnomalab.es

Instagram: https://www.instagram.com/gnomalab

Youtube: https://www.youtube.com/@gnomalab