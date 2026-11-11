# Build KHELRUN AI on Windows via GitHub Actions

1. Push this repository to GitHub.
2. Open **Actions → Build KHELRUN AI Windows Installer**.
3. Choose **Run workflow**.
4. Wait for the Windows x64 build to finish.
5. Download the `KhelrunAI-Windows-x64-Installer` artifact.

The artifact is a single `KhelrunAI-Setup.exe`. The installer contains the self-contained .NET application and LLamaSharp CPU runtime. AI model files are intentionally excluded and must be downloaded/imported separately.

The installer requests normal Windows administrator approval once, installs under Program Files, creates Start Menu/optional Desktop shortcuts, and registers a standard Windows uninstaller.
