# BuildFlow Documentation

## Package

- ID: `com.furkangok.buildflow`
- Version: `0.1.0`
- Unity: `6000.3`
- Assembly: `FurkanGok.BuildFlow.Editor` (Editor-only)

## Modules

| Folder | Responsibility |
| --- | --- |
| `Editor/Profiles` | BuildFlowProfile assets and scene resolution |
| `Editor/Validation` | Rule-based pre-build validation |
| `Editor/Build` | Execution engine and platform configurators |
| `Editor/History` | JSON reports and history index |
| `Editor/UI` | Build Manager window |
| `Editor/Packaging` | Artifact packaging extension points |
| `Editor/Delivery` | Delivery extension points |
| `Editor/Distribution` | Distribution extension points (Steam roadmap) |

## History location

Build reports are written to:

`Library/BuildFlow/History/`

This folder is project-local and outside the UPM package source.
