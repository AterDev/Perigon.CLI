import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { CustomerInfoService } from 'src/app/services/admin/customer-info/customer-info.service';
import { CustomerInfoUpdateDto } from
  'src/app/services/admin/customer-info/models/customer-info-update-dto.model';
import { CustomerInfoDetailDto } from
  'src/app/services/admin/customer-info/models/customer-info-detail-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/app.config';
import { ToKeyValuePipe } from 'src/app/share/pipe/to-key-value.pipe';
import { CustomerStatus } from 'src/app/services/admin/enum/models/customer-status.model';


@Component({
  selector: 'app-edit',
  imports: [...CommonFormModules, ToKeyValuePipe],
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit {
  CustomerStatus = CustomerStatus;

  formGroup!: FormGroup;
  id!: string;
  data = {} as CustomerInfoDetailDto;
  updateData = {} as CustomerInfoUpdateDto;
  isLoading = true;
  isProcessing = false;

  constructor(
    private service: CustomerInfoService,
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

  get remark() { return this.formGroup.get('remark') as FormControl };
  get status() { return this.formGroup.get('status') as FormControl };
  get password() { return this.formGroup.get('password') as FormControl };


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
      remark: new FormControl(this.data.remark, [Validators.maxLength(500)]),
      status: new FormControl(this.data.status, []),
      password: new FormControl(null, [Validators.maxLength(100)]),

    });
  }

  getValidatorMessage(type: string): string {
    switch (type) {
      case 'remark':
        return this.remark?.hasError('required') ? '备注必填' :
          this.remark?.hasError('maxlength') ? '备注长度不超过500位' : '';
      case 'status':
        return this.status?.hasError('required') ? '状态必填' :
          this.status?.hasError('maxlength') ? '状态长度不超过位' : '';
      case 'password':
        return this.password?.hasError('required') ? 'Password必填' :
          this.password?.hasError('maxlength') ? 'Password长度不超过100位' : '';

      default:
        return '';
    }
  }

  edit(): void {
    if (this.formGroup.valid) {
      this.isProcessing = true;
      this.updateData = this.formGroup.value as CustomerInfoUpdateDto;

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

