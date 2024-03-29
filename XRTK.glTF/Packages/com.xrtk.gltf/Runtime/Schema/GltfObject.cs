﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Utilities.Gltf.Schema.Extensions;

namespace XRTK.Utilities.Gltf.Schema
{
    [Serializable]
    public class GltfObject : GltfProperty
    {
        #region Serialized Fields

        /// <summary>
        /// Names of glTF extensions used somewhere in this asset.
        /// </summary>
        public string[] extensionsUsed;

        /// <summary>
        /// Names of glTF extensions required to properly load this asset.
        /// </summary>
        public string[] extensionsRequired;

        /// <summary>
        /// An array of accessors. An accessor is a typed view into a bufferView.
        /// </summary>
        public GltfAccessor[] accessors;

        /// <summary>
        /// An array of keyframe animations.
        /// </summary>
        public GltfAnimation[] animations;

        /// <summary>
        /// Metadata about the glTF asset.
        /// </summary>
        public GltfAssetInfo asset;

        /// <summary>
        /// An array of buffers. A buffer points to binary geometry, animation, or skins.
        /// </summary>
        public GltfBuffer[] buffers;

        /// <summary>
        /// An array of bufferViews.
        /// A bufferView is a view into a buffer generally representing a subset of the buffer.
        /// </summary>
        public GltfBufferView[] bufferViews;

        /// <summary>
        /// An array of cameras. A camera defines a projection matrix.
        /// </summary>
        public GltfCamera[] cameras;

        /// <summary>
        /// An array of images. An image defines data used to create a texture.
        /// </summary>
        public GltfImage[] images;

        /// <summary>
        /// An array of materials. A material defines the appearance of a primitive.
        /// </summary>
        public GltfMaterial[] materials;

        /// <summary>
        /// An array of meshes. A mesh is a set of primitives to be rendered.
        /// </summary>
        public GltfMesh[] meshes;

        /// <summary>
        /// An array of nodes.
        /// </summary>
        public GltfNode[] nodes;

        /// <summary>
        /// An array of samplers. A sampler contains properties for texture filtering and wrapping modes.
        /// </summary>
        public GltfSampler[] samplers;

        /// <summary>
        /// The index of the default scene.
        /// </summary>
        public int scene;

        /// <summary>
        /// An array of scenes.
        /// </summary>
        public GltfScene[] scenes;

        /// <summary>
        /// An array of skins. A skin is defined by joints and matrices.
        /// </summary>
        public GltfSkin[] skins;

        /// <summary>
        /// An array of textures.
        /// </summary>
        public GltfTexture[] textures;

        #endregion Serialized Fields

        /// <summary>
        /// The name of the gltf Object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The absolute path to the glTF Object on disk.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// The <see cref="GameObject"/> reference for the gltf Object.
        /// </summary>
        public GameObject GameObjectReference { get; internal set; }

        /// <summary>
        /// The list of registered glTF extensions found for this object.
        /// </summary>
        public List<GltfExtension> RegisteredExtensions { get; internal set; } = new List<GltfExtension>();

        /// <summary>
        /// Flag for setting object load behavior.
        /// Importers require synchronous behavior; all other loading scenarios should likely use asynchronous behavior.
        /// </summary>
        internal bool LoadAsynchronously { get; set; } = true;
    }
}
