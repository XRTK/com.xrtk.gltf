// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Attributes;
using XRTK.Definitions.CameraSystem;
using XRTK.Definitions.Platforms;
using XRTK.Interfaces.CameraSystem;
using XRTK.Providers.CameraSystem;

namespace XRTK.ProjectName.Providers.CameraSystem
{
    [RuntimePlatform(typeof(ProjectNamePlatform))]
    [System.Runtime.InteropServices.Guid("#INSERT_GUID_HERE#")]
    public class ProjectNameCameraDataProvider : BaseCameraDataProvider
    {
        /// <inheritdoc />
        public ProjectNameCameraDataProvider(string name, uint priority, BaseMixedRealityCameraDataProviderProfile profile, IMixedRealityCameraSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }
    }
}