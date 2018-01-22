using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace EeveexModManager.Controls
{
    public class SquareButton : Button
    {

        public SquareButton(double radX, double radY, Image hover) : base()
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            // Create the circle
            FrameworkElementFactory circle = new FrameworkElementFactory(typeof(Rectangle));
            circle.SetValue(Rectangle.WidthProperty, radX);
            circle.SetValue(Rectangle.HeightProperty, radY);
            //circle.SetValue(Button.ClickModeProperty, ClickMode);

            // Create the ContentPresenter to show the Button.Content
            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(ContentProperty));
            presenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            presenter.Name = $"button{new Random().Next(1, 15)}";

            Setter hoverSetter = new Setter()
            {
                Property = ContentProperty,
                Value = hover,
                TargetName = presenter.Name
            };
            Trigger hoverTrigger = new Trigger()
            {
                Property = IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(hoverSetter);

            // Create the Grid to hold both of the elements
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
            grid.AppendChild(circle);
            grid.AppendChild(presenter);

            // Set the Grid as the ControlTemplate.VisualTree
            template.VisualTree = grid;
            template.Triggers.Add(hoverTrigger);

            Template = template;
        }
    }
}
