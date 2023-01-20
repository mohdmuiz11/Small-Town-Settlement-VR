# Small-Town-Settlement-VR
Small Town Settlement is a **town-building/survival** genre game using **virtual reality**. It gives the player the ability to strategize and develop their own settlement to survive on an uninhabited island after a shipwreck. The game is developed using **Unity** and the target platform is **Windows PCVR**.

## Demo
[Click here to see the demo](https://youtu.be/6OgPKl1NC2Q)

## Objective
The objective of this project is to develop a town building simulation game that focuses on creating a self-sustaining settlement from the ground up and encourages players to help the community, as well as utilizing the VR technology to create an immersive game experience.

## Main Features
- **Grid System** which is the most complex feature. It is a 2D array of grids on top of a virtual map table, acting as a socket for buildings and roads. It has its own coordinate system.
- **Road creation** to be used within the grid system. It uses its surrounding areas to display the appropriate type of road using 2D sprites.
- **Resource management** such as inventory system, gathering resources, and creating buildings
- **Community status** which comprises of "Mood", "Wellbeing", and "Hunger"
- **NPC and side events** implemented to be used for players to interact and giving tasks to them and participating their activities.
- **Action points**, effects on building, introduction scene, user interface, teleportation, etc.

## Development Tools
Below are the tools used for the development. All 3rd party assets used in this project are subject to Unity's Extension Asset License.

### Software & Hardware
- **Unity** (general purpose game engine)
- **Blender** (open-source 3D modelling)
- **Procreate** (drawing tool, used for user interface)
- Tested on **Oculus Rift S** (2019 model)

### Plugins and 3rd party plugins
- **Unity XR Toolkit**
- **Unity's Universal Render Pipeline (URP)**
- [**Skybox Extended Shader**](https://assetstore.unity.com/packages/vfx/shaders/free-skybox-extended-shader-107400) (skybox and fog)
- [**Simple Water Shader URP**](https://assetstore.unity.com/packages/2d/textures-materials/water/simple-water-shader-urp-191449) (ocean water shader)
- [**iTween**](https://assetstore.unity.com/packages/tools/animation/itween-84) (easy-to-use animation creator)

## Future Enhancements
The following features are planned for future development:
- **Better UI tutorials**, separate from NPC dialogues
- Using road as mandatory to connect between buildings to apply their effects
- Improve grid system for buildings to occupy more than one grid
- Adding more side events for players to participate (eg: cutting woods, fishing)
- **Accessibility settings** for VR users (eg: sitting mode, height adjustment)
- Add more features (eg: culture, farming, weather)

This project is part of our Final Year Project in Kulliyyah of Information and Communication Technology, IIUM. To see the full report, click [here](https://docs.google.com/document/d/1MjXnOokWWkWo_s0p_5D1avOs2DC0pmgG/edit?usp=sharing&ouid=102531622528573220978&rtpof=true&sd=true).
