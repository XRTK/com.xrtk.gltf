// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;
using XRTK.Definitions.Utilities;
using XRTK.glTF.Providers.Controllers;

namespace XRTK.glTF.Profiles
{
    public class glTFControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        public override ControllerDefinition[] GetDefaultControllerOptions()
        {
            return new[]
            {
                new ControllerDefinition(typeof(glTFController), Handedness.Left),
                new ControllerDefinition(typeof(glTFController), Handedness.Right)
            };
        }
    }
}
