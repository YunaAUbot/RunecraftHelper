# RunecraftHelper Agent Guide

## Project

This private repository tracks the upstream RunecraftHelper history plus Chris's GameHelper2 adaptation. The adaptation consumes the shared `PriceProviderRegistry` and avoids unnecessary large render-path scans.

## Upstream workflow

The GitHub repository is private and therefore cannot be a formal fork of the public upstream, but it retains upstream Git ancestry.

After a fresh clone, configure the read-only upstream remote:

```bash
git remote add upstream https://github.com/yokkenUA/RunecraftHelper.git
git remote set-url --push upstream no_push
git fetch upstream
```

The adaptation starts at the exact commit documented in `UPSTREAM.md`; do not silently replace it with a newer upstream snapshot. Merge `upstream/main` deliberately, resolve conflicts while preserving the shared-provider boundary and bounded render work, run the full verification below, and never force-push `main`.

## Host boundary

Supply GameHelper2 via `GAMEHELPER2_HOST_ROOT` or `-p:GameHelperHostRoot=...`. NinjaPricer owns price networking, cache, league, and refresh state.

## Verification

```bash
export GAMEHELPER2_HOST_ROOT=/path/to/GameHelper2
dotnet test test/RunecraftHelper.Tests.csproj -c Release
dotnet build RunecraftHelper.csproj -c Release -p:EnableWindowsTargeting=true
git diff --check
```

A real Runestone-panel test remains the final acceptance gate for render-performance changes.

## Safety

Read-only process-memory access only. No game input, process control, process writes, injection, packet manipulation, or independent price networking.

## Git importer layout

Keep the production project and source at repository root. Every auxiliary project
and tool belongs under `test/` (tools under `test/tools/`). Preserve assembly identity
and exclude test/tools and nested obj/bin from all production SDK items.
Run `python3 test/check_import_layout.py` with `GAMEHELPER2_HOST_ROOT` and `DOTNET` set.
See [ROOT_LAYOUT.md](ROOT_LAYOUT.md) for the pinned importer contract and verification
commands that use existing host artifacts without rebuilding or modifying the host.
