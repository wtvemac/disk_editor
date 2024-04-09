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
using System.Windows.Threading;
using System.Windows.Forms;
using System.Threading;

namespace disk_editor
{
    /// <summary>
    /// Interaction logic for WaitMessage.xaml
    /// </summary>
    public partial class WaitMessage : Window
    {
        private delegate bool close_call();
        private ThreadStart initial_action = null;
        private bool initial_auto_close = true;
        private bool initial_dispatcher_invoke = false;
        private int action_count = 0;

        public void do_action(ThreadStart action, bool auto_close = false, bool dispatcher_invoke = false)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));

            Thread action_thread = new Thread(() =>
            {
                if (dispatcher_invoke)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, action);
                }
                else
                {
                    action();
                }

                if (auto_close)
                {
                    this.close_window();
                }
            });
            action_thread.Start();
        }

        public void window_loaded(object sender, EventArgs e)
        {
            if(initial_action != null)
            {
                this.do_action(initial_action, initial_auto_close, initial_dispatcher_invoke);
            }
        }

        public void Go(ThreadStart action, bool auto_close = false, bool dispatcher_invoke = false, bool modal_dialog = false)
        {
            this.action_count += 1;

            if(this.action_count > 1)
            {
                this.do_action(action, auto_close, dispatcher_invoke);
            }
            else
            {
                this.initial_action = action;
                this.initial_auto_close = auto_close;
                this.initial_dispatcher_invoke = dispatcher_invoke;

                if(modal_dialog)
                {
                    this.ShowDialog();
                }
                else
                {
                    this.Show();
                }
            }
        }

        public bool close_window()
        {
            this.action_count -= 1;

            if (this.action_count <= 0)
            {
                if (this.Dispatcher.CheckAccess() == false)
                {
                    var cb = new close_call(this.close_window);

                    this.Dispatcher.Invoke(cb);
                }
                else
                {
                    this.Close();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public WaitMessage(string wait_message, Window owning_window = null)
        {
            InitializeComponent();

            if(owning_window != null)
            {
                if (owning_window.IsVisible)
                {
                    this.Owner = owning_window;
                }
                else
                {
                    this.Owner = owning_window.Owner;
                }
            }

            this.Loaded += this.window_loaded;

            this.wait_message.Content = wait_message;
        }
    }
}
