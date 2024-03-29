﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using XRTK.Extensions;
using XRTK.Utilities.Async;
using XRTK.Utilities.Gltf.Schema;
using XRTK.Utilities.WebRequestRest;

namespace XRTK.Utilities.Gltf.Serialization
{
    public static class ConstructGltf
    {
        private static readonly int SrcBlendId = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlendId = Shader.PropertyToID("_DstBlend");
        private static readonly int ZWriteId = Shader.PropertyToID("_ZWrite");
        private static readonly int ModeId = Shader.PropertyToID("_Mode");
        private static readonly int EmissionMapId = Shader.PropertyToID("_EmissionMap");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int MetallicGlossMapId = Shader.PropertyToID("_MetallicGlossMap");
        private static readonly int GlossinessId = Shader.PropertyToID("_Glossiness");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int BumpMapId = Shader.PropertyToID("_BumpMap");

        /// <summary>
        /// Constructs the glTF Object.
        /// </summary>
        /// <param name="gltfObject"></param>
        /// <param name="setActive"></param>
        /// <returns>The new <see cref="GameObject"/> of the final constructed <see cref="GltfScene"/></returns>
        public static async void Construct(this GltfObject gltfObject, bool setActive = true)
            => await gltfObject.ConstructAsync(setActive);

        /// <summary>
        /// Constructs the glTF Object.
        /// </summary>
        /// <param name="gltfObject"></param>
        /// <param name="setActive"></param>
        /// <returns>The new <see cref="GameObject"/> of the final constructed <see cref="GltfScene"/></returns>
        public static async Task<GameObject> ConstructAsync(this GltfObject gltfObject, bool setActive = true)
        {
            if (!gltfObject.asset.version.Contains("2.0"))
            {
                Debug.LogWarning($"Expected glTF 2.0, but this asset is using {gltfObject.asset.version}");
                return null;
            }

            if (gltfObject.LoadAsynchronously) { await Awaiters.UnityMainThread; }

            var rootObject = new GameObject($"glTF Scene {gltfObject.Name}");
            rootObject.SetActive(false);

            if (gltfObject.LoadAsynchronously) { await Awaiters.BackgroundThread; }

            for (int i = 0; i < gltfObject.bufferViews?.Length; i++)
            {
                await gltfObject.ConstructBufferView(gltfObject.bufferViews[i]);
            }

            for (int i = 0; i < gltfObject.textures?.Length; i++)
            {
                await gltfObject.ConstructTextureAsync(gltfObject.textures[i]);
            }

            for (int i = 0; i < gltfObject.materials?.Length; i++)
            {
                await gltfObject.ConstructMaterialAsync(gltfObject.materials[i], i);
            }

            if (gltfObject.scenes == null)
            {
                Debug.LogError($"No scenes found for {gltfObject.Name}");
            }

            if (gltfObject.LoadAsynchronously) { await Awaiters.UnityMainThread; }

            for (int i = 0; i < gltfObject.scenes?.Length; i++)
            {
                await gltfObject.ConstructSceneAsync(gltfObject.scenes[i], rootObject);
            }

            rootObject.SetActive(setActive);
            return gltfObject.GameObjectReference = rootObject;
        }

        private static async Task ConstructBufferView(this GltfObject gltfObject, GltfBufferView bufferView)
        {
            bufferView.Buffer = gltfObject.buffers[bufferView.buffer];

            if (bufferView.Buffer.BufferData == null &&
                !string.IsNullOrEmpty(gltfObject.Uri) &&
                !string.IsNullOrEmpty(bufferView.Buffer.uri))
            {
                var path = gltfObject.Uri.PathFromURI();
                var fullPath = $"{path}{bufferView.Buffer.uri}";

                Response response;

                if (gltfObject.LoadAsynchronously)
                {
                    response = await Rest.GetAsync(fullPath);
                }
                else
                {
                    var data = File.ReadAllBytes(fullPath);
                    response = new Response(true, null, data, 200);
                }

                if (response.Successful)
                {
                    bufferView.Buffer.BufferData = response.ResponseData;
                }
            }
        }

        private static async Task ConstructTextureAsync(this GltfObject gltfObject, GltfTexture gltfTexture)
        {
            if (gltfObject.LoadAsynchronously) { await Awaiters.BackgroundThread; }

            if (gltfTexture.source >= 0)
            {
                var gltfImage = gltfObject.images[gltfTexture.source];

                byte[] imageData = null;
                Texture2D texture = null;

                if (!string.IsNullOrEmpty(gltfObject.Uri) &&
                    !string.IsNullOrEmpty(gltfImage.uri))
                {
                    var path = gltfObject.Uri.PathFromURI();

                    if (gltfObject.LoadAsynchronously)
                    {
                        texture = await Rest.DownloadTextureAsync($"{path}{gltfImage.uri}");
                    }
                    else
                    {
#if UNITY_EDITOR
                        texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"{path}{gltfImage.uri}");
#else
                        throw new NotImplementedException();
#endif
                    }
                }
                else
                {
                    var imageBufferView = gltfObject.bufferViews[gltfImage.bufferView];
                    imageData = new byte[imageBufferView.byteLength];
                    Array.Copy(imageBufferView.Buffer.BufferData, imageBufferView.byteOffset, imageData, 0, imageData.Length);
                }

                if (texture == null)
                {
                    if (gltfObject.LoadAsynchronously) { await Awaiters.UnityMainThread; }
                    // TODO Load texture async from native plugin?
                    texture = new Texture2D(2, 2);
                    gltfImage.Texture = texture;
                    gltfImage.Texture.LoadImage(imageData);
                }
                else
                {
                    gltfImage.Texture = texture;
                }

                gltfTexture.Texture = gltfImage.Texture;

                if (gltfObject.LoadAsynchronously) { await Awaiters.BackgroundThread; }
            }
        }

        private static async Task ConstructMaterialAsync(this GltfObject gltfObject, GltfMaterial gltfMaterial, int materialId)
        {
            if (gltfObject.LoadAsynchronously) { await Awaiters.UnityMainThread; }

            var material = await CreateStandardShaderMaterial(gltfObject, gltfMaterial, materialId);

            if (material == null)
            {
                Debug.LogWarning("The Standard Shader was not found. Failed to create material for glTF object");
            }
            else
            {
                gltfMaterial.Material = material;
            }

            if (gltfObject.LoadAsynchronously) { await Awaiters.BackgroundThread; }
        }

        private static async Task<Material> CreateStandardShaderMaterial(GltfObject gltfObject, GltfMaterial gltfMaterial, int materialId)
        {
            var shader = Shader.Find("Standard");

            if (shader == null) { return null; }

            var material = new Material(shader)
            {
                name = string.IsNullOrEmpty(gltfMaterial.name) ? $"glTF Material {materialId}" : gltfMaterial.name
            };

            if (gltfMaterial.pbrMetallicRoughness.baseColorTexture.index >= 0)
            {
                material.mainTexture = gltfObject.images[gltfMaterial.pbrMetallicRoughness.baseColorTexture.index].Texture;
            }

            material.color = gltfMaterial.pbrMetallicRoughness.baseColorFactor.GetColorValue();

            if (gltfMaterial.alphaMode == "MASK")
            {
                material.SetInt(SrcBlendId, (int)BlendMode.One);
                material.SetInt(DstBlendId, (int)BlendMode.Zero);
                material.SetInt(ZWriteId, 1);
                material.SetInt(ModeId, 3);
                material.SetOverrideTag("RenderType", "Cutout");
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;
            }
            else if (gltfMaterial.alphaMode == "BLEND")
            {
                material.SetInt(SrcBlendId, (int)BlendMode.One);
                material.SetInt(DstBlendId, (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt(ZWriteId, 0);
                material.SetInt(ModeId, 3);
                material.SetOverrideTag("RenderType", "Transparency");
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }

            if (gltfMaterial.emissiveTexture.index >= 0)
            {
                material.EnableKeyword("_EmissionMap");
                material.EnableKeyword("_EMISSION");
                material.SetTexture(EmissionMapId, gltfObject.images[gltfMaterial.emissiveTexture.index].Texture);
                material.SetColor(EmissionColorId, gltfMaterial.emissiveFactor.GetColorValue());
            }

            if (gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture.index >= 0)
            {
                var texture = gltfObject.images[gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture.index].Texture;

                if (texture.isReadable)
                {
                    var pixels = texture.GetPixels();
                    if (gltfObject.LoadAsynchronously) { await Awaiters.BackgroundThread; }

                    var pixelCache = new Color[pixels.Length];

                    for (int c = 0; c < pixels.Length; c++)
                    {
                        // Unity only looks for metal in R channel, and smoothness in A.
                        pixelCache[c].r = pixels[c].g;
                        pixelCache[c].g = 0f;
                        pixelCache[c].b = 0f;
                        pixelCache[c].a = pixels[c].b;
                    }

                    if (gltfObject.LoadAsynchronously) { await Awaiters.UnityMainThread; }
                    texture.SetPixels(pixelCache);
                    texture.Apply();

                    material.SetTexture(MetallicGlossMapId, texture);
                }

                material.SetFloat(GlossinessId, Mathf.Abs((float)gltfMaterial.pbrMetallicRoughness.roughnessFactor - 1f));
                material.SetFloat(MetallicId, (float)gltfMaterial.pbrMetallicRoughness.metallicFactor);
                material.EnableKeyword("_MetallicGlossMap");
                material.EnableKeyword("_METALLICGLOSSMAP");
            }

            if (gltfMaterial.normalTexture.index >= 0)
            {
                material.SetTexture(BumpMapId, gltfObject.images[gltfMaterial.normalTexture.index].Texture);
                material.EnableKeyword("_BumpMap");
            }

            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            return material;
        }

        private static async Task ConstructSceneAsync(this GltfObject gltfObject, GltfScene gltfScene, GameObject root)
        {
            for (int i = 0; i < gltfScene.nodes.Length; i++)
            {
                // Note: glTF objects are currently imported with their original scale from the glTF scene, which may apply an unexpected transform
                // to the root node. If this behavior needs to be changed, functionality should be added below to ConstructNodeAsync
                await ConstructNodeAsync(gltfObject, gltfObject.nodes[gltfScene.nodes[i]], gltfScene.nodes[i], root.transform);
            }
        }

        private static async Task ConstructNodeAsync(GltfObject gltfObject, GltfNode node, int nodeId, Transform parent)
        {
            if (gltfObject.LoadAsynchronously) { await Awaiters.UnityMainThread; }

            var nodeName = string.IsNullOrEmpty(node.name) ? $"glTF Node {nodeId}" : node.name;
            var nodeGameObject = new GameObject(nodeName);

            // If we're creating a really large node, we need it to not be visible in partial stages. So we hide it while we create it
            nodeGameObject.SetActive(false);

            if (gltfObject.LoadAsynchronously) { await Awaiters.BackgroundThread; }

            node.Matrix = node.GetTrsProperties(out var position, out var rotation, out var scale);

            if (node.Matrix == Matrix4x4.identity)
            {
                if (node.translation != null)
                {
                    position = node.translation.GetVector3Value();
                }

                if (node.rotation != null)
                {
                    rotation = node.rotation.GetQuaternionValue();
                }

                if (node.scale != null)
                {
                    scale = node.scale.GetVector3Value(false);
                }
            }

            if (gltfObject.LoadAsynchronously) { await Awaiters.UnityMainThread; }

            nodeGameObject.transform.localPosition = position;
            nodeGameObject.transform.localRotation = rotation;
            nodeGameObject.transform.localScale = scale;

            if (node.mesh >= 0)
            {
                await ConstructMeshAsync(gltfObject, nodeGameObject, node.mesh);
            }

            if (node.children != null)
            {
                for (int i = 0; i < node.children.Length; i++)
                {
                    await ConstructNodeAsync(gltfObject, gltfObject.nodes[node.children[i]], node.children[i], nodeGameObject.transform);
                }
            }

            nodeGameObject.transform.SetParent(parent, false);
            nodeGameObject.SetActive(true);
        }

        private static async Task ConstructMeshAsync(GltfObject gltfObject, GameObject parent, int meshId)
        {
            var gltfMesh = gltfObject.meshes[meshId];
            var filter = parent.gameObject.AddComponent<MeshFilter>();
            var renderer = parent.gameObject.AddComponent<MeshRenderer>();

            if (gltfMesh.primitives.Length == 1)
            {
                gltfMesh.Mesh = await ConstructMeshPrimitiveAsync(gltfObject, gltfMesh.primitives[0]);
                gltfMesh.Mesh.name = gltfMesh.name;
                filter.sharedMesh = gltfMesh.Mesh;
                renderer.sharedMaterial = gltfObject.materials[gltfMesh.primitives[0].material].Material;
                return;
            }

            var materials = new List<Material>();
            var meshCombines = new CombineInstance[gltfMesh.primitives.Length];

            for (int i = 0; i < gltfMesh.primitives.Length; i++)
            {
                meshCombines[i].mesh = await ConstructMeshPrimitiveAsync(gltfObject, gltfMesh.primitives[i]);

                var meshMaterial = gltfObject.materials[gltfMesh.primitives[i].material].Material;

                if (!materials.Contains(meshMaterial))
                {
                    materials.Add(meshMaterial);
                }
            }

            var newMesh = new Mesh();
            newMesh.CombineMeshes(meshCombines);
            gltfMesh.Mesh = filter.sharedMesh = newMesh;
            gltfMesh.Mesh.name = gltfMesh.name;
            renderer.sharedMaterials = materials.ToArray();
        }

        private static async Task<Mesh> ConstructMeshPrimitiveAsync(GltfObject gltfObject, GltfMeshPrimitive meshPrimitive)
        {
            if (gltfObject.LoadAsynchronously) { await Awaiters.BackgroundThread; }

            GltfAccessor positionAccessor = null;
            GltfAccessor normalsAccessor = null;
            GltfAccessor textCoord0Accessor = null;
            GltfAccessor textCoord1Accessor = null;
            GltfAccessor textCoord2Accessor = null;
            GltfAccessor textCoord3Accessor = null;
            GltfAccessor colorAccessor = null;
            GltfAccessor indicesAccessor = null;
            GltfAccessor tangentAccessor = null;
            GltfAccessor weight0Accessor = null;
            GltfAccessor joint0Accessor = null;
            int vertexCount = 0;

            if (meshPrimitive.Attributes.POSITION >= 0)
            {
                positionAccessor = gltfObject.accessors[meshPrimitive.Attributes.POSITION];
                positionAccessor.BufferView = gltfObject.bufferViews[positionAccessor.bufferView];
                positionAccessor.BufferView.Buffer = gltfObject.buffers[positionAccessor.BufferView.buffer];
                vertexCount = positionAccessor.count;
            }

            if (meshPrimitive.Attributes.NORMAL >= 0)
            {
                normalsAccessor = gltfObject.accessors[meshPrimitive.Attributes.NORMAL];
                normalsAccessor.BufferView = gltfObject.bufferViews[normalsAccessor.bufferView];
                normalsAccessor.BufferView.Buffer = gltfObject.buffers[normalsAccessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.TEXCOORD_0 >= 0)
            {
                textCoord0Accessor = gltfObject.accessors[meshPrimitive.Attributes.TEXCOORD_0];
                textCoord0Accessor.BufferView = gltfObject.bufferViews[textCoord0Accessor.bufferView];
                textCoord0Accessor.BufferView.Buffer = gltfObject.buffers[textCoord0Accessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.TEXCOORD_1 >= 0)
            {
                textCoord1Accessor = gltfObject.accessors[meshPrimitive.Attributes.TEXCOORD_1];
                textCoord1Accessor.BufferView = gltfObject.bufferViews[textCoord1Accessor.bufferView];
                textCoord1Accessor.BufferView.Buffer = gltfObject.buffers[textCoord1Accessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.TEXCOORD_2 >= 0)
            {
                textCoord2Accessor = gltfObject.accessors[meshPrimitive.Attributes.TEXCOORD_2];
                textCoord2Accessor.BufferView = gltfObject.bufferViews[textCoord2Accessor.bufferView];
                textCoord2Accessor.BufferView.Buffer = gltfObject.buffers[textCoord2Accessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.TEXCOORD_3 >= 0)
            {
                textCoord3Accessor = gltfObject.accessors[meshPrimitive.Attributes.TEXCOORD_3];
                textCoord3Accessor.BufferView = gltfObject.bufferViews[textCoord3Accessor.bufferView];
                textCoord3Accessor.BufferView.Buffer = gltfObject.buffers[textCoord3Accessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.COLOR_0 >= 0)
            {
                colorAccessor = gltfObject.accessors[meshPrimitive.Attributes.COLOR_0];
                colorAccessor.BufferView = gltfObject.bufferViews[colorAccessor.bufferView];
                colorAccessor.BufferView.Buffer = gltfObject.buffers[colorAccessor.BufferView.buffer];
            }

            if (meshPrimitive.indices >= 0)
            {
                indicesAccessor = gltfObject.accessors[meshPrimitive.indices];
                indicesAccessor.BufferView = gltfObject.bufferViews[indicesAccessor.bufferView];
                indicesAccessor.BufferView.Buffer = gltfObject.buffers[indicesAccessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.TANGENT >= 0)
            {
                tangentAccessor = gltfObject.accessors[meshPrimitive.Attributes.TANGENT];
                tangentAccessor.BufferView = gltfObject.bufferViews[tangentAccessor.bufferView];
                tangentAccessor.BufferView.Buffer = gltfObject.buffers[tangentAccessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.WEIGHTS_0 >= 0)
            {
                weight0Accessor = gltfObject.accessors[meshPrimitive.Attributes.WEIGHTS_0];
                weight0Accessor.BufferView = gltfObject.bufferViews[weight0Accessor.bufferView];
                weight0Accessor.BufferView.Buffer = gltfObject.buffers[weight0Accessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.JOINTS_0 >= 0)
            {
                joint0Accessor = gltfObject.accessors[meshPrimitive.Attributes.JOINTS_0];
                joint0Accessor.BufferView = gltfObject.bufferViews[joint0Accessor.bufferView];
                joint0Accessor.BufferView.Buffer = gltfObject.buffers[joint0Accessor.BufferView.buffer];
            }

            if (gltfObject.LoadAsynchronously) { await Awaiters.UnityMainThread; }

            var mesh = new Mesh
            {
                indexFormat = vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16,
            };

            if (positionAccessor != null)
            {
                mesh.vertices = positionAccessor.GetVector3Array();
            }

            if (normalsAccessor != null)
            {
                mesh.normals = normalsAccessor.GetVector3Array();
            }

            if (textCoord0Accessor != null)
            {
                mesh.uv = textCoord0Accessor.GetVector2Array();
            }

            if (textCoord1Accessor != null)
            {
                mesh.uv2 = textCoord1Accessor.GetVector2Array();
            }

            if (textCoord2Accessor != null)
            {
                mesh.uv3 = textCoord2Accessor.GetVector2Array();
            }

            if (textCoord3Accessor != null)
            {
                mesh.uv4 = textCoord3Accessor.GetVector2Array();
            }

            if (colorAccessor != null)
            {
                mesh.colors = colorAccessor.GetColorArray();
            }

            if (indicesAccessor != null)
            {
                mesh.triangles = indicesAccessor.GetIntArray();
            }

            if (tangentAccessor != null)
            {
                mesh.tangents = tangentAccessor.GetVector4Array();
            }

            if (weight0Accessor != null && joint0Accessor != null)
            {
                mesh.boneWeights = CreateBoneWeightArray(joint0Accessor.GetVector4Array(false), weight0Accessor.GetVector4Array(false), vertexCount);
            }

            mesh.RecalculateBounds();
            meshPrimitive.SubMesh = mesh;
            return mesh;
        }

        private static BoneWeight[] CreateBoneWeightArray(Vector4[] joints, Vector4[] weights, int vertexCount)
        {
            NormalizeBoneWeightArray(weights);

            var boneWeights = new BoneWeight[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                boneWeights[i].boneIndex0 = (int)joints[i].x;
                boneWeights[i].boneIndex1 = (int)joints[i].y;
                boneWeights[i].boneIndex2 = (int)joints[i].z;
                boneWeights[i].boneIndex3 = (int)joints[i].w;

                boneWeights[i].weight0 = weights[i].x;
                boneWeights[i].weight1 = weights[i].y;
                boneWeights[i].weight2 = weights[i].z;
                boneWeights[i].weight3 = weights[i].w;
            }

            return boneWeights;
        }

        private static void NormalizeBoneWeightArray(Vector4[] weights)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                var weightSum = weights[i].x + weights[i].y + weights[i].z + weights[i].w;

                if (!Mathf.Approximately(weightSum, 0))
                {
                    weights[i] /= weightSum;
                }
            }
        }
    }
}
