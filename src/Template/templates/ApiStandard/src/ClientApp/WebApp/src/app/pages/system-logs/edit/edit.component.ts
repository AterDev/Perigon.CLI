import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { SystemLogsService } from 'src/app/services/admin/system-logs/system-logs.service';
import { SystemLogsUpdateDto } from
'src/app/services/admin/system-logs/models/system-logs-update-dto.model';
import { SystemLogsDetailDto } from
'src/app/services/admin/system-logs/models/system-logs-detail-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/app.config';
import { ToKeyValuePipe } from 'src/app/share/pipe/to-key-value.pipe';


@Component({
  selector: 'app-edit',
  imports: [...CommonFormModules, ToKeyValuePipe],
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit {
  
  formGroup!: FormGroup;
  id!: string;
  data = {} as SystemLogsDetailDto;
  updateData = {} as SystemLogsUpdateDto;
  isLoading = true;
  isProcessing = false;

  constructor(
    private service: SystemLogsService,
    public snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location,
    public dialogRef: MatDialogRef<EditComponent>,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }
  ) {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id = id;
    } else {
      this.id = dlgData.id;
    }
  }

  

  ngOnInit(): void {
    this.getDetail();
  }

   getDetail(): void {
    this.service.getDetail(this.id)
      .subscribe({
        next: (res) => {
          if (res) {
            this.data = res;
            this.initForm();
            this.isLoading = false;
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isLoading = false;
        }
      });
  } 


  initForm(): void {
    this.formGroup = new FormGroup({
      
    });
  }

  getValidatorMessage(type: string): string {
    switch (type) {
      
      default:
        return '';
    }
  }

  edit(): void {
    if (this.formGroup.valid) {
      this.isProcessing = true;
      this.updateData = this.formGroup.value as SystemLogsUpdateDto;

      this.service.update(this.id, this.updateData)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('修改成功');
              this.dialogRef.close(res);
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
            this.isProcessing = false;
          },
          complete: () => {
            this.isProcessing = false;
          }
        });
    } else {
      this.snb.open('表单验证不通过，请检查填写的内容!');
    }
  }

  back(): void {
    this.location.back();
  }
}

