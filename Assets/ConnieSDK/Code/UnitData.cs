using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.Json.Serialization;
using System;

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

        public MeshWriteMode MeshWriting = MeshWriteMode.Nothing;
        public string MeshCollection = string.Empty;

        [JsonIgnore]
        public string FileName = "newAsset";
        [JsonIgnore]
        public string[] IgnoreObjects = new string[0];

        [ContextMenu("Serialize to File")]
        public void Serialize()
        {
            ConnieSerializer.SerializeObject(this);
        }

        [Flags]
        public enum MeshWriteMode
        {
            Nothing = 0,
            Write = 1,
            Bundle = 2,
        }
    }
}
#endif
