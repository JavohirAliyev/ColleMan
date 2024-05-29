using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ColleMan.Migrations
{
    /// <inheritdoc />
    public partial class collectionCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Collections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Collections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
