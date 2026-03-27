using System;
using System.Collections.Generic;

namespace HospitalManagementSystem.Core.Domain.Models;

public partial class Doctor
{
    public int Id { get; set; }

    public string? Userid { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Designation { get; set; }

    public string? Mobilenumber { get; set; }

    public string? Specilization { get; set; }

    public string? Gender { get; set; }

    public string? Bloodgroup { get; set; }

    public string? Dateofbirth { get; set; }

    public string? Status { get; set; }

    public string? Country { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual User? User { get; set; }
}
