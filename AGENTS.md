# AGENTS.md

Guidance for AI coding agents and contributors working in this repository.

## Overview

Appy.Parsing is an open-source .NET library, published to nuget.org, that turns raw text into
objects. A regex-based lexer splits text into tokens and a parser combines tokens into objects;
fluent builders define the grammar. Usage lives in [README.md](README.md), design in
[docs/Architecture.md](docs/Architecture.md).

## Build Commands

```bash
dotnet tool restore                  # cake, minver-cli, gpr
dotnet cake                          # Default target: clean, build, test, pack into .artifacts/
dotnet test src/Appy.Parsing.slnx    # tests only
dotnet test src/Appy.Parsing.slnx --filter "FullyQualifiedName~CalculatorTest"
```

- The cake build treats warnings as errors.
- `config.yml` lists the projects cake builds and their role (`Package` or `Test`). A new
  project not listed there is skipped by the build.
- Package versions live in `src/Directory.Packages.props` (central package management); a
  `PackageReference` in a csproj carries no `Version`.
- Never run the `Publish` cake target or `dotnet nuget push` locally; CI publishes.

## Structure

| Path | Purpose |
|------|---------|
| `src/Appy.Parsing/` | The library (`net10.0;net8.0`): `Builder/`, `Lexers/`, `Parsers/` |
| `src/Appy.Parsing.Tests/` | xUnit v3 tests (`net10.0`) |
| `src/Directory.Build.props` | Shared build settings, package metadata |
| `src/Directory.Build.targets` | MinVer and package validation settings |
| `src/Directory.Packages.props` | Package versions |
| `global.json` | .NET 10 SDK version |
| `build.cake`, `functions.cake`, `config.yml` | Cake build |
| `.github/workflows/` | `ci.yaml` on pull requests, `publish.yaml` on push to `master` or a tag |
| `docs/` | Architecture and plans |

## Versioning and Publishing

- [MinVer](https://github.com/adamralph/minver) derives the version from git tags
  (`MAJOR.MINOR.PATCH`, no `v` prefix). Untagged commits get a `-preview.0.N` suffix.
- The MinVer minimum major.minor (`2.0`) is set in both `src/Directory.Build.targets` and
  `build.cake`; keep them in sync.
- Pack runs package validation against the last release: `PackageValidationBaselineVersion` in
  `src/Directory.Build.targets`, suppressions in `src/Appy.Parsing/CompatibilitySuppressions.xml`.
  After each release, move the baseline to that version and drop suppressions that no longer
  apply.
- A push to `master` that touches `src/` publishes a preview package; a tag publishes that
  version. Do not create tags or releases unless a maintainer asks.
- Dropping a target framework or public API is a breaking change: call it out in the PR.

## Tests

- xUnit v3 (`xunit.v3` 3.2.2, `xunit.runner.visualstudio` 3.1.5, run through VSTest,
  `GitHubActionsTestLogger` 2.4.1) with FluentAssertions 7. Stay on FluentAssertions 7: 8 has a
  commercial licence.
- Tests build real lexers and parsers through the builders; no mocks.
- Add or update a test with every behaviour change, and write it first.

## Code Style

- `.editorconfig`: 4 spaces, CRLF; 2 spaces for XML, JSON and YAML.
- Match the existing code: `_camelCase` private fields, lower camel case private methods
  (`populateUnmatchedPart`), no explicit `private` modifier.
- Public types and members carry XML doc comments.

## Git Conventions

- Branches: `feat/…`, `fix/…`, `docs/…`, `chore/…`, from `master`.
- Commits: conventional commits, `type(scope): subject`, imperative, lower case, no trailing dot
  ([CONTRIBUTING.md](CONTRIBUTING.md#commit)). Breaking changes go in a `BREAKING CHANGE:` footer.
- No `Co-Authored-By` or tool attribution trailers.
- Public repository: never commit secrets, private feeds or internal links.

## Documentation

- [docs/Architecture.md](docs/Architecture.md): start here
- [docs/plans/](docs/plans/): implementation plans (`NNN-slug.md`)
