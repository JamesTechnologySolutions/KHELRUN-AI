<div align="center"> <img src="Assets/logo-256.png" alt="KHELRUN AI logo" width="128" /> 
KHELRUN AI
Private, offline-capable AI chat for Windows. Your models, your hardware, your data.
Platform .NET Backend Runs
Features · Quick start · Build from source · Installer · Models · Privacy
<!-- Replace with a real screenshot or GIF: --> <!-- <img src="docs/screenshot-chat.png" alt="KHELRUN AI chat window" width="800" /> --> </div> 
________________________________________
What is KHELRUN AI?
KHELRUN AI is a desktop app that downloads and runs open-source GGUF language models directly on your PC through llama.cpp (via LLamaSharp). Conversations stay on your machine, and core functionality needs no cloud account or subscription.
Built with WPF and .NET 8 by James Technology Solutions (JTS).
Features
	
Hardware-aware setup	Detects your RAM on first launch and recommends the best chat model your machine can realistically run.
Model library	Curated GGUF chat models with one-click download, plus import of your own local .gguf files.
Local, unrestricted chat	Default and "Unrestricted" system prompt presets. The app applies no content filters of its own to local models.
Fully tunable	Temperature, max tokens, context size, CPU threads, system prompt presets, text size, dark/light themes.
Performance feedback	Live token counts and tokens/second, with a Stop button to cancel generation.
Persistence	Settings and last-used model are saved to %AppData%\KhelrunAI\.
Export	Save conversations as Markdown or plain text; right-click any message to copy.
Optional web search	Off the network by default; web search (via DuckDuckGo's HTML interface) is opt-in.
Image model tracking	Import and manage image-generation model files (Safetensors, etc.). See Limitations.

Requirements
•	Windows 10 or later (x64)
•	No separate .NET install needed (the runtime is bundled with the self-contained build)
•	Enough RAM for your chosen model. 8 GB+ is typical for comfortable use of 3B to 8B quantized models.
Quick start
1.	Download KhelrunAI-Setup.exe from the Releases page.
2.	Run the installer (per-user install, no admin prompt) and accept the EULA.
3.	Launch KHELRUN AI. On first run it detects your RAM and, if no model is present and auto-download is enabled, recommends one and asks for confirmation.
4.	Watch the download progress, wait for the model to load, and start chatting.
Heads up: unsigned builds trigger a Windows SmartScreen warning. Code signing is supported by the build script if you supply a certificate.
You can open the Models window at any time to download curated models or import a local .gguf file.
Models
The Models panel lists curated chat models across several size classes:
Tier	Class
Quick	1.5B
Compact	Phi-3.5 Mini
Balanced	3B
Smart	7B
Power	8B
The catalog favors permissively licensed models (Apache / MIT) so downloads don't require gated Hugging Face repositories. You can always import any compatible GGUF file yourself.
Build from source
Prerequisites: Visual Studio 2026 (VS 2022 also works) with the .NET desktop development workload.
1.	Clone the repo: 
2.	git clone https://github.com/<your-username>/<your-repo>.git
3.	Open KhelrunAI.csproj in Visual Studio.
4.	Let NuGet restore packages. If LLamaSharp versions conflict, set LLamaSharp and LLamaSharp.Backend.Cpu to the same recent version.
5.	Set the configuration to Release | x64 and press F5.
Create the installer
Prerequisites: .NET 8 SDK and Inno Setup (7 recommended, 6 also works).
From the repo root in PowerShell:
.\build.ps1
Optional code signing:
.\build.ps1 -PfxPath C:\path\to\cert.pfx -PfxPassword "your-password"
Output: dist\KhelrunAI-Setup.exe
The installer is per-user (no UAC elevation) and includes an EULA acceptance page, a Start Menu shortcut, an optional desktop shortcut, and clean uninstall registration.
Privacy
•	Conversations and models stay on your machine by default.
•	Web search is optional and uses DuckDuckGo's HTML interface when enabled.
•	Model downloads connect to the model host (e.g. Hugging Face) only when you download a model.
See legal/PrivacyPolicy.txt and legal/EULA.txt.
Limitations
•	CPU only. GPU acceleration (CUDA, Vulkan, etc.) is not enabled in the current backend. The architecture can be extended with additional LLamaSharp backends.
•	No image generation yet. Image model files can be imported and managed, but local diffusion inference needs a compatible backend that hasn't been connected. KHELRUN AI does not currently claim Stable Diffusion, SDXL, or FLUX generation.
•	Models can be wrong. Always verify important facts.
Project structure
KhelrunAI/
├── KhelrunAI.csproj          # Main project
├── App.xaml(.cs)
├── MainWindow.xaml(.cs)      # Chat UI
├── ModelsWindow.xaml(.cs)    # Model library & downloads
├── SettingsWindow.xaml(.cs)
├── Services/
│   ├── AppSettings.cs
│   ├── LlmService.cs         # LLamaSharp chat
│   ├── ModelCatalog.cs       # Curated models + RAM detection
│   ├── ModelDownloader.cs
│   ├── ThemeService.cs
│   └── WebSearchService.cs
├── legal/
│   ├── EULA.txt
│   └── PrivacyPolicy.txt
├── Assets/
│   ├── KhelrunAI.ico         # App + installer icon (multi-size)
│   └── logo.svg, logo-*.png  # Source artwork
├── installer/
│   ├── KhelrunAI.iss         # Inno Setup script
│   └── images/               # Wizard BMP + setup icon
├── build.ps1                 # Publish + package
└── README.md
Contributing
Issues and pull requests are welcome. For larger changes, please open an issue first to discuss what you'd like to change.
License
See legal/EULA.txt. <!-- Add a LICENSE file and badge if the source is open. -->
________________________________________
<div align="center"> 
KHELRUN AI: Private local AI by James Technology Solutions
</div>

