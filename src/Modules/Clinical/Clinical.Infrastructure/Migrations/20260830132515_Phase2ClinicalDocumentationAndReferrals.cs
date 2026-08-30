using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Clinical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase2ClinicalDocumentationAndReferrals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clinical_documentations",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HistoryOfPresentingIllness = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    PastMedicalHistory = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PastSurgicalHistory = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    FamilyHistory = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SocialHistory = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    GynaecologicalHistory = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ObstetricHistory = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DrugHistory = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RosGeneral = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RosCardiovascular = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RosRespiratory = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RosGastrointestinal = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RosGenitourinary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RosMusculoskeletal = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RosNeurological = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RosDermatological = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RosEntEyes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RosEndocrine = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExamGeneralAppearance = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExamHeadAndNeck = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExamCardiovascular = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExamRespiratory = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExamAbdominal = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExamGenitourinary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExamMusculoskeletal = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExamNeurological = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExamSkin = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExamLymphatic = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LastSavedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSavedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinical_documentations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clinical_documentations_consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "clinical",
                        principalTable: "consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "referrals",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferredToFacility = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReferredToUnit = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReferredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_referrals_consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "clinical",
                        principalTable: "consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clinical_documentations_ConsultationId",
                schema: "clinical",
                table: "clinical_documentations",
                column: "ConsultationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_referrals_ConsultationId",
                schema: "clinical",
                table: "referrals",
                column: "ConsultationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinical_documentations",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "referrals",
                schema: "clinical");
        }
    }
}
