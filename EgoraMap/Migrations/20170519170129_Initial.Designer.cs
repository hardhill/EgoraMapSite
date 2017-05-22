using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using EgoraMap.Models;

namespace EgoraMap.Migrations
{
    [DbContext(typeof(DbEgoraContext))]
    [Migration("20170519170129_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("EgoraMap.Models.Participant", b =>
                {
                    b.Property<int>("ParticipantId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateReg");

                    b.Property<string>("Email");

                    b.Property<string>("Name");

                    b.Property<string>("Phone");

                    b.HasKey("ParticipantId");

                    b.ToTable("Participants");
                });

            modelBuilder.Entity("EgoraMap.Models.Photo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("PhotoName")
                        .IsRequired();

                    b.Property<DateTime>("Photocreated");

                    b.Property<int?>("RouteId");

                    b.HasKey("Id");

                    b.HasIndex("RouteId");

                    b.ToTable("Photos");
                });

            modelBuilder.Entity("EgoraMap.Models.Route", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("RouteImage")
                        .IsRequired();

                    b.Property<string>("RouteKML")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Routes");
                });

            modelBuilder.Entity("EgoraMap.Models.Photo", b =>
                {
                    b.HasOne("EgoraMap.Models.Route", "Route")
                        .WithMany("Photos")
                        .HasForeignKey("RouteId");
                });
        }
    }
}
