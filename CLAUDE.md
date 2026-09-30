# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

@./AGENTS.md

## Build & Test

```bash
dotnet tool restore
dotnet cake                          # clean, build, test, pack into .artifacts/
dotnet test src/Appy.Parsing.slnx    # tests only
```

Never run the `Publish` cake target or `dotnet nuget push`; publishing happens in CI only.
