// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Attributes;
using XRTK.Definitions.Platforms;
using XRTK.Interfaces.InputSystem;
using XRTK.ProjectName.Profiles;
using XRTK.Providers.Controllers;

namespace XRTK.ProjectName.Providers.Controllers
{
    [RuntimePlatform(typeof(ProjectNamePlatform))]
    [System.Runtime.InteropServices.Guid("#INSERT_GUID_HERE#")]
    public class ProjectNameControllerDataProvider : BaseControllerDataProvider
    {
        /// <inheritdoc />
        public ProjectNameControllerDataProvider(string name, uint priority, ProjectNameControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }
    }
}
