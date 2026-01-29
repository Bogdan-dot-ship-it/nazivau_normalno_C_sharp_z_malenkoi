using Core;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class WorkshopDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<RepairOrder> RepairOrders { get; set; }
        public DbSet<RepairOrderAssignment> RepairOrderAssignments { get; set; }
        public DbSet<RepairOrderStatus> RepairOrderStatuses { get; set; }
        public DbSet<RepairOrderStatusHistory> RepairOrderStatusHistories { get; set; }
        public DbSet<WorkAct> WorkActs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(DataAccess.Database.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User and UserRole
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<Device>()
                .HasOne<Client>()
                .WithMany()
                .HasForeignKey(d => d.ClientId);

            modelBuilder.Entity<Device>()
                .HasOne<DeviceType>()
                .WithMany()
                .HasForeignKey(d => d.DeviceTypeId);

            modelBuilder.Entity<RepairOrder>()
                .HasOne<Device>()
                .WithMany()
                .HasForeignKey(ro => ro.DeviceId);

            modelBuilder.Entity<RepairOrder>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ro => ro.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairOrder>()
                .HasOne<RepairOrderStatus>()
                .WithMany()
                .HasForeignKey(ro => ro.CurrentStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairOrderAssignment>()
                .HasOne(roa => roa.RepairOrder)
                .WithMany()
                .HasForeignKey(roa => roa.OrderId);

            modelBuilder.Entity<RepairOrderAssignment>()
                .HasOne(roa => roa.User)
                .WithMany()
                .HasForeignKey(roa => roa.UserId);

            modelBuilder.Entity<RepairOrderStatusHistory>()
                .HasOne(rosh => rosh.RepairOrder)
                .WithMany()
                .HasForeignKey(rosh => rosh.OrderId);

            modelBuilder.Entity<RepairOrderStatusHistory>()
                .HasOne(rosh => rosh.Status)
                .WithMany()
                .HasForeignKey(rosh => rosh.StatusId);

            modelBuilder.Entity<RepairOrderStatusHistory>()
                .HasOne(rosh => rosh.User)
                .WithMany()
                .HasForeignKey(rosh => rosh.UserId);

            modelBuilder.Entity<WorkAct>()
                .HasOne(wa => wa.RepairOrder)
                .WithMany()
                .HasForeignKey(wa => wa.OrderId);

            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId);

            modelBuilder.Entity<UserRole>().HasKey(ur => ur.RoleId);
            modelBuilder.Entity<Client>().HasKey(c => c.ClientId);
            modelBuilder.Entity<Device>().HasKey(d => d.DeviceId);
            modelBuilder.Entity<DeviceType>().HasKey(dt => dt.DeviceTypeId);
            modelBuilder.Entity<RepairOrder>().HasKey(ro => ro.OrderId);
            modelBuilder.Entity<RepairOrderAssignment>().HasKey(roa => roa.AssignmentId);
            modelBuilder.Entity<RepairOrderStatus>().HasKey(ros => ros.StatusId);
            modelBuilder.Entity<RepairOrderStatusHistory>().HasKey(rosh => rosh.HistoryId);
            modelBuilder.Entity<WorkAct>().HasKey(wa => wa.ActId);
            modelBuilder.Entity<AuditLog>().HasKey(al => al.LogId);
        }
    }
}
