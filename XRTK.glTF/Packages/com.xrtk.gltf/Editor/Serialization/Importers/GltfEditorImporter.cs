#if UNITY_2020_3_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

using System.IO;
using UnityEditor;
using UnityEngine;

namespace XRTK.Utilities.Gltf.Serialization.Importers
{
    public static class GltfEditorImporter
    {
        public static async void OnImportGltfAsset(AssetImportContext context)
        {
            var importedObject = await GltfUtility.ImportGltfObjectFromPathAsync(context.assetPath);

            if (importedObject == null ||
                importedObject.GameObjectReference == null)
            {
                Debug.LogError("Failed to import glTF object");
                return;
            }

            var gltfAsset = (GltfAsset)ScriptableObject.CreateInstance(typeof(GltfAsset));

            gltfAsset.GltfObject = importedObject;
            gltfAsset.name = $"{gltfAsset.GltfObject.Name}{Path.GetExtension(context.assetPath)}";
            gltfAsset.Model = importedObject.GameObjectReference;
            context.AddObjectToAsset("main", gltfAsset.Model);
            context.SetMainObject(importedObject.GameObjectReference);
            context.AddObjectToAsset("glTF data", gltfAsset);

            var reImport = false;

            for (var i = 0; i < gltfAsset.GltfObject.textures?.Length; i++)
            {
                var gltfTexture = gltfAsset.GltfObject.textures[i];

                if (gltfTexture == null) { continue; }

                var path = AssetDatabase.GetAssetPath(gltfTexture.Texture);

                if (string.IsNullOrWhiteSpace(path))
                {
                    var textureName = gltfTexture.name;

                    if (string.IsNullOrWhiteSpace(textureName))
                    {
                        textureName = $"Texture_{i}";
                        gltfTexture.Texture.name = textureName;
                    }

                    context.AddObjectToAsset(textureName, gltfTexture.Texture);
                }
                else
                {
                    if (gltfTexture.Texture.isReadable) { continue; }

                    var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    Debug.Assert(textureImporter != null);
                    textureImporter.isReadable = true;
                    textureImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                    {
                        format = TextureImporterFormat.RGBA32
                    });
                    textureImporter.SaveAndReimport();
                    reImport = true;
                }
            }

            if (reImport)
            {
                var importer = AssetImporter.GetAtPath(context.assetPath);
                importer.SaveAndReimport();
                AssetDatabase.SaveAssets();
                return;
            }

            for (var i = 0; i < gltfAsset.GltfObject.meshes?.Length; i++)
            {
                var gltfMesh = gltfAsset.GltfObject.meshes[i];

                var meshName = string.IsNullOrWhiteSpace(gltfMesh.name) ? $"Mesh_{i}" : gltfMesh.name;

                gltfMesh.Mesh.name = meshName;
                context.AddObjectToAsset($"{meshName}", gltfMesh.Mesh);
            }

            if (gltfAsset.GltfObject.materials != null)
            {
                foreach (var gltfMaterial in gltfAsset.GltfObject.materials)
                {
                    context.AddObjectToAsset(gltfMaterial.name, gltfMaterial.Material);
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
