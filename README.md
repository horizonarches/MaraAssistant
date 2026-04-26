# Author note: This code proof-of-concept (Mara) is an original work by the author.
# The author states the following:
# 1. Scaffolding and code assistance was obtained via Github Copilot, currently using GPT-5.3-Codex.
# 2. No code from any existing project or effort was used to construct Mara.
# 3. The inspiration for the implemented functionality set was from the author.
# 4. The following README content was AI generated.  Proceed accordingly.

# Patient Care Chatbot Portal (Windows .NET MAUI)

Front-end .NET MAUI chatbot portal for medical patient care with:
- Administrative screen and modal configuration
- User chat screen with voice input and audio response hooks
- Informational screen driven by config metadata
- Persistent top navigation banner
- Bootstrap-inspired visual styling

## Installed Components (for this project)
The following components were installed to enable build/run on this PC:
- .NET SDK 9 (`Microsoft.DotNet.SDK.9`)
- .NET MAUI Windows workload (`maui-windows`)
- Windows App Runtime 1.7 (`Microsoft.WindowsAppRuntime.1.7`)

## Cleanup / Revert Checklist (DO NOT RUN unless you want to remove setup)
This section is for future cleanup planning only.

### 1) Confirm what is currently installed
- `dotnet --info`
- `dotnet workload list`
- `winget list Microsoft.DotNet.SDK.9`
- `winget list Microsoft.WindowsAppRuntime.1.7`

### 2) Optional removal commands (if ever needed later)
- `dotnet workload uninstall maui-windows`
- `winget uninstall --id Microsoft.DotNet.SDK.9 --exact`
- `winget uninstall --id Microsoft.WindowsAppRuntime.1.7 --exact`

### 3) Optional post-clean verification
- `dotnet --list-sdks`
- `dotnet workload list`
- `winget list Microsoft.DotNet.SDK.9`
- `winget list Microsoft.WindowsAppRuntime.1.7`

## Security Note
No direct/manual registry edits were performed by project code changes. Standard Microsoft installers may create normal runtime/package registration entries required by .NET and Windows App Runtime.
