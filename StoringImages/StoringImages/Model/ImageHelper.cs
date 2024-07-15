using System;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Data.SQLite;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;


namespace StoringImages.Model
{
    public class ImageHelper
    {
        private dBHelper helper = null;
        private string fileLocation = string.Empty;
        private bool isSucces = false;
        private int maxImageSize = 2097152;

        // Add a constructor that takes a string parameter
        public ImageHelper(string dbPath)
        {
            // Initialize dBHelper with the provided database path
            helper = new dBHelper(dbPath);
        }

        private string FileLocation
        {
            get { return fileLocation; }
            set
            {
                fileLocation = value;
            }
        }

        public Boolean GetSucces()
        {
            return isSucces;
        }

        private Image LoadImage()
        {
            // Initialize image object
            Image image = null;

            // Open file dialog for selecting an image
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = @"C:\";
            dlg.Title = "Select Image File";
            dlg.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.gif;*.tiff;*.nef)|*.jpg;*.jpeg;*.png;*.gif;*.tiff;*.nef";

            // Show the dialog and get the selected file location
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.FileLocation = dlg.FileName;

                // Check if a valid file location is selected
                if (!string.IsNullOrEmpty(FileLocation) && File.Exists(FileLocation))
                {
                    Cursor.Current = Cursors.WaitCursor;

                    // Get file information and calculate the filesize
                    FileInfo info = new FileInfo(FileLocation);
                    long fileSize = info.Length;
                    maxImageSize = (int)fileSize; // Adjusted casting to int

                    // Retrieve image data and create Image object
                    using (FileStream stream = File.Open(FileLocation, FileMode.Open))
                    {
                        BinaryReader br = new BinaryReader(stream);
                        byte[] data = br.ReadBytes(maxImageSize);
                        image = new Image(dlg.SafeFileName, data, fileSize);
                    }

                    Cursor.Current = Cursors.Default;
                }
            }

            return image;
        }

        public Int32 InsertImage()
        {
            DataRow dataRow = null;
            isSucces = false;
            Image image = LoadImage();
            //if no file was selected and no image was created return 0
            if (image == null) return 0;
            if (image != null)
            {
                // Determin the ConnectionString
                string connectionString = dbFunctions.ConnectionStringSQLite;
                // Determin the DataAdapter = CommandText + Connection
                string commandText = "SELECT * FROM ImageStore WHERE 1=0";
                // Make a new object
                helper = new dBHelper(connectionString);
                {
                    // Load Data
                    if (helper.Load(commandText, "image_id") == true)
                    {
                        // Add a row and determin the row
                        helper.DataSet.Tables[0].Rows.Add(
                        helper.DataSet.Tables[0].NewRow());
                        dataRow = helper.DataSet.Tables[0].Rows[0];
                        // Enter the given values
                        dataRow["imageFileName"] = image.FileName;
                        dataRow["imageBlob"] = image.ImageData;
                        dataRow["imageFileSizeBytes"] = image.FileSize;
                        try
                        {
                            // Save -> determin succes
                            if (helper.Save() == true)
                            {
                                isSucces = true;
                            }
                            else
                            {
                                isSucces = false;
                                MessageBox.Show("Error during Insertion");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Show the Exception --> Dubbel Id/Name ?
                            MessageBox.Show(ex.Message);
                        }
                    }//END IF
                }
            }
            //return the new image_id
            return Convert.ToInt32(dataRow[0].ToString());
        }

        public void DeleteImage(Int32 imageID)
        {
            //Set variables
            isSucces = false;
            // Determin the ConnectionString
            string connectionString = dbFunctions.ConnectionStringSQLite;
            // Determin the DataAdapter = CommandText + Connection
            string commandText = "SELECT * FROM ImageStore WHERE image_id=" + imageID;
            // Make a new object
            helper = new dBHelper(connectionString);
            {
                // Load Data
                if (helper.Load(commandText, "image_id") == true)
                {
                    // Determin if the row was found
                    if (helper.DataSet.Tables[0].Rows.Count == 1)
                    {
                        // Found, delete row
                        helper.DataSet.Tables[0].Rows[0].Delete();
                        try
                        {
                            // Save -> determin succes
                            if (helper.Save() == true)
                            {
                                isSucces = true;
                            }
                            else
                            {
                                isSucces = false;
                                MessageBox.Show("Delete failed");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Show the Exception --> Dubbel ContactId/Name ?
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
        }

        public void SaveAsImage(Int32 imageID)
        {
            DataRow dataRow = null;
            Image image = null;
            bool isSuccess = false;

            // Display SaveFileDialog for user to select save location
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = @"C:\";
            dlg.Title = "Save Image File";

            // Setting filters for various image formats
            dlg.Filter = "Tag Image File Format (*.tiff)|*.tiff"
                       + "|Graphics Interchange Format (*.gif)|*.gif"
                       + "|Portable Network Graphic Format (*.png)|*.png"
                       + "|Joint Photographic Experts Group Format (*.jpg;*.jpeg)|*.jpg;*.jpeg"
                       + "|Bitmap Image File Format (*.bmp)|*.bmp"
                       + "|Nikon Electronic Format (*.nef)|*.nef";

            // Show the SaveFileDialog and proceed if the user selects a file
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;

                // Ensure only one of the specified extensions is used, or add default if none match
                string defaultExt = ".png";
                bool extFound = false;
                foreach (string ext in new string[] { ".tiff", ".gif", ".png", ".jpg", ".jpeg", ".bmp", ".nef" })
                {
                    if (dlg.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        extFound = true;
                        break;
                    }
                }
                if (!extFound)
                {
                    dlg.FileName += defaultExt;
                }

                // Get connection string and command text
                string connectionString = dbFunctions.ConnectionStringSQLite;
                string commandText = "SELECT * FROM ImageStore WHERE image_id=" + imageID;

                // Create dbHelper instance
                helper = new dBHelper(connectionString);

                // Load data from database
                if (helper.Load(commandText, ""))
                {
                    // Get data row and create Image object
                    dataRow = helper.DataSet.Tables[0].Rows[0];
                    image = new Image(
                        (string)dataRow["imageFileName"],
                        (byte[])dataRow["imageBlob"],
                        (long)dataRow["imageFileSizeBytes"]
                    );

                    // Save image using FileStream
                    using (FileStream stream = new FileStream(dlg.FileName, FileMode.Create))
                    {
                        BinaryWriter bw = new BinaryWriter(stream);
                        bw.Write(image.ImageData);
                        isSuccess = true;
                    }
                }

                Cursor.Current = Cursors.Default;
            }

            // Display success or failure message
            if (isSuccess)
            {
                MessageBox.Show("Save successful");
            }
            else
            {
                MessageBox.Show("Save failed");
            }
        }


    }
}
