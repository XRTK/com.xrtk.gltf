// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Attributes;
using XRTK.Definitions.CameraSystem;
using XRTK.Definitions.Platforms;
using XRTK.Interfaces.CameraSystem;
using XRTK.Providers.CameraSystem;

namespace XRTK.glTF.Providers.CameraSystem
{
    [RuntimePlatform(typeof(glTFPlatform))]
    [System.Runtime.InteropServices.Guid("0e52b086-18fe-46f5-aef6-7391f3223210")]
    public class glTFCameraDataProvider : BaseCameraDataProvider
    {
        /// <inheritdoc />
        public glTFCameraDataProvider(string name, uint priority, BaseMixedRealityCameraDataProviderProfile profile, IMixedRealityCameraSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }
    }
}