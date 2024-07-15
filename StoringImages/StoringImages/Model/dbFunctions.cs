using System;

namespace StoringImages.Model
{
    public class dbFunctions
    {
        public static string ConnectionStringSQLite
        {
            get
            {
                string database = @"ImageLib.s3db";
                string connectionString = $"Data Source={database};Version=3;";
                return connectionString;
            }
        }
    }
}
