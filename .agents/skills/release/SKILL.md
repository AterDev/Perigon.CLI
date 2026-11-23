---
name: release
description: Prepare and publish Perigon.CLI patch releases, including versioning, release notes, bilingual docs, validation, and the nuget-branch CI handoff.
---

# Perigon.CLI Release Workflow

Use this skill when releasing `Perigon.CLI` or when the user asks to package and publish a version. The NuGet publishing workflow runs automatically when `origin/nuget` receives a push.

## Before editing

- Inspect `git status` in both `Perigon.CLI` and `Perigon.docs` repositories.
- Preserve unrelated user changes. Stage only files belonging to the release.
- Confirm the exact current version from `src/Apps/CommandLine/CommandLine.csproj`.

## Release contents

- Unless the user specifies a version, increment only the patch component of `<Version>` in `src/Apps/CommandLine/CommandLine.csproj`.
- Update `<PackageReleaseNotes>` with only the changes being released.
- Update the matching Chinese and English pages under `Perigon.docs/Content/docs/Perigon`, normally the command-line page and the version update notes. Keep their structure and meaning aligned.
- Add or update reusable release guidance in this skill and `Contribution.md` when the workflow changes.

## Validation

Run the focused tests for the changed feature, then the full test project:

```powershell
dotnet test tests/StudioMod.Tests/CoreMod.Tests.csproj --configuration Release --verbosity minimal
```

The CI-equivalent managed NuGet packaging command is:

```powershell
pwsh ./scripts/PublishToLocal.ps1
```

Use `pwsh ./scripts/PublishToLocal.ps1 -Install` to install the freshly built package as a global tool for the current user. The package ID and version come from the CLI project file.

## Branch handoff

1. Commit the Perigon.CLI release changes on the current development branch with a conventional emoji-prefixed message such as `🚀 release: prepare Perigon.CLI 10.1.12`.
2. Push that branch if it is the requested source branch.
3. Fetch `origin/nuget`, verify it has no unexpected divergence, merge the release commit into a local `nuget` branch, and push `nuget` without force-pushing.
4. add git tag `v<version>` to the release commit and push the tag.

If the `nuget` branch has unrelated commits or conflicts, stop before merging and report the exact divergence. Do not reset, discard, or force-push user work.

## Docs repository

Commit and push only the release-related files in `Perigon.docs`; leave any pre-existing uncommitted documentation changes untouched. Verify both `zh-CN` and `en-US` pages before handing off.
