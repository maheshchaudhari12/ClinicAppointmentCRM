using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicAppointmentCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentCRM.Data
{
    public class ClinicDbContext : DbContext
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One-to-one: Admin ↔ UserLogin
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.UserLogin)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-one: Doctor ↔ UserLogin
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.UserLogin)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-one: Patient ↔ UserLogin
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.UserLogin)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-one: Reception ↔ UserLogin
            modelBuilder.Entity<Reception>()
                .HasOne(r => r.UserLogin)
                .WithOne(u => u.Reception)
                .HasForeignKey<Reception>(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing: AdminActivationLog
            modelBuilder.Entity<AdminActivationLog>()
                .HasOne(log => log.ActivatedAdmin)
                .WithMany(admin => admin.ActivatedAdmins)
                .HasForeignKey(log => log.ActivatedAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdminActivationLog>()
                .HasOne(log => log.ActivatedBy)
                .WithMany(admin => admin.ActivatedBy)
                .HasForeignKey(log => log.ActivatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        }


        public DbSet<Models.Appointment> Appointments { get; set; }
        public DbSet<Models.Prescription> Prescriptions { get; set; }
        public DbSet<Models.Patient> Patients { get; set; }
        public DbSet<Models.Doctor> Doctors { get; set; }
        public DbSet<Models.Reception> Receptions { get; set; }
        public DbSet<Models.UserLogin> UserLogins { get; set; }
        public DbSet<Models.Notification> Notifications { get; set; }
        public DbSet<Models.Admin> Admins { get; set; }
        public DbSet<Models.AdminActivationLog> AdminActivationLogs { get; set; }

    }
}