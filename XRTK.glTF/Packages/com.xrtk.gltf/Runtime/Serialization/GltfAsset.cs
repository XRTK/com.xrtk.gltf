﻿using UnityEngine;
using XRTK.Utilities.Gltf.Schema;

namespace XRTK.Utilities.Gltf
{
    public class GltfAsset : ScriptableObject
    {
        [SerializeField]
        private GameObject model;

        public GameObject Model
        {
            get => model;
            set => model = value;
        }

        [SerializeField]
        private GltfObject gltfObject;

        public GltfObject GltfObject
        {
            get => gltfObject;
            set => gltfObject = value;
        }
    }
}