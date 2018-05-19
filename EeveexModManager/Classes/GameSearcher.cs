using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using System.Windows;

using Microsoft.Win32;

using EeveexModManager.Controls;
using EeveexModManager.Interfaces;

namespace EeveexModManager.Classes
{
    public class GameSearcher
    {
        public IGameDefault GameDefault { get; protected set; }

        public string RegistryName { get; protected set; }
        public string InstallationPath { get; protected set; }
        public string ExecutablePath { get; protected set; }

        public Game AssociatedGame { get; protected set; }
        public List<Game> ConfirmedGames { get; protected set; }
        
        public bool Exists { get; protected set; } = false;
        public bool Confirmed { get; protected set; } = false;
        public bool Search { get; set; } = true;

        public GameDetector_Control GuiControl { get; protected set; }

        public GameSearcher(IGameDefault def, GameDetector_Control control, List<Game> games)
        {
            GameDefault = def;
            GuiControl = control;
            ConfirmedGames = games;
        }

        public const int GameStateTextSize = 20;

        public void RestartSearch()
        {
            GuiControl.ResetGUI();
            Search = true;
            Exists = false;
            Confirmed = false;
            StartSearch();
        }

        public void StopSearch()
        {
            Search = false;
            if (!Exists)
            {
                GuiControl.CancelButton.State_ToDisabled();
                GuiControl.ButtonsPanel.Children.Add(new TextBlock()
                {
                    Text = "Game Wasn't Found!",
                    FontSize = GameStateTextSize,
                    Margin = new Thickness(10, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Red
                });
            }

        }

        public void ConfirmGame()
        {
            Confirmed = true;
            AssociatedGame = Game.CreateByName(InstallationPath, RegistryName, GameDefault);
            ConfirmedGames.Add(AssociatedGame);
        }

        public void StartSearch()
        {
            GuiControl.CancelButton.State_ToEnabled();
            if (GameIsInstalled())
            {
                Exists = true;
                GuiControl.FoundGame();
                GuiControl.ButtonsPanel.Children.Add(new TextBlock()
                {
                    Text = "Game Was Found!",
                    Margin = new Thickness(30, 0, 0, 0),
                    FontSize = GameStateTextSize,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Green
                });
            }
            StopSearch();
        }

        public bool GameIsInstalled()
        {
            RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (var item in GameDefault.Registry_Names)
            {
                if (Search)
                {
                    foreach (var regKey in parentKey.GetSubKeyNames().Select(x => parentKey.OpenSubKey(x))
                        .Where(x => x.GetValue("DisplayName")?.ToString().ToLower().Replace(" ", string.Empty) ==
                        item.ToLower().Replace(" ", string.Empty)))
                    {
                        if (Search)
                        {
                            try
                            {
                                InstallationPath = regKey.GetValue("InstallLocation").ToString();
                                RegistryName = item;
                                return true;
                            }
                            catch { }
                        }
                    }
                }
            }
            return false;
        }
        

    }
}
