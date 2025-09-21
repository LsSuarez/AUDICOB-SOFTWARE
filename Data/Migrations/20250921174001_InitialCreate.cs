using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PROYECTO_AUDICOB.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Modificación de la columna "TwoFactorEnabled" para PostgreSQL
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ALTER COLUMN \"TwoFactorEnabled\" TYPE boolean USING \"TwoFactorEnabled\"::boolean;");
            
            // Modificación de la columna "PhoneNumberConfirmed" para PostgreSQL
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ALTER COLUMN \"PhoneNumberConfirmed\" TYPE boolean USING \"PhoneNumberConfirmed\"::boolean;");
            
            // Modificación de la columna "EmailConfirmed" para PostgreSQL
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ALTER COLUMN \"EmailConfirmed\" TYPE boolean USING \"EmailConfirmed\"::boolean;");
            
            // Conversión de la columna "LockoutEnabled" de integer a boolean
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ALTER COLUMN \"LockoutEnabled\" TYPE boolean USING \"LockoutEnabled\"::boolean;");
            
            // Conversión de la columna "LockoutEnd" de TEXT a "timestamp with time zone"
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ALTER COLUMN \"LockoutEnd\" TYPE timestamp with time zone USING \"LockoutEnd\"::timestamp with time zone;");
            
            // Otros cambios para columnas en "AspNetUserTokens"
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "AspNetUserTokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 128);

            // Otros cambios en la tabla "AspNetUsers"
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SecurityStamp",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedUserName",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "LockoutEnabled",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "EmailConfirmed",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyStamp",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AccessFailedCount",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            // Otros cambios en la tabla "AspNetUserRoles", "AspNetUserLogins", etc.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir la conversión explícita para "PhoneNumberConfirmed"
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ALTER COLUMN \"PhoneNumberConfirmed\" TYPE integer USING \"PhoneNumberConfirmed\"::integer;");
            
            // Revertir la conversión para "TwoFactorEnabled"
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ALTER COLUMN \"TwoFactorEnabled\" TYPE integer USING \"TwoFactorEnabled\"::integer;");
            
            // Revertir la conversión para "LockoutEnd"
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ALTER COLUMN \"LockoutEnd\" TYPE TEXT USING \"LockoutEnd\"::TEXT;");
            
            // Revertir la conversión para "EmailConfirmed"
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ALTER COLUMN \"EmailConfirmed\" TYPE integer USING \"EmailConfirmed\"::integer;");

            // Otros cambios para revertir columnas
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "AspNetUserTokens",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            // Otros cambios en las tablas si es necesario
        }
    }
}
