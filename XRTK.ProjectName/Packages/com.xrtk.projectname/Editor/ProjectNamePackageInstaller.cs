// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using UnityEditor;
using XRTK.Editor;
using XRTK.Editor.Utilities;
using XRTK.Extensions;

namespace XRTK.ProjectName.Editor
{
    [InitializeOnLoad]
    internal static class ProjectNamePackageInstaller
    {
        private static readonly string DefaultPath = $"{MixedRealityPreferences.ProfileGenerationPath}ProjectName";
        private static readonly string HiddenPath = Path.GetFullPath($"{PathFinderUtility.ResolvePath<IPathFinder>(typeof(ProjectNamePathFinder)).ToForwardSlashes()}\\{MixedRealityPreferences.HIDDEN_PROFILES_PATH}");

        static ProjectNamePackageInstaller()
        {
            if (!EditorPreferences.Get($"{nameof(ProjectNamePackageInstaller)}", false))
            {
                EditorPreferences.Set($"{nameof(ProjectNamePackageInstaller)}", PackageInstaller.TryInstallAssets(HiddenPath, DefaultPath));
            }
        }
    }
}
