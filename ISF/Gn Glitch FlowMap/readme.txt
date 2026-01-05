Gn PixSort Flow Map
[EN] Description
A high-end hybrid engine that merges Pixel Sorting with Vector Flow displacement. Unlike traditional sorters that are limited to horizontal or vertical axes, this shader uses a secondary "Flow Map" image to dictate the direction of the sorting trails. It interprets Red channel data as X-axis movement and Green as Y-axis, allowing for complex, swirling, and custom-path digital decay.

Flow Map Driven: Use any texture, gradient, or noise as a vector field to guide the sorting process in 360 degrees.

Triple Image Input: Supports a Source image, a Flow Map for direction, and an optional External Mask for precise spatial clipping.

8 Sorting Bias Modes: Fine-tune the "speed" of the flow based on 8 different image properties (Luminance, Hue, Saturation, etc.).

Blockiness & Jitter: Integrated mosaic engine for low-res aesthetics and a jitter/noise control to add organic instability to the trails.

Directional Fringe: A unique edge-lighting system that tints the leading or trailing edges of the sorting based on the flow direction.

Dynamic Feedback: Persistent buffer for liquid trails with a "Bias to Feedback" control that modulates persistence based on the pixel's luminance or color.

[ES] Descripción
Un avanzado motor híbrido que fusiona el Pixel Sorting con el desplazamiento por flujo de vectores (Flow Maps). A diferencia de los sorters tradicionales limitados a ejes horizontales o verticales, este shader utiliza una segunda imagen de "Mapa de Flujo" para dictar la dirección de las estelas. Interpreta el canal Rojo como movimiento en el eje X y el Verde en el eje Y, permitiendo degradaciones digitales complejas, circulares o con rutas personalizadas.

Guiado por Mapa de Flujo: Utiliza cualquier textura, degradado o ruido como campo vectorial para guiar el proceso de ordenación en 360 grados.

Triple Entrada de Imagen: Soporta una imagen de Fuente, un Mapa de Flujo para la dirección y una Máscara Externa opcional para recortes espaciales precisos.

8 Modos de Sesgo (Bias): Ajusta la "velocidad" del flujo basándose en 8 propiedades distintas de la imagen (Luminancia, Matiz, Saturación, etc.).

Blockiness y Jitter: Motor de mosaico integrado para estéticas de baja resolución y control de ruido/jitter para añadir inestabilidad orgánica a las estelas.

Fringe Direccional: Un sistema único de iluminación de bordes que tiñe los filos delanteros o traseros de la ordenación según la dirección del flujo.

Feedback Dinámico: Buffer persistente para estelas líquidas con un control de "Bias to Feedback" que modula la persistencia según la luminancia o el color del píxel.

Credits: Gnomalab + Gemini (AI)

Web: www.gnomalab.es

Instagram: https://www.instagram.com/gnomalab

Youtube: https://www.youtube.com/@gnomalab