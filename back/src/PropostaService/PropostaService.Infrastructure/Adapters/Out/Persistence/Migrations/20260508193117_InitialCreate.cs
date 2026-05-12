using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PropostaService.Infrastructure.Adapters.Out.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposSeguro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Chave = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposSeguro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Propostas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeCliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DocumentoCliente = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TipoSeguroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoSeguro = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ValorSeguro = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propostas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Propostas_TiposSeguro_TipoSeguroId",
                        column: x => x.TipoSeguroId,
                        principalTable: "TiposSeguro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TiposSeguro",
                columns: new[] { "Id", "Chave", "Nome" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Auto", "Seguro Auto" },
                    { new Guid("12345678-1234-1234-1234-123456789001"), "Transporte", "Seguro Transporte" },
                    { new Guid("12345678-1234-1234-1234-123456789002"), "ResponsabilidadeCivil", "Seguro Responsabilidade Civil" },
                    { new Guid("12345678-1234-1234-1234-123456789003"), "Pet", "Seguro Pet" },
                    { new Guid("12345678-1234-1234-1234-123456789004"), "AcidentesPessoais", "Seguro Acidentes Pessoais" },
                    { new Guid("12345678-1234-1234-1234-123456789005"), "Garantia", "Seguro Garantia" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Residencial", "Seguro Residencial" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Vida", "Seguro Vida" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Empresarial", "Seguro Empresarial" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Viagem", "Seguro Viagem" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Saude", "Seguro Saude" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Odontologico", "Seguro Odontologico" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "Moto", "Seguro Moto" },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "Celular", "Seguro Celular" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Equipamentos", "Seguro Equipamentos" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Patrimonial", "Seguro Patrimonial" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Condominio", "Seguro Condominio" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Previdencia", "Seguro Previdencia" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Rural", "Seguro Rural" },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Nautico", "Seguro Nautico" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_TipoSeguroId",
                table: "Propostas",
                column: "TipoSeguroId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposSeguro_Chave",
                table: "TiposSeguro",
                column: "Chave",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Propostas");

            migrationBuilder.DropTable(
                name: "TiposSeguro");
        }
    }
}
