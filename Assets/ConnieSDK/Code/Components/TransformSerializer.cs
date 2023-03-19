using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ConnieSDK
{
    public static class TransformSerializer
    {
        public static string Serialize (Transform tr)
        {
            return "{}";
        }
    }
}
