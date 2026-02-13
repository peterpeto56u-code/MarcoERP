using System.Collections.Generic;
using MarcoERP.Application.Common;

namespace MarcoERP.Application.Interfaces.Settings
{
    /// <summary>
    /// Phase 8D: Reflection-based module dependency analyzer.
    /// Inspects [Module] attributes and constructor dependencies to
    /// detect boundary violations against <see cref="ModuleRegistry"/>.
    /// </summary>
    public interface IModuleDependencyInspector
    {
        /// <summary>
        /// Validates all module dependencies.
        /// Returns a list of violation descriptions (empty = clean).
        /// </summary>
        List<ModuleDependencyViolation> ValidateDependencies();
    }

    /// <summary>
    /// A single boundary violation detected by the inspector.
    /// </summary>
    public sealed class ModuleDependencyViolation
    {
        public string ClassName { get; set; }
        public string SourceModule { get; set; }
        public string DependencyType { get; set; }
        public string DependencyModule { get; set; }
        public string Message { get; set; }
    }
}
