#if UNITY_EDITOR
using Newtonsoft.Json;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    public class AuthorInfo
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("email")]
        public string Email;
        [JsonProperty("url")]
        public string URL;
    }
}
#endif