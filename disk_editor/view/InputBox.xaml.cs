using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;

namespace disk_editor
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Fluent.RibbonWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Answer
        {
            get { return GetValue(AnswerProperty) as string; }
            set { SetValue(AnswerProperty, value); }
        }
        public static readonly DependencyProperty AnswerProperty =
            DependencyProperty.Register("Answer",
                                        typeof(string),
                                        typeof(InputBox));


        private RelayCommand _cancel_command;
        public ICommand cancel_command
        {
            get
            {
                if (_cancel_command == null)
                {
                    _cancel_command = new RelayCommand(x => on_cancel_click(), x => true);
                }

                return _cancel_command;
            }
        }

        private RelayCommand _submit_command;
        public ICommand submit_command
        {
            get
            {
                if (_submit_command == null)
                {
                    _submit_command = new RelayCommand(x => on_submit_click(), x => true);
                }

                return _submit_command;
            }
        }

        public void on_cancel_click()
        {
            this.close_window();
        }

        public void on_submit_click()
        {
            this.Answer = this.string_input.Text;

            this.close_window();
        }

        public void close_window()
        {
            this.Close();
        }

        public string ShowDialog()
        {
            base.ShowDialog();

            return this.Answer;
        }

        public void window_loaded(Object sender, RoutedEventArgs e)
        {
            
        }

        public InputBox(string question_text = "Enter your answer:", string title_text = "Please answer", string initial_value = "")
        {
            InitializeComponent();

            this.Title = title_text;
            this.question_text.Content = question_text;
            this.string_input.Text = initial_value;

            this.Answer = null;

            this.DataContext = this;

            this.Loaded += window_loaded;
        }
    }
}
