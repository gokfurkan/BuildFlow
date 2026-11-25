# BuildFlow

A professional Unity Editor toolkit for build automation and release management across **Android**, **iOS**, and **Windows**, with a planned **Steam** distribution workflow.

Package ID: `com.furkangok.buildflow`  
Version: `0.1.0`  
Unity: `6000.3`  
Type: **Editor-only** (no runtime assembly, no game build dependencies)

---

## Supported platforms

| Target | Unity output |
| --- | --- |
| Windows Standalone x64 | `.exe` + Data |
| Android APK | `.apk` |
| Android AAB | `.aab` |
| iOS | Xcode project export |

**Steam** is not a Unity BuildTarget. It is planned as a distribution destination that consumes a successful Windows Standalone build.

---

## Main features (v0.1.0)

- BuildFlow profiles (`ScriptableObject`) stored in your project
- Scene selection (custom list or Editor Build Settings)
- Optional Unity 6 native Build Profile reference
- Structured validation (errors, warnings, information)
- Real builds via `BuildPipeline.BuildPlayer` + `BuildReport`
- JSON build reports and persistent history under `Library/BuildFlow/History/`
- Build Manager window: Dashboard, Profiles, Build, History
- Extension points for packaging, delivery, and distribution

---

## Installation

### Local / development

Add a local package reference to your Unity project `Packages/manifest.json`:

```json
"com.furkangok.buildflow": "file:../../BuildFlow"
```

Adjust the relative path for your folder layout.

### Git URL (when published)

```json
"com.furkangok.buildflow": "https://github.com/<org>/BuildFlow.git"
```

---

## Quick start

1. Open Unity `6000.3` or newer compatible with this package.
2. Install BuildFlow through UPM.
3. Open **Tools > BuildFlow > Build Manager**.
4. Create a profile from the Profiles page (or **Assets > Create > BuildFlow > Build Profile**).
5. Configure target, version, build number, scenes, and output directory.
6. Open the **Build** page → **Validate**.
7. Fix blocking errors, then **Start Build**.
8. Inspect the result, JSON report, and **History** page.

Profiles are saved under:

`Assets/BuildFlow/Profiles/`

---

## Creating a build profile

1. **Assets > Create > BuildFlow > Build Profile**, or use **Create Profile** in Build Manager.
2. Set:
   - Target platform
   - Development / Release configuration
   - Application version and build number
   - Output directory and artifact naming format
   - Scenes (or use Editor Build Settings)
   - Optional native Unity Build Profile asset
3. Save the asset in your project (not inside the package).

---

## Running a build

BuildFlow will:

1. Validate the selected profile
2. Block execution when validation errors exist
3. Prepare a safe output folder (unique path, clean, or confirmed overwrite)
4. Execute Unity’s build pipeline
5. Record success/failure from `BuildReport.summary.result`
6. Write a JSON report to `Library/BuildFlow/History/`

---

## iOS limitations

- A successful iOS run exports an **Xcode project**.
- It is **not** an IPA.
- Compiling, signing, and exporting an IPA require **macOS** and **Xcode**.
- BuildFlow states this clearly in validation and build notes.

---

## Steam roadmap

Planned for a later release:

- Steam AppID
- Depot IDs
- Content staging from Windows builds
- VDF configuration
- SteamCMD upload
- Beta branches
- Explicit release confirmation

No Steam upload controls are shown in v0.1.0.

---

## Architecture

```text
BuildFlowProfile
        │
        ▼
 BuildValidator ──► ValidationReport
        │
        ▼
BuildExecutionService
   ├─ Windows / Android / iOS configurators
   ├─ BuildPipeline.BuildPlayer / BuildPlayerWithProfileOptions
   └─ BuildReport → BuildFlowBuildResult
        │
        ├─ BuildReportGenerator (JSON)
        └─ BuildHistoryService (Library/BuildFlow/History)

Post-build extension points (interfaces only in v0.1.0):
  IArtifactPackager | IDeliveryProvider | IDistributionProvider
```

---

## Screenshots

Screenshots will be added after the first public UI pass is captured from the Unity Editor.

---

## Known limitations

- No Steam upload yet
- No ZIP / SHA-256 / cloud delivery implementations yet
- No build cancellation API exposed in the UI
- iOS IPA compilation is out of scope for this package version
- Native Unity Build Profile platform is read via serialized fields where public API is unavailable

---

## Roadmap

- SteamPipe distribution provider
- Artifact packaging (ZIP, SHA-256, BuildInfo.json)
- Delivery providers (SMTP, Drive, S3)
- Remote Mac build agent integration
- Additional validation rules and CI-friendly CLI entry points

---

## License

MIT — see [LICENSE](LICENSE).
