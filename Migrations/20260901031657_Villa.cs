using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalVilla_API.Migrations
{
    /// <inheritdoc />
    public partial class Villa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "Villa",
                newName: "UpdateDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdateDate",
                table: "Villa",
                newName: "UpdatedDate");
        }
    }
}
