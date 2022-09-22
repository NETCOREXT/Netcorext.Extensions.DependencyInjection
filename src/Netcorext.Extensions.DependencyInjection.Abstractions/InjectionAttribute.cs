using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InjectionAttribute : Attribute
{
    public InjectionAttribute() { }

    public InjectionAttribute(int index)
    {
        Index = index;
    }

    [Description("Sortting Index")]
    [DefaultValue(0)]
    public int Index { get; set; }
}