﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ACRS.Data;
using ACRS.Models;
using ACRS.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ACRS.Controllers
{
    [EnableCors("CORSPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : Controller
    {
        private CoursesController _coursesController;
        private readonly ApplicationDbContext _context;

        public DownloadController(ApplicationDbContext context, CoursesController coursesController)
        {
            _context = context;
            _coursesController = coursesController;
        }

        [HttpGet("waitlist/ineligible")]
        public async Task<ActionResult> DownWaitlistIneligableAsync()
        {
            List<Student> students = _context.Students.ToList();
            List<Course> courses = _context.Courses.ToList();
            // Assume all waitlist distinct!
            List<Waitlist> waitlists = _context.Waitlists.ToList();
            List<List<StudentEligibility>> allEligabilities = new List<List<StudentEligibility>>();

            foreach (Course course in courses)
            {
                allEligabilities.Add(await _coursesController.GetInEligableCourseByCourseIdAsync(course.CourseId));
            }

            Dictionary<Waitlist, StudentEligibility> failedWaitlistDict = new Dictionary<Waitlist, StudentEligibility>();

            foreach (Waitlist waitlist in waitlists)
            {
                bool hasMatch = false;

                foreach (List<StudentEligibility> studentEligibilities in allEligabilities)
                {
                    foreach (StudentEligibility eligibility in studentEligibilities)
                    {
                        if (waitlist.StudentId == eligibility.StudentId && waitlist.CourseId == eligibility.CourseId)
                        {
                            failedWaitlistDict.Add(waitlist, eligibility);
                            hasMatch = true;
                            break;
                        }
                    }

                    if (hasMatch)
                    {
                        break;
                    }
                }
            }

            List<KeyValuePair<Waitlist, StudentEligibility>> failedList = failedWaitlistDict.ToList();

            failedList.Sort((a, b) => a.Key.WaitlistId.CompareTo(b.Key.WaitlistId));

            List<string> headers = new List<string>()
            {
                "Id",
                "Student Id",
                "Student Name",
                "Course Id",
                "CRN",
                "Term",
                "Failed Prerequisites"
            };

            List<List<string>> data = new List<List<string>>();



            if (failedList.Count > 0)
            {
                KeyValuePair<Waitlist, StudentEligibility> last = failedList.Last();

                foreach (KeyValuePair<Waitlist, StudentEligibility> f in failedList)
                {
                    Waitlist waitlist = f.Key;
                    StudentEligibility eligibility = f.Value;

                    string studentName = null;

                    try
                    {
                        studentName = students.Where(s => s.StudentId == waitlist.StudentId).First().StudentName;
                    }
                    catch (Exception)
                    {
                        studentName = "";
                    }

                    List<string> row = new List<string>()
                {
                    waitlist.WaitlistId.ToString(),
                    waitlist.StudentId,
                    studentName,
                    waitlist.CourseId,
                    waitlist.CRN,
                    waitlist.Term,
                    eligibility.FailedPrereqs.Count > 0 ? eligibility.FailedPrereqs[0] : null
                };

                    data.Add(row);

                    if (eligibility.FailedPrereqs.Count > 1)
                    {
                        foreach (string courseId in eligibility.FailedPrereqs.Skip(1))
                        {
                            List<string> courseIdRow = new List<string>()
                        {
                            null, null, null, null, null, null, courseId
                        };

                            data.Add(courseIdRow);
                        }
                    }

                    data.Add(new List<string>(6));

                    if (!f.Equals(last))
                    {
                        data.Add(headers);
                    }
                }
            }

            // data = data.OrderBy(a => a[0]).ToList();

            Stream excel = ExcelWriter.CreateAsStream(headers, data);

            return new FileStreamResult(excel, "application/octet-stream")
            {
                FileDownloadName = "waitlist-ineligible.xlsx"
            };
        }

        [HttpGet("waitlist/eligible")]
        public async Task<ActionResult> DownloadWaitlistEligableAsync()
        {
            List<Student> students = _context.Students.ToList();
            List<Course> courses = _context.Courses.ToList();
            List<Waitlist> waitlists = _context.Waitlists.ToList();
            List<List<StudentEligibility>> allEligabilities = new List<List<StudentEligibility>>();

            foreach (Course course in courses)
            {
                allEligabilities.Add(await _coursesController.GetEligableCourseByCourseIdAsync(course.CourseId));
            }

            List<string> headers = new List<string>
            {
                "Id",
                "Student Id",
                "Student Name",
                "Course Id",
                "CRN",
                "Term"
            };

            List<List<string>> data = new List<List<string>>();

            foreach (Waitlist entry in waitlists)
            {
                bool isEligable = false;

                foreach (List<StudentEligibility> eligibilities in allEligabilities)
                {
                    isEligable = eligibilities.Any(s => s.CourseId == entry.CourseId && s.StudentId == entry.StudentId);

                    if (isEligable)
                    {
                        break;
                    }
                }

                if (!isEligable)
                {
                    continue;
                }

                string studentName = null;

                try
                {
                    studentName = students.Where(s => s.StudentId == entry.StudentId).First().StudentName;
                }
                catch (Exception)
                {
                    studentName = "";
                }

                data.Add(new List<string>
                {
                    entry.WaitlistId.ToString(),
                    entry.StudentId,
                    studentName,
                    entry.CourseId,
                    entry.CRN,
                    entry.Term
                });
            }

            Stream excel = ExcelWriter.CreateAsStream(headers, data);

            return new FileStreamResult(excel, "application/octet-stream")
            {
                FileDownloadName = "waitlist-eligible.xlsx"
            };
        }
    }
}
