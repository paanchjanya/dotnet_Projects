import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, 
         Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BookService } from '../../core/services/book.service';

@Component({
  selector: 'app-book-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './book-form.component.html',
  styleUrl: './book-form.component.scss'
})
export class BookFormComponent implements OnInit {
  form!: FormGroup;
  isEditMode = false;
  bookId!: number;
  submitting = false;
  today = new Date().toISOString().split('T')[0];

  genres = [
    'Programming', 'Software Engineering', 'Computer Science',
    'Web Development', 'Artificial Intelligence', 'Business',
    'Interview Prep', 'Science Fiction', 'Self Help'
  ];

  constructor(
    private fb: FormBuilder,
    private bookService: BookService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  // ✅ Fix 1: Rejects blank/whitespace-only strings
  noWhitespaceValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value || '';
    return value.trim().length === 0 ? { whitespace: true } : null;
  }

  // ✅ Fix 2: Only allows letters, spaces, hyphens, apostrophes (valid name chars)
  alphabeticOnlyValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value || '';
    const valid = /^[a-zA-Z\s\-'.]+$/.test(value.trim());
    return value.trim().length > 0 && !valid ? { notAlphabetic: true } : null;
  }

  pastDateValidator(control: AbstractControl): ValidationErrors | null {
    const selected = new Date(control.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return selected > today ? { futureDate: true } : null;
  }

  ngOnInit() {
    this.form = this.fb.group({
      title: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(150),
        this.noWhitespaceValidator          // 👈 blocks spaces-only
      ]],
      author: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100),
        this.alphabeticOnlyValidator        // 👈 blocks numbers
      ]],
      genre: ['', Validators.required],
      price: [null, [
        Validators.required,
        Validators.min(0),
        Validators.max(9999.99)
      ]],
      publishedDate: ['', [
        Validators.required,
        this.pastDateValidator
      ]]
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.bookId = +id;
      this.bookService.getById(this.bookId).subscribe(book => {
        this.form.patchValue({
          ...book,
          publishedDate: book.publishedDate.split('T')[0]
        });
      });
    }
  }

  get f() { return this.form.controls; }

  onSubmit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.submitting = true;

    const payload = {
      ...this.form.value,
      title: this.form.value.title.trim(),    // 👈 trim before sending
      author: this.form.value.author.trim(),  // 👈 trim before sending
      publishedDate: new Date(this.form.value.publishedDate).toISOString()
    };

    const request = this.isEditMode
      ? this.bookService.update(this.bookId, payload)
      : this.bookService.create(payload);

    request.subscribe({
      next: () => this.router.navigate(['/books']),
      error: () => { this.submitting = false; }
    });
  }
}