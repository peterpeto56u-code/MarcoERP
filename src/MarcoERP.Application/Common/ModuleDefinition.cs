using System;
using System.Collections.Generic;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.Common
{
    /// <summary>
    /// Phase 8B: Describes a module and its allowed dependencies.
    /// </summary>
    public sealed class ModuleDefinition
    {
        public SystemModule Module { get; }
        public IReadOnlyList<SystemModule> AllowedDependencies { get; }

        public ModuleDefinition(SystemModule module, params SystemModule[] allowedDependencies)
        {
            Module = module;
            AllowedDependencies = Array.AsReadOnly(allowedDependencies);
        }
    }
}
