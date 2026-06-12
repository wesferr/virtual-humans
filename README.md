# Virtual Humans: LLM vs. Human-Driven Conversation in VR Medical Simulation


This repository contains the source code for a comprehensive research study examining the effectiveness of Large Language Model (LLM)-driven versus human-driven conversations in a Virtual Reality (VR) medical simulation environment. The project is presented as part of an SVR (IEEE Symposium on 3D User Interfaces) paper.

This work explores how conversational agents powered by LLMs compare to human interaction patterns in clinical training scenarios, with implications for medical education, clinical simulation, and AI-assisted healthcare training.

## Key Features

- **VR Medical Simulation Environment** - A fully functional virtual reality medical simulation built with C# and Unity
- **LLM Integration** - Integration of Large Language Models for autonomous conversational agents
- **Human-Driven Conversation System** - Framework for human-controlled dialogue in VR
- **User Study Framework** - Complete research infrastructure for conducting comparative user studies
- **Advanced Character Animation** - Lip-sync animation, facial expressions, and head movement using SALSA LipSync Suite
- **Shader-Based Visuals** - Custom shaders for realistic rendering and visual feedback

### Planned Extended Versions of the Virtual Human Structure

<div align="center" width="50%">
  <img width="2132" height="1875" alt="pipeline" src="https://github.com/user-attachments/assets/7c162d09-0c9f-4b6a-afb3-0df54f45f732" />
</div>

## Technology Stack

### Primary Languages
- **C# (42.6%)** - Core game logic and simulation framework
- **ShaderLab (12.7%)** - Custom graphics and visual effects
- **C/C++ (8.7% + 8.6%)** - Native performance-critical components
- **CMake (7.6%)** - Build system configuration
- **Jupyter Notebook (13.7%)** - Data analysis and research documentation

### Key Technologies
- **Unity** - VR development platform
- **SALSA LipSync Suite** - Realistic lip-sync and facial animation
- **Large Language Models** - For autonomous conversational agents
- **VR Framework** - Support for major VR platforms

## Installation & Setup

### Requirements
- Unity 2021 LTS or later
- VR-capable development environment (HTC Vive, Meta Quest, or Valve Index compatible)
- C# 7.3 or later
- Visual Studio 2019+ (recommended IDE)

### Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/wesferr/virtual-humans.git
   cd virtual-humans
   ```

2. **Open in Unity:**
   - Launch Unity Hub
   - Click "Open Project"
   - Navigate to the `virtual-humans` directory
   - Select and open the project

3. **Install VR Support:**
   - Open Project Settings → XR Plug-in Management
   - Enable your target VR platform (Oculus, SteamVR, etc.)

4. **Configure LLM Integration:**
   - Set up API keys for your preferred LLM service in the configuration files
   - Refer to the documentation for detailed setup instructions

5. **Load Initial Scene:**
   - Navigate to `Assets/Scenes/` and open the main simulation scene

## Research Study Components

### LLM-Driven Conversation
- Autonomous agent powered by large language models
- Real-time dialogue generation and context awareness
- Natural language understanding and response generation
- Consistent agent personality and behavior patterns

### User Study Framework
- Quantitative metrics collection (task completion, response times)
- Qualitative feedback gathering
- Session logging and analytics
- Statistical analysis tools

## Project Publications

This work is presented in:
- **Conference:** SVR – Symposium on Virtual and Augmented Reality
- **Topic:** Evaluation of LLM-driven virtual human conversation in VR medical training
