import { Routes } from '@angular/router';


export const routes: Routes = [
  { path: '', redirectTo: 'books', pathMatch: 'full' },
  {
    path: 'books',
    loadComponent: () =>
      import('./pages/book-list/book-list.component').then(m => m.BookListComponent)
  },
  {
    path: 'books/new',
    loadComponent: () =>
      import('./pages/book-form/book-form.component').then(m => m.BookFormComponent)
  },
  {
    path: 'books/edit/:id',
    loadComponent: () =>
      import('./pages/book-form/book-form.component').then(m => m.BookFormComponent)
  }
];