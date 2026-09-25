using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method)] sealed class ModuleInitializerAttribute : Attribute { }
}

static class Startup
{
    [ModuleInitializer]
    internal static void Init()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
            var requested = new System.Reflection.AssemblyName(e.Name).Name;
            // Gib die bereits geladene Assembly gleichen Namens zurück (Version egal)
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == requested);
        };
    }
}
