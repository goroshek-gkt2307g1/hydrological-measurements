using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Гидрологические_измерения.Entities;

public partial class DatabaseOfHydrologicalMeasurementsContext : DbContext
{
    public DatabaseOfHydrologicalMeasurementsContext()
    {
    }

    public DatabaseOfHydrologicalMeasurementsContext(DbContextOptions<DatabaseOfHydrologicalMeasurementsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Equipment> Equipments { get; set; }

    public virtual DbSet<EquipmentStatus> EquipmentStatuses { get; set; }

    public virtual DbSet<EquipmentType> EquipmentTypes { get; set; }

    public virtual DbSet<Hydropost> Hydroposts { get; set; }

    public virtual DbSet<HydropostType> HydropostTypes { get; set; }

    public virtual DbSet<Measurement> Measurements { get; set; }

    public virtual DbSet<Person> Persons { get; set; }

    public virtual DbSet<PersonRole> PersonRoles { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectStatus> ProjectStatuses { get; set; }

    public virtual DbSet<SurveyLine> SurveyLines { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-E3J5VUQ;Initial Catalog='database of hydrological measurements';Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId).HasName("PK__EQUIPMEN__197068AF68CF1D77");

            entity.ToTable("EQUIPMENTS");

            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.EquipmentStatusIdFk).HasColumnName("equipment_status_id_FK");
            entity.Property(e => e.EquipmentTypeIdFk).HasColumnName("equipment_type_id_FK");
            entity.Property(e => e.HydropostIdFk).HasColumnName("hydropost_id_FK");
            entity.Property(e => e.LastCalibration).HasColumnName("last_calibration");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.EquipmentStatusIdFkNavigation).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.EquipmentStatusIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EQUIPMENTS_STATUS");

            entity.HasOne(d => d.EquipmentTypeIdFkNavigation).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.EquipmentTypeIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EQUIPMENTS_TYPES");

            entity.HasOne(d => d.HydropostIdFkNavigation).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.HydropostIdFk)
                .HasConstraintName("FK_EQUIPMENTS_HYDROPOSTS");
        });

        modelBuilder.Entity<EquipmentStatus>(entity =>
        {
            entity.HasKey(e => e.EquipmentStatusId).HasName("PK__EQUIPMEN__9708D7D9E81D490C");

            entity.ToTable("EQUIPMENT_STATUSES");

            entity.HasIndex(e => e.Name, "UQ__EQUIPMEN__72E12F1B7430A684").IsUnique();

            entity.Property(e => e.EquipmentStatusId).HasColumnName("equipment_status_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<EquipmentType>(entity =>
        {
            entity.HasKey(e => e.EquipmentTypeId).HasName("PK__EQUIPMEN__D8B1EC05E0F09F0A");

            entity.ToTable("EQUIPMENT_TYPES");

            entity.HasIndex(e => e.Name, "UQ__EQUIPMEN__72E12F1BD4C9C701").IsUnique();

            entity.Property(e => e.EquipmentTypeId).HasColumnName("equipment_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Hydropost>(entity =>
        {
            entity.HasKey(e => e.HydropostId).HasName("PK__HYDROPOS__5A018055043A912E");

            entity.ToTable("HYDROPOSTS");

            entity.HasIndex(e => e.ManagerIdFk, "UQ_HYDROPOSTS_MANAGER").IsUnique();

            entity.HasIndex(e => e.Name, "UQ__HYDROPOS__72E12F1BCFFDA546").IsUnique();

            entity.Property(e => e.HydropostId).HasColumnName("hydropost_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.HydropostTypeIdFk).HasColumnName("hydropost_type_id_FK");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longtude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("longtude");
            entity.Property(e => e.ManagerIdFk).HasColumnName("manager_id_FK");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.ZeroWaterLevel)
                .HasColumnType("decimal(10, 3)")
                .HasColumnName("zero_water_level");

            entity.HasOne(d => d.HydropostTypeIdFkNavigation).WithMany(p => p.Hydroposts)
                .HasForeignKey(d => d.HydropostTypeIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HYDROPOSTS_TYPES");

            entity.HasOne(d => d.ManagerIdFkNavigation).WithOne(p => p.Hydropost)
                .HasForeignKey<Hydropost>(d => d.ManagerIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HYDROPOSTS_PERSONS");
        });

        modelBuilder.Entity<HydropostType>(entity =>
        {
            entity.HasKey(e => e.HydropostTypeId).HasName("PK__HYDROPOS__BAA95A9E58EBA580");

            entity.ToTable("HYDROPOST_TYPES");

            entity.HasIndex(e => e.Name, "UQ__HYDROPOS__72E12F1B9A473E1F").IsUnique();

            entity.Property(e => e.HydropostTypeId).HasColumnName("hydropost_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Measurement>(entity =>
        {
            entity.HasKey(e => e.MeasurementsId).HasName("PK__MEASUREM__EA6932D3DAD9F562");

            entity.ToTable("MEASUREMENTS");

            entity.Property(e => e.MeasurementsId).HasColumnName("measurements_id");
            entity.Property(e => e.Area)
                .HasColumnType("decimal(10, 3)")
                .HasColumnName("area");
            entity.Property(e => e.Comment)
                .HasMaxLength(500)
                .HasColumnName("comment");
            entity.Property(e => e.EquipmentIdFk).HasColumnName("equipment_id_FK");
            entity.Property(e => e.IcePhenomena)
                .HasMaxLength(100)
                .HasColumnName("ice_phenomena");
            entity.Property(e => e.MeasuredAt).HasColumnName("measured_at");
            entity.Property(e => e.PersonIdFk).HasColumnName("person_id_FK");
            entity.Property(e => e.ProjectIdFk).HasColumnName("project_id_FK");
            entity.Property(e => e.SurveyLineIdFk).HasColumnName("survey_line_id_FK");
            entity.Property(e => e.WaterConsumption)
                .HasColumnType("decimal(10, 3)")
                .HasColumnName("water_consumption");
            entity.Property(e => e.WaterLevel)
                .HasColumnType("decimal(10, 3)")
                .HasColumnName("water_level");
            entity.Property(e => e.WaterTemperature)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("water_temperature");
            entity.Property(e => e.WaterTransparency)
                .HasColumnType("decimal(5, 1)")
                .HasColumnName("water_transparency");
            entity.Property(e => e.Width)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("width");

            entity.HasOne(d => d.EquipmentIdFkNavigation).WithMany(p => p.Measurements)
                .HasForeignKey(d => d.EquipmentIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEASUREMENTS_EQUIPMENTS");

            entity.HasOne(d => d.PersonIdFkNavigation).WithMany(p => p.Measurements)
                .HasForeignKey(d => d.PersonIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEASUREMENTS_PERSONS");

            entity.HasOne(d => d.ProjectIdFkNavigation).WithMany(p => p.Measurements)
                .HasForeignKey(d => d.ProjectIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEASUREMENTS_PROJECTS");

            entity.HasOne(d => d.SurveyLineIdFkNavigation).WithMany(p => p.Measurements)
                .HasForeignKey(d => d.SurveyLineIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEASUREMENTS_SURVEY_LINES");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.PersonId).HasName("PK__PERSONS__543848DF9D696CDE");

            entity.ToTable("PERSONS");

            entity.HasIndex(e => e.Login, "UQ__PERSONS__7838F27220EB01A1").IsUnique();

            entity.Property(e => e.PersonId).HasColumnName("person_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(70)
                .HasColumnName("last_name");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .HasColumnName("login");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(65)
                .HasColumnName("middle_name");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
            entity.Property(e => e.RoleIdFk).HasColumnName("role_id_FK");

            entity.HasOne(d => d.RoleIdFkNavigation).WithMany(p => p.People)
                .HasForeignKey(d => d.RoleIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PERSONS_ROLES");
        });

        modelBuilder.Entity<PersonRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__PERSON_R__760965CCDB691AA8");

            entity.ToTable("PERSON_ROLES");

            entity.HasIndex(e => e.Name, "UQ__PERSON_R__72E12F1B9E5659FB").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__PROJECTS__BC799E1F29687941");

            entity.ToTable("PROJECTS");

            entity.HasIndex(e => e.Name, "UQ__PROJECTS__72E12F1B396886D7").IsUnique();

            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Client)
                .HasMaxLength(200)
                .HasColumnName("client");
            entity.Property(e => e.ContractorIdFk).HasColumnName("contractor_id_FK");
            entity.Property(e => e.ElevationSystem)
                .HasMaxLength(50)
                .HasColumnName("elevation_system");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Purpose)
                .HasMaxLength(500)
                .HasColumnName("purpose");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.StatusIdFk).HasColumnName("status_id_FK");

            entity.HasOne(d => d.ContractorIdFkNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.ContractorIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PROJECTS_MANAGER");

            entity.HasOne(d => d.StatusIdFkNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.StatusIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PROJECTS_STATUS");
        });

        modelBuilder.Entity<ProjectStatus>(entity =>
        {
            entity.HasKey(e => e.ProjectStatusId).HasName("PK__PROJECT___1932924C2C3888A1");

            entity.ToTable("PROJECT_STATUSES");

            entity.HasIndex(e => e.Name, "UQ__PROJECT___72E12F1B4D585379").IsUnique();

            entity.Property(e => e.ProjectStatusId).HasColumnName("project_status_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SurveyLine>(entity =>
        {
            entity.HasKey(e => e.SurveyLineId).HasName("PK__SURVEY_L__32CCE9211D6D4E6E");

            entity.ToTable("SURVEY_LINES");

            entity.HasIndex(e => new { e.HydropostIdFk, e.Name }, "UQ_SURVEY_LINES_HYDROPOST").IsUnique();

            entity.Property(e => e.SurveyLineId).HasColumnName("survey_line_id");
            entity.Property(e => e.DistanceFromBase)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("distance_from_base");
            entity.Property(e => e.HydropostIdFk).HasColumnName("hydropost_id_FK");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.HydropostIdFkNavigation).WithMany(p => p.SurveyLines)
                .HasForeignKey(d => d.HydropostIdFk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SURVEY_LINES_HYDROPOSTS");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
