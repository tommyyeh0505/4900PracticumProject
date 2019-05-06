import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, MatSort, MatTableDataSource, MatTooltipModule } from '@angular/material';
import { NgModule } from '@angular/core';
import { Course } from 'src/app/models/course';
import { CourseService } from 'src/app/services/course.service';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { Router } from '@angular/router';



/**
 * @title Data table with sorting, pagination, and filtering.
 */
@Component({
  selector: 'course',
  styleUrls: ['course.component.css'],
  templateUrl: 'course.component.html',
})

export class CourseComponent implements OnInit {
  displayedColumns: string[] = ['courseId', 'crn', 'term', 'passingGrade', 'view', 'delete'];
  dataSource: MatTableDataSource<Course>;
  courses: Course[] = [];
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  constructor(
    private courseServce: CourseService,
    private authService: AuthenticationService,
    private router: Router
  ) {
    this.getCourses();
    // Create 100 users


    // Assign the data to the data source for the table to render


  }
  ngOnInit() {
    this.getCourses();
  }

  initTable(data) {
    this.dataSource = new MatTableDataSource(data);
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  getCourses() {
    this.courseServce.getCourses().subscribe((data: Course[]) => {
      this.courses = data;
      this.initTable(this.courses);
    });

  }

  getCourse(course: Course) {

  }


  deleteCourse(course: Course) {
    this.courseServce.deleteCourse(course);
    let itemIndex = this.dataSource.data.findIndex(obj => obj.courseId === course.courseId);
    this.dataSource.data.splice(itemIndex, 1);
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

  }


  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }
}

