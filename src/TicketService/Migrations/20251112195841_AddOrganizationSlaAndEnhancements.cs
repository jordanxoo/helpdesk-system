using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketService.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationSlaAndEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "sla_id",
                table: "tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "slas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    response_time_critical = table.Column<int>(type: "integer", nullable: false),
                    response_time_high = table.Column<int>(type: "integer", nullable: false),
                    response_time_medium = table.Column<int>(type: "integer", nullable: false),
                    response_time_low = table.Column<int>(type: "integer", nullable: false),
                    resolution_time_critical = table.Column<int>(type: "integer", nullable: false),
                    resolution_time_high = table.Column<int>(type: "integer", nullable: false),
                    resolution_time_medium = table.Column<int>(type: "integer", nullable: false),
                    resolution_time_low = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_slas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ticket_attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    storage_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    download_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_attachments", x => x.id);
                    table.ForeignKey(
                        name: "FK_ticket_attachments_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    field_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    old_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    new_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_ticket_audit_logs_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sla_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                    table.ForeignKey(
                        name: "FK_organizations_slas_sla_id",
                        column: x => x.sla_id,
                        principalTable: "slas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ticket_tags",
                columns: table => new
                {
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_tags", x => new { x.tag_id, x.ticket_id });
                    table.ForeignKey(
                        name: "FK_ticket_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ticket_tags_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_organization_id",
                table: "tickets",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_sla_id",
                table: "tickets",
                column: "sla_id");

            migrationBuilder.CreateIndex(
                name: "ix_organizations_is_active",
                table: "organizations",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_organizations_name",
                table: "organizations",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_organizations_sla_id",
                table: "organizations",
                column: "sla_id");

            migrationBuilder.CreateIndex(
                name: "ix_slas_is_active",
                table: "slas",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_slas_name",
                table: "slas",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ticket_attachments_ticket_id",
                table: "ticket_attachments",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_attachments_uploaded_at",
                table: "ticket_attachments",
                column: "uploaded_at");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_attachments_uploaded_by_id",
                table: "ticket_attachments",
                column: "uploaded_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_audit_logs_action",
                table: "ticket_audit_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_audit_logs_created_at",
                table: "ticket_audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_audit_logs_ticket_id",
                table: "ticket_audit_logs",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_audit_logs_user_id",
                table: "ticket_audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_tags_ticket_id",
                table: "ticket_tags",
                column: "ticket_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tickets_organizations_organization_id",
                table: "tickets",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tickets_slas_sla_id",
                table: "tickets",
                column: "sla_id",
                principalTable: "slas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tickets_organizations_organization_id",
                table: "tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_tickets_slas_sla_id",
                table: "tickets");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "ticket_attachments");

            migrationBuilder.DropTable(
                name: "ticket_audit_logs");

            migrationBuilder.DropTable(
                name: "ticket_tags");

            migrationBuilder.DropTable(
                name: "slas");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropIndex(
                name: "ix_tickets_organization_id",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "ix_tickets_sla_id",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "sla_id",
                table: "tickets");
        }
    }
}
