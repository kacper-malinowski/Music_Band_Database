namespace BB_sheets_client {
    public class Song {
        public int song_id; 
        public string name;
        public bool isSelected;
        public Song(int _song_id, string _name) {
            song_id = _song_id;
            name = _name;
            isSelected = false;
        }
    }
}
