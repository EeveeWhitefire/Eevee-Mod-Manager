using EeveexModManager.Define;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EeveexModManager.Controls
{
    public class ShapedButton<T> : Button where T : Shape
    {

        ControlTemplate template;
        FrameworkElementFactory circle;
        FrameworkElementFactory presenter;
        FrameworkElementFactory grid;

        #region Image States
        protected Image Img_Enabled;

        protected Image Img_Disabled;

        protected Image Img_MouseOver;
        #endregion

        public ShapedButton(double radX, double radY, string baseTextureName) : base()
        {
            #region Image State Constructors
            Img_MouseOver = new Image()
            {
                Width = radX,
                Height = radY,
                Source = new BitmapImage(new Uri($"pack://application:,,,/EeveexModManager;component/Resources/{baseTextureName}_Hover.png", UriKind.Absolute)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Img_Disabled = new Image()
            {
                Width = radX,
                Height = radY,
                Source = new BitmapImage(new Uri($"pack://application:,,,/EeveexModManager;component/Resources/{baseTextureName}_Disabled.png", UriKind.Absolute)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Img_Enabled = new Image()
            {
                Width = radX,
                Height = radY,
                Source = new BitmapImage(new Uri($"pack://application:,,,/EeveexModManager;component/Resources/{baseTextureName}.png", UriKind.Absolute)),
                VerticalAlignment = VerticalAlignment.Center
            };
            #endregion

            template = new ControlTemplate(typeof(Button));
            // Create the circle
            circle = new FrameworkElementFactory(typeof(T));
            circle.SetValue(Shape.WidthProperty, radX);
            circle.SetValue(Shape.HeightProperty, radY);
            //circle.SetValue(Button.ClickModeProperty, ClickMode);

            // Create the ContentPresenter to show the Button.Content
            presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(ContentProperty));
            presenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            presenter.Name = $"button{new Random().Next(1, 15)}";


            // Create the Grid to hold both of the elements
            grid = new FrameworkElementFactory(typeof(Grid));
            grid.AppendChild(circle);
            grid.AppendChild(presenter);

            // Set the Grid as the ControlTemplate.VisualTree
            template.VisualTree = grid;

            Setter hoverSetter = new Setter()
            {
                Property = ContentProperty,
                Value = Img_MouseOver,
                TargetName = presenter.Name
            };
            Trigger hoverTrigger = new Trigger()
            {
                Property = IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(hoverSetter);

            template.Triggers.Add(hoverTrigger);

            Template = template;

            State_ToEnabled();
        }

        public void State_ToHover()
        {
            Content = Img_MouseOver;
        }

        public void State_ToEnabled()
        {
            if(Visibility == Visibility.Hidden)
            {
                Visibility = Visibility.Visible;
            }
            Content = Img_Enabled;
            IsEnabled = true;
        }
        public void State_ToDisabled()
        {
            Content = Img_Disabled;
            IsEnabled = false;
        }
    }
}
