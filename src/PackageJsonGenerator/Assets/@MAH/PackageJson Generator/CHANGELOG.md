# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this package adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Package documentation: `README.md`, `CHANGELOG.md` and `LICENSE.md`.
- `documentationUrl`, `changelogUrl` and `licensesUrl` in `package.json`.
- Unity versions `6000.3` to `6000.6` (Unity 6.3 to 6.6) and `5.0` can now be selected.
- Invalid input (package name, e-mail, URLs, dependency names and sample paths) is now reported with an error message below the field.
- The result of **Create package.json** and **Load package.json** (success, warnings or the reason for an error) is now shown in the window.
- **Create package.json** now asks for confirmation before overwriting an existing `package.json`.
- The entered values are now kept when scripts are recompiled (domain reload).

### Changed

- The default Unity version is now `6000.0` (was `6000.1`). When you change the Major version, the Minor version is set to the oldest one in the list.
- The Unity version is now shown as numbers (for example `6000 (Unity 6)` and `3`) instead of enum names.

- Dependencies are now always written as exact versions (`"X.Y.Z"`), because Unity Package Manager does not support version ranges. When loading a `package.json`, comparison values such as `>=1.2.0` are converted to `1.2.0`, and ranges such as `1.0.0 - 2.0.0` are converted to their lower bound, with a warning in the Console.

### Removed

- The version condition options (`>`, `>=`, `<`, `<=`, `Range`) for dependencies.

### Fixed

- Creating a `package.json` no longer throws an exception when two dependencies have the same name. An error is shown in the Console instead.
- Dependency rows whose name is still the default `com.` (or otherwise invalid) are no longer written to the file. A warning is shown in the Console.
- Dependency names with three or more parts (for example `com.example.tool`) can now be typed by hand. The name is validated when you press Enter or leave the field.
- Dependency versions that cannot be read when loading a `package.json` are no longer dropped silently. A warning is shown in the Console.
- Selecting **Other** as the license now saves the SPDX ID you enter. When loading a `package.json` with a license that is not in the list, it is shown in the **Other** field.
- Sample paths are now always written with `/` separators (for example `Samples~/Basic`), on every OS. Paths written with `\` by older versions are converted when loading.
- Sample paths are now detected by folder name, so folders such as `MySamples` are no longer treated as `Samples`.
- Sample paths can now be typed by hand. The path is converted when you press Enter or leave the field.
- New samples and the sample **Reset** button now always use `Samples~` as the path. The **Reset** button did not work before.
- Resetting the optional settings now also resets **Hide in editor**.
- Loading a `package.json` no longer throws an exception when `unity` or `version` is missing or has an unexpected format (for example `"6000"`, `"1.2"` or `"1.2.3.4"`). The value is filled in and a warning is shown in the Console.
- A Unity version that is not in the list (for example `6000.7`) is now kept when loading a `package.json`, and is shown as "not in the list".
- Selecting an installed package with a preview version (for example `1.0.0-pre.3`) in the dependencies list no longer throws an exception.
- When **Load package.json** fails (the file does not exist or is not valid JSON), the entered values are no longer cleared.
- Loading a `package.json` that is not valid JSON no longer throws an exception.
- Creating a `package.json` in a folder that does not exist, or when the file cannot be written, no longer throws an exception.
- A package name loaded from a `package.json` is now converted to lowercase. If it is still invalid, a warning is shown, and **Create package.json** stops with an error instead of writing the invalid name.

## [1.0.1] - 2025-07-06

### Fixed

- Added missing `.meta` files to the package.

## [1.0.0] - 2025-07-05

### Added

- First release.
- PackageJson Generator window (**MAH > Generator > PackageJson Generator Window**).
- Create a `package.json` from required settings: name, version, display name, description, Unity version, license and author.
- Optional settings: Unity release, hide in editor, changelog/documentation/license URLs, keywords, samples and dependencies.
- Load an existing `package.json` into the window for editing.

[Unreleased]: https://github.com/MAH-Lib/Unity-PackageJsonGenerator/compare/1.0.1...HEAD
[1.0.1]: https://github.com/MAH-Lib/Unity-PackageJsonGenerator/compare/v1.0.0...1.0.1
[1.0.0]: https://github.com/MAH-Lib/Unity-PackageJsonGenerator/releases/tag/v1.0.0
