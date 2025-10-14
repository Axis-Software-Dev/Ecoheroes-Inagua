<div align="center">
    <img alt='EcoHéroes' src='Assets/Logo/Images/LOGOTYPE ECOHEROES UNITY.png'/>
    <br/><br/><br/><br/>
    <img alt='Logo Axis' src='Assets/Logo/Images/AXIS LOGO PNG 2025.png' width='300'/>
    <br/><br/><br/><br/>
    <!-- Main badges -->
    <a href="https://github.com/Axis-Software-Dev/Ecoheroes-Inagua/actions">
        <img alt="GitHub Actions Status" src="https://img.shields.io/github/workflow/status/Axis-Software-Dev/Ecoheroes-Inagua/CI?style=for-the-badge"/>
    </a>
    <a href="https://github.com/Axis-Software-Dev/Ecoheroes-Inagua/releases">
        <img alt="GitHub Release" src="https://img.shields.io/github/v/release/Axis-Software-Dev/Ecoheroes-Inagua?style=for-the-badge"/>
    </a>
    <a href="https://github.com/Axis-Software-Dev/Ecoheroes-Inagua/issues">
        <img alt="GitHub issues" src="https://img.shields.io/github/issues/Axis-Software-Dev/Ecoheroes-Inagua?style=for-the-badge"/>
    </a>
    <a href="https://github.com/Axis-Software-Dev/Ecoheroes-Inagua/blob/main/LICENSE.md">
        <img alt="License" src="https://img.shields.io/github/license/Axis-Software-Dev/Ecoheroes-Inagua?style=for-the-badge"/>
    </a>
    <br/><br/>
    <!-- Extra interesting badges -->
    <a href="https://github.com/Axis-Software-Dev/Ecoheroes-Inagua/graphs/contributors">
        <img alt="Contributors" src="https://img.shields.io/github/contributors/Axis-Software-Dev/Ecoheroes-Inagua?style=for-the-badge"/>
    </a>
    <a href="https://github.com/Axis-Software-Dev/Ecoheroes-Inagua/stargazers">
        <img alt="GitHub stars" src="https://img.shields.io/github/stars/Axis-Software-Dev/Ecoheroes-Inagua?style=for-the-badge"/>
    </a>
    <a href="https://github.com/Axis-Software-Dev/Ecoheroes-Inagua/network/members">
        <img alt="Forks" src="https://img.shields.io/github/forks/Axis-Software-Dev/Ecoheroes-Inagua?style=for-the-badge"/>
    </a>
    <a href="https://github.com/Axis-Software-Dev/Ecoheroes-Inagua/releases">
        <img alt="Downloads" src="https://img.shields.io/github/downloads/Axis-Software-Dev/Ecoheroes-Inagua/total?style=for-the-badge"/>
    </a>
    <a href="https://github.com/Axis-Software-Dev/Ecoheroes-Inagua/commits/main">
        <img alt="Last Commit" src="https://img.shields.io/github/last-commit/Axis-Software-Dev/Ecoheroes-Inagua?style=for-the-badge"/>
    </a>
</div>

## About the Project

Are you ready to become a hero and save Aguascalientes from a devastating drought?

**EcoHéroes - Misión Aguas\!\!\!** is an immersive virtual reality experience developed with Unity for the Meta Quest 3. This game takes players on an educational adventure to learn about **water culture**, the importance of **aquifers**, and the challenges of water scarcity in Aguascalientes, Mexico.

Team up with an extraterrestrial named Hidrolito to stop the villainous "Calor Infernal" and repair the city's water infrastructure. Your mission is to answer a crucial quiz, gain the superpower of **"Eco-Consciousness,"** and become the ultimate "Gigante del Agua."

---

## Gameplay and Features

- **Interactive Storytelling:** A narrative-driven experience with friendly characters like Hidrolito.
- **Educational Quiz:** Test your knowledge about water resources and conservation to unlock your super-powered abilities.
- **Immersive VR Interactions:** Use your VR controllers to interact with holographic models, solve puzzles, and repair sabotaged water systems.
- **Themed Environment:** Explore a stylized, realistic version of Aguascalientes, including the iconic Exedra Plaza and a detailed water well installation.
- **Free Exploration:** After completing your mission, you can explore the virtual well to learn more about its construction and operation.

---

## How to Play

### Prerequisites

- A Meta Quest 3 headset.
- A PC with [SideQuest](https://sidequestvr.com/) (or similar software) for sideloading applications.

### How to build from source code

#### Required Software

- Unity 6000.2 or later
- Android SDK (API Level 29 or higher)
- Meta Quest Developer Hub (recommended)
- ADB (Android Debug Bridge) for device deployment

#### Meta Quest Setup

1. Enable Developer Mode on your Quest device

- Create a Meta Developer account at developer.oculus.com
- Enable Developer Mode in the Meta Quest mobile app
- Connect your Quest to the same Wi-Fi network as your computer

2. Install Meta Quest Developer Hub

- Download from developer.oculus.com/downloads
- This provides device management and deployment tools
- Unity Project Configuration

#### Unity Project Configuration

#### 1. Platform Settings

`File → Build Profiles → Meta Quest → Switch Platform`

#### 2. XR Plugin Management

`Edit → Project Settings → XR Plug-in Management`

Ensure these providers are enabled:

✅ Oculus (for Quest 2/3/Pro)
✅ OpenXR (for future compatibility)
✅ Mock HMD (for testing without headset)

#### 3. Quality Settings

`Edit → Project Settings → Quality`

##### For optimal Quest performance:

- Rendering Pipeline: Universal Render Pipeline ✅
- Anti Aliasing: 4x Multi Sampling
- Anisotropic Textures: Per Texture
- Texture Quality: Full Res

#### Build and run

`File → Build Profiles → Meta Quest`

1. Click "Build" or "Build And Run"
2. Choose output folder (e.g., /Builds/Android/)
3. Wait for build completion (5-15 minutes depending on project size)

### Installation Instructions

1. Download the `EcoHéroes.apk` file from our [Releases page](https://github.com/Axis-Software-Dev/Ecoheroes-Inagua/releases).
2. Connect your Meta Quest 3 to your PC via a USB-C cable.
3. Use SideQuest to sideload the `EcoHéroes.apk` file onto your headset.
4. Once installed, the game will appear in the "Unknown Sources" section of your headset's app library.

---

## Media

### Gameplay Trailer

Check out our gameplay trailer to see the game in action\!

[](https://www.google.com/search?q=link/to/your/youtube-or-vimeo-video)

### Screenshots

Take a look at some in-game screenshots below.

|     |     |
| :-- | :-- |
|     |     |
|     |     |

---

## The Team

- **Luis Fernando Márquez**: Director
- **Josué Lozada**: Team lead, 3D modelling, narrative and Script
- **Dany Téllez Girón**: Lead programmer
- **Carlos Tejada**: Art, 3D modelling, animation, rigging
- **Derahy Martínez**: Art, 3D modelling, animation, rigging
- **Paulina Torres**: Art, 3D modelling, animation, rigging
- **Osmar Hernández**: Programming, scripting

---

## 📝 License

This project is licensed under the [MIT License](LICENSE.md).

---
