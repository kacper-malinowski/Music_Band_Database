using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics.Metrics;
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
            string query = $"INSERT INTO user (first_name, last_name, email, password, instrument_id) VALUES ('{firstName}', '{lastName}', '{email}', '{password}', {instrumentID});";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.ExecuteNonQuery();
                Console.WriteLine("User added successfully");
            }
        }

        public bool ValidateLogin(string email, string password) {
            string query = $"SELECT * FROM user WHERE (email LIKE '{email}' AND password LIKE '{password}');";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        public string GetUsername(string email) {
            string query = $"SELECT first_name FROM user WHERE email LIKE '{email}';";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                string username = command.ExecuteScalar().ToString();
                return username;
            }
        }
        public bool IsAdmin(string email) {
            string query = $"SELECT is_admin FROM user WHERE email LIKE '{email}';";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
        private bool ObjectExists(string objectName, string objectType) {

            string query = $"SELECT COUNT(*) FROM {objectType} WHERE name = '{objectName}';";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
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

        /*public List<object> GetTableContent(string tableName) {
            List<object> content = new List<object>();

            string query = $"SELECT * FROM {tableName};";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                using (MySqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {

                        for (int i = 0; i < reader.FieldCount; i++) {
                            content[i] = reader.GetValue(i);
                        }
                        tableContent.Add(row);
                    }
                }
            }
            return tableContent;
        }*/

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
            if (ObjectExists(instrumentName, "instrument")) {
                Console.WriteLine($"{instrumentName} already exists!");
                return;
            }


            string query = $"INSERT INTO instrument (name) VALUES ('{instrumentName}');";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                command.ExecuteNonQuery();
            }

        }
        public void DeleteInstrument(string instrumentName) {
            if (ObjectExists(instrumentName, "song")) {


                int instrumentID = findInstrumentByName(instrumentName);
                string query = $"DELETE FROM instrument WHERE instrument_id = {instrumentID};";
                using (MySqlCommand command = new MySqlCommand(query, connection)) {
                    command.ExecuteNonQuery();
                }

            }
        }

        public int findSongByName(string songName) {

            string query = $"SELECT song_id FROM song WHERE name = '{songName}';";
            using (MySqlCommand command = new MySqlCommand(query, connection)) {
                int songID = Convert.ToInt32(command.ExecuteScalar());
                return songID;
            }

        }
        public void CreateSong(string songName) {
            if (ObjectExists(songName, "song")) {
                Console.WriteLine($"{songName} already exists!");
                return;
            }

            ConnectionString cs = new ConnectionString();
            string connectionString = cs.GetConnectionString();
            using (MySqlConnection connection = new MySqlConnection(connectionString)) {
                connection.Open();
                string query = $"INSERT INTO song (name) VALUES ('{songName}');";
                using (MySqlCommand command = new MySqlCommand(query, connection)) {
                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteSong(string songName) {
            if (ObjectExists(songName, "song")) {

                int songID = findInstrumentByName(songName);
                string query = $"DELETE FROM song WHERE song_id = {songID};";
                using (MySqlCommand command = new MySqlCommand(query, connection)) {
                    command.ExecuteNonQuery();
                }

            }
        }
        public async void AddConcert(DateTime dateTime, string description, List<Song> selectedSongs) {
            using (var transaction = await connection.BeginTransactionAsync()) {
                try {
                    string query = "INSERT INTO concert (date, description) VALUES (@date, @description)";
                    var command = new MySqlCommand(query, connection, transaction);
                    command.Parameters.AddWithValue("@date", dateTime);
                    command.Parameters.AddWithValue("@description", description);
                    await command.ExecuteNonQueryAsync();
                    long concertId = command.LastInsertedId;

                    foreach (var song in selectedSongs) {
                        query = $"INSERT INTO concert_song (concert_id, song_id) VALUES ('{concertId}', '{song.song_id}')";
                        var songConcertCommand = new MySqlCommand(query, connection, transaction);
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

    }
}

