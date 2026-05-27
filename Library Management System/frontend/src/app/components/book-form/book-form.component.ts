import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BookService } from '../../services/book.service';
import { Book } from '../../models/book.model';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function pastOrPresentDateValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }
    const selectedDate = new Date(control.value);
    const today = new Date();
    selectedDate.setHours(0, 0, 0, 0);
    today.setHours(0, 0, 0, 0);
    return selectedDate > today ? { futureDate: true } : null;
  };
}

@Component({
  selector: 'app-book-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './book-form.component.html',
  styleUrls: ['./book-form.component.css']
})
export class BookFormComponent implements OnInit {
  bookForm!: FormGroup;
  isEditMode: boolean = false;
  bookId?: number;
  isLoading: boolean = false;
  isSaving: boolean = false;
  errorMessage: string = '';

  categories: string[] = ['Programming', 'Algorithms', 'Databases', 'Web Development', 'System Design','Software Engineering' ,'Biography', 'Fiction', 'Other'];

  constructor(
    private fb: FormBuilder,
    private bookService: BookService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.checkEditMode();
  }

  initForm(): void {
    this.bookForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      author: ['', [Validators.required, Validators.maxLength(100)]],
      category: ['', [Validators.required]],
      price: ['', [Validators.required, Validators.min(0.01)]],
      publishedDate: ['', [Validators.required, pastOrPresentDateValidator()]],
      isAvailable: [true]
    });
  }

  checkEditMode(): void {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.isEditMode = true;
        this.bookId = +idParam;
        this.loadBook(this.bookId);
      }
    });
  }

  loadBook(id: number): void {
    this.isLoading = true;
    this.bookService.getBookById(id).subscribe({
      next: (book) => {
        this.bookForm.patchValue({
          title: book.title,
          author: book.author,
          category: book.category,
          price: book.price,
          publishedDate: book.publishedDate,
          isAvailable: book.isAvailable
        });

        // Disable fields that cannot be edited as per requirement:
        // "The librarian can edit: Price, Category, Availability status"
        this.bookForm.get('title')?.disable();
        this.bookForm.get('author')?.disable();
        this.bookForm.get('publishedDate')?.disable();
        
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading book:', err);
        this.errorMessage = 'Failed to load book details. The book may not exist.';
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.bookForm.invalid) {
      this.bookForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    // Retrieve raw values to preserve disabled inputs (title and author) in the payload
    const formValue = this.bookForm.getRawValue() as Book;

    if (this.isEditMode && this.bookId) {
      this.bookService.updateBook(this.bookId, formValue).subscribe({
        next: () => {
          this.router.navigate(['/books']);
        },
        error: (err) => {
          console.error('Error updating book:', err);
          if (err.error && typeof err.error === 'object') {
            this.errorMessage = Object.values(err.error).flat().join(' ') || 'Failed to update book.';
          } else {
            this.errorMessage = 'Failed to update book. Please try again.';
          }
          this.isSaving = false;
        }
      });
    } else {
      this.bookService.addBook(formValue).subscribe({
        next: () => {
          this.router.navigate(['/books']);
        },
        error: (err) => {
          console.error('Error adding book:', err);
          if (err.error && typeof err.error === 'object') {
            this.errorMessage = Object.values(err.error).flat().join(' ') || 'Failed to add book.';
          } else {
            this.errorMessage = 'Failed to add book. Please try again.';
          }
          this.isSaving = false;
        }
      });
    }
  }

  isFieldInvalid(field: string): boolean {
    const control = this.bookForm.get(field);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  getFieldError(field: string): string {
    const control = this.bookForm.get(field);
    if (!control || !control.errors) return '';

    if (control.errors['required']) return 'This field is mandatory.';
    if (control.errors['min']) return 'Price must be greater than 0.';
    if (control.errors['maxlength']) return `Length cannot exceed ${control.errors['maxlength'].requiredLength} characters.`;
    if (control.errors['futureDate']) return 'Published date cannot be in the future.';

    return 'Invalid field value.';
  }
}
