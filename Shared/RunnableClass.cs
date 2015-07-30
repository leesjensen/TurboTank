using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// In addition to implementing this interface an ExecutableClass must impelement a constructor that takes a single DynObject parameter.
    /// </summary>
    public interface IRunnableClass
    {
        void Run();
        void RunAsync();
        void Stop();
    }

}
