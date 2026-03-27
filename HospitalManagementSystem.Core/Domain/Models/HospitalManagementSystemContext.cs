using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Core.Domain.Models;

public partial class HospitalManagementSystemContext : DbContext
{
    public HospitalManagementSystemContext()
    {
    }

    public HospitalManagementSystemContext(DbContextOptions<HospitalManagementSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseNpgsql("server=localhost;port=5433;UserName=postgres;password=root;database=hospital_management_system");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("appointments_pkey");

            entity.ToTable("appointments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Appointmentdate)
                .HasMaxLength(20)
                .HasColumnName("appointmentdate");
            entity.Property(e => e.Doctorid).HasColumnName("doctorid");
            entity.Property(e => e.Patientid).HasColumnName("patientid");
            entity.Property(e => e.Problem).HasColumnName("problem");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.Doctorid)
                .HasConstraintName("fk_doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.Patientid)
                .HasConstraintName("fk_patient");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("doctor_pkey");
            entity.Property(e => e.Id)
    .HasColumnName("id")
    .ValueGeneratedOnAdd();
            entity.ToTable("doctor");

            //entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bloodgroup)
                .HasMaxLength(10)
                .HasColumnName("bloodgroup");
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .HasColumnName("country");
            entity.Property(e => e.Dateofbirth)
                .HasMaxLength(30)
                .HasColumnName("dateofbirth");
            entity.Property(e => e.Designation)
                .HasMaxLength(50)
                .HasColumnName("designation");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.Mobilenumber)
                .HasMaxLength(50)
                .HasColumnName("mobilenumber");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Specilization)
                .HasMaxLength(50)
                .HasColumnName("specilization");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasColumnName("status");
            entity.Property(e => e.Userid)
                .HasMaxLength(50)
                .HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Doctors)
                .HasPrincipalKey(p => p.Uuid)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("doctor_userid_fkey");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("patient_pkey");

            entity.ToTable("patient");

            entity.Property(e => e.Id)
    .HasColumnName("id")
    .ValueGeneratedOnAdd();

            //entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bloodgroup)
                .HasMaxLength(10)
                .HasColumnName("bloodgroup");
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .HasColumnName("country");
            entity.Property(e => e.Dateofbirth)
                .HasMaxLength(50)
                .HasColumnName("dateofbirth");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.Mobilenumber)
                .HasMaxLength(50)
                .HasColumnName("mobilenumber");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Userid)
                .HasMaxLength(50)
                .HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Patients)
                .HasPrincipalKey(p => p.Uuid)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_patient_user");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prescription_pkey");

            entity.ToTable("prescription");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Afternoon1).HasColumnName("afternoon1");
            entity.Property(e => e.Afternoon2).HasColumnName("afternoon2");
            entity.Property(e => e.Afternoon3).HasColumnName("afternoon3");
            entity.Property(e => e.Afternoon4).HasColumnName("afternoon4");
            entity.Property(e => e.Checkupafterdays).HasColumnName("checkupafterdays");
            entity.Property(e => e.Createddate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createddate");
            entity.Property(e => e.Doctorid).HasColumnName("doctorid");
            entity.Property(e => e.Evening1).HasColumnName("evening1");
            entity.Property(e => e.Evening2).HasColumnName("evening2");
            entity.Property(e => e.Evening3).HasColumnName("evening3");
            entity.Property(e => e.Evening4).HasColumnName("evening4");
            entity.Property(e => e.Medicaltest1)
                .HasMaxLength(50)
                .HasColumnName("medicaltest1");
            entity.Property(e => e.Medicaltest2)
                .HasMaxLength(50)
                .HasColumnName("medicaltest2");
            entity.Property(e => e.Medicine1)
                .HasMaxLength(50)
                .HasColumnName("medicine1");
            entity.Property(e => e.Medicine2)
                .HasMaxLength(50)
                .HasColumnName("medicine2");
            entity.Property(e => e.Medicine3)
                .HasMaxLength(50)
                .HasColumnName("medicine3");
            entity.Property(e => e.Medicine4)
                .HasMaxLength(50)
                .HasColumnName("medicine4");
            entity.Property(e => e.Morning1).HasColumnName("morning1");
            entity.Property(e => e.Morning2).HasColumnName("morning2");
            entity.Property(e => e.Morning3).HasColumnName("morning3");
            entity.Property(e => e.Morning4).HasColumnName("morning4");
            entity.Property(e => e.Patientid).HasColumnName("patientid");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.Doctorid)
                .HasConstraintName("fk_prescription_doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.Patientid)
                .HasConstraintName("fk_prescription_patient");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Uuid, "users_uuid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Uuid)
                .HasMaxLength(50)
                .HasColumnName("uuid");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("users_roleid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
