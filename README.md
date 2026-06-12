# Virtual Humans: LLM vs. Human-Driven Conversation in VR Medical Simulation

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Language](https://img.shields.io/badge/Language-C%23-green.svg)](https://github.com/wesferr/virtual-humans)

## Overview

This repository contains the source code for a comprehensive research study examining the effectiveness of Large Language Model (LLM)-driven versus human-driven conversations in a Virtual Reality (VR) medical simulation environment. The project is presented as part of an SVR (IEEE Symposium on 3D User Interfaces) paper.

This work explores how conversational agents powered by LLMs compare to human interaction patterns in clinical training scenarios, with implications for medical education, clinical simulation, and AI-assisted healthcare training.

## Key Features

- **VR Medical Simulation Environment** - A fully functional virtual reality medical simulation built with C# and Unity
- **LLM Integration** - Integration of Large Language Models for autonomous conversational agents
- **Human-Driven Conversation System** - Framework for human-controlled dialogue in VR
- **User Study Framework** - Complete research infrastructure for conducting comparative user studies
- **Advanced Character Animation** - Lip-sync animation, facial expressions, and head movement using SALSA LipSync Suite
- **Shader-Based Visuals** - Custom shaders for realistic rendering and visual feedback

## Project Structure

```
virtual-humans/
├── Assets/                    # Unity project assets
│   ├── Scripts/              # C# game and simulation logic
│   ├── Scenes/               # VR simulation scenes
│   ├── Models/               # 3D character and environment models
│   ├── Plugins/              # Third-party integrations (SALSA LipSync, etc.)
│   └── Shaders/              # Custom ShaderLab materials and effects
├── Documentation/            # Research documentation and guides
└── README.md                 # This file
```

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

### Human-Driven Conversation
- Real-time human operator control of conversational agent
- Human-in-the-loop dialogue management
- Natural interaction patterns based on human expertise
- Baseline for comparison studies

### User Study Framework
- Quantitative metrics collection (task completion, response times)
- Qualitative feedback gathering
- Session logging and analytics
- Statistical analysis tools

## Data Analysis

The `Notebooks/` directory contains Jupyter notebooks for:
- Data processing and cleaning
- Statistical analysis of study results
- Visualization of comparative metrics
- User feedback analysis

## Project Publications

This work is presented in:
- **Conference:** IEEE SVR (Symposium on 3D User Interfaces)
- **Topic:** Comparative analysis of LLM-driven vs. human-driven conversation in VR medical training

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Code of Conduct

This project is dedicated to creating a respectful and inclusive environment for all contributors. Please refer to our [Code of Conduct](CODE_OF_CONDUCT.md) for guidelines.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- **SALSA LipSync Suite** by Crazy Minnow Studio for character animation
- **Unity Technologies** for the VR development platform
- All research participants in the user studies
- Faculty advisors and research supervisors

## Contact & Support

For questions, issues, or collaboration inquiries:
- **GitHub Issues:** [Open an issue](https://github.com/wesferr/virtual-humans/issues)
- **Repository:** [wesferr/virtual-humans](https://github.com/wesferr/virtual-humans)

## Citation

If you use this code in your research, please cite:

```bibtex
@inproceedings{ferreira2025llm_human_vr,
  title={LLM vs. Human Driven Conversation: A User Study in Virtual Reality Medical Simulation},
  author={Ferreira, Wesley},
  booktitle={Proceedings of the IEEE Symposium on 3D User Interfaces (SVR)},
  year={2025}
}
```

---

**Last Updated:** June 2026
