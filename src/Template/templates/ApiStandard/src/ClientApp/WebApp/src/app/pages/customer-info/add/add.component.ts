import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { CustomerInfoService } from 'src/app/services/admin/customer-info/customer-info.service';
import { CustomerInfo } from 'src/app/services/admin/customer-info/models/customer-info.model';
import { CustomerInfoAddDto } from
'src/app/services/admin/customer-info/models/customer-info-add-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/app.config';
import { ToKeyValuePipe } from 'src/app/share/pipe/to-key-value.pipe';
import { TeamType } from 'src/app/services/admin/enum/models/team-type.model';
import { GenderType } from 'src/app/services/admin/enum/models/gender-type.model';



@Component({
  selector: 'app-add',
  imports: [...CommonFormModules, ToKeyValuePipe],
  templateUrl: './add.component.html',
  styleUrls: ['./add.component.scss']
})
export class AddComponent implements OnInit {
  TeamType = TeamType;
GenderType = GenderType;

  formGroup!: FormGroup;
  data = {} as CustomerInfoAddDto;
  isLoading = true;
  isProcessing = false;
  constructor(
    private service: CustomerInfoService,
    public snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location,
    public dialogRef: MatDialogRef<AddComponent>,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }
  ) {

  }

  get userName() { return this.formGroup.get('userName') as FormControl }; 
  get teamType() { return this.formGroup.get('teamType') as FormControl }; 
  get password() { return this.formGroup.get('password') as FormControl }; 
  get genderType() { return this.formGroup.get('genderType') as FormControl }; 
  get remark() { return this.formGroup.get('remark') as FormControl }; 
  get teamId() { return this.formGroup.get('teamId') as FormControl }; 

  ngOnInit(): void {
    this.initForm();
    this.isLoading = false;
  }

  initForm(): void {
    this.formGroup = new FormGroup({
      userName: new FormControl(null, [Validators.required, Validators.maxLength(200)]),
      teamType: new FormControl(null, []),
      password: new FormControl(null, [Validators.maxLength(200)]),
      genderType: new FormControl(null, []),
      remark: new FormControl(null, [Validators.maxLength(500)]),
      teamId: new FormControl(null, []),

    });
  }
  getValidatorMessage(type: string): string {
    switch (type) {
      case 'userName':
        return this.userName?.hasError('required') ? '用户名必填' : 
          this.userName?.hasError('maxlength') ? '用户名长度不超过200位': '';
      case 'teamType':
        return this.teamType?.hasError('required') ? '团队类型必填' : 
          this.teamType?.hasError('maxlength') ? '团队类型长度不超过位': '';
      case 'password':
        return this.password?.hasError('required') ? '密码必填' : 
          this.password?.hasError('maxlength') ? '密码长度不超过200位': '';
      case 'genderType':
        return this.genderType?.hasError('required') ? '性别必填' : 
          this.genderType?.hasError('maxlength') ? '性别长度不超过位': '';
      case 'remark':
        return this.remark?.hasError('required') ? '说明备注必填' : 
          this.remark?.hasError('maxlength') ? '说明备注长度不超过500位': '';
      case 'teamId':
        return this.teamId?.hasError('required') ? '团队id必填' : 
          this.teamId?.hasError('maxlength') ? '团队id长度不超过位': '';
      
      default:
        return '';
    }
  }

  add(): void {
    if (this.formGroup.valid) {
      this.isProcessing = true;
      const data = this.formGroup.value as CustomerInfoAddDto;
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

