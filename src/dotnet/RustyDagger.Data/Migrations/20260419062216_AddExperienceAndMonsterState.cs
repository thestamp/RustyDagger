using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RustyDagger.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExperienceAndMonsterState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipientAccountId = table.Column<int>(type: "integer", nullable: false),
                    FromName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rankings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HeroId = table.Column<int>(type: "integer", nullable: false),
                    HeroName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Fame = table.Column<int>(type: "integer", nullable: false),
                    Guts = table.Column<int>(type: "integer", nullable: false),
                    Wits = table.Column<int>(type: "integer", nullable: false),
                    Charm = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rankings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Heroes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Guts = table.Column<int>(type: "integer", nullable: false),
                    Wits = table.Column<int>(type: "integer", nullable: false),
                    Charm = table.Column<int>(type: "integer", nullable: false),
                    Wounds = table.Column<int>(type: "integer", nullable: false),
                    Fame = table.Column<int>(type: "integer", nullable: false),
                    Stipend = table.Column<int>(type: "integer", nullable: false),
                    Social = table.Column<int>(type: "integer", nullable: false),
                    Favor = table.Column<int>(type: "integer", nullable: false),
                    Actions = table.Column<int>(type: "integer", nullable: false),
                    Marks = table.Column<int>(type: "integer", nullable: false),
                    Place = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    FightRank = table.Column<int>(type: "integer", nullable: false),
                    MagicRank = table.Column<int>(type: "integer", nullable: false),
                    ThiefRank = table.Column<int>(type: "integer", nullable: false),
                    IeatsuRank = table.Column<int>(type: "integer", nullable: false),
                    Traits = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false),
                    GearJson = table.Column<string>(type: "text", nullable: false),
                    PackJson = table.Column<string>(type: "text", nullable: false),
                    LooksJson = table.Column<string>(type: "text", nullable: false),
                    CurrentMonsterJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastPlayed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heroes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Heroes_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Heroes_AccountId",
                table: "Heroes",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Heroes_Name",
                table: "Heroes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mails_RecipientAccountId",
                table: "Mails",
                column: "RecipientAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Rankings_Fame",
                table: "Rankings",
                column: "Fame",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Rankings_Level",
                table: "Rankings",
                column: "Level",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Heroes");

            migrationBuilder.DropTable(
                name: "Mails");

            migrationBuilder.DropTable(
                name: "Rankings");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
