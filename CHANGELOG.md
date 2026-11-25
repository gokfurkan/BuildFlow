# Changelog

All notable changes to BuildFlow are documented in this file.

## [0.1.0] - 2026-11-25

### Added

- Editor-only UPM package foundation (`com.furkangok.buildflow`)
- `BuildFlowProfile` ScriptableObject with Windows, Android APK/AAB, and iOS Xcode targets
- Optional Unity 6 native `BuildProfile` reference with platform mismatch validation
- Validation system with structured rule results (Error / Warning / Information)
- Build execution service using `BuildPipeline.BuildPlayer` and BuildReport summary results
- Windows Standalone x64, Android APK/AAB, and iOS Xcode project export support
- JSON build reports and history stored under `Library/BuildFlow/History/`
- Build Manager EditorWindow (Dashboard, Profiles, Build, History)
- Extension-point interfaces for packaging, delivery, and distribution
- Steam distribution roadmap documentation (no upload implementation)
- Editor automated tests for profiles, validation, naming, and history serialization

### Notes

- Steam is modeled as a Windows distribution destination, not a Unity BuildTarget
- iOS success means Xcode project export only; IPA compilation requires macOS and Xcode
