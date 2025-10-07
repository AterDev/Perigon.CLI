import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { SystemUserService } from 'src/app/services/admin/system-user.service';
import { SystemUserUpdateDto } from
'src/app/services/admin/models/system-user-update-dto.model';
import { SystemUserDetailDto } from
'src/app/services/admin/models/system-user-detail-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { ToKeyValuePipe } from 'src/app/share/pipe/to-key-value.pipe';
import { GenderType } from 'src/app/services/admin/enum/gender-type.model';


@Component({
  selector: 'app-edit',
  imports: [...CommonFormModules, ToKeyValuePipe],
  templateUrl: './edit.html',
  styleUrls: ['./edit.scss']
})
export class Edit implements OnInit {
  GenderType = GenderType;

  formGroup!: FormGroup;
  id!: string;
  data = {} as SystemUserDetailDto;
  updateData = {} as SystemUserUpdateDto;
  isLoading = true;
  isProcessing = false;

  constructor(
    private service: SystemUserService,
    public snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location,
    public dialogRef: MatDialogRef<Edit>,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }
  ) {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id = id;
    } else {
      this.id = dlgData.id;
    }
  }

    get userName() { return this.formGroup.get('userName') as FormControl };
  get password() { return this.formGroup.get('password') as FormControl };
  get realName() { return this.formGroup.get('realName') as FormControl };
  get email() { return this.formGroup.get('email') as FormControl };
  get phoneNumber() { return this.formGroup.get('phoneNumber') as FormControl };
  get avatar() { return this.formGroup.get('avatar') as FormControl };
  get sex() { return this.formGroup.get('sex') as FormControl };


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
        userName: new FormControl(this.data.userName, [Validators.maxLength(30)]),
      password: new FormControl(null, [Validators.maxLength(60)]),
      realName: new FormControl(this.data.realName, [Validators.maxLength(30)]),
      email: new FormControl(this.data.email, [Validators.maxLength(100)]),
      phoneNumber: new FormControl(this.data.phoneNumber, [Validators.maxLength(20)]),
      avatar: new FormControl(this.data.avatar, [Validators.maxLength(200)]),
      sex: new FormControl(this.data.sex, []),

    });
  }

  getValidatorMessage(type: string): string {
    switch (type) {
      case 'userName':
      return this.userName?.hasError('required') ? '用户名必填' :
        this.userName?.hasError('maxlength') ? '用户名长度不超过30位': '';
    case 'password':
      return this.password?.hasError('required') ? '密码必填' :
        this.password?.hasError('maxlength') ? '密码长度不超过60位': '';
    case 'realName':
      return this.realName?.hasError('required') ? '真实姓名必填' :
        this.realName?.hasError('maxlength') ? '真实姓名长度不超过30位': '';
    case 'email':
      return this.email?.hasError('required') ? 'Email必填' :
        this.email?.hasError('maxlength') ? 'Email长度不超过100位': '';
    case 'phoneNumber':
      return this.phoneNumber?.hasError('required') ? 'PhoneNumber必填' :
        this.phoneNumber?.hasError('maxlength') ? 'PhoneNumber长度不超过20位': '';
    case 'avatar':
      return this.avatar?.hasError('required') ? '头像url必填' :
        this.avatar?.hasError('maxlength') ? '头像url长度不超过200位': '';
    case 'sex':
      return this.sex?.hasError('required') ? '性别必填' :
        this.sex?.hasError('maxlength') ? '性别长度不超过位': '';

      default:
        return '';
    }
  }

  edit(): void {
    if (this.formGroup.valid) {
      this.isProcessing = true;
      this.updateData = this.formGroup.value as SystemUserUpdateDto;

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

