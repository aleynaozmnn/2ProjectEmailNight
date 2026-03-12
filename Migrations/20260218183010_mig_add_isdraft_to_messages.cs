using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project2_EmailNight.Migrations
{
    /// <inheritdoc />
    public partial class mig_add_isdraft_to_messages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Messages");
        }
    }
}
