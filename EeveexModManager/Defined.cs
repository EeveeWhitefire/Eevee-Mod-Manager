using System;
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
    public static class Defined
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
        public const string EMM_VERSION = "v0.0.0";
        public const string TITLE = "Eevee Mod Manager " + EMM_VERSION;
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
            private const string _alternateBackground = "#FF7C7C7C";

            public static Brush LightBlue = (Brush)(new BrushConverter().ConvertFrom(_lightBlue));
            public static Brush DarkBlue = (Brush)(new BrushConverter().ConvertFrom(_darkBlue));
            public static Brush DisabledGrey = (Brush)(new BrushConverter().ConvertFrom(_disabled));
            public static Brush Green = (Brush)(new BrushConverter().ConvertFrom(_green));
            public static Brush Yellow = (Brush)(new BrushConverter().ConvertFrom(_yellow));
            public static Brush Red = (Brush)(new BrushConverter().ConvertFrom(_red));
            public static Brush Background = (Brush)(new BrushConverter().ConvertFrom(_background));
            public static Brush AlternateBackground = (Brush)(new BrushConverter().ConvertFrom(_alternateBackground));
        }
    }

    public static class ExtensionMethods
    {
        public static bool IsEmpty(this string str)
            => str.Length == 0;

        public static bool IsEmpty<T>(this IEnumerable<T> collec)
            => collec.Count() == 0;
    }

    public class BinaryNodeChildren<T> : List<BinaryNode<T>>
    {
        public BinaryNode<T> Parent { get; protected set; }
        public BinaryNode<T> Head { get; protected set; }
        public BinaryNodeChildren(BinaryNode<T> p, BinaryNode<T> h)
        {
            Parent = p;
            Head = h;
        }

        public BinaryNode<T> Get(T v)
        {
            if (Parent.Exists(v))
            {
                foreach (var node in this)
                {
                    var res = node.Get(v);
                    if (res != null)
                        return res;
                }
                return null;
            }
            else return null;
        }

        public new void Clear()
        {
            Parent = null;
            foreach (var node in this)
            {
                node.Clear();
            }
            base.Clear();
        }

        public void ChangeParent(BinaryNode<T> n)
        {
            Parent = n;
            foreach (var node in this)
            {
                node.ChangeParent(n);
            }
        }

        public void ChangeHead(BinaryNode<T> n)
        {
            if (Head.Equals(Parent))
                ChangeParent(n);
            else
                Head.Children.ChangeParent(n);
            Head = n;
        }
    }
    public class BinaryNode
    {
        public static BinaryNode<string> GetDirectoryTree(string dir, BinaryNode<string> s = null)
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            BinaryNode<string> curr;
            if (s != null)
                curr = new BinaryNode<string>(info.Name, s, s.Head, false);
            else
                curr = new BinaryNode<string>(info.Name, s, false);

            curr.SetChildren(info.GetFiles().Select(x => new BinaryNode<string>(x.Name, curr, curr.Head)));
            foreach (var folder in info.GetDirectories())
            {
                curr.Insert(GetDirectoryTree(folder.FullName, curr));
            }

            return curr;
        }
    }
    public class BinaryNode<T> : BinaryNode
    {
        public BinaryNode<T> Parent { get; protected set; } = null;
        public BinaryNodeChildren<T> Children { get; protected set; } = null;
        public bool IsStandalone
        {
            get;
            protected set;
        } = false;
        public T Value { get; protected set; }
        public int Count
        {
            get { return Children != null ? Children.Count : 0; }
        }
        public BinaryNode<T> Head { get; protected set; }

        public BinaryNode(T value, bool isStandalone = true)
        {
            Value = value;
            Head = this;
            Parent = null;
            if(!isStandalone)
                Children = new BinaryNodeChildren<T>(this, Head);

        }
        public BinaryNode(T value, BinaryNode<T> p, bool isStandalone = true) : this(value, isStandalone)
        {
            Parent = p;
        }
        public BinaryNode(T value, BinaryNode<T> p, BinaryNode<T> head, bool isStandalone = true) : this(value, p, isStandalone)
        {
            Head = head;
        }
        public void Insert(T v)
        {
            Insert(new BinaryNode<T>(v, this, Head));
        }
        public void Insert(BinaryNode<T> n)
        {
            if (!IsStandalone)
            {
                Children = Children ?? new BinaryNodeChildren<T>(this, Head);
                Children.Add(n);
            }
        }

        public void ToggleIsStandalone()
        {
            IsStandalone = !IsStandalone;
            if (IsStandalone)
            {
                Clear();
                Children = null;
            }
            else
            {
                Children = new BinaryNodeChildren<T>(this, Head);
            }
        }

        public void SetChildren(IEnumerable<BinaryNode<T>> ch)
        {
            IsStandalone = false;
            Children.ChangeParent(null);
            Children.ChangeHead(null);
            Children = new BinaryNodeChildren<T>(this, Head);
            Children.AddRange(ch);
            Children.ChangeHead(Head);
            Children.ChangeParent(this);
        }

        public void MoveChildren(BinaryNode<T> n)
        {
            if (!IsStandalone)
            {
                Children.ChangeParent(n);
                Children = new BinaryNodeChildren<T>(this, Head);
            }
        }

        public void Clear()
        {
            if(!IsStandalone)
            {
                Children.Clear();
            }
        }
        
        public override string ToString()
            => ToStringIndented();

        public string ToStringIndented(int tabs = 0)
        {
            if (IsStandalone)
                return $"{new String('\t', tabs)}*{Value}";
            else if (Count == 0)
                return $"{new String('\t', tabs)}|{Value}|";
            else 
                return $"{new String('\t', tabs)}|{Value}:\n{string.Join("\n", Children.Select(x => x.ToStringIndented(tabs + 1)))}\n{new String('\t', tabs) }|";
        }

        public void ChangeParent(BinaryNode<T> n)
        {
            Parent = n;
        }

        public bool Exists(T v)
        {
            return Value.Equals(v) || Children.Count(x => x.Exists(v)) > 0;
        }

        public BinaryNode<T> Get(T v)
        {
            if (Exists(v))
            {
                if (Value.Equals(v))
                    return this;
                else
                    return Children.Get(v);
            }
            else
                return null;
        }

        public void ChangeHead(BinaryNode<T> n)
        {
            if (!Head.Equals(this))
            {
                Head = n;
            }
            Children.ChangeHead(n);
        }
    }

    public class Assistant
    {
        public static IList<string> GetAllFilesInDir(string d)
        {
            List<string> files = new List<string>();

            ProcessDirectory(files, d);

            return files;
        }
        public static void ProcessDirectory(List<string> files, string p)
        {
            files.AddRange(Directory.GetFiles(p));

            var dirs = Directory.GetDirectories(p);
            foreach (var item in dirs)
            {
                ProcessDirectory(files, item);
            }
        }

        public static ImageSource LoadGameImage(string id)
            => LoadImageFromResources("Icon - " + id + ".png");

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
