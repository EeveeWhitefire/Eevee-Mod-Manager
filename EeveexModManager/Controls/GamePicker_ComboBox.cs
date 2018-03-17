using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;

using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;
using System;

namespace EeveexModManager.Controls
{
    public class GamePicker_ComboBox : ComboBox
    {
        private Action<Game> _changeGame;
        private Action _utility;
        public List<Db_Game> Games { get; protected set; }

        private double _imageSize, _fontSize;

        public GamePicker_ComboBox(double imageSize, double fontSize, Action OnUtility = null, Action<Game> onValueChange = null)
        {
            if (OnUtility != null)
            {
                Items.Add(new TextBlock()
                {
                    Text = "Search for games...",
                    FontSize = 25,
                    Height = 50,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left
                });
            }
            _utility = OnUtility;

            SelectionChanged += new SelectionChangedEventHandler(SelectionChanged_Event);
            _changeGame = onValueChange;
            Games = new List<Db_Game>();

            _imageSize = imageSize;
            _fontSize = fontSize;
        }

        public GamePicker_ComboBox(IEnumerable<Db_Game> games, double imageSize, double fontSize, Action OnUtility = null, Action<Game> onValueChange = null) : 
            this(imageSize, fontSize, OnUtility, onValueChange)
        {
            AddGames(games);
        }

        public void AddGames(IEnumerable<Db_Game> games)
        {
            Games.AddRange(games.ToList());
            foreach (var game in Games.Select(x => x.EncapsulateToSource()))
            {
                Items.Add(new GamePicker_Control(game, _imageSize, _fontSize));
            }
            //SelectedIndex = Games.FindIndex(x => x.IsCurrent) + 1;
        }

        public void AddGame(Game g)
        {
            Games.Add(g.EncapsulateToDb());
            Items.Add(new GamePicker_Control(g, _imageSize, _fontSize));
        }

        void SelectionChanged_Event(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            if(index == 0 && _utility != null)
            {
                _utility();
            }
            else
            {
                _changeGame?.Invoke(Games[index - 1].EncapsulateToSource());
            }
        }
    }
}
