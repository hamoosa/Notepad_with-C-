using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp;

namespace notepad
{
    public partial class Form1 : Form
    {
        private int tabPageCount = 0;
        private string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // Get desktop folder path
        private bool isHeaderVisible = false;
        private bool isFooterVisible = false;
        public System.Drawing.Font currentFont;
        public Color currentForeColor;
        public Color currentBackColor;

        public Form1()
        {
            InitializeComponent();
        }
        private void UpdateWordCount(TabPage tabPage)
        {
            RichTextBox richTextBox = tabPage.Controls[0] as RichTextBox;
            Label wordCountLabel = tabPage.Controls.OfType<Label>().FirstOrDefault();
            if (richTextBox != null && wordCountLabel != null)
            {
                string[] words = richTextBox.Text.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                int wordCount = words.Length;
                wordCountLabel.Text = "Word count: " + wordCount.ToString();
            }
        }
        private void RichTextBox_TextChanged(object sender, EventArgs e)
        {
            RichTextBox richTextBox = (RichTextBox)sender;
            TabPage tabPage = (TabPage)richTextBox.Parent;
            UpdateWordCount(tabPage);
        }

        private void findWordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Find and replace words in the current tab's content
            FindAndReplace();
        }
        private void addPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewTabPage();
        }

        private void deletePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count > 0)
            {
                tabControl1.TabPages.RemoveAt(tabControl1.SelectedIndex);
                tabPageCount--;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveCurrentTabPageContent();
        }
        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            // Open an existing text file and display its content in a new tab
            OpenFolder();
        }
        //save As button
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Prompt user to choose a folder where the text files will be saved
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                folderPath = folderBrowserDialog1.SelectedPath;
                SaveCurrentTabPageContent(true);
            }
        }
        private void AddNewTabPage()
        {
            TabPage tabPage = new TabPage("" + (tabPageCount + 1));
            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Dock = DockStyle.Fill;
            richTextBox.TextChanged += RichTextBox_TextChanged;
            //richTextBox.MouseDoubleClick += richTextBox_MouseDoubleClick;
            richTextBox.MouseUp += richTextBox_MouseUp;
            richTextBox.KeyDown += richTextBox_KeyDown;
            tabPage.Controls.Add(richTextBox);
            Label wordCountLabel = new Label();
            wordCountLabel.Text = "Word count: 0";
            wordCountLabel.Dock = DockStyle.Bottom;
            tabPage.Controls.Add(wordCountLabel);

            tabControl1.TabPages.Add(tabPage);
            tabPageCount++;
        }
        private void SaveCurrentTabPageContent(bool saveAs = false)
        {
            if (tabControl1.TabCount > 0)
            {
                foreach (TabPage tabPage in tabControl1.TabPages)
                {
                    RichTextBox richTextBox = tabPage.Controls[0] as RichTextBox;
                    if (richTextBox != null)
                    {
                        string fileName;
                        if (!saveAs && tabPage.Tag != null) // If Save is clicked and a file has been previously saved
                        {
                            fileName = tabPage.Tag.ToString(); // Use the existing file path
                        }
                        else
                        {
                            // Prompt user to choose a file name
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                            saveFileDialog.InitialDirectory = folderPath; // Set initial directory to the chosen folder
                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                fileName = saveFileDialog.FileName;
                                tabPage.Tag = fileName; // Store the file path for future save operations
                            }
                            else
                            {
                                return;
                            }
                        }

                        // Save the content to the file
                        File.WriteAllText(fileName, richTextBox.Text);
                    }
                }
            }
            else
            {
                MessageBox.Show("No tab pages to save.");
            }
        }
        
        private void OpenFolder()
        {
            // Show FolderBrowserDialog to choose a folder
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                // Clear existing tabs
                tabControl1.TabPages.Clear();
                tabPageCount = 0;

                // Get the selected folder path
                string selectedFolder = folderBrowserDialog1.SelectedPath;

                // Get all text files in the selected folder
                string[] files = Directory.GetFiles(selectedFolder, "*.txt");
                foreach (string file in files)
                {
                    // Read the content of each text file
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string fileContent = File.ReadAllText(file);

                    // Create a new tab page and display the content
                    AddNewTabPage();
                    RichTextBox richTextBox = tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls[0] as RichTextBox;
                    if (richTextBox != null)
                    {
                        richTextBox.Text = fileContent;
                        tabControl1.TabPages[tabControl1.TabPages.Count - 1].Tag = file; // Store the file path for future save operations
                        tabControl1.TabPages[tabControl1.TabPages.Count - 1].Text = fileName; // Set the tab title to the file name
                        UpdateWordCount(tabControl1.TabPages[tabControl1.TabPages.Count - 1]); // Update word count label
                    }
                }
            }
        }
        private void FindAndReplace()
        {
            if (tabControl1.TabCount > 0)
            {
                // Show FindAndReplaceForm to input find and replace words
                using (FindAndReplaceForm findAndReplaceForm = new FindAndReplaceForm())
                {
                    if (findAndReplaceForm.ShowDialog() == DialogResult.OK)
                    {
                        string findWord = findAndReplaceForm.FindWord;
                        string replaceWord = findAndReplaceForm.ReplaceWord;

                        RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                        
                       
                         if (richTextBox != null)
                        {
                            MessageBox.Show("found and replaced ");
                            string content = richTextBox.Text;
                            content = content.Replace(findWord, replaceWord);
                            richTextBox.Text = content;
                        }

                    }
                }
            }
            else
            {
                MessageBox.Show("No tab pages to find and replace.");
            }
        }

        private void boldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatting(FontStyle.Bold);
        }

        private void italicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatting(FontStyle.Italic);
        }
        private void ApplyFormatting(FontStyle style)
        {
            if (tabControl1.SelectedTab != null)
            {
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                if (richTextBox != null)
                {
                    // Apply the specified formatting to the selected text
                    richTextBox.SelectionFont = new Font(richTextBox.Font, richTextBox.SelectionFont.Style ^ style);
                }
            }
        }

        private void underlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatting(FontStyle.Underline);
        }

        private void fontSizeToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem != null && e.ClickedItem.Tag != null)
            {
                int fontSize = (int)e.ClickedItem.Tag;
                ApplyFontSize(fontSize);
            }
        }
        private void ApplyFontSize(int size)
        {
            if (tabControl1.SelectedTab != null)
            {
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                if (richTextBox != null)
                {
                    // Get the current font style of the selected text
                    FontStyle fontStyle = richTextBox.SelectionFont.Style;

                    // Create a new font with the specified size and existing style
                    Font newFont = new Font(richTextBox.Font.FontFamily, size, fontStyle);

                    // Apply the new font to the selected text
                    richTextBox.SelectionFont = newFont;
                }
            }
        }

        private void styleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
           
            {
                richTextBox.SelectionFont = new Font(fontDialog1.Font.FontFamily, fontDialog1.Font.Size, fontDialog1.Font.Style);
            }
            
        }

        private void cutCtrlXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            
            {
                richTextBox.Cut();
            }
        }

        private void copyCtrlCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
           
            {
                richTextBox.Copy();
            }
        }

        private void addHeaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 addHeaderForm = new Form2();
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            if (addHeaderForm.ShowDialog() == DialogResult.OK)
            {
                string headerText = addHeaderForm.HeaderText;

                // Calculate the width of the visible area of the RichTextBox
                int visibleWidth = richTextBox.Size.Width;

                // Adjust for scrollbar, if visible
                if (richTextBox.ClientSize.Width < richTextBox.ClientRectangle.Width)
                {
                    visibleWidth -= SystemInformation.VerticalScrollBarWidth;
                }

                // Insert the header text and horizontal line above the existing text
                richTextBox.SelectionStart = 0;
                richTextBox.SelectedText = Environment.NewLine + Environment.NewLine + headerText 
                    + Environment.NewLine + Environment.NewLine + new string('_', visibleWidth / 6 - 1);
            }
        }

        private void addFooterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 footerForm = new Form2();
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            if (footerForm.ShowDialog() == DialogResult.OK)
            {
                string headerText = footerForm.HeaderText;

                // Calculate the width of the visible area of the RichTextBox
                int visibleWidth = richTextBox.Size.Width;

                // Adjust for scrollbar, if visible
                if (richTextBox.ClientSize.Width < richTextBox.ClientRectangle.Width)
                {
                    visibleWidth -= SystemInformation.VerticalScrollBarWidth;
                }

                // Insert the header text and horizontal line above the existing text
                richTextBox.SelectionStart = richTextBox.TextLength;
                richTextBox.SelectedText = Environment.NewLine + new string('_', visibleWidth / 6 - 1) +
                    Environment.NewLine + headerText + Environment.NewLine;
            }
        }
        
        private void richTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                RichTextBox richTextBox = sender as RichTextBox;
                if (richTextBox != null && richTextBox.TextLength > 0)
                {
                    // Get the current line index
                    int currentLineIndex = richTextBox.GetLineFromCharIndex(richTextBox.SelectionStart);

                    // Check if there is a next line
                    if (currentLineIndex < richTextBox.Lines.Length - 1)
                    {
                        // Get the character index at the beginning of the next line
                        int nextLineStartIndex = richTextBox.GetFirstCharIndexFromLine(currentLineIndex + 1);

                        // Set the selection start to the beginning of the next line
                        richTextBox.SelectionStart = nextLineStartIndex;

                        // Cancel the event to prevent the default behavior of the down arrow key
                        e.Handled = true;
                    }
                }
            }
        }

        private void PasteCtrlVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;

            {
                richTextBox.Paste();
            }
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;

            {
                richTextBox.Undo();
            }
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;

            {
                richTextBox.Redo();
            }
        }

        private void ToolStripMenExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ToolStripMenPrintPrev_Click(object sender, EventArgs e)
        {
           
        }

        private void ToolStripMenSelectAll_Click(object sender, EventArgs e)
        {
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;

            {
                richTextBox.SelectAll();
            }
        }
        void ToggleStyle(FontStyle styleToToggle)
        {
            try {
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                richTextBox.SelectionFont = new Font(richTextBox.SelectionFont, richTextBox.SelectionFont.Style ^ styleToToggle);

                }
            catch
            {
                MessageBox.Show("page not added yet");
                tspFormat_Panter.Checked = false;
                return;
            }

        }


        private void tspFormat_Panter_Click(object sender, EventArgs e)
        {
            try
            {
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                if (richTextBox.SelectedText != "")
                {
                    currentFont = richTextBox.SelectionFont;
                    currentForeColor = richTextBox.SelectionColor;
                    currentBackColor = richTextBox.SelectionBackColor;
                }
                else
                {
                    MessageBox.Show("Select a text to copy the format of first");
                    tspFormat_Panter.Checked = false;
                    return;
                }
            }
            catch
            {
                MessageBox.Show("page not added yet");
                tspFormat_Panter.Checked = false;
                return;
            }
        }
        private void richTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (tspFormat_Panter.Checked == false) return;
            //RichTextBox richTextBox = sender as RichTextBox;
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;

            richTextBox.SelectionFont = new Font(
                currentFont.FontFamily,
                currentFont.Size,
                currentFont.Style
            );
            richTextBox.SelectionBackColor = currentBackColor; // Set background color
            richTextBox.SelectionColor = currentForeColor; // Set text color
            tspFormat_Panter.Checked = false;
        }


        private void TsbBold_Click(object sender, EventArgs e)
        {
            ToggleStyle(FontStyle.Bold);
   
   
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void TsbItalic_Click(object sender, EventArgs e)
        {
            ToggleStyle(FontStyle.Italic);
        }

        private void Tsbstrickout_Click(object sender, EventArgs e)
        {
            ToggleStyle(FontStyle.Strikeout);
        }

        private void TsUnderline_Click(object sender, EventArgs e)
        {
            ToggleStyle(FontStyle.Underline);
        }

        private void TsbHighlight_Click(object sender, EventArgs e)
        {
            
            ColorDialog colorDialog1 = new ColorDialog();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
            
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                if (richTextBox != null)
                {
                    richTextBox.SelectionBackColor = colorDialog1.Color; // Set background color
                }

                }
                catch
                {
                    MessageBox.Show("page not added yet");
                    tspFormat_Panter.Checked = false;
                    return;
                }

            }
        }

        private void Tsbfontcolor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog1 = new ColorDialog();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
              try
              { 
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                if (richTextBox != null)
                {
                    richTextBox.SelectionColor = colorDialog1.Color;
                }
              }
                catch
                {
                    MessageBox.Show("page not added yet");
                    tspFormat_Panter.Checked = false;
                    return;
                }

            }
        }

        private void TsbPaste_Click(object sender, EventArgs e)
        {
            try
            {
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;

                {
                    richTextBox.Paste();
                }
            }

            catch
            {
                MessageBox.Show("page not added yet");
                tspFormat_Panter.Checked = false;
                return;
            }
        }

        private void TsbNewPage_Click(object sender, EventArgs e)
        {
            AddNewTabPage();
        }

        private void TsbOpen_Click(object sender, EventArgs e)
        {
            OpenFolder();
        }

        private void TsbSave_Click(object sender, EventArgs e)
        {
            SaveCurrentTabPageContent();
        }

        private void TsbfontSize_Click(object sender, EventArgs e)
        {
            
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                int newSize;
                if (Int32.TryParse(comboBox1.Text, out newSize))
                {
                    // Create a new Font with the specified size and the same family and style as the current font
                    Font newFont = new Font(richTextBox.SelectionFont.FontFamily, newSize, richTextBox.SelectionFont.Style);

                    // Apply the new font to the selected text
                    richTextBox.SelectionFont = newFont;
                }

            }
            catch
            {
                MessageBox.Show("page not added yet");
                tspFormat_Panter.Checked = false;
                return;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddNewTabPage();
        }

        private void textWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
             RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
           
                if (textWrapToolStripMenuItem.Checked == true)
                {
                    textWrapToolStripMenuItem.Checked = false;
                    richTextBox.WordWrap = false;
                }
                else
                {
                textWrapToolStripMenuItem.Checked = true;
                    richTextBox.WordWrap = true;
                }
           
        }
    }
}
