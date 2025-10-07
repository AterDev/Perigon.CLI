import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { SystemRoleService } from 'src/app/services/admin/system-role.service';
import { SystemRoleAddDto } from
  'src/app/services/admin/models/system-role-add-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/share/shared-modules';

@Component({
  selector: 'app-add',
  imports: [...CommonFormModules],
  templateUrl: './add.html',
  styleUrls: ['./add.scss']
})
export class Add implements OnInit {

  formGroup!: FormGroup;
  data = {} as SystemRoleAddDto;
  isLoading = true;
  isProcessing = false;
  constructor(
    private service: SystemRoleService,
    public snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location,
    public dialogRef: MatDialogRef<Add>,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }
  ) {

  }

  get name() { return this.formGroup.get('name') as FormControl };
  get nameValue() { return this.formGroup.get('nameValue') as FormControl };
  get isSystem() { return this.formGroup.get('isSystem') as FormControl };

  ngOnInit(): void {
    this.initForm();
    this.isLoading = false;
  }

  initForm(): void {
    this.formGroup = new FormGroup({
      name: new FormControl(null, [Validators.required, Validators.maxLength(30)]),
      nameValue: new FormControl(null, [Validators.required, Validators.maxLength(60)]),
      isSystem: new FormControl(false, []),

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

  add(): void {
    if (this.formGroup.valid) {
      this.isProcessing = true;
      const data = this.formGroup.value as SystemRoleAddDto;
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

