﻿using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GirafRest.Migrations
{
    public partial class manytomanyReleationBetweenActivitiesAndPictograms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Pictograms_PictogramKey",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_PictogramKey",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "PictogramKey",
                table: "Activities");

            migrationBuilder.CreateTable(
                name: "PictogramRelations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActivityId = table.Column<long>(nullable: false),
                    PictogramId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PictogramRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PictogramRelations_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PictogramRelations_Pictograms_PictogramId",
                        column: x => x.PictogramId,
                        principalTable: "Pictograms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PictogramRelations_ActivityId",
                table: "PictogramRelations",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_PictogramRelations_PictogramId",
                table: "PictogramRelations",
                column: "PictogramId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PictogramRelations");

            migrationBuilder.AddColumn<long>(
                name: "PictogramKey",
                table: "Activities",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_PictogramKey",
                table: "Activities",
                column: "PictogramKey");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Pictograms_PictogramKey",
                table: "Activities",
                column: "PictogramKey",
                principalTable: "Pictograms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
