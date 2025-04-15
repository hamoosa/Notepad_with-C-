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
        public System.Drawing.FontStyle newFontStyle;


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
            richTextBox.MouseClick += richTextBox_MouseClick;
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
        //private void OpenTextFile()
        //{
        //    // Show OpenFileDialog to choose a text file
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
        //    if (openFileDialog.ShowDialog() == DialogResult.OK)
        //    {
        //        // Read the content of the selected text file
        //        string fileName = openFileDialog.FileName;
        //        string fileContent = File.ReadAllText(fileName);

        //        // Create a new tab page and display the content
        //        AddNewTabPage();
        //        RichTextBox richTextBox = tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls[0] as RichTextBox;
        //        if (richTextBox != null)
        //        {
        //            richTextBox.Text = fileContent;
        //            tabControl1.TabPages[tabControl1.TabPages.Count - 1].Tag = fileName; // Store the file path for future save operations
        //        }
        //    }
        //}
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
            ToggleHeader();
        }

        private void addFooterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleFooter();
        }
        private void ToggleHeader()
        {
            if (tabControl1.SelectedTab != null)
            {
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                if (richTextBox != null)
                {
                    if (!isHeaderVisible)
                    {
                        // Calculate the position for the header line
                        int lineHeight = TextRenderer.MeasureText("A", richTextBox.Font).Height; // Height of one line
                        int headerPositionY = richTextBox.GetPositionFromCharIndex(richTextBox.GetFirstCharIndexFromLine(0)).Y + (5 * lineHeight); // Y-coordinate of the position of the header line, 5 lines after the beginning
                        int headerLineWidth = richTextBox.ClientSize.Width; // Width of the line

                        // Add the header line
                        using (Graphics g = richTextBox.CreateGraphics())
                        {
                            using (Pen pen = new Pen(Color.Gray))
                            {
                                g.DrawLine(pen, 0, headerPositionY, headerLineWidth, headerPositionY);
                            }
                        }

                        isHeaderVisible = true;
                    }
                    else
                    {
                        // Remove the header line (no action needed as it's just a visual line)

                        isHeaderVisible = false;
                    }
                }
            }
        }



        private void ToggleFooter()
        {
            if (tabControl1.SelectedTab != null)
            {
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                if (richTextBox != null)
                {
                    if (!isFooterVisible)
                    {
                        // Calculate the position for the footer line
                        int lineHeight = TextRenderer.MeasureText("A", richTextBox.Font).Height; // Height of one line
                        int numLines = richTextBox.GetLineFromCharIndex(richTextBox.Text.Length) + 1; // Total number of lines
                        int footerPositionY = richTextBox.GetPositionFromCharIndex(richTextBox.GetFirstCharIndexFromLine(numLines - 1)).Y + (3 * lineHeight); // Y-coordinate of the position of the footer line, 3 lines after the end
                        int footerLineWidth = richTextBox.ClientSize.Width; // Width of the line

                        // Add the footer line
                        using (Graphics g = richTextBox.CreateGraphics())
                        {
                            using (Pen pen = new Pen(Color.Gray))
                            {
                                g.DrawLine(pen, 0, footerPositionY, footerLineWidth, footerPositionY);
                            }
                        }

                        isFooterVisible = true;
                    }
                    else
                    {
                        // Remove the footer line (no action needed as it's just a visual line)

                        isFooterVisible = false;
                    }
                }
            }
        }
        //private void richTextBox_MouseDoubleClick(object sender, MouseEventArgs e)
        //{
        //    RichTextBox richTextBox = sender as RichTextBox;
        //    if (richTextBox != null)
        //    {
        //        // Get the line index
        //        int lineIndex = richTextBox.GetLineFromCharIndex(richTextBox.GetCharIndexFromPosition(e.Location));

        //        // Get the character index at the beginning of the clicked line
        //        int lineStartIndex = richTextBox.GetFirstCharIndexFromLine(lineIndex);

        //        // Calculate the offset from the beginning of the line
        //        int offset = richTextBox.GetPositionFromCharIndex(lineStartIndex).X;

        //        // Calculate the character index based on the offset and click position
        //        int charIndex = lineStartIndex + richTextBox.GetCharIndexFromPosition(new Point(offset + e.X, e.Y));

        //        // Set the selection start to the calculated character index
        //        richTextBox.SelectionStart = charIndex;
        //    }
        //}
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
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            // Backup the current font style
            FontStyle currentStyle = richTextBox.SelectionFont.Style;

            // Check what we want to toggle
            if (styleToToggle == FontStyle.Bold)
            {
                // Check if bold is off
                if (richTextBox.SelectionFont.Bold == false)
                {
                    // Add bold 
                    currentStyle |= FontStyle.Bold;
                }
                else
                {
                    // Turn off bold
                    currentStyle &= ~FontStyle.Bold;
                }
            }
            else if (styleToToggle == FontStyle.Italic)
            {
                // Check if Italic is off
                if (richTextBox.SelectionFont.Italic == false)
                {
                    // Add Italic 
                    currentStyle |= FontStyle.Italic;
                }
                else
                  {
                    // Turn off Italic
                    currentStyle &= ~FontStyle.Italic;
                  }
            }
            else if (styleToToggle == FontStyle.Underline)
            {
                // Check if Underline is off
                if (richTextBox.SelectionFont.Underline == false)
                {
                    // Add Underline 
                    currentStyle |= FontStyle.Underline;
                }
                else
                {
                    // Turn off Underline
                    currentStyle &= ~FontStyle.Underline;
                }
            }
            else if (styleToToggle == FontStyle.Strikeout)
            {
                // Check if Strikeout is off
                if (richTextBox.SelectionFont.Strikeout == false)
                {
                    // Add Strikeout 
                    currentStyle |= FontStyle.Strikeout;
                }
                else
                {
                    // Turn off Strikeout
                    currentStyle &= ~FontStyle.Strikeout;
                }
            }

            // Replace the current font with the new style
            richTextBox.SelectionFont = new Font(richTextBox.SelectionFont, currentStyle);
        }


        private void tspFormat_Panter_Click(object sender, EventArgs e)
        {
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            if (richTextBox.SelectedText == "")
            {
                MessageBox.Show("page not added yet");
                tspFormat_Panter.Checked = false;
                return;
            }


            if (richTextBox.SelectionFont != null)
            {
                currentFont = richTextBox.SelectionFont;

                if (richTextBox.SelectionFont.Bold == true)
                {
                    newFontStyle = FontStyle.Bold;
                }
                else if (richTextBox.SelectionFont.Italic == true)
                {
                    newFontStyle = FontStyle.Italic;
                }
                else
                {
                    newFontStyle = FontStyle.Regular;
                }
            }
        }
        private void richTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (tspFormat_Panter.Checked == false) return;
            RichTextBox richTextBox = sender as RichTextBox;
            if (richTextBox != null)
            {
                var start = richTextBox.SelectionStart;
                int startIndex = 0, wordLength = 0;
                void getWordAtIndex(int index, out int starting, out int ending)
                {
                    string wordSeparators = " .,;-!?\r\n\"";
                    int cp0 = index;
                    int cp2 = richTextBox.Find(wordSeparators.ToCharArray(), index);

                    for (int c = index; c > 0; c--)
                    { if (wordSeparators.Contains(richTextBox.Text[c])) { cp0 = c + 1; break; } }
                    int l = cp2 - cp0;
                    starting = cp0;
                    ending = l;
                }
                getWordAtIndex(start, out startIndex, out wordLength);
                richTextBox.Select(startIndex, wordLength);
                richTextBox.SelectionFont = new Font(
                    currentFont.FontFamily,
                    currentFont.Size,
                    newFontStyle
                );
                tspFormat_Panter.Checked = false;
            }
        }

        private void TsbBold_Click(object sender, EventArgs e)
        {
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            if ( richTextBox.SelectionFont.Bold == false)
            {
                richTextBox.SelectionFont = new Font(richTextBox.SelectionFont, FontStyle.Bold);

            }
            else
            {
                richTextBox.SelectionFont = new Font(richTextBox.SelectionFont, FontStyle.Regular);
            }
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void TsbItalic_Click(object sender, EventArgs e)
        {
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            if (richTextBox.SelectionFont.Italic == false)
            {
                richTextBox.SelectionFont = new Font(richTextBox.SelectionFont, FontStyle.Italic);

            }
            else
            {
                richTextBox.SelectionFont = new Font(richTextBox.SelectionFont, FontStyle.Regular);
            }
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
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                if (richTextBox != null)
                {
                    richTextBox.SelectionBackColor = colorDialog1.Color; // Set background color
                    richTextBox.SelectionColor = richTextBox.ForeColor; // Set text color
                }
            }
        }

        private void Tsbfontcolor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog1 = new ColorDialog();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                if (richTextBox != null)
                {
                    richTextBox.SelectionColor = colorDialog1.Color;
                }
            }
        }

        private void TsbPaste_Click(object sender, EventArgs e)
        {
            RichTextBox richTextBox = tabControl1.SelectedTab.Controls[0] as RichTextBox;

            {
                richTextBox.Paste();
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
    }
}
