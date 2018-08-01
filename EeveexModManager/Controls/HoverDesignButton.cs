using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EeveexModManager.Controls
{
    public class HoverDesignButton : Button
    {
        [Bindable(true)]
        public BitmapImage MouseOverDesign
        {
            get { return (BitmapImage)GetValue(MouseOverDesignProperty); }
            set { SetValue(MouseOverDesignProperty, value); }
        }
        [Bindable(true)]
        public BitmapImage MouseNotOverDesign
        {
            get { return (BitmapImage)GetValue(MouseNotOverDesignProperty); }
            set { SetValue(MouseNotOverDesignProperty, value); }
        }
        [Bindable(true)]
        public BitmapImage CurrentDesign
        {
            get { return (BitmapImage)GetValue(CurrentDesignProperty); }
            set { SetValue(CurrentDesignProperty, value); }
        }
        [Bindable(true)]
        public BitmapImage DisabledDesign
        {
            get { return (BitmapImage)GetValue(DisabledDesignProperty); }
            set { SetValue(DisabledDesignProperty, value); }
        }

        public static readonly DependencyProperty MouseOverDesignProperty =
            DependencyProperty.Register("MouseOverDesign", typeof(BitmapImage), typeof(HoverDesignButton));
        public static readonly DependencyProperty MouseNotOverDesignProperty =
            DependencyProperty.Register("MouseNotOverDesign", typeof(BitmapImage), typeof(HoverDesignButton));
        public static readonly DependencyProperty CurrentDesignProperty =
            DependencyProperty.Register("MouseNotOverDesign", typeof(BitmapImage), typeof(HoverDesignButton));
        public static readonly DependencyProperty DisabledDesignProperty =
            DependencyProperty.Register("DisabledDesign", typeof(BitmapImage), typeof(HoverDesignButton));

        public HoverDesignButton() : base()
        {
        }

        public void ChangeState(bool newState)
        {
            IsEnabled = newState;
            if(!newState)
            {
                CurrentDesign = DisabledDesign;
            }
        }
    }
}
