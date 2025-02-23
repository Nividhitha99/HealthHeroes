using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Health_Heroes.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Email_Address = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Email_Address);
                });

            migrationBuilder.CreateTable(
                name: "Donor_Details",
                columns: table => new
                {
                    SSN = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Email_Address = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: true),
                    Blood_Type = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Illness = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Height = table.Column<double>(type: "REAL", nullable: true),
                    Weight = table.Column<double>(type: "REAL", nullable: true),
                    Gender = table.Column<char>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donor_Details", x => x.SSN);
                    table.ForeignKey(
                        name: "FK_Donor_Details_User_Email_Address",
                        column: x => x.Email_Address,
                        principalTable: "User",
                        principalColumn: "Email_Address",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Donor_Details_Email_Address",
                table: "Donor_Details",
                column: "Email_Address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Donor_Details");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
