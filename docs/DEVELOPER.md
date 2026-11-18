# KHELRUN AI — Developer & Deployment Notes

## Architecture overview

KHELRUN AI is a WPF (.NET 8) desktop application.

| Layer | Responsibility |
|-------|----------------|
| UI (MainWindow, ModelsWindow, SettingsWindow) | Presentation, user interaction, progress display |
| Services/AppSettings | Persistence of user preferences (incl. GpuLayers, BatchSize) |
| Services/LlmService | GGUF model load + streaming chat via LLamaSharp (CPU + GPU) |
| Services/ModelCatalog | Curated model list, RAM detection, recommendation |
| Services/ModelDownloader | HTTP download with progress and basic integrity check |
| Services/ThemeService | Dark / light theme switching |
| Services/WebSearchService | Optional DuckDuckGo HTML search |

Inference supports **CPU + GPU**. Packages referenced:

- `LLamaSharp.Backend.Cpu`
- `LLamaSharp.Backend.Cuda12`
- `LLamaSharp.Backend.Vulkan`

`ModelParams.GpuLayerCount` controls offload:
- `-1` / high value → auto / max layers
- `0` → pure CPU
- positive → exact layer count

## Branding rules

- Product name: **KHELRUN AI**
- Publisher: **JTS** (James Technology Solutions)
- Executable: `KhelrunAI.exe`
- Namespace / RootNamespace: `KhelrunAI`
- Settings path: `%AppData%\KhelrunAI\`
- Install path (default): `%LocalAppData%\KHELRUN AI\`

There must be **no** remaining "LocalMind" identifiers in user-facing strings, metadata, or namespaces.

## Building a release

```powershell
# From the project root
.\build.ps1
# Produces: dist\KhelrunAI-Setup.exe
```

Requirements on the build machine:
- .NET 8 SDK
- Inno Setup 7 recommended
- Optional: Windows SDK `signtool.exe` + a code-signing certificate for signed builds

## Signing

Pass certificate details to the build script:

```powershell
.\build.ps1 -PfxPath C:\certs\jts.pfx -PfxPassword "..."
```

Do **not** claim the distributed binary is signed unless a valid certificate was actually used.

## Adding models

Edit `Services/ModelCatalog.cs` — the `ChatModels` list. Each entry needs a public download URL (or a clear note that the model is gated), approximate size, and minimum RAM guidance.

## Extending image generation

The Models window already supports importing image-related files and recording them. To enable local generation:

1. Choose a backend (e.g. ONNX Runtime + DirectML, or a managed Stable Diffusion library).
2. Add a dedicated service that loads the selected model and exposes text-to-image / img2img APIs.
3. Wire a generation workspace in the UI that only enables controls supported by the active backend.

Do not expose controls for features the current backend cannot perform.

## Testing checklist (Windows)

- [ ] Clean install on a machine without the app previously installed
- [ ] EULA acceptance required by installer
- [ ] First-run hardware detection and model recommendation
- [ ] Model download progress, cancel, and successful load
- [ ] Import local GGUF and switch models
- [ ] Chat streaming, Stop, export, copy
- [ ] Settings persistence across restarts (including GPU layers)
- [ ] Theme switch
- [ ] GPU offload (verify with NVIDIA CUDA 12 or Vulkan-capable GPU; tokens/sec should rise significantly)
- [ ] Force CPU-only mode (GpuLayers = 0) still works
- [ ] Offline operation after models are present
- [ ] Uninstall removes application files; user data under AppData can remain

## Known limitations (v1.1)

- Image generation is library + import only; no full diffusion pipeline yet
- Web search depends on DuckDuckGo HTML availability
- No automatic application update channel in this version (manual reinstall of newer Setup.exe)
- CUDA requires matching NVIDIA driver + CUDA 12 runtime on the end-user machine

## Version

Current product version: **1.1.1**