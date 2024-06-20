using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly AppDBContext _context;

    public PrescriptionsController(AppDBContext context)
    {
        _context = context;
    }

    [HttpPost]
public async Task<IActionResult> CreatePrescription([FromBody] PrescriptionDTO prescriptionDto)
{
    var patient = await FindOrCreatePatientAsync(prescriptionDto);
    if (patient == null)
    {
        return BadRequest("Patient not found or created.");
    }

    var doctor = await _context.Doctors.FindAsync(prescriptionDto.DoctorId);
    if (doctor == null)
    {
        return BadRequest("Doctor not found");
    }

    if (prescriptionDto.Medicaments.Count > 10)
    {
        return BadRequest("Cannot include more than 10 medicaments");
    }

    var prescription = CreatePrescriptionFromDto(prescriptionDto, patient, doctor);

    foreach (var med in prescriptionDto.Medicaments)
    {
        var result = await AddMedicamentToPrescriptionAsync(prescription, med);
        if (result != null)
        {
            return result;
        }
    }

    _context.Prescriptions.Add(prescription);
    await _context.SaveChangesAsync();

    return Ok(prescription);
}

private async Task<Patient> FindOrCreatePatientAsync(PrescriptionDTO prescriptionDto)
{
    var patient = await _context.Patients.FindAsync(prescriptionDto.PatientId);
    if (patient == null)
    {
        patient = new Patient { FirstName = prescriptionDto.PatientFirstName, LastName = prescriptionDto.PatientLastName };
        _context.Patients.Add(patient);
    }

    return patient;
}

private Prescription CreatePrescriptionFromDto(PrescriptionDTO prescriptionDto, Patient patient, Doctor doctor)
{
    return new Prescription
    {
        Date = prescriptionDto.Date,
        DueDate = prescriptionDto.DueDate,
        Patient = patient,
        Doctor = doctor,
        PrescriptionMedicaments = new List<PrescriptionMedicament>()
    };
}

private async Task<IActionResult> AddMedicamentToPrescriptionAsync(Prescription prescription, MedicamentDTO medDto)
{
    var medicament = await _context.Medicaments.FindAsync(medDto.Id);
    if (medicament == null)
    {
        return BadRequest($"Medicament with ID {medDto.Id} not found");
    }

    prescription.PrescriptionMedicaments.Add(new PrescriptionMedicament
    {
        Medicament = medicament,
        Dose = medDto.Dose
    });

    return null;
}


    [HttpGet("{patientId}")]
    public async Task<IActionResult> GetPatientPrescriptions(int patientId)
    {
        var patient = await _context.Patients
            .Where(p => p.Id == patientId)
            .Include(p => p.Prescriptions)
            .ThenInclude(pr => pr.Doctor)
            .Include(p => p.Prescriptions)
            .ThenInclude(pr => pr.PrescriptionMedicaments)
            .ThenInclude(pm => pm.Medicament)
            .FirstOrDefaultAsync();

        if (patient == null)
        {
            return NotFound();
        }

        return Ok(patient);
    }
}