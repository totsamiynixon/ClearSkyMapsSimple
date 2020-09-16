for creating new migration use
Add-Migration $(Migration Name) -ConnectionString "$(connection string from config.json)" -ConnectionProviderName System.Data.SqlClient (some test on the second line)
