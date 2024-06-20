namespace WebApplication1.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; } // Hash hasła

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}