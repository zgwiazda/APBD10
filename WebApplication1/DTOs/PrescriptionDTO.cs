namespace WebApplication1.DTOs;

public class PrescriptionDTO
{
    public int PatientId { get; set; }
    public string PatientFirstName { get; set; }
    public string PatientLastName { get; set; }
    public int DoctorId { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public List<MedicamentDTO> Medicaments { get; set; }

}
public class MedicamentDTO
{
    public int Id { get; set; }
    public int Dose { get; set; }
}