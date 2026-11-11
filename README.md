# KHELRUN AI

**Made by JTS (James Technology Solutions)**

A private, unrestricted, offline-capable AI desktop application for Windows.
Built with WPF and .NET 8. Runs open-source GGUF models locally on your CPU via llama.cpp (LLamaSharp).

## What it is

KHELRUN AI is a locally operated AI chat platform. It downloads and runs GGUF language models on your own hardware, keeps conversations on your PC, and does not require a cloud account or subscription for core functionality.

## Highlights

- **Hardware-aware model recommendation** — On first launch the app detects available RAM and offers the best chat model that can realistically run on the machine.
- **Professional UI** — Modern dark/light themes, sidebar navigation, polished chat interface.
- **Model library** — Curated GGUF chat models with one-click download, plus import of local GGUF files.
- **Image model tracking** — Import and manage compatible image-generation model files (Safetensors, etc.). Full local diffusion inference can be extended with a compatible backend; the library and import path are in place.
- **Unrestricted local chat** — Default and “Unrestricted” system prompts with no application-level content filters on local models.
- **Settings** — Theme, text size, system prompt presets, temperature, max tokens, context size, CPU threads, web search, auto-load / auto-download behaviour.
- **Persistence** — Settings and last-used model stored under `%AppData%\KhelrunAI\`.
- **Export** — Export conversation to Markdown or plain text; right-click messages to copy.
- **Performance feedback** — Token counts and tokens/second; Stop button cancels generation.

## Requirements

- Windows 10 or later (x64)
- .NET 8 runtime is bundled (self-contained publish)
- Sufficient RAM for the chosen model (typically 8 GB+ for comfortable use of 3B–8B quantized models)

## Build (Visual Studio 2026)

1. Install **Visual Studio 2026** with the **.NET desktop development** workload (VS 2022 also works).
2. Open `KhelrunAI.csproj`.
3. Allow NuGet to restore packages. If LLamaSharp versions conflict, set both `LLamaSharp` and `LLamaSharp.Backend.Cpu` to the same recent version.
4. Set configuration to **Release | x64** and run (F5).

## Create the Windows installer

1. Install the .NET 8 SDK and [Inno Setup 7](https://jrsoftware.org/isinfo.php) (Inno Setup 6 also works).
2. From this folder in PowerShell:

   ```powershell
   .\build.ps1
   ```

   Optional code signing:

   ```powershell
   .\build.ps1 -PfxPath C:\path\to\cert.pfx -PfxPassword "your-password"
   ```

3. Output: `dist\KhelrunAI-Setup.exe`

The installer is per-user (no UAC elevation), includes the EULA acceptance page, Start Menu shortcut, optional desktop shortcut, and clean uninstall registration.

## First launch

1. The app detects approximate system RAM.
2. If no model is present and auto-download is enabled, it recommends a model and asks for confirmation.
3. Progress is shown during download; integrity is checked by content length.
4. The model is loaded and you can start chatting.

You can also open the Models window at any time to download curated models or import a local `.gguf` file.

## Models

Curated chat models are listed in the Models panel (Quick 1.5B, Phi-3.5 Mini, Balanced 3B, Smart 7B, Power 8B class options, etc.). Prefer public-license models (Apache / MIT) to avoid gated Hugging Face repositories.

Image models can be imported; generation depends on a diffusion backend that can be integrated later. The application does not claim full Stable Diffusion / SDXL / FLUX generation until a backend is connected.

## Privacy

- Conversations and models stay on your machine by default.
- Optional web search uses DuckDuckGo’s HTML interface.
- See `legal/PrivacyPolicy.txt` and `legal/EULA.txt`.

## Project layout

```
KhelrunAI/
  KhelrunAI.csproj          # Main project
  App.xaml / App.xaml.cs
  MainWindow.xaml(.cs)      # Chat UI
  ModelsWindow.xaml(.cs)    # Model library & downloads
  SettingsWindow.xaml(.cs)
  Services/
    AppSettings.cs
    LlmService.cs           # LLamaSharp chat
    ModelCatalog.cs         # Curated models + RAM detection
    ModelDownloader.cs
    ThemeService.cs
    WebSearchService.cs
  legal/
    EULA.txt
    PrivacyPolicy.txt
  Assets/
    KhelrunAI.ico           # App + installer icon (multi-size)
    logo.svg / logo-*.png   # Source artwork
  installer/
    KhelrunAI.iss           # Inno Setup script
    images/                 # Wizard BMP + setup icon
  build.ps1                 # Publish + package
  README.md
```

## Notes

- GPU acceleration (CUDA / Vulkan / etc.) is not enabled in the current CPU backend. The architecture can be extended with additional LLamaSharp backends.
- Always verify important facts; models can produce incorrect information.
- Code signing is supported by the build script when a valid certificate is supplied; unsigned builds will trigger SmartScreen warnings.

**KHELRUN AI** — Private local AI by JTS.
