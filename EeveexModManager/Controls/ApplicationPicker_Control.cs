using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EeveexModManager.Controls
{
    public class ApplicationPicker_Control : StackPanel
    {
        private IconExtractor _iconExtractor;
        public GameApplication App { get; }
        public ApplicationPicker_Control(GameApplication app) : base()
        {
            Orientation = Orientation.Horizontal;

            _iconExtractor = new IconExtractor(app.ExecutablePath);
            int iconCount = _iconExtractor.Count;
            System.Drawing.Icon[] splitIcons = IconUtil.Split(_iconExtractor.GetIcon(0));
            ImageSource imageSource;

            System.Drawing.Bitmap bitmap;
            try
            {
                int c = 0;
                do
                {
                    bitmap = IconUtil.ToBitmap(splitIcons[c]);
                    c++;
                }
                while (bitmap.Size.Width < 256 && c >= 0 && c < splitIcons.Length);
            }
            catch (Exception)
            {

                throw;
            }
            var stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            imageSource = BitmapFrame.Create(stream);

            Children.Add(new Image()
            {
                Source = imageSource,
                VerticalAlignment = VerticalAlignment.Center,
                MaxHeight = 75,
                MaxWidth = 75,
                Margin = new Thickness(0, 0, 20, 0)
            });

            Children.Add(new TextBlock()
            {
                Text = app.Name,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 25
            });
            App = app;
        }
    }
}
