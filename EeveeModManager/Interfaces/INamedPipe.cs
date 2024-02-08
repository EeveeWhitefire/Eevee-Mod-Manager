using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Interfaces
{
    public interface INamedPipe
    {
        string Name { get; }
    }
}
