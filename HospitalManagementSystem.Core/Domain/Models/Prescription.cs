using System;
using System.Collections.Generic;

namespace HospitalManagementSystem.Core.Domain.Models;

public partial class Prescription
{
    public int Id { get; set; }

    public int? Doctorid { get; set; }

    public int? Patientid { get; set; }

    public string? Medicaltest1 { get; set; }

    public string? Medicaltest2 { get; set; }

    public string? Medicine1 { get; set; }

    public string? Medicine2 { get; set; }

    public string? Medicine3 { get; set; }

    public string? Medicine4 { get; set; }

    public short? Morning1 { get; set; }

    public short? Morning2 { get; set; }

    public short? Morning3 { get; set; }

    public short? Morning4 { get; set; }

    public short? Afternoon1 { get; set; }

    public short? Afternoon2 { get; set; }

    public short? Afternoon3 { get; set; }

    public short? Afternoon4 { get; set; }

    public short? Evening1 { get; set; }

    public short? Evening2 { get; set; }

    public short? Evening3 { get; set; }

    public short? Evening4 { get; set; }

    public int? Checkupafterdays { get; set; }

    public DateTime? Createddate { get; set; }

    public virtual Doctor? Doctor { get; set; }

    public virtual Patient? Patient { get; set; }
}
