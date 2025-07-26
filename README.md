# TraineeService – Windows Service for Trainee Table Backup

This project is a .NET Windows Service (C#) that periodically (every 10 minutes) backs up data from the `Trainees` table to a mirror table `TraineesBackup` in the same SQL Server database. Backup table structure is auto-created if it doesn't exist. Success and error logs are written to file.

## Features

- **Windows Service**: Runs in the background, no UI necessary.
- **Automatic Backup**: Every 10 minutes (configurable for testing as 5 seconds), reads all rows from `Trainees` and mirrors to `TraineesBackup`.
- **Upsert Logic**: Updates rows if already backed up, inserts otherwise (prevents duplicates).
- **Table Auto-Create**: If `TraineesBackup` doesn't exist, creates with correct structure (**NO IDENTITY**).
- **Logging**:
  - Success log: `D:\TraineeServiceSuccessLog.txt`
  - Error log: `D:\TraineeServiceErrorLog.txt`

## Table Structures

```sql
-- Main Table
CREATE TABLE Trainees (
    TraineeID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    PhoneNumber NVARCHAR(20),
    Department NVARCHAR(100),
    JoiningDate DATE,
    Gender NVARCHAR(100),
    Photo VARBINARY(MAX)
);

-- Backup Table (No IDENTITY in TraineeID)
CREATE TABLE TraineesBackup (
    TraineeID INT PRIMARY KEY,
    Name NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    PhoneNumber NVARCHAR(20),
    Department NVARCHAR(100),
    JoiningDate DATE,
    Gender NVARCHAR(100),
    Photo VARBINARY(MAX)
);
```

## How It Works

1. **On Service Start**: Initializes a timer (default: 10 minutes, can be set shorter for testing).
2. **On Timer Tick**:
    - Ensures backup table exists (creates if missing).
    - Reads all rows from `Trainees`.
    - For each row:
      - If exists in `TraineesBackup` (by `TraineeID`), updates data.
      - Else, inserts as new backup row.
    - Logs success or any errors to file.

## Configuration

### Timer Interval:
- Change in `Service1.cs` → `timer.Interval = 600000;` (600000 ms = 10 mins)
- Use a smaller interval (e.g., 5000 for 5 seconds) when testing.

### Connection String:
- Set in `BackupManager.cs`
```csharp
string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TraineeDB;Integrated Security=True";
```

## Logs

- **Success Log**: `D:\TraineeServiceSuccessLog.txt`
- **Error Log**: `D:\TraineeServiceErrorLog.txt`
- **Service Events (Start/Stop)**: `D:\TraineeServiceLogs.txt`

## How To Build & Run

### Build Project in Visual Studio

### Install as Windows Service

1. Open command prompt as administrator
2. Go to .NET Framework folder:
```cmd
cd C:\Windows\Microsoft.NET\Framework64\v4.0.30319
```
3. Install:
```cmd
InstallUtil.exe "D:\your\path\TraineeService.exe"
```

### Start Service

Use `services.msc` UI or command:
```cmd
sc start TraineeService
```

### Check Logs and DB

- See the log files for `"Backup success..."`
- Query `SELECT * FROM TraineesBackup;` in SQL Server to verify backup

## Troubleshooting

- **Database Login Failed?**
  - Make sure Windows Service account has DB access (grant `db_owner` to `NT AUTHORITY\SYSTEM` or use SQL authentication).

- **Table Not Found Error?**
  - Confirm table name and existence in SSMS.

- **File Locking Errors (on rebuild)?**
  - Stop the service before rebuilding; use Task Manager if needed.

- **Error 1053 When Starting Service?**
  - `OnStart` method should only start timer; don't do heavy work directly there.

## Customizations

- Change Backup Table Name: Set your preferred name in both code and DB structure.
- Add Extra Logging: Enhance logs for row count, changes, etc. (see `BackupManager.cs`).
- Backup Deletion Sync: Not supported out-of-the-box—implement if you want deleted main table rows also removed in backup.

## Example Commit Messages

- Implement service start/stop logging
- Add automatic `TraineesBackup` table creation and upsert logic
- Log success and error events to separate files
- Fix identity issue by creating backup table without IDENTITY
- Refactor connection string for `TraineeDB`

## Author

**Claysys Training / Anbarasan P / https://github.com/Anbarasan-P**

---

## Questions?

Feel free to raise an issue or ping the repo owner!

Happy coding, bro! 🚀
