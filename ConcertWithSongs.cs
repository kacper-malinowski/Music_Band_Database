
namespace BB_sheets_client {
    public class ConcertWithSongs {
        public int ConcertId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public List<Song> Songs { get; set; }
    }
}