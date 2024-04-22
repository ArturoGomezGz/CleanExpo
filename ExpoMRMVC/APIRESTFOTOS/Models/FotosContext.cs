using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace APIRESTFOTOS.Models
{
    public partial class FotosContext : DbContext
    {
        public FotosContext()
        {
        }

        public FotosContext(DbContextOptions<FotosContext> options)
            : base(options)
        {
        }

        public virtual DbSet<MAtributosFoto> MAtributosFotos { get; set; } = null!;
        public virtual DbSet<MFirma> MFirmas { get; set; } = null!;
        public virtual DbSet<MFoto> MFotos { get; set; } = null!;
        public virtual DbSet<MImgDoc> MImgDocs { get; set; } = null!;
        public virtual DbSet<MImgDocAclaracion> MImgDocAclaracions { get; set; } = null!;
        public virtual DbSet<MImgDocTicket> MImgDocTickets { get; set; } = null!;
        public virtual DbSet<MImgGruposCofepri> MImgGruposCofepris { get; set; } = null!;
        public virtual DbSet<MTipoArchivo> MTipoArchivos { get; set; } = null!;

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Modern_Spanish_CI_AI");

            modelBuilder.Entity<MAtributosFoto>(entity =>
            {
                entity.HasKey(e => e.IdAtributo);

                entity.ToTable("mAtributosFoto");

                entity.Property(e => e.Descripcion).HasMaxLength(15);
            });

            modelBuilder.Entity<MFirma>(entity =>
            {
                entity.HasKey(e => e.IdFirma);

                entity.ToTable("mFirmas");

                entity.Property(e => e.FeModifica).HasColumnType("smalldatetime");

                entity.Property(e => e.Firma).HasColumnType("image");

                entity.Property(e => e.Obsv).HasMaxLength(150);

                entity.Property(e => e.Usuario).HasMaxLength(12);
            });

            modelBuilder.Entity<MFoto>(entity =>
            {
                entity.HasKey(e => e.IdFoto);

                entity.ToTable("mFotos");

                entity.Property(e => e.FeModifica).HasColumnType("smalldatetime");

                entity.Property(e => e.Foto).HasColumnType("image");

                entity.Property(e => e.Obvs).HasMaxLength(20);
            });

            modelBuilder.Entity<MImgDoc>(entity =>
            {
                entity.HasKey(e => e.IdImgDoc);

                entity.ToTable("mImgDoc");

                entity.HasOne(d => d.IdTipoArchivoNavigation)
                    .WithMany(p => p.MImgDocs)
                    .HasForeignKey(d => d.IdTipoArchivo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_mImgDoc_mTipoArchivo");
            });

            modelBuilder.Entity<MImgDocAclaracion>(entity =>
            {
                entity.HasKey(e => e.IdImgDoc);

                entity.ToTable("mImgDocAclaracion");

                entity.Property(e => e.EvidenciaMr).HasColumnName("EvidenciaMR");
            });

            modelBuilder.Entity<MImgDocTicket>(entity =>
            {
                entity.HasKey(e => e.IdImgDoc);

                entity.ToTable("mImgDocTicket");

                entity.HasOne(d => d.IdTipoArchivoNavigation)
                    .WithMany(p => p.MImgDocTickets)
                    .HasForeignKey(d => d.IdTipoArchivo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_mImgDocTicket_mTipoArchivo");
            });

            modelBuilder.Entity<MImgGruposCofepri>(entity =>
            {
                entity.HasKey(e => e.IdImagen);

                entity.ToTable("mImgGruposCofepris");

                entity.Property(e => e.IdImagen).ValueGeneratedOnAdd();

                entity.Property(e => e.Imagen).HasColumnType("image");
            });

            modelBuilder.Entity<MTipoArchivo>(entity =>
            {
                entity.HasKey(e => e.IdTipoArchivo);

                entity.ToTable("mTipoArchivo");

                entity.Property(e => e.IdTipoArchivo).ValueGeneratedOnAdd();

                entity.Property(e => e.TipoArchivo).HasMaxLength(10);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
