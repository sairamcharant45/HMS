using System;
using System.Collections.Generic;

namespace HospitalManagementSystem.Core.Domain.Models;

public partial class Appointment
{
    public int Id { get; set; }

    public int? Doctorid { get; set; }

    public int? Patientid { get; set; }

    public string? Problem { get; set; }

    public string? Appointmentdate { get; set; }

    public string? Status { get; set; }

    public virtual Doctor? Doctor { get; set; }

    public virtual Patient? Patient { get; set; }
}
