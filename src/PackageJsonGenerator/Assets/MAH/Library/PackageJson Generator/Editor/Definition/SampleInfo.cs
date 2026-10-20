#if UNITY_EDITOR
using Newtonsoft.Json;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
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