/*using Microsoft.AspNetCore.Mvc;

namespace BB_sheets_client {
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase {
        /*private readonly DataAccess dataAccess;

        public DownloadController(DataAccess _dataAccess) {
            dataAccess = _dataAccess;
        }

        [HttpGet("download-zip/{userInstrumentID}")]
        public IActionResult DownloadZIP(int userInstrumentID) {
            try {
                byte[] fileBytes = dataAccess.DownloadZIP(userInstrumentID);
                string fileName = "sheet_music.zip";
                return File(fileBytes, "application/zip", fileName);
            }
            catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}*/