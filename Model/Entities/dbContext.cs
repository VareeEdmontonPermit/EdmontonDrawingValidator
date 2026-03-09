using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EdmontonDrawingValidator.Models.Entities
{
    public partial class dbContext : DbContext
    {
        public dbContext()
        {
        }

        public dbContext(DbContextOptions<dbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TblProjectMaster> TblProjectMasters { get; set; } = null!;
        public virtual DbSet<TblProjectUse> TblProjectUses { get; set; } = null!;
        public virtual DbSet<TblZone> TblZones { get; set; } = null!;
        public virtual DbSet<TblZoneWiseUse> TblZoneWiseUses { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=ConnectionStrings:EdmontonPermit");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TblProjectMaster>(entity =>
            {
                entity.HasKey(e => e.FldProjectId);

                entity.ToTable("tbl_ProjectMaster");

                entity.Property(e => e.FldProjectId).HasColumnName("fld_ProjectID");

                entity.Property(e => e.FldCreationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("fld_CreationDate");

                entity.Property(e => e.FldProjectName)
                    .HasMaxLength(500)
                    .HasColumnName("fld_ProjectName");

                entity.Property(e => e.FldZoneId).HasColumnName("fld_ZoneID");
                entity.Property(e => e.FldDwgFilePath).HasColumnName("fld_DwgFilePath");
            });

            modelBuilder.Entity<TblProjectUse>(entity =>
            {
                entity.ToTable("tbl_ProjectUse");

                entity.Property(e => e.TblProjectUseId).HasColumnName("tbl_ProjectUseID");

                entity.Property(e => e.FldProjectId).HasColumnName("fld_ProjectID");

                entity.Property(e => e.FldZoneWiseUseId).HasColumnName("fld_ZoneWiseUseID");
            });

            modelBuilder.Entity<TblZone>(entity =>
            {
                entity.HasKey(e => e.FldZoneId);

                entity.ToTable("tbl_Zone");

                entity.Property(e => e.FldZoneId).HasColumnName("fld_ZoneID");

                entity.Property(e => e.FldCode)
                    .HasMaxLength(100)
                    .HasColumnName("fld_Code");

                entity.Property(e => e.FldName)
                    .HasMaxLength(250)
                    .HasColumnName("fld_Name");

                entity.Property(e => e.FldPrimaryZone)
                    .HasMaxLength(250)
                    .HasColumnName("fld_PrimaryZone");

                entity.Property(e => e.FldSecondaryZone)
                    .HasMaxLength(250)
                    .HasColumnName("fld_SecondaryZone");

                entity.Property(e => e.FldSection)
                    .HasMaxLength(250)
                    .HasColumnName("fld_Section");
            });

            modelBuilder.Entity<TblZoneWiseUse>(entity =>
            {
                entity.HasKey(e => e.FldZoneWiseUseId);

                entity.ToTable("tbl_ZoneWiseUse");

                entity.Property(e => e.FldZoneWiseUseId).HasColumnName("fld_ZoneWiseUseID");

                entity.Property(e => e.FldActualUse)
                    .HasMaxLength(250)
                    .HasColumnName("fld_ActualUse");

                entity.Property(e => e.FldUseCategory)
                    .HasMaxLength(250)
                    .HasColumnName("fld_UseCategory");

                entity.Property(e => e.FldUseSubCategory)
                    .HasMaxLength(250)
                    .HasColumnName("fld_UseSubCategory");

                entity.Property(e => e.FldZoneId).HasColumnName("fld_ZoneID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
