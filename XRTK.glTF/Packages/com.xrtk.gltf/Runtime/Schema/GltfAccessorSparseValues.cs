// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace XRTK.Utilities.Gltf.Schema
{
    /// <summary>
    /// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/accessor.sparse.values.schema.json
    /// </summary>
    [Serializable]
    public class GltfAccessorSparseValues : GltfProperty
    {
        /// <summary>
        /// The index of the bufferView with sparse values.
        /// Referenced bufferView can't have ARRAY_BUFFER or ELEMENT_ARRAY_BUFFER target.
        /// </summary>
        public int bufferView = -1;

        /// <summary>
        /// The offset relative to the start of the bufferView in bytes. Must be aligned.
        /// <minimum>0</minimum>
        /// </summary>
        public int byteOffset = -1;
    }
}