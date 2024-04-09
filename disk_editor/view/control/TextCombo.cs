using System.Windows;
using System.Windows.Controls;

namespace disk_editor
{
    public class TextCombo : ComboBox
    {
        public CharacterCasing? CharacterCasing
        {
            get { return GetValue(CharacterCasingProperty) as CharacterCasing?; }
            set { SetValue(CharacterCasingProperty, value); }
        }
        public static readonly DependencyProperty CharacterCasingProperty =
            DependencyProperty.Register("CharacterCasing",
                                        typeof(CharacterCasing?),
                                        typeof(TextCombo));

        public double? TextboxWidth
        {
            get { return GetValue(TextboxWidthProperty) as double?; }
            set { SetValue(TextboxWidthProperty, value); }
        }
        public static readonly DependencyProperty TextboxWidthProperty =
            DependencyProperty.Register("TextboxWidth",
                                        typeof(double?),
                                        typeof(TextCombo));

        static TextCombo()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TextCombo),
                new FrameworkPropertyMetadata(typeof(TextCombo))
            );
        }
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TextComboItem();
        }
    }

    public class TextComboItem : ComboBoxItem
    {
        static TextComboItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TextComboItem),
                new FrameworkPropertyMetadata(typeof(TextComboItem))
            );
        }
    }
}