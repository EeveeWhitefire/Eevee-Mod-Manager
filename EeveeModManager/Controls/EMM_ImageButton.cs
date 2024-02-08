using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EeveexModManager.Controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:EeveexModManager.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:EeveexModManager.Controls;assembly=EeveexModManager.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:EMM_Button/>
    ///
    /// </summary>
    public class EMM_ImageButton : Button
    {
        [Bindable(true)]
        public Brush MouseOverDesign
        {
            get { return (Brush)GetValue(MouseOverDesignProperty); }
            set { SetValue(MouseOverDesignProperty, value); }
        }
        [Bindable(true)]
        public Brush MouseNotOverDesign
        {
            get { return (Brush)GetValue(MouseNotOverDesignProperty); }
            set { SetValue(MouseNotOverDesignProperty, value); }
        }

        [Bindable(true)]
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(MouseNotOverDesignProperty); }
            set { SetValue(MouseNotOverDesignProperty, value); }
        }


        public static readonly DependencyProperty MouseOverDesignProperty =
            DependencyProperty.Register("MouseOverDesign", typeof(Brush), typeof(EMM_ImageButton));
        public static readonly DependencyProperty MouseNotOverDesignProperty =
            DependencyProperty.Register("MouseNotOverDesign", typeof(Brush), typeof(EMM_ImageButton));
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(EMM_ImageButton));

        static EMM_ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EMM_ImageButton), new FrameworkPropertyMetadata(typeof(EMM_ImageButton)));
        }
    }
}
