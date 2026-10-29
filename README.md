# ⚗️ Gnomalab: Digital Alchemy Suite

**VDMX ISF Effects Suite**: ISF shaders for VDMX

Welcome to the Digital Alchemy Suite. This is a personal collection of shaders in ISF (Interactive Shader Format), built for signal manipulation, glitch aesthetics and binary processes applied to live video.

---

## 🚀 About the Development and Inspiration

This project grew out of the wish to bring aesthetic concepts and traditional post-production processes, which normally require a render, into a live workflow.

Many of these effects are inspired by classic commercial plugins for After Effects and other compositing platforms, then "hacked" and ported to ISF so that the pixel is computed on the spot, under the heat of a live performance. The collection ranges from pure mathematical distortions to homages to media archaeology, such as the analog scan processing of the Vasulkas and their legendary Rutt-Etra synthesizer.

**Authorship note:** I am not a programmer. This suite is the result of a creative collaboration with Gemini (Google AI), which acted as the technical bridge to turn ideas, structures and logic I had in mind into code I did not know how to write. It is an example of how AI can empower artists to build their own tools.

---

## 🛠️ Compatibility and Performance

- **Tested on:** VDMX6 Plus.
- **Compatibility:** Should work in VDMX6 and VDMX5, and in any software that supports the ISF standard. Only VDMX6 Plus has been tested so far.
- **Performance:** Some shaders are computationally heavy, especially those with chained passes (multi-pass) or persistent feedback. Performance depends directly on your GPU.
- **User interface:** These are personal tools, so parameter names are in a mix of English and Spanish. Feel free to edit or rename them to suit you.

---

## 📥 Installing in VDMX

### Automatic installation (recommended)

1. Clone or download this repository:
   ```bash
   git clone https://github.com/Gnomalab/VDMX-ISF-Effects-Suite.git
   ```

2. Copy the contents of the `ISF` folder into VDMX's shader folder:
   ```bash
   cp -r ISF/* ~/Library/Graphics/ISF/
   ```

3. Restart VDMX. The shaders will appear in your FX library.

### Manual installation

1. Download the repository as a ZIP from GitHub.
2. Go to `~/Library/Graphics/ISF/` on your Mac (you can use `Cmd+Shift+G` in Finder).
3. Drag the individual shader folders from `ISF/` into that location.
4. Restart VDMX.

**💡 Tip:** Each shader folder includes its own `readme.txt` with a full technical description.

---

## 📂 The Laboratory (Effect Catalog)

The suite is grouped into four categories to make it easy to browse inside VDMX. Effect names below match the folder and file names in `ISF/`.

### 1. Pixel Sorting & Vector Flow 🌊
- **Gn Pixort** (`Gn  Pixort`): Advanced pixel sorting for VDMX6.
- **Gn Pixort S3**: The most advanced version. No more straight lines: spirals, vortices and magnetic fields, with 8 flow modes.
- **Gn Pixort Flow Map**: Movement is driven by a second image. Full control of the flow field.
- **Gn Pixort AE**: Temporal pixel sorting with 360° vector control and external mask support.
- **Gn Pixel Sorting**: Persistent pixel sorter with rhythmic speed modulation.
- **Gn Pixel Sort Clean**: Sorting that keeps the background intact.
- **Gn Glitch FlowMap**: Flow mapper in which a second image dictates the direction of the sort.
- **Gn FlowMap**: Flow map generated from an input source.

### 2. Glitch & Digital Vandalism 👾
- **Gn Vandalizer**: Selective destruction. Slices, melt and TV warp, focused by precision masks.
- **Gn XORmear**: Pure bitwise corruption. Textures that look like they grew out of the data error itself.
- **Gn XORter**: Hybrid binary destruction and displacement, combining a bitwise XOR engine with persistent pixel sorting.
- **Gn Brutalist Crush**: XOR bit-crush chained with pixel sorting, with a selectable processing order.
- **Gn Glitch Control**: The "Swiss army knife" for RGB offsets, stretching and copies, with switchable processing order.
- **Gn Broke in Bad**: Luminance-driven displacement, fine-grain version.
- **Gn Datamosh**: A fake datamosh effect.
- **Gn Q-Smear Noise**: Chaotic smear with angles quantized by brightness and an unstable feedback engine.

### 3. Video Synthesis & Signal 📺
- **Gn Vasulkas FX**: A tribute to the Rutt-Etra. Turns video into a vibrant 3D topographic sculpture.
- **Gn Modulation Synth**: Treats video as voltage. Analog-style modulation with CMYK and HSV color spaces.
- **Gn Signal Trinity**: A routing matrix that applies three different processes to shadows, midtones and highlights independently.
- **Gn Topo Scan**: Generator of rhythmic contour lines inspired by technical cartography.

### 4. Temporal Deformation & Feedback 🌀
- **Gn Slit-scan**: Slit-scan with map-based deformation. Time is no longer linear.
- **Gn Pixel Melt**: Viscous melting with 360° gravity.
- **Gn Pixel Repeat**: Mask-based pixel repetition with rhythmic geometric trails.
- **Gn Edge Turbulence**: Pro feedback with noise, vortex and edge color. Stable, optimized version.

---

## 👤 Credits

- **Concept and artistic direction:** Gnomalab
- **Technical bridge & thought partner:** Gemini AI
- **Web:** [www.gnomalab.es](http://www.gnomalab.es/)
- **Instagram:** [@gnomalab](https://www.instagram.com/gnomalab)
- **YouTube:** [Gnomalab](https://www.youtube.com/@gnomalab)

---

2026 Gnomalab. Made by an artist, for artists.
**MIT License**: use it, modify it, share it. If it helps your art, consider buying me a beer 🍺
