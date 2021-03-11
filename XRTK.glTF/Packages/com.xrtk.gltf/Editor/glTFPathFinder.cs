// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Editor.Utilities;

namespace XRTK.glTF.Editor
{
    /// <summary>
    /// Dummy scriptable object used to find the relative path of the com.xrtk.gltf.
    /// </summary>
    ///// <inheritdoc cref="IPathFinder" />
    public class glTFPathFinder : ScriptableObject, IPathFinder
    {
        ///// <inheritdoc />
        public string Location => $"/Editor/{nameof(glTFPathFinder)}.cs";
    }
}
