using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics.Metrics;
using System.IO.Compression;
using System.Security.Policy;
using System.Xml.Linq;

namespace BB_sheets_client {
    public class DataAccess {
        MySqlConnection connection;

        public DataAccess() {
            ConnectionString cs = new ConnectionString();
            string connectionString = cs.GetConnectionString();
            connection = new MySqlConnection(connectionString);
            connection.Open();
        }
        public void ConnectDatabase() {
            List<string> tableNames = new List<string>();

            string query = "SHOW tables;";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                using (var reader = command.ExecuteReader()) {
                    foreach (var row in reader) {
                        tableNames.Add(reader.GetString(0));
                    }
                    foreach (var name in tableNames) {
                        Console.WriteLine(name);

                    }
                }
            }

        }

        public void CreateUser(string firstName, string lastName, string email, string password, string instrumentName) {
            if (firstName == null || lastName == null || email == null || password == null || instrumentName == null) {
                Console.WriteLine("Invalid input");
                return;
            }

            int instrumentID = findInstrumentByName(instrumentName);
            if (instrumentID == 0) {
                Console.WriteLine($"{instrumentName} doesn't exist!");
                return;
            }
            string query = "INSERT INTO user (first_name, last_name, email, password, instrument_id) VALUES (@firstName, @lastName, @Email, @Password, @InstrumentID)";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@firstName", firstName);
                command.Parameters.AddWithValue("@lastName", lastName);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);
                command.Parameters.AddWithValue("@InstrumentID", instrumentID);
                command.ExecuteNonQuery();
                Console.WriteLine("User added successfully");
            }
        }

        public bool ValidateLogin(string email, string password) {
            string query = "SELECT COUNT(*) FROM user WHERE email = @Email AND password = @Password";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        public string GetUsername(string email) {
            string query = "SELECT is_admin FROM user WHERE email = @Email";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@Email", email);
                string username = command.ExecuteScalar().ToString();
                return username;
            }
        }
        public bool IsAdmin(string email) {
            string query = "SELECT is_admin FROM user WHERE email = @Email";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@Email", email);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
        public int GetInstrumentFromEmail(string email) {
            string query = "SELECT instrument_id FROM user WHERE email = @Email";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@Email", email);
                int instrumentID = Convert.ToInt32(command.ExecuteScalar());
                return instrumentID;
            }
        }
        private bool ObjectNameExists(string objectName, string objectType) {

            string query = $"SELECT COUNT(*) FROM {objectType} WHERE name = @ObjectName";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@ObjectName", objectName);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }

        }

        public List<string> GetInstruments() {
            List<string> instrumentNames = new List<string>();

            string query = "SELECT name FROM instrument";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                using (var reader = command.ExecuteReader()) {
                    foreach (var row in reader) {
                        instrumentNames.Add(reader.GetString(0));
                    }
                }
            }

            return instrumentNames.ToList();
        }

        public List<Song> GetSongs() {
            List<Song> songList = new List<Song>();
            int SongId;
            string Name;

            string query = "SELECT * FROM song";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                using (var reader = command.ExecuteReader()) {
                    foreach (var row in reader) {
                        SongId = reader.GetInt32("song_id");
                        Name = reader.GetString("name");

                        songList.Add(new Song(SongId, Name));
                    }
                }
            }

            return songList.ToList();
        }

        public List<string> GetTables() {
            List<string> tables = new List<string>();
            string query = "SHOW TABLES;";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                using (MySqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        tables.Add(reader.GetString(0));
                    }
                }
            }
            return tables;
        }

        public List<Dictionary<string, object>> GetTableContent(string tableName) {
            List<Dictionary<string, object>> tableContent = new List<Dictionary<string, object>>();
            string query = $"SELECT * FROM {tableName};";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                using (MySqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++) {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        tableContent.Add(row);
                    }
                }
            }
            return tableContent;
        }
        public int findInstrumentByName(string instrumentName) {

            string query = $"SELECT instrument_id FROM instrument WHERE name = '{instrumentName}';";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                int instrumentID = Convert.ToInt32(command.ExecuteScalar());
                return instrumentID;
            }

        }
        public void CreateInstrument(string instrumentName) {
            if (ObjectNameExists(instrumentName, "instrument")) {
                Console.WriteLine($"{instrumentName} already exists!");
                return;
            }

            string query = "INSERT INTO instrument (name) VALUES (@InstrumentName)";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@InstrumentName", instrumentName);
                command.ExecuteNonQuery();
            }

        }
        public void DeleteInstrument(string instrumentName) {
            if (ObjectNameExists(instrumentName, "song")) {


                int instrumentID = findInstrumentByName(instrumentName);
                string query = "DELETE FROM instrument WHERE instrument_id = @InstrumentID";
                using (MySqlCommand command = new MySqlCommand(query, connection)) {
                    command.Parameters.AddWithValue("@InstrumentID", instrumentID);
                    command.ExecuteNonQuery();
                }

            }
        }

        public int findSongByName(string songName) {

            string query = "SELECT song_id FROM song WHERE name = @SongName";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@SongName", songName);
                int songID = Convert.ToInt32(command.ExecuteScalar());
                return songID;
            }

        }
        public void CreateSong(string songName) {
            if (ObjectNameExists(songName, "song")) {
                Console.WriteLine($"{songName} already exists!");
                return;
            }

            string query = $"INSERT INTO song (name) VALUES (@SongName)";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@SongName", songName);
                command.ExecuteNonQuery();

            }
        }
        public void DeleteSong(string songName) {
            if (ObjectNameExists(songName, "song")) {

                string query = "DELETE FROM song WHERE song.name = @SongID";
                using (MySqlCommand command = new MySqlCommand(query, connection)) {
                    command.Parameters.AddWithValue("@SongID", songName);
                    command.ExecuteNonQuery();
                }

            }
        }
        public async void AddConcert(DateTime dateTime, string description, List<Song> selectedSongs) {
            using (var transaction = await connection.BeginTransactionAsync()) {
                try {
                    string query = "INSERT INTO concert (date, description) VALUES (@date, @description)";
                    var command = new MySqlCommand(query, connection/*, transaction*/);
                    command.Parameters.AddWithValue("@date", dateTime);
                    command.Parameters.AddWithValue("@description", description);
                    await command.ExecuteNonQueryAsync();

                    int concertId = (int)command.LastInsertedId;

                    foreach (var song in selectedSongs) {
                        query = $"INSERT INTO concert_song (concert_id, song_id) VALUES ({concertId},{song.song_id})";
                        var songConcertCommand = new MySqlCommand(query, connection/*, transaction*/);
                        await songConcertCommand.ExecuteNonQueryAsync();
                    }
                    await transaction.CommitAsync();
                }
                catch {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public void CreateSheetMusic() {
            string rootDirectory = "D:\\Studia\\BB\\repertoire";
            string[] instrumentFolders = Directory.GetDirectories(rootDirectory);
            string query = "TRUNCATE sheet_music;";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.ExecuteNonQuery();
            }
            foreach (string instrumentFolder in instrumentFolders) {
                string folderName = Path.GetFileName(instrumentFolder);
                int instrumentID;
                Console.WriteLine("Sprawdzam " + folderName);
                query = $"SELECT instrument_id FROM instrument WHERE instrument.name LIKE '{folderName}';";
                using (MySqlCommand command = new MySqlCommand(query, connection)) {
                    //command.Parameters.AddWithValue("@folderName", folderName);
                    Console.WriteLine(command.CommandText);
                    instrumentID = Convert.ToInt32(command.ExecuteScalar());
                    
                }
                query = "INSERT INTO `bb_sheets_db`.`sheet_music` (`instrument_id`,`path`) VALUES(@instrumentID,@instrumentFolder);";
                using (MySqlCommand command = new MySqlCommand(query, connection)) {
                    command.Parameters.AddWithValue("@instrumentID", instrumentID);
                    command.Parameters.AddWithValue("@instrumentFolder", instrumentFolder);
                    command.ExecuteNonQuery();
                }
            }
        }
        public Stream DownloadZIP(int userInstrumentID) {

            string? path = GetFolderPath(userInstrumentID);
            if (string.IsNullOrEmpty(path)) {
                throw new Exception("Path not found.");
            }

            string zipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");

            try {
                ZipFile.CreateFromDirectory(path, zipPath);
                byte[] fileBytes = File.ReadAllBytes(zipPath);
                File.Delete(zipPath);
                var fileStream = new MemoryStream(fileBytes);

                return fileStream;
            }
            catch (Exception ex) {
                throw new Exception("Error creating zip file.", ex);
            }
        }

        private string? GetFolderPath(int userInstrumentID) {
            string query = "SELECT path FROM sheet_music WHERE instrument_id = @userInstrumentID";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@userInstrumentID", userInstrumentID);
                return command.ExecuteScalar()?.ToString();
            }
        }
        public List<ConcertWithSongs> GetUpcomingConcerts() {
            var upcomingConcerts = new List<ConcertWithSongs>();

            string query = @"
            SELECT c.concert_id, c.date, c.description, s.song_id, s.name AS song_name
            FROM concert c
            LEFT JOIN concert_song cs ON c.concert_id = cs.concert_id
            LEFT JOIN song s ON cs.song_id = s.song_id
            WHERE c.date >= CURDATE()
            ORDER BY c.date";

            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        int concertId = reader.GetInt32("concert_id");
                        var concert = upcomingConcerts.FirstOrDefault(c => c.ConcertId == concertId);

                        if (concert == null) {
                            concert = new ConcertWithSongs {
                                ConcertId = concertId,
                                Date = reader.GetDateTime("date"),
                                Description = reader.GetString("description"),
                                Songs = new List<Song>()
                            };
                            upcomingConcerts.Add(concert);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("song_id"))) {
                            var song = new Song(reader.GetInt32("song_id"), reader.GetString("song_name"));
                            concert.Songs.Add(song);
                        }
                    }
                }
            }

            return upcomingConcerts;
        }
    }

}

