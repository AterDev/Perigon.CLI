import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { SystemUserService } from 'src/app/services/admin/system-user.service';
import { SystemUserAddDto } from
  'src/app/services/admin/models/system-user-add-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { GenderType } from 'src/app/services/admin/enum/gender-type.model';
import { SystemRoleService } from 'src/app/services/admin/system-role.service';
import { forkJoin, Observable } from 'rxjs';
import { SystemRoleItemDto } from 'src/app/services/admin/models/system-role-item-dto.model';
import { PageListOfSystemRoleItemDto } from 'src/app/services/admin/models/page-list-of-system-role-item-dto.model';



@Component({
  selector: 'app-add',
  imports: [...CommonFormModules],
  templateUrl: './add.html',
  styleUrls: ['./add.scss']
})
export class Add implements OnInit {
  GenderType = GenderType;
  formGroup!: FormGroup;
  data = {} as SystemUserAddDto;
  roles = [] as SystemRoleItemDto[];
  isLoading = true;
  isProcessing = false;
  constructor(
    private service: SystemUserService,
    private roleService: SystemRoleService,
    public snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location,
    public dialogRef: MatDialogRef<Add>,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }
  ) {

  }

  get userName() { return this.formGroup.get('userName') as FormControl };
  get password() { return this.formGroup.get('password') as FormControl };
  get realName() { return this.formGroup.get('realName') as FormControl };
  get email() { return this.formGroup.get('email') as FormControl };
  get roleIds() { return this.formGroup.get('roleIds') as FormControl };

  ngOnInit(): void {

    this.initData();
  }

  initData(): void {

    forkJoin([this.getRoles()])
      .subscribe({
        next: ([roles]) => {
          if (roles.data) {
            this.roles = roles.data;
          }
        },
        complete: () => {
          this.isLoading = false;
          this.initForm();
        }
      });
  }
  getRoles(): Observable<PageListOfSystemRoleItemDto> {
    {
      return this.roleService.filter({
        pageIndex: 1,
        pageSize: 99
      });
    }
  }

  initForm(): void {
    this.formGroup = new FormGroup({
      userName: new FormControl(null, [Validators.required, Validators.maxLength(30)]),
      password: new FormControl(null, [Validators.required, Validators.maxLength(60)]),
      realName: new FormControl(null, [Validators.maxLength(30)]),
      email: new FormControl(null, [Validators.maxLength(100)]),
      roleIds: new FormControl(null, [Validators.required]),
    });
  }
  getValidatorMessage(type: string): string {
    switch (type) {
      case 'userName':
        return this.userName?.hasError('required') ? '用户名必填' :
          this.userName?.hasError('maxlength') ? '用户名长度不超过30位' : '';
      case 'password':
        return this.password?.hasError('required') ? '密码必填' :
          this.password?.hasError('maxlength') ? '密码长度不超过60位' : '';
      case 'realName':
        return this.realName?.hasError('required') ? '真实姓名必填' :
          this.realName?.hasError('maxlength') ? '真实姓名长度不超过30位' : '';
      case 'email':
        return this.email?.hasError('required') ? '邮箱必填' :
          this.email?.hasError('maxlength') ? '邮箱长度不超过100位' : '';
      default:
        return '';
    }
  }

  add(): void {
    if (this.formGroup.valid) {
      this.isProcessing = true;
      const data = this.formGroup.value as SystemUserAddDto;
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

