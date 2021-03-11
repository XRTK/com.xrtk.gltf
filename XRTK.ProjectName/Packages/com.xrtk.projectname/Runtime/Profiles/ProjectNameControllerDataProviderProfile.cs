// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;
using XRTK.Definitions.Utilities;
using XRTK.ProjectName.Providers.Controllers;

namespace XRTK.ProjectName.Profiles
{
    public class ProjectNameControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        public override ControllerDefinition[] GetDefaultControllerOptions()
        {
            return new[]
            {
                new ControllerDefinition(typeof(ProjectNameController), Handedness.Left),
                new ControllerDefinition(typeof(ProjectNameController), Handedness.Right)
            };
        }
    }
}
