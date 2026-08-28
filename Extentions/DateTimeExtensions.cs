using System;

namespace VeterinaryClinic.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Calculates a person's/pet's age in whole years as of today,
        /// correctly accounting for whether their birthday has occurred yet this year.
        /// </summary>
        public static int CalculateAge(this DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
