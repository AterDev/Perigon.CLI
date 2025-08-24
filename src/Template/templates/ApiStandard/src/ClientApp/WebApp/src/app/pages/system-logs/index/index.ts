import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { SystemLogsService } from 'src/app/services/admin/system-logs.service';
import { SystemLogsItemDto } from 'src/app/services/admin/models/system-logs-item-dto.model';
import { SystemLogsFilterDto } from 'src/app/services/admin/models/system-logs-filter-dto.model';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { FormGroup } from '@angular/forms';
import { Observable, forkJoin } from 'rxjs';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { TypedCellDefDirective } from 'src/app/share/typed-cell-def.directive';
import { UserActionType } from 'src/app/services/admin/enum/user-action-type.model';
import { EnumTextPipe } from 'src/app/pipe/admin/enum-text.pipe';
import { ToKeyValuePipe } from 'src/app/share/pipe/to-key-value.pipe';
import { PageListOfSystemLogsItemDto } from 'src/app/services/admin/models/page-list-of-system-logs-item-dto.model';


@Component({
  selector: 'app-index',
  imports: [...CommonListModules, ...CommonFormModules, TypedCellDefDirective, ToKeyValuePipe, EnumTextPipe],
  templateUrl: './index.html',
  styleUrls: ['./index.scss']
})
export class Index implements OnInit {
  UserActionType = UserActionType;

  @ViewChild(MatPaginator, { static: true }) paginator!: MatPaginator;
  isLoading = true;
  isProcessing = false;
  total = 0;
  data: SystemLogsItemDto[] = [];
  columns: string[] = ['actionUserName','targetName','actionType','createdTime', 'actions'];
  dataSource!: MatTableDataSource<SystemLogsItemDto>;
  dialogRef!: MatDialogRef<{}, any>;
  @ViewChild('myDialog', { static: true }) myTmpl!: TemplateRef<{}>;
  mydialogForm!: FormGroup;
  filter: SystemLogsFilterDto;
  pageSizeOption = [12, 20, 50];
  constructor(
    private service: SystemLogsService,
    private snb: MatSnackBar,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
  ) {

    this.filter = {
      pageIndex: 1,
      pageSize: 12
    };
  }

  ngOnInit(): void {
    forkJoin([this.getListAsync()])
    .subscribe({
      next: ([res]) => {
        if (res) {
          if (res.data) {
            this.data = res.data;
            this.total = res.count;
            this.dataSource = new MatTableDataSource<SystemLogsItemDto>(this.data);
          }
        }
      },
      error: (error) => {
        this.snb.open(error.detail);
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  getListAsync(): Observable<PageListOfSystemLogsItemDto> {
    return this.service.filter(this.filter);
  }

  getList(event?: PageEvent): void {
    if(event) {
      this.filter.pageIndex = event.pageIndex + 1;
      this.filter.pageSize = event.pageSize;
    }
    this.service.filter(this.filter)
      .subscribe({
        next: (res) => {
          if (res) {
            if (res.data) {
              this.data = res.data;
              this.total = res.count;
              this.dataSource = new MatTableDataSource<SystemLogsItemDto>(this.data);
            }
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isLoading = false;
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }

  jumpTo(pageNumber: string): void {
    const number = parseInt(pageNumber);
    if (number > 0 && number < this.paginator.getNumberOfPages()) {
      this.filter.pageIndex = number;
      this.getList();
    }
  }

}

