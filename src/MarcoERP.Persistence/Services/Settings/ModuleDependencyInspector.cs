using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MarcoERP.Application.Common;
using MarcoERP.Application.Interfaces.Settings;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Persistence.Services.Settings
{
    /// <summary>
    /// Phase 8D: Reflection-based module dependency inspector.
    /// Scans all classes with [Module] attribute, inspects constructor
    /// parameters, and validates against <see cref="ModuleRegistry"/>.
    /// Report-only — never blocks startup.
    /// </summary>
    public sealed class ModuleDependencyInspector : IModuleDependencyInspector
    {
        // Assemblies to scan for [Module]-decorated classes
        private readonly Assembly[] _assemblies;

        public ModuleDependencyInspector(params Assembly[] assemblies)
        {
            _assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
        }

        /// <inheritdoc />
        public List<ModuleDependencyViolation> ValidateDependencies()
        {
            var violations = new List<ModuleDependencyViolation>();

            // Collect all [Module]-decorated types
            var moduleTypes = _assemblies
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
                })
                .Where(t => t.GetCustomAttribute<ModuleAttribute>() != null)
                .ToList();

            // Build a lookup: interface type → SystemModule (based on the implementing class's [Module])
            var interfaceModuleMap = BuildInterfaceModuleMap(moduleTypes);

            foreach (var type in moduleTypes)
            {
                var attr = type.GetCustomAttribute<ModuleAttribute>();
                var sourceModule = attr.Module;

                var definition = ModuleRegistry.GetDefinition(sourceModule);
                if (definition == null) continue;

                var allowedModules = new HashSet<SystemModule>(definition.AllowedDependencies)
                {
                    sourceModule,        // a module may depend on itself
                    SystemModule.Common  // Common is always allowed
                };

                // Inspect ALL constructor parameters
                var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                foreach (var ctor in ctors)
                {
                    foreach (var param in ctor.GetParameters())
                    {
                        var depModule = ResolveModule(param.ParameterType, interfaceModuleMap);
                        if (depModule == null) continue;              // unknown → skip
                        if (allowedModules.Contains(depModule.Value)) continue; // allowed

                        violations.Add(new ModuleDependencyViolation
                        {
                            ClassName = type.Name,
                            SourceModule = sourceModule.ToString(),
                            DependencyType = param.ParameterType.Name,
                            DependencyModule = depModule.Value.ToString(),
                            Message = $"{type.Name} [{sourceModule}] → {param.ParameterType.Name} [{depModule.Value}]: تبعية غير مصرح بها"
                        });
                    }
                }
            }

            return violations;
        }

        // ──────────────────────────────────────────────
        // Interface → Module resolution
        // ──────────────────────────────────────────────

        /// <summary>
        /// Builds a map from interface types to their module, based on:
        /// 1. The implementing class's [Module] attribute
        /// 2. Namespace-based heuristic as fallback
        /// </summary>
        private static Dictionary<Type, SystemModule> BuildInterfaceModuleMap(
            IReadOnlyList<Type> moduleTypes)
        {
            var map = new Dictionary<Type, SystemModule>();

            foreach (var type in moduleTypes)
            {
                var attr = type.GetCustomAttribute<ModuleAttribute>();
                if (attr == null) continue;

                // Map all interfaces this class implements
                foreach (var iface in type.GetInterfaces())
                {
                    // Skip common framework interfaces
                    if (IsCommonInterface(iface)) continue;

                    if (!map.ContainsKey(iface))
                    {
                        map[iface] = attr.Module;
                    }
                }
            }

            return map;
        }

        /// <summary>
        /// Resolves the SystemModule for a constructor parameter type.
        /// </summary>
        private static SystemModule? ResolveModule(
            Type paramType,
            Dictionary<Type, SystemModule> interfaceModuleMap)
        {
            // 1. Direct map from implementing class's module
            if (interfaceModuleMap.TryGetValue(paramType, out var mapped))
                return mapped;

            // 2. Namespace-based heuristic
            return ResolveModuleByNamespace(paramType);
        }

        /// <summary>
        /// Heuristic: map namespace segments to SystemModule.
        /// Covers repository interfaces, DTOs, validators that
        /// don't have [Module] themselves.
        /// </summary>
        private static SystemModule? ResolveModuleByNamespace(Type type)
        {
            var ns = type.Namespace ?? string.Empty;

            // Cross-cutting / framework types → Common (always allowed)
            if (IsCommonInterface(type))
                return SystemModule.Common;

            // Module-specific namespaces (interfaces, repos, DTOs, validators)
            if (ContainsSegment(ns, "Accounting")) return SystemModule.Accounting;
            if (ContainsSegment(ns, "Inventory"))  return SystemModule.Inventory;
            if (ContainsSegment(ns, "Sales"))      return SystemModule.Sales;
            if (ContainsSegment(ns, "Purchases"))  return SystemModule.Purchases;
            if (ContainsSegment(ns, "Treasury"))   return SystemModule.Treasury;
            if (ContainsSegment(ns, "Reports"))    return SystemModule.Reporting;
            if (ContainsSegment(ns, "Security"))   return SystemModule.Security;
            if (ContainsSegment(ns, "Settings"))   return SystemModule.Settings;
            if (ContainsSegment(ns, "Governance")) return SystemModule.Governance;

            // FluentValidation IValidator<T> — resolve by generic argument
            if (type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("IValidator"))
            {
                var innerType = type.GetGenericArguments().FirstOrDefault();
                if (innerType != null)
                    return ResolveModuleByNamespace(innerType);
            }

            return null; // Unknown — will be skipped
        }

        /// <summary>
        /// Returns true for cross-cutting interfaces that every module may use.
        /// </summary>
        private static bool IsCommonInterface(Type type)
        {
            var name = type.Name;
            var ns = type.Namespace ?? string.Empty;

            // Common domain/application interfaces
            if (name == "IUnitOfWork" ||
                name == "ICurrentUserService" ||
                name == "IDateTimeProvider" ||
                name == "IAuditLogger" ||
                name == "IServiceProvider" ||
                name == "IJournalNumberGenerator")
                return true;

            // FluentValidation
            if (ns.StartsWith("FluentValidation"))
                return true;

            // System / Microsoft namespaces
            if (ns.StartsWith("System") || ns.StartsWith("Microsoft"))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if namespace contains a segment (e.g., ".Sales." or ends with ".Sales").
        /// </summary>
        private static bool ContainsSegment(string ns, string segment)
            => ns.Contains($".{segment}.") ||
               ns.EndsWith($".{segment}") ||
               ns.Contains($".{segment},");
    }
}
