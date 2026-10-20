#if UNITY_EDITOR
using System.Runtime.CompilerServices;

//開発用プロジェクトの EditMode テスト（Assets/@MAH/Tests/、パッケージには含めない）から internal のクラスを呼ぶため
[assembly: InternalsVisibleTo("MAH.PackageJson_Generator.Editor.Tests")]
#endif
