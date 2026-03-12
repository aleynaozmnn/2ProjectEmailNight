using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project2_EmailNight.Migrations
{
    /// <inheritdoc />
    public partial class mig_add_isstarred : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStarred",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStarred",
                table: "Messages");
        }
    }
}
