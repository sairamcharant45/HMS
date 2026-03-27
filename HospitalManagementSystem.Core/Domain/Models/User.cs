using System;
using System.Collections.Generic;

namespace HospitalManagementSystem.Core.Domain.Models;

public partial class User
{
    public int Id { get; set; }

    public string Uuid { get; set; } = null!;

    public int? Roleid { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phonenumber { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    public virtual Role? Role { get; set; }
}
