import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { SystemLogsService } from 'src/app/services/admin/system-logs/system-logs.service';
import { SystemLogs } from 'src/app/services/admin/system-logs/models/system-logs.model';
import { SystemLogsAddDto } from
'src/app/services/admin/system-logs/models/system-logs-add-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/app.config';
import { ToKeyValuePipe } from 'src/app/share/pipe/to-key-value.pipe';



@Component({
  selector: 'app-add',
  imports: [...CommonFormModules, ToKeyValuePipe],
  templateUrl: './add.component.html',
  styleUrls: ['./add.component.scss']
})
export class AddComponent implements OnInit {
  
  formGroup!: FormGroup;
  data = {} as SystemLogsAddDto;
  isLoading = true;
  isProcessing = false;
  constructor(
    private service: SystemLogsService,
    public snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location,
    public dialogRef: MatDialogRef<AddComponent>,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }
  ) {

  }


  ngOnInit(): void {
    this.initForm();
    this.isLoading = false;
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

  add(): void {
    if (this.formGroup.valid) {
      this.isProcessing = true;
      const data = this.formGroup.value as SystemLogsAddDto;
      this.service.add(data)
      .subscribe({
        next: (res) => {
          if (res) {
            this.snb.open('添加成功');
            this.dialogRef.close(res);
            //this.router.navigate(['../index'], { relativeTo: this.route });
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

