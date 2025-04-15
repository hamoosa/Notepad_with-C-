using System;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class FindAndReplaceForm : Form
    {
        public string FindWord { get; private set; }
        public string ReplaceWord { get; private set; }

        public FindAndReplaceForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Get the words to find and replace from the text boxes
            FindWord = textBox1.Text;
            ReplaceWord = textBox2.Text;

            // Close the form and return DialogResult.OK
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Close the form and return DialogResult.Cancel
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
