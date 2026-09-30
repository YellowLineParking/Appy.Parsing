# .NET 10 Migration

- **Status:** In Progress
- **Branch:** `feat/net10`

## Summary

Move Appy.Parsing from `net6.0;netstandard2.0` to `net10.0;net8.0` on the .NET 10 SDK, with tests
on `net10.0`, and release it as 2.0.0. The build, packages, test stack and GitHub Actions line up
with [Appy.Configuration](https://github.com/YellowLineParking/Appy.Configuration), which already
made this move. No public API or behaviour change is intended; only the supported frameworks change.

## Decisions

| Decision | Context | Alternatives Considered |
|----------|---------|------------------------|
| Library `net10.0;net8.0`, tests `net10.0` | net10.0 and net8.0 are the supported LTS releases. net6.0 is out of support; `netstandard2.0` only needed the `Nullable` polyfill. The code has no framework-specific paths, so one test TFM covers it. | Also `net9.0`: a short-term release that net8.0 assets already cover. Keep `netstandard2.0`: .NET Framework users can stay on 1.x. |
| Release 2.0.0 | Dropping `net6.0` and `netstandard2.0` breaks .NET Framework, net6 and net7 users: a SemVer major. | 1.2.0, as Appy.Configuration did for the same drop: undersells the break. |
| MinVer as the reference (2.3.0, package and CLI), phase `preview`, minimum major.minor `2.0` | One versioning setup across both repos. With the minimum set, the preview published on merge reads `2.0.0-preview.0.N`, not `1.1.1-preview`. | MinVer 8: needs a workaround in `build.cake` because Cake.MinVer only exposes the phase option that MinVer 8 rejects. |
| Central package management, Traversal build, `.sln` kept | Same layout as the reference repo. | `.slnx`: the reference still uses `.sln`. |
| xUnit v3 3.2.2 with `xunit.runner.visualstudio` 3.1.5, FluentAssertions 7.2.2 | xUnit v2 is in maintenance mode; 3.2.2 matches the sibling repos. FluentAssertions 8 has a commercial licence; 7.x is the last Apache-2.0 line. | xUnit v3 4.x: newer, not yet used alongside. xUnit v2 as the reference: leaves a migration for later. |
| CI installs only the .NET 10 SDK (from `global.json`) | The SDK builds both TFMs; tests run on net10.0 only. | Install 8/9/10 as the reference: unused runtimes. |
| Package validation against 1.1.0 | Guards the API from this release on. The only expected findings are the two dropped TFMs (PKV006), suppressed explicitly. | None (the reference has none): a TFM or API drop could ship silently. |
| Drop `Microsoft.SourceLink.GitHub`, `Nullable`, unused `Moq` | Source Link ships in the .NET 8+ SDK; the polyfill and `Moq` are unused on net8+. | Keep them as the reference does: dead dependencies. |
| Nullable reference types stay off | Annotating the public API is a separate change. | Enable now: widens a framework-only PR. |

## Tasks

Each task is one commit. The `!` marks the breaking change.

- [ ] 1. `feat(dotnet)!: target net10.0 and net8.0`: `global.json` SDK `10.0.100` with
  `rollForward: latestFeature` (drop `projects`); library TFMs; test TFM `net10.0`; remove the
  `Nullable` package, the `Microsoft.NETCore.App.Ref` download and the annotator properties;
  `PackageTags` to `NET10;NET8;parsing;lexer`. Test: the existing 14 tests pass on net10.0.
- [ ] 2. `chore(deps): adopt central package management and upgrade packages`: add
  `src/Directory.Packages.props`, move all versions out of `Directory.Build.props` and the csproj
  files, drop `Moq` and `Microsoft.SourceLink.GitHub`. Versions: Microsoft.NET.Test.Sdk 18.10.1,
  FluentAssertions 7.2.2, NodaTime 3.3.5, GitHubActionsTestLogger 3.0.5, XunitXml.TestLogger 8.0.0,
  MinVer 2.3.0. Test: build with `-warnaserror`, 14 tests pass.
- [ ] 3. `test: migrate tests to xUnit v3`: `xunit` to `xunit.v3` 3.2.2,
  `xunit.runner.visualstudio` 3.1.5, `OutputType` `Exe`, per the xUnit v3 migration guide. Test:
  14 tests discovered and passing (a count of 0 is a failure).
- [ ] 4. `chore(build): align cake build and versioning with .NET 10`: port `build.cake` from the
  reference without the Docker tasks (Cake 6 `DotNet*` API, Traversal support); add `src/build.csproj`
  (Microsoft.Build.Traversal 4.1.82, pinned in `global.json`); `dotnet-tools.json` cake.tool 6.3.0,
  minver-cli 2.3.0, gpr 0.1.294; addins Cake.MinVer 4.0.0, Cake.Yaml 6.0.0, YamlDotNet 16.2.0 (as
  the reference). `MinVerMinimumMajorMinor=2.0` in `Directory.Build.targets` and
  `WithMinimumMajorMinor("2.0")` in `build.cake`. Test: `dotnet cake` prints the same version as the
  `.nupkg` file name.
- [ ] 5. `feat(packages): enable package validation against 1.1.0`: `EnablePackageValidation`,
  `PackageValidationBaselineVersion=1.1.0`, and a generated `CompatibilitySuppressions.xml` with only
  PKV006 for `net6.0` and `netstandard2.0`. Test: pack fails before the suppression file (red) and
  passes with it (green).
- [ ] 6. `ci: update GitHub Actions for .NET 10`: `ci.yaml` and `publish.yaml` as the reference:
  `actions/checkout@v6`, `actions/cache@v5`, `actions/setup-dotnet@v5` reading `global.json` in
  every job. Triggers stay as they are; no Docker steps. Test: PR checks green on all three OSes.
- [ ] 7. `docs: update docs for .NET 10`: README supported frameworks (net8.0, net10.0; 1.x stays
  available for `netstandard2.0` and `net6.0`), AGENTS.md (central package management, xUnit v3),
  this plan's status. The README badge moves to 2.0.0 after the release, as a `docs(readme)` commit.

## Verification

- [ ] `dotnet build src/Appy.Parsing.sln -c Release -warnaserror`: 0 warnings, 0 errors.
- [ ] `dotnet test src/Appy.Parsing.sln`: 14 passed on net10.0 (4 facts, 10 theory cases).
- [ ] `dotnet cake` (Default target) is green, and `.artifacts/Appy.Parsing.2.0.0-preview.0.N.nupkg`
  holds `lib/net8.0` and `lib/net10.0` only.
- [ ] Package validation runs on pack; the suppression file lists only the two PKV006 entries.
- [ ] No `net6.0`, `net9.0` or `netstandard2.0` left outside the suppression file and the docs that
  explain the change.
- [ ] No `.cs` change under `src/Appy.Parsing/`, so the public API is unchanged.
- [ ] Nothing is published or tagged from a local machine.
- [ ] Done when: the PR is merged, a maintainer tags `2.0.0`, the publish workflow is green, and
  nuget.org lists Appy.Parsing 2.0.0 for net8.0 and net10.0. Merging also publishes a
  `2.0.0-preview.0.N` package first; that is expected.

## Key Files

```
global.json                                     # SDK 10, Traversal SDK pin
src/Directory.Build.props                       # package metadata, validation settings
src/Directory.Build.targets                     # MinVer settings
src/Directory.Packages.props                    # (new) central package versions
src/build.csproj                                # (new) Traversal build
src/Appy.Parsing/Appy.Parsing.csproj            # library TFMs
src/Appy.Parsing/CompatibilitySuppressions.xml  # (new) PKV006 for dropped TFMs
src/Appy.Parsing.Tests/Appy.Parsing.Tests.csproj  # test TFM, xUnit v3
build.cake, dotnet-tools.json                   # Cake 6, MinVer
.github/workflows/ci.yaml, publish.yaml         # action versions, SDK setup
README.md, AGENTS.md                            # supported frameworks, conventions
```
