#if UNITY_EDITOR
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    public class PackageJsonInfo
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("version")]
        public string Version;
        [JsonProperty("description")]
        public string Description;
        [JsonProperty("displayName")]
        public string DisplayName;
        [JsonProperty("unity")]
        public string Unity;
        [JsonProperty("unityRelease")]
        public string UnityRelease;
        [JsonProperty("license")]
        public string License;
        [JsonProperty("licensesUrl")]
        public string LicensesURL;
        [JsonProperty("changelogUrl")]
        public string ChangelogURL;
        [JsonProperty("documentationUrl")]
        public string DocumentationURL;
        [JsonProperty("hideInEditor")]
        public bool HideInEditor;
        [JsonProperty("author")]
        public AuthorInfo Author;
        [JsonProperty("keywords")]
        public List<string> Keywords;
        [JsonProperty("dependencies")]
        public Dictionary<string, string> Dependencies;
        [JsonProperty("samples")]
        public List<SampleInfo> Samples;
    }
}
#endif