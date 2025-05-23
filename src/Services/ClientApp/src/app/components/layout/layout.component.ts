import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, NavigationStart, Router, RouterLinkActive, RouterLink, RouterOutlet } from '@angular/router';
import { LoginService } from 'src/app/auth/login.service';
import { Project } from 'src/app/services/project/models/project.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ProjectService } from 'src/app/services/project/project.service';
import { MatBottomSheet, MatBottomSheetRef } from '@angular/material/bottom-sheet';
import { AdvanceService } from 'src/app/services/advance/advance.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription, fromEvent } from 'rxjs';
import { MatDialog, MatDialogRef, MatDialogContent } from '@angular/material/dialog';
import { MatAutocompleteSelectedEvent, MatAutocompleteTrigger, MatAutocomplete } from '@angular/material/autocomplete';
import { StringComponent } from 'src/app/pages/tools/string/string.component';
import { MatDrawerContainer, MatDrawer, MatDrawerContent } from '@angular/material/sidenav';
import { ChatBotComponent } from '../chatbot/chatbot.component';
import { NgIf } from '@angular/common';
import { MatToolbar } from '@angular/material/toolbar';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatMenuTrigger, MatMenu, MatMenuItem } from '@angular/material/menu';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';
import { QuickNavComponent } from '../quick-nav/quick-nav.component';
import { CdkScrollable } from '@angular/cdk/scrolling';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { MatOption } from '@angular/material/core';
@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css'],
  imports: [MatDrawerContainer, MatDrawer, ChatBotComponent, NgIf, MatDrawerContent, MatToolbar, MatButton, RouterLinkActive, RouterLink, MatMenuTrigger, MatIcon, MatMenu, MatMenuItem, MatIconButton, MatTooltip, RouterOutlet, QuickNavComponent, CdkScrollable, MatDialogContent, MatFormField, MatLabel, MatInput, FormsModule, MatAutocompleteTrigger, MatAutocomplete, MatOption]
})
export class LayoutComponent implements OnInit {
  isLogin = false;
  isAdmin = false;
  isDarkTheme = true;
  openedChat = false;
  username?: string | null = null;
  type: string | null = null;
  projects = [] as Project[];
  projectName = '';
  toolName: string | null = null;
  toolsOptions: string[] = ['StringConvert', 'JsonToCSharp'];
  filteredOptions: string[];
  version: string | null = null;
  @ViewChild("projectSheet", { static: true }) projectSheet!: TemplateRef<{}>;
  bottomSheetRef!: MatBottomSheetRef<{}>;
  dialogRef!: MatDialogRef<{}, any>;
  @ViewChild('quickDialog', { static: true }) quickTmpl!: TemplateRef<{}>;

  keyboardSubscription: Subscription | null = null;

  constructor(
    private auth: LoginService,
    private service: ProjectService,
    private projectState: ProjectStateService,
    private bottomSheet: MatBottomSheet,
    private dialog: MatDialog,
    private advance: AdvanceService,
    public snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute
  ) {
    // this layout is out of router-outlet, so we need to listen router event and change isLogin status
    router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        console.log(event);
        this.isLogin = this.auth.isLogin;
        this.isAdmin = this.auth.isAdmin;
        this.username = this.auth.userName;
      }
    });
    this.route.queryParamMap.subscribe((query) => {
      var type = query.get('type');
      if (type === 'desktop') {
        localStorage.setItem('type', 'desktop');
        this.type = 'desktop'
      }
    });
    this.projectName = this.projectState.project?.displayName || '';
    this.version = this.projectState.version;
    this.filteredOptions = this.toolsOptions.slice();

    const themeMedia = window.matchMedia('(prefers-color-scheme: dark)');
    if (themeMedia.matches) {
      this.isDarkTheme = true;
    } else {
      this.isDarkTheme = false;
    }
    themeMedia.addEventListener('change', (e) => {
      if (e.matches) {
        this.isDarkTheme = true;
      } else {
        this.isDarkTheme = false;
      }
    });
  }

  filter(value: string): void {
    const filterValue = value;
    this.filteredOptions = this.toolsOptions.filter(o => o.toLowerCase().includes(filterValue));
  }

  ngOnInit(): void {
    this.isLogin = this.auth.isLogin;
    this.isAdmin = this.auth.isAdmin;
    if (this.isLogin) {
      this.username = this.auth.userName!;
    }
    this.getVersion();
    this.listenKeyboard();
  }

  ngOnDestroy(): void {
    this.keyboardSubscription?.unsubscribe();
  }

  listenKeyboard() {
    this.keyboardSubscription?.unsubscribe();
    this.keyboardSubscription = fromEvent<KeyboardEvent>(document, 'keydown')
      .subscribe((event: KeyboardEvent) => {
        if (event.ctrlKey && event.key === '/') {
          this.openQuick();
        }
      });
  }
  openChat(): void {
    if (!this.openedChat) {
      this.getOpenAIKey();
    } else {
      this.openedChat = false;
    }
  }

  openQuick(): void {
    this.dialogRef = this.dialog.open(this.quickTmpl, {
      minWidth: '400px'
    });
  }

  selectTool(event: MatAutocompleteSelectedEvent): void {
    this.toolName = event.option.value;
    this.dialogRef.close();
    if (this.toolName === 'StringConvert') {
      this.dialogRef = this.dialog.open(StringComponent, {
        minWidth: '400px'
      });
    } else if (this.toolName === 'JsonToCSharp') {
      this.router.navigateByUrl('/tools/json2Type');
    }
    this.toolName = null;
  }

  getOpenAIKey(): void {
    this.advance.getConfig("deepSeekApiKey")
      .subscribe({
        next: (res) => {
          if (!res || !res.value) {
            this.snb.open('您还未配置ApiKey!', '', {
              verticalPosition: 'top',
              horizontalPosition: 'end',
            });
          } else {
            this.openedChat = true;
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        }
      });
  }

  getVersion(): void {
    this.service.getVersion()
      .subscribe({
        next: (res) => {
          if (res) {
            this.projectState.setVersion(res);
            this.version = res;
          }
        },
      });
  }
  login(): void {
    this.router.navigateByUrl('/login')
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/index');
    location.reload();
  }
}
