#if UNITY_EDITOR
using System;
using Newtonsoft.Json;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    [Serializable]
    public class SampleInfo
    {
        [JsonProperty("displayName")]
        public string DisplayName;
        [JsonProperty("description")]
        public string Description;
        [JsonProperty("path")]
        public string Path;
    }
}
#endif