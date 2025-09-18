namespace PhotoGalleryApp.Models
{
    public class ApplicationUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Role { get; set; } = "User"; // "Admin" or "User"
    }
}
