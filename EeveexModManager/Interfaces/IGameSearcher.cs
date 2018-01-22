using EeveexModManager.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EeveexModManager.Interfaces
{
    public interface IGameSearcher
    {
        string Name { get; }
        TextBox SearchTextBox { get; }
        string RegistryName { get; }
        string InstallationPath { get; }

        bool Exists { get; }
        bool Confirmed { get; }
        bool Search { get; }
    }
}
