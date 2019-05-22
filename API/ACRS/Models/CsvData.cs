﻿using CsvHelper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ACRS.Models
{
    public class CsvData
    {
        public string CRN { get; set; }
        public string CourseId { get; set; }
        public string Term { get; set; }
        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public int FinalGrade { get; set; }
        public string RawGrade { get; set; }

        public CsvData(CsvReader reader, IConfiguration configuration)
        {
            CourseId = reader[configuration.GetValue<int>("CsvColumns:CourseId")];
            CRN = reader[configuration.GetValue<int>("CsvColumns:CRN")];
            Term = reader[configuration.GetValue<int>("CsvColumns:Term")];
            StudentId = reader[configuration.GetValue<int>("CsvColumns:StudentId")];
            StudentName = reader[configuration.GetValue<int>("CsvColumns:StudentName")];
            RawGrade = reader[configuration.GetValue<int>("CsvColumns:Grade")].Trim();
        }
    }
}
