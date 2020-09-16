for creating new migration use (some text on the first line)
Add-Migration $(Migration Name) -ConnectionString "$(connection string from config.json)" -ConnectionProviderName System.Data.SqlClient
