using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

#if UNITY_EDITOR
#nullable enable
namespace ConnieSDK
{
    public class UnitData : MonoBehaviour
    {
        public UnitDefinition Definition = new UnitDefinition();

        [ContextMenu("Serialize to File")]
        public void Serialize()
        {
            ConnieSerializer.SerializeObject(Definition, transform);
        }
    }

}
#endif
