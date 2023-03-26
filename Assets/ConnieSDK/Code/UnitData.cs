using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.Json.Serialization;

#if UNITY_EDITOR
#nullable enable
namespace ConnieSDK
{
    public class UnitData : MonoBehaviour
    {
        public string UnitName = string.Empty;
        public ObjectType Type = ObjectType.Generic;

        public string DescriptionShort = string.Empty;
        [TextArea(minLines:5, maxLines:10)]
        public string DescriptionLong = string.Empty;

        [JsonIgnore]
        public string FileName = "newAsset";
        [JsonIgnore]
        public string[] IgnoreObjects = new string[0];

        [ContextMenu("Serialize to File")]
        public void Serialize()
        {
            ConnieSerializer.SerializeObject(this);
        }
    }
}
#endif
