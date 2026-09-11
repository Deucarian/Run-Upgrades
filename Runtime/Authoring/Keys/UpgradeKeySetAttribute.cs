using System;

namespace Deucarian.RunUpgrades
{
    /// <summary>Marks an authoritative set of named UpgradeKey fields or properties for the Inspector.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class UpgradeKeySetAttribute : Attribute { }
}
