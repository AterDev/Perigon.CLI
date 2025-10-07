import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { SystemMenu } from 'src/app/services/admin/models/system-menu.model';
import { SystemMenuService } from 'src/app/services/admin/system-menu.service';
import { SystemRoleService } from 'src/app/services/admin/system-role.service';
import { Observable, forkJoin } from 'rxjs';
import { MatTreeModule, MatTreeNestedDataSource } from '@angular/material/tree';
import { MatCheckboxChange, MatCheckboxModule } from '@angular/material/checkbox';
import { CommonModules } from 'src/app/share/shared-modules';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { PageListOfSystemMenu } from 'src/app/services/admin/models/page-list-of-system-menu.model';

@Component({
  selector: 'app-menus',
  imports: [CommonModules, MatTreeModule, MatCheckboxModule, MatButtonModule, MatCardModule],
  templateUrl: './menus.html',
  styleUrl: './menus.scss'
})
export class Menus {
  id!: string;
  isLoading = true;
  data: SystemMenu[] = [];
  menuNods = [] as MenuNode[];
  dataSource = new MatTreeNestedDataSource<MenuNode>();
  selectedIds = [] as string[];

  constructor(
    private route: ActivatedRoute,
    private snb: MatSnackBar,
    private service: SystemMenuService,
    private roleService: SystemRoleService,
    public dialogRef: MatDialogRef<Menus>,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }
  ) {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id = id;
    } else {
      this.id = dlgData.id;
    }
  }

  childrenAccessor = (node: MenuNode) => node.children ?? [];
  hasChild = (_: number, node: MenuNode) => !!node.children && node.children.length > 0;

  transformMenuToNode(menu: SystemMenu[], checkedMenus: SystemMenu[]): MenuNode[] {
    return menu.map((item) => {
      return {
        id: item.id,
        name: item.name,
        children: item.children ? this.transformMenuToNode(item.children, checkedMenus) : null,
        checked: checkedMenus.some(checkedMenu => checkedMenu.id === item.id)
      };
    });
  }

  ngOnInit(): void {
    forkJoin([this.getMemusAsync(), this.getRoleMenusAsync()])
      .subscribe({
        next: ([menus, roleMenus]) => {
          if (menus.data && roleMenus.data) {
            this.menuNods = this.transformMenuToNode(menus.data, roleMenus.data);
            this.dataSource.data = this.menuNods;
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

  getMemusAsync(): Observable<PageListOfSystemMenu> {
    return this.service.filter({ pageIndex: 1, pageSize: 999 });
  }

  getRoleMenusAsync(): Observable<PageListOfSystemMenu> {
    return this.service.filter({
      pageIndex: 1,
      pageSize: 100,
      roleId: this.id
    });
  }

  onCheckboxChange(node: MenuNode, event: any): void {
    node.checked = event.checked
  }

  onParentNodeChange(node: MenuNode, event: MatCheckboxChange) {
    node.children?.forEach(child => {
      child.checked = event.checked;
    });
  }

  isSelectAll(node: MenuNode) {
    if (node.children) {
      return node.children.every(child => child.checked === true);
    }
    return false;
  }

  hasSelected(node: MenuNode) {
    if (node.children) {
      if (node.children.every(child => child.checked === true)) {
        return false;
      }
      return node.children.some(child => child.checked === true);
    }
    return false;
  }

  getSelectedMenuIds(menus: MenuNode[]): void {
    for (const element of menus) {
      if (element.checked) {
        this.selectedIds.push(element.id);
      }
      if (element.children) {
        this.getSelectedMenuIds(element.children);
      }
    }
  }

  save(): void {
    this.selectedIds = [];
    this.getSelectedMenuIds(this.menuNods);

    this.roleService.updateMenus({
      id: this.id,
      menuIds: this.selectedIds
    }).subscribe({
      next: (res) => {
        if (res) {
          this.snb.open('保存成功');
          this.dialogRef.close(res);
        }
      },
      error: (error) => {
        this.snb.open(error.detail);
      }
    });
  }
}


interface MenuNode {
  id: string;
  name: string;
  children: MenuNode[] | null;
  checked: boolean;
}
