// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Attributes;
using XRTK.Definitions.Platforms;
using XRTK.Interfaces.InputSystem;
using XRTK.glTF.Profiles;
using XRTK.Providers.Controllers;

namespace XRTK.glTF.Providers.Controllers
{
    [RuntimePlatform(typeof(glTFPlatform))]
    [System.Runtime.InteropServices.Guid("d9ea4042-8f55-462f-90d9-23f6ec1c405e")]
    public class glTFControllerDataProvider : BaseControllerDataProvider
    {
        /// <inheritdoc />
        public glTFControllerDataProvider(string name, uint priority, glTFControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }
    }
}
