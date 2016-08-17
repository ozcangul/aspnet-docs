using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ContosoUniversity.Migrations
{
    public partial class ComplexDataModel2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Instructors_InstructorID",
                table: "Course");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseInstructor_Instructors_InstructorID",
                table: "CourseInstructor");

            migrationBuilder.DropForeignKey(
                name: "FK_Department_Instructors_InstructorID",
                table: "Department");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeAssignment_Instructors_InstructorID",
                table: "OfficeAssignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Instructors",
                table: "Instructors");

            migrationBuilder.DropIndex(
                name: "IX_Course_InstructorID",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "InstructorID",
                table: "Course");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Instructor",
                table: "Instructors",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseInstructor_Instructor_InstructorID",
                table: "CourseInstructor",
                column: "InstructorID",
                principalTable: "Instructors",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Instructor_InstructorID",
                table: "Department",
                column: "InstructorID",
                principalTable: "Instructors",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeAssignment_Instructor_InstructorID",
                table: "OfficeAssignment",
                column: "InstructorID",
                principalTable: "Instructors",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.RenameTable(
                name: "Instructors",
                newName: "Instructor");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseInstructor_Instructor_InstructorID",
                table: "CourseInstructor");

            migrationBuilder.DropForeignKey(
                name: "FK_Department_Instructor_InstructorID",
                table: "Department");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeAssignment_Instructor_InstructorID",
                table: "OfficeAssignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Instructor",
                table: "Instructor");

            migrationBuilder.AddColumn<int>(
                name: "InstructorID",
                table: "Course",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Instructors",
                table: "Instructor",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_Course_InstructorID",
                table: "Course",
                column: "InstructorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Instructors_InstructorID",
                table: "Course",
                column: "InstructorID",
                principalTable: "Instructor",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseInstructor_Instructors_InstructorID",
                table: "CourseInstructor",
                column: "InstructorID",
                principalTable: "Instructor",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Instructors_InstructorID",
                table: "Department",
                column: "InstructorID",
                principalTable: "Instructor",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeAssignment_Instructors_InstructorID",
                table: "OfficeAssignment",
                column: "InstructorID",
                principalTable: "Instructor",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.RenameTable(
                name: "Instructor",
                newName: "Instructors");
        }
    }
}
