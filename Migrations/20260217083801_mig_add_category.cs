using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project2_EmailNight.Migrations
{
    /// <inheritdoc />
    public partial class mig_add_category : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Messages");
        }
    }
}
