using System;
using System.Data;
using System.Data.SqlClient;


namespace TraineeService
{
    public class BackupManager
    {
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TraineeDB;Integrated Security=True";

        public void BackupTraineeTable()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Create backup table if not exists
                    string createBackupTable = @"
                    IF OBJECT_ID('TraineesBackup', 'U') IS NULL
                    BEGIN
                        CREATE TABLE TraineesBackup (
                            TraineeID INT PRIMARY KEY,
                            Name NVARCHAR(100),
                            Email NVARCHAR(100) UNIQUE,
                            PhoneNumber NVARCHAR(20),
                            Department NVARCHAR(100),
                            JoiningDate DATE,
                            Gender NVARCHAR(100),
                            Photo VARBINARY(MAX)
                        )
                    END";
                    using (SqlCommand command = new SqlCommand(createBackupTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }


                    // 2. Get Trainees data
                    SqlDataAdapter dataAdapter = new SqlDataAdapter("SELECT * FROM Trainees", connection);
                    DataTable dataTrainees = new DataTable();
                    dataAdapter.Fill(dataTrainees);

                    
                    foreach (DataRow row in dataTrainees.Rows)
                    {
                        int traineeId = Convert.ToInt32(row["TraineeID"]);
                        string name = row["Name"].ToString();
                        string email = row["Email"].ToString();
                        string phone = row["PhoneNumber"].ToString();
                        string department = row["Department"].ToString();
                        DateTime joiningDate = Convert.ToDateTime(row["JoiningDate"]);
                        string gender = row["Gender"].ToString();
                        byte[] photo = row["Photo"] == DBNull.Value ? null : (byte[])row["Photo"];

                        SqlCommand checkCommand = new SqlCommand(
                            "SELECT COUNT(*) FROM TraineesBackup WHERE TraineeID=@TraineeID", connection);
                        checkCommand.Parameters.AddWithValue("@TraineeID", traineeId);

                        int rowExists = (int)checkCommand.ExecuteScalar();
                        if (rowExists == 0)
                        {
                            SqlCommand insertCommand = new SqlCommand(
                                @"INSERT INTO TraineesBackup (TraineeID, Name, Email, PhoneNumber, Department, JoiningDate, Gender, Photo)
                                  VALUES (@TraineeID, @Name, @Email, @PhoneNumber, @Department, @JoiningDate, @Gender, @Photo)", connection);
                            insertCommand.Parameters.AddWithValue("@TraineeID", traineeId);
                            insertCommand.Parameters.AddWithValue("@Name", name);
                            insertCommand.Parameters.AddWithValue("@Email", email);
                            insertCommand.Parameters.AddWithValue("@PhoneNumber", phone);
                            insertCommand.Parameters.AddWithValue("@Department", department);
                            insertCommand.Parameters.AddWithValue("@JoiningDate", joiningDate);
                            insertCommand.Parameters.AddWithValue("@Gender", gender);
                            insertCommand.Parameters.AddWithValue("@Photo", (object)photo ?? DBNull.Value);
                            insertCommand.ExecuteNonQuery();
                        }
                        else
                        {
                            SqlCommand updateCommand = new SqlCommand(
                                @"UPDATE TraineesBackup SET
                                    Name=@Name, Email=@Email, PhoneNumber=@PhoneNumber,
                                    Department=@Department, JoiningDate=@JoiningDate, Gender=@Gender, Photo=@Photo
                                  WHERE TraineeID=@TraineeID", connection);
                            updateCommand.Parameters.AddWithValue("@TraineeID", traineeId);
                            updateCommand.Parameters.AddWithValue("@Name", name);
                            updateCommand.Parameters.AddWithValue("@Email", email);
                            updateCommand.Parameters.AddWithValue("@PhoneNumber", phone);
                            updateCommand.Parameters.AddWithValue("@Department", department);
                            updateCommand.Parameters.AddWithValue("@JoiningDate", joiningDate);
                            updateCommand.Parameters.AddWithValue("@Gender", gender);
                            updateCommand.Parameters.AddWithValue("@Photo", (object)photo ?? DBNull.Value);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
                System.IO.File.AppendAllText(@"D:\TraineeServiceSuccessLog.txt",
        $"{DateTime.Now}: Backup success!\r\n");

            }
            catch (Exception error)
            {
                // For Debugging
                System.IO.File.AppendAllText(@"D:\TraineeServiceErrorLog.txt", error.ToString());
            }
        }
    }
}
