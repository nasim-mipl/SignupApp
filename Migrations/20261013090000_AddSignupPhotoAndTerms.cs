using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SignupApp.Data;

#nullable disable

namespace SignupApp.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261013090000_AddSignupPhotoAndTerms")]
    public partial class AddSignupPhotoAndTerms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath", table: "Users", type: "text", nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "AgreeTerms", table: "Users", type: "boolean", nullable: false, defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PhotoPath", table: "Users");
            migrationBuilder.DropColumn(name: "AgreeTerms", table: "Users");
        }
    }
}