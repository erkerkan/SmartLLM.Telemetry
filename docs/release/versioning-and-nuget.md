# Versioning and NuGet Publishing

## Versioning strategy

- **Semantic Versioning** (SemVer 2.0.0).
- Repo version in `Directory.Build.props` (`VersionPrefix`).
- CI sets `VersionSuffix` for prerelease builds (`ci.{run}`).
- Git tags `v1.2.3` trigger stable release.

## Package IDs

All packages publish under the same version line per release:

- `SmartLLM.Telemetry.Core`
- `SmartLLM.Telemetry.OpenTelemetry`
- … (see README package matrix)

## Local pack

```bash
dotnet pack -c Release -o ./artifacts
```

## NuGet.org publish (maintainers)

1. Create GitHub release tag `v0.1.0`.
2. `release.yml` builds, packs, pushes to NuGet using `NUGET_API_KEY` secret.
3. Verify symbols pushed to Symbol Server (optional).

## API stability

| Version | Policy |
|---------|--------|
| `0.x` | Breaking changes allowed with minor bump + changelog |
| `1.0+` | Breaking changes only in major; `[Obsolete]` one minor before removal |

## Prerelease channels

| Channel | Tag pattern | NuGet label |
|---------|-------------|-------------|
| CI | every main push | `ci.{run}` |
| Preview | `v1.0.0-rc.1` | `rc.1` |
| Stable | `v1.0.0` | (none) |

## Source linking

Packages include Source Link (`PublishRepositoryUrl`, `EmbedUntrackedSources`) for debugger stepping.
