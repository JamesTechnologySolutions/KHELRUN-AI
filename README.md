# KHELRUN AI

**Made by JTS (James Technology Solutions)**

A private, unrestricted, offline-capable AI desktop application for Windows.
Built with WPF and .NET 8. Runs open-source GGUF models locally on **CPU and/or GPU** via llama.cpp (LLamaSharp).

## What it is

KHELRUN AI is a locally operated AI chat platform. It downloads and runs GGUF language models on your own hardware, keeps conversations on your PC, and does not require a cloud account or subscription for core functionality.

## Highlights (v1.1.1)

- **CPU + GPU acceleration** — Bundles CPU, CUDA 12, and Vulkan backends. Set GPU layers in Settings (Auto / exact count / CPU-only). Dramatically faster inference when a compatible GPU is present.
- **Hardware-aware model recommendation** — On first launch the app detects available RAM and offers the best chat model that can realistically run on the machine.
- **Professional UI** — Modern dark/light themes, sidebar navigation, polished chat interface with live tokens/sec feedback.
- **Model library** — Curated GGUF chat models with one-click download, plus import of local GGUF files.
- **Image model tracking** — Import and manage compatible image-generation model files (Safetensors, etc.). Full local diffusion inference can be extended with a compatible backend; the library and import path are in place.
- **Unrestricted local chat** — Default and “Unrestricted” system prompts with no application-level content filters on local models.
- **Settings** — Theme, text size, system prompt presets, temperature, max tokens, context size, CPU threads, **GPU layers**, batch size, web search, auto-load / auto-download behaviour.
- **Persistence** — Settings and last-used model stored under `%AppData%\KhelrunAI\`.
- **Export** — Export conversation to Markdown or plain text; right-click messages to copy.
- **Performance feedback** — Token counts and tokens/second; Stop button cancels generation.

## Requirements

- Windows 10 or later (x64)
- .NET 8 runtime is bundled (self-contained publish)
- Sufficient RAM for the chosen model (typically 8 GB+ for comfortable use of 3B–8B quantized models)
- Optional: NVIDIA GPU with CUDA 12 drivers **or** any GPU with Vulkan support for acceleration

## Build (Visual Studio 2022 / 2026)

1. Install **Visual Studio** with the **.NET desktop development** workload.
2. Open `KhelrunAI.csproj`.
3. Allow NuGet to restore packages (LLamaSharp + Cpu + Cuda12 + Vulkan backends).
4. Set configuration to **Release | x64** and run (F5).

## Create the Windows installer

1. Install the .NET 8 SDK and [Inno Setup 7](https://jrsoftware.org/isinfo.php).
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
4. The model is loaded (GPU layers auto-offloaded when possible) and you can start chatting.

You can also open the Models window at any time to download curated models or import a local `.gguf` file.

## Works on any CPU / any GPU

KHELRUN AI ships with multiple backends so it runs on essentially any modern Windows x64 PC:

| Hardware | Backend used |
|----------|--------------|
| NVIDIA (newer) | CUDA 12 |
| NVIDIA (older) | CUDA 11 |
| AMD / Intel / NVIDIA without CUDA | Vulkan |
| No GPU or drivers missing | Pure CPU (AVX2) |

You do **not** need to install anything extra for CPU mode. For GPU:

- NVIDIA → install a recent Game Ready / Studio driver (CUDA 11 or 12 compatible).
- AMD / Intel → ensure Vulkan is available (usually already present with current drivers).

### GPU / performance tips

- Open **Settings → Performance & Hardware**.
- **GPU offload (layers)**:
  - `-1` (Auto) — recommended. Offloads as many layers as the GPU can hold.
  - `0` — force pure CPU (useful for debugging or if drivers are missing).
  - Positive number — exact layer count (tune if you hit out-of-memory).
- Higher **Batch size** speeds up the initial prompt evaluation.
- After changing GPU/CPU/context settings, reload the model when prompted.
- If tokens/sec stays low, try setting GPU layers to 0 then back to Auto, or check that your GPU drivers are up to date.

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
  KhelrunAI.csproj          # Main project (CPU + CUDA12 + Vulkan backends)
  App.xaml / App.xaml.cs
  MainWindow.xaml(.cs)      # Chat UI
  ModelsWindow.xaml(.cs)    # Model library & downloads
  SettingsWindow.xaml(.cs)  # Includes GPU layer controls
  Services/
    AppSettings.cs
    LlmService.cs           # LLamaSharp chat with GPU offload
    ModelCatalog.cs         # Curated models + RAM detection
    ModelDownloader.cs
    ThemeService.cs
    WebSearchService.cs
  legal/
  Assets/
  installer/
  build.ps1
  README.md
```

## Notes

- GPU acceleration works when the corresponding backend package is present and the system has working CUDA 12 or Vulkan drivers. The app falls back to CPU gracefully.
- Always verify important facts; models can produce incorrect information.
- Code signing is supported by the build script when a valid certificate is supplied; unsigned builds will trigger SmartScreen warnings.

**KHELRUN AI** — Private local AI by JTS. v1.1.1