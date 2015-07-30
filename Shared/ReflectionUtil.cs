using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class ReflectionUtil
    {
        public static object LoadClass(string className, params object[] args)
        {
            int namespaceSeperatorPos = className.IndexOf('.');
            if (namespaceSeperatorPos == -1) throw new Exception("Root class namespace must be the name of the containing assembly. Provided name: " + className);

            string assemblyFile = className.Substring(0, namespaceSeperatorPos) + ".dll";
            assemblyFile = Path.Combine(IOUtil.GetModuleDirectory("Shared.dll"), assemblyFile);

            ObjectHandle handle = Activator.CreateInstanceFrom(assemblyFile, className, true, 0, null, args, null, null);
            return handle.Unwrap();
        }
    }
}
