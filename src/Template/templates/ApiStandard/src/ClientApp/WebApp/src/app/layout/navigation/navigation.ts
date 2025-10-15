import { Component, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseMatModules, CommonModules } from 'src/app/share/shared-modules';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-navigation',
  imports: [...BaseMatModules, ...CommonModules, MatSidenavModule, MatExpansionModule, MatListModule],
  templateUrl: './navigation.html',
  styleUrl: './navigation.scss'
})
export class NavigationComponent {
  events: string[] = [];
  opened = true;
  expanded = true;
  menus = signal<Menu[]>([]);
  constructor(
    private http: HttpClient,
  ) {
  }
  ngOnInit(): void {
    this.updateMenus();
  }

  toggle(): void {
    this.opened = !this.opened;
  }

  updateMenus(): void {
    this.http.get<Menu[]>('/assets/menus.json?_t=' + Date.now(), { responseType: 'json' })
      .subscribe({
        next: (res) => {
          this.menus.set(res.sort((a, b) => a.sort - b.sort));

          // const userMenus = JSON.parse(localStorage.getItem('menus') ?? 'null') ?? [];
          // const userMenuCodes = userMenus.map((item: any) => item.accessCode);
          // this.menus = this.mergeMenu(userMenuCodes, this.menus);
        }
      });
  }
  mergeMenu(userMenuCodes: string[], menus: Menu[]): Menu[] {
    // 只保留有权限的菜单
    return menus.filter((item) => {
      if (userMenuCodes.includes(item.accessCode)) {
        if (item.children) {
          item.children = this.mergeMenu(userMenuCodes, item.children);
        }
        return true;
      }
      return false;
    });
  }
}
export interface Menu {
  name: string,
  path: string | null,
  accessCode: string,
  icon: string,
  sort: number,
  menuType: 0 | 1,
  children?: Menu[] | null,
}
