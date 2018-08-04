﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;

using ApplicationSettings = EeveexModManager.Classes.ApplicationSettings;
using System.Windows.Media;

namespace EeveexModManager
{
    public class Defined
    {
        public const int MODPICKINGBUTTON_SIZE = 40;
        public const string DEFAULTMODVERSION = "1.0.0";
        public const string DEFAULTMODID = "Unknown";
        public const string DEFAULTMODAUTHOR = "Unknown";
        public const string DEFAULTSOURCEURI = "Unknown";
        public const string NEXUSAPI_BASE = @"https://api.nexusmods.com/v1";
        public const string NAMED_PIPE_CLIENT_EXECUTABLE = "named-pipe-client.exe";
        public const string NAMED_PIPE_NAME = "EeveeModManager";
        public const string REGISTRY_KEY_NAME = "Eevee";
        public const string DEFAULT_MODS_VIEW_SEARCHER_MESSAGE = "Enter mod name here...";
        public const int MAX_GAME_DETECTORS_IN_COLUMN = 4;
        public const int GAME_OPTION_IMAGE_SIZE = 80;
        public static ApplicationSettings Settings = new ApplicationSettings();

        public class Colors
        {
            private const string _lightBlue = "#FF218FEE";
            private const string _darkBlue = "#FF155D99";
            private const string _green = "#FF00FF21";
            private const string _red = "#FFFF0000";
            private const string _yellow = "#FFFFD800";
            private const string _background = "#FF373737";
            private const string _disabled = "#FF808080";

            public static Brush LightBlue = (Brush)(new BrushConverter().ConvertFrom(_lightBlue));
            public static Brush DarkBlue = (Brush)(new BrushConverter().ConvertFrom(_darkBlue));
            public static Brush DisabledGrey = (Brush)(new BrushConverter().ConvertFrom(_disabled));
            public static Brush Green = (Brush)(new BrushConverter().ConvertFrom(_green));
            public static Brush Yellow = (Brush)(new BrushConverter().ConvertFrom(_yellow));
            public static Brush Red = (Brush)(new BrushConverter().ConvertFrom(_red));
        }
    }

    public static class ExtensionMethods
    {
        public static bool IsEmpty(this string str)
            => str.Length == 0;

        public static bool IsEmpty<T>(this IEnumerable<T> collec)
            => collec.Count() == 0;
    }

    public class Assistant
    {
        public static List<string> GetAllFilesInDir(string d)
        {
            List<string> files = new List<string>();

            ProcessDirectory(ref files, d);

            return files;
        }
        public static void ProcessDirectory(ref List<string> files, string p)
        {
            files.AddRange(Directory.GetFiles(p));

            var dirs = Directory.GetDirectories(p);
            foreach (var item in dirs)
            {
                ProcessDirectory(ref files, item);
            }
        }

        public static Image LoadGameImage(string id, double size)
            => new Image()
            {
                Width = size,
                Source = LoadImageFromResources("Icon - " + id + ".png"),
                Height = size,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

        public static BitmapImage LoadImageFromResources(string filename)
        {
            return new BitmapImage(new Uri($"pack://application:,,,/EeveexModManager;component/Resources/{filename}", UriKind.Absolute));
        }
    }
    public class MultiplicationMathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)value;
            double percentage = System.Convert.ToDouble(parameter);
            int res = (int)((double)percentage * v);
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class AdditionMathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double a = (double)value;
            double b = System.Convert.ToDouble(parameter);
            int res = (int)(a + b);
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
