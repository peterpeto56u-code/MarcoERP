using System;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.Common
{
    /// <summary>
    /// Phase 8C: Declares which <see cref="SystemModule"/> a class belongs to.
    /// Used by the Dependency Inspector (Phase 8D) for boundary validation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute
    {
        public SystemModule Module { get; }

        public ModuleAttribute(SystemModule module)
        {
            Module = module;
        }
    }
}
