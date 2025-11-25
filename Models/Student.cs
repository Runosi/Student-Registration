using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProject.Models
{
    [Table("STUDENTS", Schema = "STUDENT38")] // match your schema and table name
    public class Student
    {
        [Key]
        [Column("STUDENTIDINTERNAL")]
        public int StudentIdInternal { get; set; }

        [Required(ErrorMessage= "First name is required.")]
        [Column("FIRSTNAME")]
        public string FirstName { get; set; }

        [Column("MIDDLENAME")]
        public string? MiddleName { get; set; }

        [Column("LASTNAME")]
        public string LastName { get; set; }

        [Column("EMAIL")]
        public string Email { get; set; }

        [Column("PASSWORD")]
        public string Password { get; set; }

        [Column("PHONE")]
        public string Phone { get; set; }

        [Column("PROGRAM")]
        public string Program { get; set; }

        [Column("ENROLLMENTTYPE")]
        public string EnrollmentType { get; set; }

        [Column("EXPECTEDGRADUATIONYEAR")]
        public int? ExpectedGraduationYear { get; set; }

        [Column("ISVERIFIED")]
        public int IsVerified { get; set; } = 0;

        [Column("VERIFICATIONTOKEN")]
        public string? VerificationToken { get; set; }

        [Required(ErrorMessage="Date of Birth is required.")]
        [Column("DATEOFBIRTH")]
        public DateTime DateOfBirth { get; set; }

        [Column("NATIONALITY")]
        public string? Nationality { get; set; }

        [Column("GENDER")]
        public string Gender { get; set; }

        [Column("STREETADDRESS")]
        public string? StreetAddress { get; set; }

        [Column("CITY")]
        public string? City { get; set; }

        [Column("PROVINCE")]
        public string? Province { get; set; }

        [Column("ZIPCODE")]
        public string? ZipCode { get; set; }

        [Column("COUNTRY")]
        public string? Country { get; set; }

        [Column("ADDITIONALINFO")]
        public string? AdditionalInfo { get; set; }

        [Column("REGISTRATIONDATE")]
        public DateTime RegistrationDate { get; set; }
    }
}