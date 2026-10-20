# Unity-PackageJsonGenerator

[日本語 / Japanese](README.ja.md)

An Editor extension that lets you create the `package.json` file required to publish a UPM (Unity Package Manager) package, using a simple GUI.

## Requirements

| Item | Version |
|---|---|
| Unity | 6000.0 or later |
| Dependency | `com.unity.nuget.newtonsoft-json` 3.2.1 (installed automatically) |

## Installation

### Via Package Manager (Git URL)

1. Copy the Git URL below.

   ```
   https://github.com/MAH-Lib/Unity-PackageJsonGenerator.git?path=PackageJsonGenerator
   ```

2. Open **Window > Package Manager**, click the **+** button, and select **Install package from git URL...**
3. Paste the URL and click **Install**.

### Via manifest.json

Add the following line to the `dependencies` section of `Packages/manifest.json`:

```json
"com.mah.package-json-generator": "https://github.com/MAH-Lib/Unity-PackageJsonGenerator.git?path=PackageJsonGenerator"
```

To pin a specific version, append a release tag to the URL (for example `...?path=PackageJsonGenerator#1.0.1`). See [Tags](../../tags) for the available tags.

## Usage

> The window's labels are currently in Japanese. The English meaning is shown in parentheses below.

1. From the menu bar, select **MAH > Generator > PackageJson Generator Window**.

   ![Image1](Image/Image1.png)

2. Fill in the fields in the Editor window.

   - **必須設定項目 (Required settings)**: these must be filled in. In the author settings, **E-mail** and **URL** are optional.

     ![Image2](Image/Image2.png)

   - **任意設定項目 (Optional settings)**: fill in only what you need.

     ![Image3](Image/Image3.png)

3. Click **Browse** and select the folder where `package.json` will be created. **Reset** sets the folder back to `Assets`.

   ![Image4](Image/Image4.png)

4. Click **Create package.json** to write the file.

   ![Image5](Image/Image5.png)

### Editing an existing package.json

Set the folder that contains the existing `package.json`, then click **Load package.json**. The fields are filled with the file's contents. You can edit them and click **Create package.json** to save again.

## Fields

| Section | Field (label in the window) | `package.json` key | Notes |
|---|---|---|---|
| Required | Package name (パッケージ名) | `name` | Lowercase, dot-separated (e.g. `com.company.my-tool`) |
| Required | Version (バージョン) | `version` | Major / Minor / Patch |
| Required | Display name (UPM表示名) | `displayName` | Name shown in Package Manager |
| Required | Description (概要) | `description` | |
| Required | Unity version (対応 Unity バージョン設定) | `unity` | Minimum supported Unity version (e.g. `6000.0`). Choose from Unity 5.0 to Unity 6.6 (`6000.6`) |
| Required | License (ライセンス) | `license` | Choose from common SPDX licenses |
| Required | Author (著者情報設定) | `author` | Name is required. E-mail and URL are optional |
| Optional | Release version (リリースバージョン) | `unityRelease` | e.g. `0f1` |
| Optional | Hide in editor (UMP除外フラグ) | `hideInEditor` | |
| Optional | Changelog / Documentation / License URL | `changelogUrl` / `documentationUrl` / `licensesUrl` | Must start with `http://` or `https://` |
| Optional | Keywords (キーワード) | `keywords` | |
| Optional | Samples (サンプル) | `samples` | Path is converted to start with `Samples~` |
| Optional | Dependencies (外部パッケージ) | `dependencies` | Exact versions only (`X.Y.Z`), as required by Unity Package Manager. You can pick packages installed in your project |

Empty optional fields are not written to the file.

## Known limitations

- The entered values are kept when scripts are recompiled, but they are cleared when the window is closed.

These will be addressed in future updates.

## Support

- **Bug reports and feature requests**: [Issues](../../issues)
- **Questions and discussions**: [Discussions](../../discussions)

Thank you for your contributions!

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
