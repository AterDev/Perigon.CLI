import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { SystemRoleService } from 'src/app/services/admin/system-role.service';
import { SystemRoleUpdateDto } from
  'src/app/services/admin/models/system-role-update-dto.model';
import { SystemRoleDetailDto } from
  'src/app/services/admin/models/system-role-detail-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/share/shared-modules';

@Component({
  selector: 'app-edit',
  imports: [...CommonFormModules],
  templateUrl: './edit.html',
  styleUrls: ['./edit.scss']
})
export class Edit implements OnInit {

  formGroup!: FormGroup;
  id!: string;
  data = {} as SystemRoleDetailDto;
  updateData = {} as SystemRoleUpdateDto;
  isLoading = true;
  isProcessing = false;

  constructor(
    private service: SystemRoleService,
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

  get name() { return this.formGroup.get('name') as FormControl };
  get nameValue() { return this.formGroup.get('nameValue') as FormControl };
  get isSystem() { return this.formGroup.get('isSystem') as FormControl };


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
      name: new FormControl(this.data.name, [Validators.maxLength(30)]),
      nameValue: new FormControl(this.data.nameValue, [Validators.maxLength(60)]),
      isSystem: new FormControl(this.data.isSystem, []),

    });
  }

  getValidatorMessage(type: string): string {
    switch (type) {
      case 'name':
        return this.name?.hasError('required') ? '角色显示名称必填' :
          this.name?.hasError('maxlength') ? '角色显示名称长度不超过30位' : '';
      case 'nameValue':
        return this.nameValue?.hasError('required') ? '角色名，系统标识必填' :
          this.nameValue?.hasError('maxlength') ? '角色名，系统标识长度不超过60位' : '';
      case 'isSystem':
        return this.isSystem?.hasError('required') ? '是否系统内置,系统内置不可删除必填' :
          this.isSystem?.hasError('maxlength') ? '是否系统内置,系统内置不可删除长度不超过位' : '';

      default:
        return '';
    }
  }

  edit(): void {
    if (this.formGroup.valid) {
      this.isProcessing = true;
      this.updateData = this.formGroup.value as SystemRoleUpdateDto;

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

