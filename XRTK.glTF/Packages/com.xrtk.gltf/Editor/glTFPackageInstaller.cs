// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using UnityEditor;
using XRTK.Editor;
using XRTK.Editor.Utilities;
using XRTK.Extensions;

namespace XRTK.glTF.Editor
{
    [InitializeOnLoad]
    internal static class glTFPackageInstaller
    {
        private static readonly string DefaultPath = $"{MixedRealityPreferences.ProfileGenerationPath}glTF";
        private static readonly string HiddenPath = Path.GetFullPath($"{PathFinderUtility.ResolvePath<IPathFinder>(typeof(glTFPathFinder)).ToForwardSlashes()}\\{MixedRealityPreferences.HIDDEN_PROFILES_PATH}");

        static glTFPackageInstaller()
        {
            if (!EditorPreferences.Get($"{nameof(glTFPackageInstaller)}", false))
            {
                EditorPreferences.Set($"{nameof(glTFPackageInstaller)}", PackageInstaller.TryInstallAssets(HiddenPath, DefaultPath));
            }
        }
    }
}
