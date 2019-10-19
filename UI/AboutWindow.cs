using System;
using System.Windows.Forms;

namespace MinishMaker.UI
{
    public partial class AboutWindow : Form
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void linkLabel1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/minishmaker/minishmaker");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void linkLabel2_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://minishmaker.com");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
