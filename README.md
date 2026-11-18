<div align="center">

<img src="Assets/logo-256.png" alt="KHELRUN AI logo" width="128" />

# KHELRUN AI

**Private, offline-capable AI chat for Windows. Your models, your hardware, your data.**

[![Platform](https://img.shields.io/badge/platform-Windows%2010%2B%20(x64)-0078D6?logo=windows)](#requirements)
[![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Backend](https://img.shields.io/badge/backend-llama.cpp-lightgrey)](https://github.com/ggerganov/llama.cpp)
[![Runs](https://img.shields.io/badge/runs-100%25%20local-success)](#privacy)

[Features](#features) &nbsp;|&nbsp; [Quick start](#quick-start) &nbsp;|&nbsp; [Build from source](#build-from-source) &nbsp;|&nbsp; [Installer](#create-the-installer) &nbsp;|&nbsp; [Models](#models) &nbsp;|&nbsp; [Privacy](#privacy)

<!-- Replace with a real screenshot or GIF: -->
<!-- <img src="docs/screenshot-chat.png" alt="KHELRUN AI chat window" width="800" /> -->

</div>

---

## Overview

KHELRUN AI is a Windows desktop application that downloads and runs open-source **GGUF** language models directly on your PC through [llama.cpp](https://github.com/ggerganov/llama.cpp) (via LLamaSharp). Conversations stay on your machine, and core functionality requires no cloud account or subscription.

Built with **WPF** and **.NET 8** by **James Technology Solutions (JTS)**.

## Features

| Feature | Description |
|---|---|
| **Hardware-aware setup** | Detects available RAM on first launch and recommends the best chat model your machine can realistically run. |
| **Model library** | Curated GGUF chat models with one-click download, plus import of local `.gguf` files. |
| **Local, unrestricted chat** | Default and "Unrestricted" system prompt presets. The application applies no content filters of its own to local models. |
| **Configurable inference** | Temperature, max tokens, context size, CPU threads, system prompt presets, text size, and dark/light themes. |
| **Performance feedback** | Live token counts and tokens per second, with a Stop button to cancel generation. |
| **Persistence** | Settings and last-used model are stored under `%AppData%\KhelrunAI\`. |
| **Export** | Export conversations to Markdown or plain text; right-click any message to copy. |
| **Optional web search** | Opt-in search via DuckDuckGo's HTML interface. |
| **Image model tracking** | Import and manage image-generation model files (Safetensors, etc.). See [Limitations](#limitations). |

## Requirements

- Windows 10 or later (x64)
- No separate .NET install required (the runtime is bundled in the self-contained build)
- Sufficient RAM for the chosen model; 8 GB or more is typical for comfortable use of 3B to 8B quantized models

## Quick start

1. Download `KhelrunAI-Setup.exe` from the [Releases](../../releases) page.
2. Run the installer (per-user install, no administrator prompt) and accept the EULA.
3. Launch KHELRUN AI. On first run it detects your RAM and, if no model is present and auto-download is enabled, recommends a model and asks for confirmation.
4. Wait for the download and model load to complete, then begin chatting.

> **Note:** Unsigned builds trigger a Windows SmartScreen warning. The build script supports code signing if you supply a certificate.

You can open the **Models** window at any time to download curated models or import a local `.gguf` file.

## Models

The Models panel lists curated chat models across several size classes:

| Tier | Class |
|---|---|
| Quick | 1.5B |
| Compact | Phi-3.5 Mini |
| Balanced | 3B |
| Smart | 7B |
| Power | 8B |

The catalog favors permissively licensed models (Apache / MIT) so that downloads do not require gated Hugging Face repositories. Any compatible GGUF file can also be imported manually.

## Build from source

**Prerequisites:** Visual Studio 2026 (Visual Studio 2022 also works) with the **.NET desktop development** workload.

1. Clone the repository:
   ```bash
   git clone https://github.com/<your-username>/<your-repo>.git
   ```
2. Open `KhelrunAI.csproj` in Visual Studio.
3. Allow NuGet to restore packages. If LLamaSharp versions conflict, set `LLamaSharp` and `LLamaSharp.Backend.Cpu` to the same recent version.
4. Set the configuration to **Release | x64** and run with **F5**.

## Create the installer

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and [Inno Setup](https://jrsoftware.org/isinfo.php) (version 7 recommended; version 6 also works).

From the repository root in PowerShell:

```powershell
.\build.ps1
```

Optional code signing:

```powershell
.\build.ps1 -PfxPath C:\path\to\cert.pfx -PfxPassword "your-password"
```

**Output:** `dist\KhelrunAI-Setup.exe`

The installer is per-user (no UAC elevation) and includes an EULA acceptance page, a Start Menu shortcut, an optional desktop shortcut, and clean uninstall registration.

## Privacy

- Conversations and models stay on your machine by default.
- Web search is optional and uses DuckDuckGo's HTML interface when enabled.
- Network access for model downloads occurs only when you download a model.

See [`legal/PrivacyPolicy.txt`](legal/PrivacyPolicy.txt) and [`legal/EULA.txt`](legal/EULA.txt).

## Limitations

- **CPU only.** GPU acceleration (CUDA, Vulkan, etc.) is not enabled in the current backend. The architecture can be extended with additional LLamaSharp backends.
- **No image generation yet.** Image model files can be imported and managed, but local diffusion inference requires a compatible backend that has not yet been connected. KHELRUN AI does not currently claim Stable Diffusion, SDXL, or FLUX generation.
- **Model accuracy.** Language models can produce incorrect information. Verify important facts.

## Project structure

```
KhelrunAI/
|-- KhelrunAI.csproj          Main project
|-- App.xaml(.cs)
|-- MainWindow.xaml(.cs)      Chat UI
|-- ModelsWindow.xaml(.cs)    Model library and downloads
|-- SettingsWindow.xaml(.cs)
|-- Services/
|   |-- AppSettings.cs
|   |-- LlmService.cs         LLamaSharp chat
|   |-- ModelCatalog.cs       Curated models and RAM detection
|   |-- ModelDownloader.cs
|   |-- ThemeService.cs
|   `-- WebSearchService.cs
|-- legal/
|   |-- EULA.txt
|   `-- PrivacyPolicy.txt
|-- Assets/
|   |-- KhelrunAI.ico         App and installer icon (multi-size)
|   `-- logo.svg, logo-*.png  Source artwork
|-- installer/
|   |-- KhelrunAI.iss         Inno Setup script
|   `-- images/               Wizard BMP and setup icon
|-- build.ps1                 Publish and package
`-- README.md
```

## Contributing

Issues and pull requests are welcome. For larger changes, please open an issue first to discuss the proposed change.

## License

KHELRUN AI is **source-available** under the [PolyForm Noncommercial License 1.0.0](LICENSE). You are free to view, fork, modify, and contribute to the code for any noncommercial purpose, including personal use, study, research, and use by nonprofits and educational institutions. Commercial use, including selling the software or building a commercial product or service on it, requires a separate written agreement with James Technology Solutions.

This is not an OSI-approved open-source license. For commercial licensing inquiries, contact: `<your-contact-email>`.

The installed application is additionally covered by [`legal/EULA.txt`](legal/EULA.txt).

---

<div align="center">

**KHELRUN AI** &mdash; Private local AI by [James Technology Solutions](https://github.com/<your-username>)

</div>
