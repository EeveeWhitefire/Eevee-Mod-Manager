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
        Action<Game> ChangeGame;
        Action Utility;
        List<Db_Game> Games;

        public GamePicker_ComboBox(IEnumerable<Db_Game> games, double imageSize, double fontSize, Action OnUtility, Action<Game> onValueChange = null) : base()
        {
            Games = games.ToList();
            foreach (var game in Games.Select(x => x.EncapsulateToSource()))
            {
                Items.Add(new GamePicker_Control(game, imageSize, fontSize));
            }

            Items.Add(new TextBlock()
            {
                Text = "Run Game Detection",
                FontSize = 20,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            });

            SelectedIndex = Games.FindIndex(x => x.IsCurrent);
            Utility = OnUtility;

            SelectionChanged += new SelectionChangedEventHandler(SelectionChanged_Event);
            ChangeGame = onValueChange;
        }

        void SelectionChanged_Event(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            if(index + 1 == (sender as ComboBox).Items.Count)
            {
                Utility();
            }
            else
            {
                ChangeGame(Games[index].EncapsulateToSource());
            }
        }
    }
}
