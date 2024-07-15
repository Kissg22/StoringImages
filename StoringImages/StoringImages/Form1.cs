using StoringImages.Model;
using System;
using System.Windows.Forms;

namespace StoringImages
{
    public partial class Form1 : Form
    {
        private dBHelper dbHelper;
        private ImageHelper imageHelper;
        private string dbFilePath = "ImageLib.s3db"; // SQLite adatbázis elérési útvonala

        private ContextMenuStrip contextMenuStrip1;

        public Form1()
        {
            InitializeComponent();
            dbHelper = new dBHelper(dbFilePath);
            imageHelper = new ImageHelper(dbFilePath);
            LoadImages(); // Képek betöltése az adatbázisból

            // Initialize ContextMenuStrip
            contextMenuStrip1 = new ContextMenuStrip();
            ToolStripMenuItem saveMenuItem = new ToolStripMenuItem("Save As");
            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("Delete");
            ToolStripMenuItem insertMenuItem = new ToolStripMenuItem("New");

            saveMenuItem.Click += contextMenuStrip1_ItemClicked;
            deleteMenuItem.Click += contextMenuStrip1_ItemClicked;
            insertMenuItem.Click += contextMenuStrip1_ItemClicked;

            contextMenuStrip1.Items.Add(saveMenuItem);
            contextMenuStrip1.Items.Add(deleteMenuItem);
            contextMenuStrip1.Items.Add(insertMenuItem);

            // Attach ContextMenuStrip to DataGridView
            dataGridViewImages.ContextMenuStrip = contextMenuStrip1;
        }

        private void LoadImages()
        {
            string commandText = "SELECT * FROM ImageStore";
            if (dbHelper.Load(commandText, "ImageStore"))
            {
                dataGridViewImages.DataSource = dbHelper.DataSet.Tables["ImageStore"];
            }
            else
            {
                MessageBox.Show("Hiba történt a képek betöltése közben.");
            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = sender as ToolStripMenuItem;
            if (clickedItem != null)
            {
                if (clickedItem.Text == "Save As")
                {
                    imageHelper.SaveAsImage((int)dataGridViewImages.SelectedRows[0].Cells["ImageStore_Id"].Value);
                }
                else if (clickedItem.Text == "Delete")
                {
                    imageHelper.DeleteImage((int)dataGridViewImages.SelectedRows[0].Cells["ImageStore_Id"].Value);
                    LoadImages();
                }
                else if (clickedItem.Text == "New")
                {
                    imageHelper.InsertImage();
                    LoadImages();
                }
            }
        }

        private void dataGridViewImages_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Select the row under the mouse pointer
                dataGridViewImages.ClearSelection();
                DataGridView.HitTestInfo hitTestInfo = dataGridViewImages.HitTest(e.X, e.Y);
                if (hitTestInfo.RowIndex >= 0)
                {
                    dataGridViewImages.Rows[hitTestInfo.RowIndex].Selected = true;
                }

                // Show context menu at the mouse location
                contextMenuStrip1.Show(dataGridViewImages, e.Location);
            }
        }
    }
}
