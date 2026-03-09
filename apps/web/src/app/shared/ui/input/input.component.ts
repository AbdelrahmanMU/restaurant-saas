import { Component, Input, forwardRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';

@Component({
  selector: 'ui-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './input.component.html',
  styleUrl: './input.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true
    }
  ]
})
export class InputComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() type: 'text' | 'tel' | 'password' = 'text';
  @Input() placeholder = '';
  @Input() autocomplete = 'off';
  @Input() inputmode: string | null = null;
  @Input() error: string | null = null;
  @Input() disabled = false;
  @Input() id = `ui-input-${Math.random().toString(36).slice(2, 8)}`;

  value = '';
  showPassword = signal(false);

  private onChange: (v: string) => void = () => {};
  private onTouched: () => void = () => {};

  get effectiveType(): string {
    if (this.type === 'password') {
      return this.showPassword() ? 'text' : 'password';
    }
    return this.type;
  }

  onInput(event: Event) {
    const val = (event.target as HTMLInputElement).value;
    this.value = val;
    this.onChange(val);
  }

  onBlur() { this.onTouched(); }

  togglePassword() { this.showPassword.update(v => !v); }

  writeValue(val: string) { this.value = val ?? ''; }
  registerOnChange(fn: (v: string) => void) { this.onChange = fn; }
  registerOnTouched(fn: () => void) { this.onTouched = fn; }
  setDisabledState(isDisabled: boolean) { this.disabled = isDisabled; }
}
